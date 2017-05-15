using System.Collections.Generic;
using System.Diagnostics;

namespace logreview
{
    [DebuggerDisplay("{ToString(),nq}")]
    internal sealed class Review
    {
        public Review(IReadOnlyCollection<ReviewClient> clients, IReadOnlyCollection<string> userNamesSucceeded, IReadOnlyCollection<string> userNamesFailed, IReadOnlyCollection<string> clientVersions)
        {
            Clients = clients;
            UserNamesSucceeded = userNamesSucceeded;
            UserNamesFailed = userNamesFailed;
            ClientVersions = clientVersions;
        }

        public IReadOnlyCollection<ReviewClient> Clients { get; }

        public IReadOnlyCollection<string> UserNamesSucceeded { get; }

        public IReadOnlyCollection<string> UserNamesFailed { get; }

        public IReadOnlyCollection<string> ClientVersions { get; }

        public override string ToString()
        {
            return $"{Clients.Count} clients";
        }
    }
}
