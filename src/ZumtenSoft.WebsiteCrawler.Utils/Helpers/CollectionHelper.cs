using System;
using System.Collections.Generic;

namespace ZumtenSoft.WebsiteCrawler.Utils.Helpers
{
    public static class CollectionHelper
    {


        public static void TryAdd<T>(this ICollection<T> list, T item) where T : class
        {
            if (item != null)
                list.Add(item);
        }

        public static void Add<T1, T2>(this IList<Pair<T1, T2>> list, T1 value1, T2 value2)
        {
            list.Add(new Pair<T1, T2>(value1, value2));
        }

        public static ICollection<T> AddRange<T>(this ICollection<T> list, params T[] values)
        {
            foreach (var v in values)
                list.Add(v);
            return list;
        }

        public static ICollection<T> AddRange<T>(this ICollection<T> list, IEnumerable<T> values)
        {
            foreach (var v in values)
                list.Add(v);
            return list;
        }

        public static void RemoveRange<T>(this ICollection<T> list, IEnumerable<T> toDelete)
        {
            foreach (var item in toDelete)
                list.Remove(item);
        }

        public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> toDelete)
        {
            foreach (var item in toDelete)
                dict.Remove(item);
        }

        public static void Sort<T>(this IList<T> list, Func<T, T, int> compare)
        {
            const int startIndex = 0;
            int endIndex = list.Count - 1;
            //This extension method is visible where the namespace
            //Extensions is brought into scope

            //Bubble Sort
            for (int i = startIndex; i < endIndex; i++)
                for (int j = endIndex; j > i; j--)
                    if (compare(list[j], list[j - 1]) < 0)
                        list.Exchange(j, j - 1);
        }

        private static void Exchange<T>(this IList<T> list, int index1, int index2)
        {
            //This extension methods is only visible within SortingExtensionsProvider

            T tmp = list[index1];
            list[index1] = list[index2];
            list[index2] = tmp;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            TValue value;
            if (dict.TryGetValue(key, out value))
                return value;
            return default(TValue);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            TValue value;
            if (dict.TryGetValue(key, out value))
                return value;
            return defaultValue;
        }
    }
}