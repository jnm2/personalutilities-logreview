using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace logreview
{
    [DebuggerDisplay("{ToString(),nq}")]
    internal sealed class ReviewClient
    {
        public ReviewClient(
            IPAddress ipAddress,
            IReadOnlyList<DateTime> eventTimes,
            IReadOnlyList<DateTime> blockedEventTimes,
            IReadOnlyCollection<string> userNamesSucceeded,
            IReadOnlyCollection<string> userNamesFailed,
            IReadOnlyCollection<string> clientVersions)
        {
            if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));
            if (eventTimes == null) throw new ArgumentNullException(nameof(eventTimes));
            if (eventTimes.Count == 0) throw new ArgumentException("Event times cannot be an empty collection.", nameof(eventTimes));

            IPAddress = ipAddress;
            EventTimes = eventTimes;
            BlockedEventTimes = blockedEventTimes;
            UserNamesSucceeded = userNamesSucceeded;
            UserNamesFailed = userNamesFailed;
            ClientVersions = clientVersions;
        }

        public IPAddress IPAddress { get; }

        public IReadOnlyList<DateTime> EventTimes { get; }

        public IReadOnlyList<DateTime> BlockedEventTimes { get; }

        public IReadOnlyCollection<string> UserNamesSucceeded { get; }

        public IReadOnlyCollection<string> UserNamesFailed { get; }

        public IReadOnlyCollection<string> ClientVersions { get; }

        public override string ToString()
        {
            var min = EventTimes.Min();
            var max = EventTimes.Max();

            return min.Date == max.Date
                ? $"{IPAddress}, {EventTimes.Count} events {min:g} to {max:t}"
                : $"{IPAddress}, {EventTimes.Count} events {min:d} to {max:d}";
        }
    }
}
