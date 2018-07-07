using System.Collections.Generic;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Utils
{
    public static class DictionaryHelper
    {
        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            TValue value;
            dict.TryGetValue(key, out value);
            return value;
        }
    }
}
