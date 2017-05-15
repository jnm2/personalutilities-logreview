using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace logreview
{
    internal sealed class IPAddressComparer : IComparer<IPAddress>
    {
        public static readonly IPAddressComparer Instance = new IPAddressComparer();
        private IPAddressComparer() { }

        public int Compare(IPAddress x, IPAddress y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            return ((IStructuralComparable)x.GetAddressBytes()).CompareTo(y.GetAddressBytes(), Comparer<byte>.Default);
        }
    }
}
