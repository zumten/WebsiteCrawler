using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ZumtenSoft.WebsiteCrawler.Utils.Collections
{
    public class ReferenceComparer<T> : IEqualityComparer<T>, IComparer<T>
    {
        private static readonly ReferenceComparer<T> _instance = new ReferenceComparer<T>();
        public static ReferenceComparer<T> Instance { get { return _instance; } }

        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }

        public int Compare(T x, T y)
        {
            return RuntimeHelpers.GetHashCode(x).CompareTo(RuntimeHelpers.GetHashCode(y));
        }
    }
}
