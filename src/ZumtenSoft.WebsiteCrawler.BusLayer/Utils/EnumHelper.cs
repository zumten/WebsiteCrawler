using System;
using System.Collections.Generic;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Utils
{
    /// <summary>
    /// Méthodes utilitaires pour travailler avec les enums.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class EnumHelper<T> where T : struct, IComparable
    {
        private static readonly Dictionary<string, T> Cache = new Dictionary<string, T>();
        private static readonly Dictionary<string, T> CacheLowerCase = new Dictionary<string, T>();


        static EnumHelper()
        {
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                Cache[value.ToString()] = value;
                CacheLowerCase[value.ToString().ToLower()] = value;
            }
        }

        public static ICollection<T> Values
        {
            get { return Cache.Values; }
        }

        public static ICollection<string> Names
        {
            get { return Cache.Keys; }
        }

        public static T Parse(string name, bool ignoreCase = false)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            T value;

            if (ignoreCase)
            {
                if (CacheLowerCase.TryGetValue(name.Trim().ToLower(), out value))
                    return value;
            }
            else
            {
                if (Cache.TryGetValue(name.Trim(), out value))
                    return value;
            }

            throw new ArgumentException("Invalid name for type " + typeof(T).Name, "name");
        }

        public static T? TryParse(string name, bool ignoreCase = false)
        {
            T value;

            if (ignoreCase)
            {
                if (name != null && CacheLowerCase.TryGetValue(name.Trim().ToLower(), out value))
                    return value;
            }
            else
            {
                if (name != null && Cache.TryGetValue(name.Trim(), out value))
                    return value;
            }

            return null;
        }

        public static T TryParse(string name, T defaultValue, bool ignoreCase = false)
        {
            return TryParse(name, ignoreCase) ?? defaultValue;
        }
    }
}
