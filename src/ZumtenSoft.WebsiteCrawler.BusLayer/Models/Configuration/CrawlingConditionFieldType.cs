using System;
using System.Collections.Generic;
using System.Diagnostics;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration
{
    [DebuggerDisplay(@"\{CrawlingConditionFieldType Name={Name}, RequireParam={RequireParam}\}")]
    public class CrawlingConditionFieldType
    {
        private CrawlingConditionFieldType(string name, bool requireParam, Func<Uri, string, string> extractValue)
        {
            Name = name;
            RequireParam = requireParam;
            ExtractValue = extractValue;
        }

        public string Name { get; private set; }
        public bool RequireParam { get; private set; }
        public Func<Uri, string, string> ExtractValue { get; private set; }

        private static readonly Dictionary<string, CrawlingConditionFieldType> Mapping = new Dictionary<string, CrawlingConditionFieldType>(StringComparer.InvariantCultureIgnoreCase); 
        private static CrawlingConditionFieldType Create(string name, bool requireParam, Func<Uri, string, string> extractValue)
        {
            CrawlingConditionFieldType element = new CrawlingConditionFieldType(name, requireParam, extractValue);
            Mapping.Add(element.Name, element);
            return element;
        }

        public static CrawlingConditionFieldType Parse(string name)
        {
            return Mapping.TryGetValue(name);
        }

        public static readonly CrawlingConditionFieldType Host = Create("Host", false, (r, _) => r.Host);
        public static readonly CrawlingConditionFieldType Scheme = Create("Scheme", false, (r, _) => r.Scheme);
        public static readonly CrawlingConditionFieldType Path = Create("Path", false, (r, _) => r.AbsolutePath);
        public static readonly CrawlingConditionFieldType Uri = Create("Uri", false, (r, _) => r.AbsoluteUri);
        public static readonly CrawlingConditionFieldType Query = Create("Query", false, (r, _) => r.Query);
        public static ICollection<CrawlingConditionFieldType> Values { get { return Mapping.Values; } }
    }
}