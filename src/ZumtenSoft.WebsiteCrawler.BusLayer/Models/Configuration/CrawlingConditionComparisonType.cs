using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration
{
    [DebuggerDisplay(@"\{CrawlingConditionComparisonType Name={Name}\}")]
    public class CrawlingConditionComparisonType
    {
        private CrawlingConditionComparisonType(string key, string name, Func<string, string, bool> condition)
        {
            Key = key;
            Name = name;
            Condition = condition;
        }

        public string Key { get; private set; }
        public string Name { get; private set; }
        public Func<string, string, bool> Condition { get; private set; }

        private static readonly Dictionary<string, CrawlingConditionComparisonType> Mapping = new Dictionary<string, CrawlingConditionComparisonType>(StringComparer.InvariantCultureIgnoreCase); 
        private static CrawlingConditionComparisonType Create(string key, string name, Func<string, string, bool> condition)
        {
            CrawlingConditionComparisonType element = new CrawlingConditionComparisonType(key, name, condition);
            Mapping.Add(element.Key, element);
            return element;
        }

        public static CrawlingConditionComparisonType Parse(string name)
        {
            return Mapping.TryGetValue(name);
        }

        public static new readonly CrawlingConditionComparisonType Equals = Create("Equals", "equals", (x, y) => String.Equals(x, y, StringComparison.InvariantCultureIgnoreCase));
        public static readonly CrawlingConditionComparisonType StartsWith = Create("StartsWith", "starts with", (x, y) => x.IndexOf(y, StringComparison.InvariantCultureIgnoreCase) >= 0);
        public static readonly CrawlingConditionComparisonType EndsWith = Create("EndsWith", "ends with", (x, y) => x.IndexOf(y, StringComparison.InvariantCultureIgnoreCase) >= 0);
        public static readonly CrawlingConditionComparisonType Contains = Create("Contains", "contains", (x, y) => x.IndexOf(y, StringComparison.InvariantCultureIgnoreCase) >= 0);
        public static readonly CrawlingConditionComparisonType MatchingRegex = Create("MatchingRegex", "matching regex", (x, y) => Regex.IsMatch(x, y, RegexOptions.IgnoreCase));
        public static readonly CrawlingConditionComparisonType NotMatchingRegex = Create("NotMatchingRegex", "not matching regex", (x, y) => !Regex.IsMatch(x, y, RegexOptions.IgnoreCase));
        public static ICollection<CrawlingConditionComparisonType> Values { get { return Mapping.Values; } }
    }
}