using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace logreview
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            WriteSummary(
                BuildReview(
                    args,
                    GetRemoteAddressesToOmit("omit.txt"),
                    ex => Console.WriteLine(ex.Message)),
                Console.Out);
        }

        private static ICollection<IPAddress> GetRemoteAddressesToOmit(string filename)
        {
            var omitRemoteAddresses = new HashSet<IPAddress>();

            foreach (var line in File.ReadAllLines(filename))
                omitRemoteAddresses.Add(IPAddress.Parse(line));

            return omitRemoteAddresses;
        }

        private static Review BuildReview(IEnumerable<string> searchPaths, ICollection<IPAddress> omitRemoteAddresses, Action<IOException> ioExceptionHandler)
        {
            var builder = new ReviewBuilder();

            foreach (var arg in searchPaths)
            foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(arg), Path.GetFileName(arg), SearchOption.AllDirectories))
            {
                try
                {
                    using (var stream = File.OpenRead(file))
                    {
                        ReadLog(builder, stream, omitRemoteAddresses);
                    }
                }
                catch (IOException ex)
                {
                    ioExceptionHandler.Invoke(ex);
                }
            }

            return builder.Build();
        }

        private static void ReadLog(ReviewBuilder builder, Stream stream, ICollection<IPAddress> omitRemoteAddresses)
        {
            using (var xml = XmlReader.Create(stream))
            {
                while (xml.ReadToFollowing("event"))
                {
                    xml.MoveToAttribute("time");
                    var time = DateTime.Parse(xml.Value);
                    xml.MoveToAttribute("name");
                    var eventName = xml.Value;

                    xml.MoveToElement();
                    if (!(xml.ReadToDescendant("session") && xml.MoveToAttribute("remoteAddress"))) continue;

                    var remoteAddressRaw = xml.Value;
                    var portIndex = remoteAddressRaw.IndexOf(':');
                    var remoteAddress = IPAddress.Parse(portIndex != -1 ? remoteAddressRaw.Substring(0, portIndex) : remoteAddressRaw);
                    if (omitRemoteAddresses.Contains(remoteAddress)) continue;

                    builder.AddRemoteEvent(time, remoteAddress, eventName == "I_CONNECT_REJECTED");

                    switch (eventName)
                    {
                        case "I_LOGON_AUTH_FAILED":
                        case "I_LOGON_AUTH_SUCCEEDED":
                            if (!(xml.ReadToNextSibling("authentication") && xml.MoveToAttribute("userName")))
                                throw new NotImplementedException("I_LOGON_AUTH_FAILED without userName");
                            builder.AddLoginAttempt(remoteAddress, xml.Value, eventName == "I_LOGON_AUTH_SUCCEEDED");
                            break;
                        case "I_CONNECT_VERSION_RECEIVED":
                            if (!(xml.ReadToNextSibling("parameters") && xml.MoveToAttribute("clientVersion")))
                                throw new NotImplementedException("I_CONNECT_VERSION_RECEIVED without clientVersion");
                            builder.AddClientVersion(remoteAddress, xml.Value);
                            break;
                    }
                }
            }
        }

        private static void WriteSummary(Review review, TextWriter writer)
        {
            if (review.UserNamesSucceeded.Count != 0)
            {
                writer.WriteLine();
                writer.WriteLine("Usernames (successful logins):");
                foreach (var userName in review.UserNamesSucceeded.OrderBy(_ => _))
                    writer.WriteLine(userName);
            }

            writer.WriteLine();
            writer.WriteLine("Usernames (failed logins):");
            foreach (var userName in review.UserNamesFailed.OrderBy(_ => _))
                writer.WriteLine(userName);

            var addressesByWhetherLoginAttempted = review.Clients.ToLookup(
                _ => _.UserNamesSucceeded.Count != 0 || _.UserNamesFailed.Count != 0,
                _ => _.IPAddress);

            writer.WriteLine();
            writer.WriteLine("Clients with login attempts:");
            foreach (var address in addressesByWhetherLoginAttempted[true].OrderBy(_ => _, IPAddressComparer.Instance))
                writer.WriteLine(address);

            writer.WriteLine();
            writer.WriteLine("Clients without login attempts:");
            foreach (var address in addressesByWhetherLoginAttempted[false].OrderBy(_ => _, IPAddressComparer.Instance))
                writer.WriteLine(address);

            writer.WriteLine();
            writer.WriteLine("User agents:");
            foreach (var clientVersion in review.ClientVersions.OrderBy(_ => _))
                writer.WriteLine(clientVersion);
        }
    }
}
