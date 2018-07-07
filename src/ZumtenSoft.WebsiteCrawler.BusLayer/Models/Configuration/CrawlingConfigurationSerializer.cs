using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction.Behaviors;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;
using ZumtenSoft.Linq2ObsCollection.Collections;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration
{
    public static class CrawlingConfigurationSerializer
    {
        public static CrawlingConfig Clone(CrawlingConfig config)
        {
            return DeserializeConfiguration(SerializeConfiguration(config));
        }

        public static CrawlingRule Clone(CrawlingRule rule)
        {
            CrawlingRule clone = DeserializeRule(SerializeRule(rule), new List<CrawlingBucket>());
            clone.TargetBucket = rule.TargetBucket;
            return clone;
        }

        public static CrawlingBucket Clone(CrawlingBucket bucket)
        {
            return DeserializeBucket(SerializeBucket(bucket));
        }

        public static XElement SerializeConfiguration(CrawlingConfig config)
        {
            return new XElement("CrawlingConfig",
                new XAttribute("Guid", config.Guid),
                new XAttribute("Name", config.Name),
                new XAttribute("Description", config.Description),
                new XElement("Buckets", config.Buckets.Select(SerializeBucket)),
                new XElement("Rules", config.Rules.Select(SerializeRule)),
                new XElement("StartingUrls", config.StartingUrls.Select(SerializeStartingUrl)));
        }

        private static XElement SerializeBucket(CrawlingBucket bucket)
        {
            return new XElement("Bucket",
                new XAttribute("Guid", bucket.Guid),
                new XAttribute("Name", bucket.Name),
                new XAttribute("Description", bucket.Description),
                new XAttribute("NbThreads", bucket.NbThreads),
                new XAttribute("NbRetry", bucket.NbRetry),
                new XAttribute("LimitRequests", bucket.LimitRequests),
                new XElement("HostMappings", bucket.HostMappings.Select(SerializeHostMapping)));
        }

        private static XElement SerializeRule(CrawlingRule rule)
        {
            return new XElement("Rule",
                new XAttribute("Guid", rule.Guid),
                new XAttribute("Name", rule.Name),
                new XAttribute("Description", rule.Description),
                new XAttribute("Behavior", rule.Behavior),
                rule.TargetBucket == null ? null : new XAttribute("TargetBucket", rule.TargetBucket.Guid),
                new XElement("Conditions", rule.Conditions.Select(SerializeCondition)));
        }

        private static XElement SerializeCondition(CrawlingCondition condition)
        {
            return new XElement("Condition",
                new XAttribute("Guid", condition.Guid),
                new XAttribute("FieldType", condition.FieldType.Name),
                new XAttribute("ComparisonType", condition.ComparisonType.Key),
                new XAttribute("Value", condition.Value));
        }

        private static XElement SerializeStartingUrl(CrawlingStartingUrl startingUrl)
        {
            return new XElement("StartingUrl",
                new XAttribute("Guid", startingUrl.Guid),
                new XAttribute("Name", startingUrl.Name),
                new XAttribute("Value", startingUrl.Value.AbsoluteUri));
        }

        private static XElement SerializeHostMapping(CrawlingHostMapping mapping)
        {
            return new XElement("HostMapping",
                new XAttribute("Host", mapping.Host),
                new XAttribute("IPAddress", mapping.IPAddress));
        }

        public static CrawlingConfig DeserializeConfiguration(XElement element)
        {
            CrawlingConfig config = new CrawlingConfig
            {
                Guid = (Guid)element.Attribute("Guid"),
                Name = (string)element.Attribute("Name"),
                Description = (string)element.Attribute("Description"),
            };

            config.Buckets.AddRange(element.Elements("Buckets").Elements().Select(DeserializeBucket));
            config.Rules.AddRange(element.Elements("Rules").Elements().Select(x => DeserializeRule(x, config.Buckets)));
            config.StartingUrls.AddRange(element.Elements("StartingUrls").Elements().Select(DeserializeStartingUrl));
            return config;
        }

        private static CrawlingBucket DeserializeBucket(XElement element)
        {
            CrawlingBucket bucket = new CrawlingBucket
            {
                Guid = (Guid) element.Attribute("Guid"),
                Name = (string) element.Attribute("Name"),
                Description = (string) element.Attribute("Description"),
                NbThreads = (int) element.Attribute("NbThreads"),
                NbRetry = (int) element.Attribute("NbRetry"),
                LimitRequests = (int) element.Attribute("LimitRequests")
            };

            bucket.HostMappings.AddRange(element.Elements("HostMappings").Elements().Select(DeserializeHostMapping));

            return bucket;
        }

        private static CrawlingRule DeserializeRule(XElement element, ICollection<CrawlingBucket> buckets)
        {
            Guid? targetBucket = (Guid?) element.Attribute("TargetBucket");
            CrawlingRule rule = new CrawlingRule
            {
                Guid = (Guid) element.Attribute("Guid"),
                Name = (string) element.Attribute("Name"),
                Description = (string) element.Attribute("Description"),
                Behavior = EnumHelper<ResourceBehavior>.Parse((string)element.Attribute("Behavior")),
                TargetBucket = buckets.FirstOrDefault(x => x.Guid == targetBucket),
            };

            rule.Conditions.AddRange(element.Elements("Conditions").Elements().Select(DeserializeCondition));
            return rule;
        }

        private static CrawlingCondition DeserializeCondition(XElement element)
        {
            CrawlingCondition condition = new CrawlingCondition
            {
                Guid = (Guid) element.Attribute("Guid"),
                ComparisonType = CrawlingConditionComparisonType.Parse((string)element.Attribute("ComparisonType")),
                FieldType = CrawlingConditionFieldType.Parse((string)element.Attribute("FieldType")),
                Value = (string)element.Attribute("Value")
            };

            return condition;
        }

        private static CrawlingStartingUrl DeserializeStartingUrl(XElement element)
        {
            return new CrawlingStartingUrl
            {
                Guid = (Guid) element.Attribute("Guid"),
                Name = (string) element.Attribute("Name"),
                Value = new Uri((string)element.Attribute("Value"))
            };
        }

        private static CrawlingHostMapping DeserializeHostMapping(XElement element)
        {
            return new CrawlingHostMapping
            {
                Host = (string)element.Attribute("Host"),
                IPAddress = IPAddress.Parse((string)element.Attribute("IPAddress")),
            };
        }
    }
}
