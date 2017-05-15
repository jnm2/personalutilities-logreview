using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace logreview
{
    internal sealed class ReviewBuilder
    {
        private struct ClientInfo
        {
            public readonly List<(DateTime time, bool isBlocked)> Events;
            public readonly HashSet<string> UserNamesSucceeded;
            public readonly HashSet<string> UserNamesFailed;
            public readonly HashSet<string> ClientVersions;

            private ClientInfo(bool _)
            {
                Events = new List<(DateTime time, bool isBlocked)>();
                UserNamesSucceeded = new HashSet<string>();
                UserNamesFailed = new HashSet<string>();
                ClientVersions = new HashSet<string>();
            }

            public static ClientInfo New() => new ClientInfo(default(bool));
        }

        private readonly Dictionary<IPAddress, ClientInfo> clients = new Dictionary<IPAddress, ClientInfo>();

        private readonly Dictionary<string, (int successes, int failures, int clientsSucceeded, int clientsFailed)> userNames = new Dictionary<string, (int successes, int failures, int clientsSucceeded, int clientsFailed)>();
        private readonly Dictionary<string, (int instances, int clients)> clientVersions = new Dictionary<string, (int, int)>();



        public void AddRemoteEvent(DateTime time, IPAddress remoteAddress, bool isBlocked)
        {
            GetClientInfo(remoteAddress).Events.Add((time, isBlocked));
        }

        private ClientInfo GetClientInfo(IPAddress remoteAddress)
        {
            if (!clients.TryGetValue(remoteAddress, out var info))
                clients.Add(remoteAddress, info = ClientInfo.New());
            return info;
        }

        public Review Build()
        {
            return new Review(
                (from client in clients select new ReviewClient(
                    client.Key,
                    (from e in client.Value.Events where !e.isBlocked select e.time).ToList(),
                    (from e in client.Value.Events where e.isBlocked select e.time).ToList(),
                    client.Value.UserNamesSucceeded.ToList(),
                    client.Value.UserNamesFailed.ToList(),
                    client.Value.ClientVersions.ToList())
                ).ToList(),
                (from userName in userNames where userName.Value.successes != 0 select userName.Key).ToList(),
                (from userName in userNames where userName.Value.failures != 0 select userName.Key).ToList(),
                clientVersions.Keys.ToList());
        }

        public void AddLoginAttempt(IPAddress remoteAddress, string userName, bool success)
        {
            var value = userNames.GetValueOrDefault(userName);

            if (success)
            {
                value.successes++;
                if (GetClientInfo(remoteAddress).UserNamesSucceeded.Add(userName))
                    value.clientsSucceeded++;
            }
            else
            {
                value.failures++;
                if (GetClientInfo(remoteAddress).UserNamesFailed.Add(userName))
                    value.clientsFailed++;
            }

            userNames[userName] = value;
        }

        public void AddClientVersion(IPAddress remoteAddress, string clientVersion)
        {
            var value = clientVersions.GetValueOrDefault(clientVersion);
            value.instances++;
            if (GetClientInfo(remoteAddress).ClientVersions.Add(clientVersion)) value.clients++;
            clientVersions[clientVersion] = value;
        }
    }
}