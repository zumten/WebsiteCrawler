using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction.Behaviors;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;
using ZumtenSoft.WebsiteCrawler.Utils.Helpers;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Reporting
{
    public static class ResourcesSerializer
    {
        public static XElement SerializeResourceCollection(ICollection<Resource> resources)
        {
            return new XElement("Resources",
                resources.Select(SerializeResource));
        }

        private static XElement SerializeResource(Resource resource)
        {
            return new XElement("Resource",
                new XAttribute("Url", resource.Url.AbsoluteUri),
                new XAttribute("Behavior", resource.Behavior),
                new XAttribute("Status", resource.Status),
                new XAttribute("HttpStatus", resource.HttpStatus),
                new XAttribute("ContentType", resource.ContentType),
                new XAttribute("TimeStart", resource.TimeStart),
                new XAttribute("TimeLoading", resource.TimeLoading.ToString()),
                new XAttribute("TimeProcessing", resource.TimeProcessing.ToString()),
                new XAttribute("Size", resource.Size),
                new XAttribute("CompressedSize", resource.CompressedSize),
                resource.ViewStateSize.HasValue ? new XAttribute("ViewStateSize", resource.ViewStateSize.Value) : null,

                from header in resource.Headers
                select new XElement("Header",
                    new XAttribute("Key", header.Key.Clean()),
                    new XAttribute("Value", header.Value.Clean())),

                from meta in resource.Content
                select new XElement("Content",
                    new XAttribute("Key", meta.Key.ToString()),
                    OptionalAttribute("SubKey", meta.SubKey),
                    new XAttribute("Value", meta.Value.Clean())),

                from link in resource.References
                select new XElement("Reference",
                    new XAttribute("Type", link.Type),
                    link.SubType == ReferenceSubType.None ? null : new XAttribute("SubType", link.SubType),
                    new XAttribute("Url", link.Url.OriginalString),
                    link.Count == 1 ? null : new XAttribute("Count", link.Count)),
                
                from error in resource.Errors
                select new XElement("Error",
                    new XAttribute("Type", error.Type),
                    new XAttribute("Message", error.Message),
                    OptionalAttribute("Value", error.Value),
                    OptionalAttribute("Line", error.Line)));
        }

        public static Resource DeserializeResource(XElement node)
        {
            Uri url = new Uri((string)node.Attribute("Url"));
            ResourceBehavior behavior = EnumHelper<ResourceBehavior>.Parse((string) node.Attribute("Behavior"));
            Resource resource = new Resource(url, behavior);
            resource.Status = EnumHelper<ResourceStatus>.Parse((string)node.Attribute("Status"));
            resource.HttpStatus = EnumHelper<HttpStatusCode>.TryParse((string)node.Attribute("HttpStatus")) ?? (HttpStatusCode)0;
            resource.ContentType = EnumHelper<ResourceContentType>.Parse((string)node.Attribute("ContentType"));
            resource.TimeStart = TryParseDateTime(node.Attribute("TimeStart"));
            resource.TimeLoading = TimeSpan.Parse((string)node.Attribute("TimeLoading"));
            resource.TimeProcessing = TimeSpan.Parse((string)node.Attribute("TimeProcessing"));
            resource.Size = (int)node.Attribute("Size");
            resource.CompressedSize = (int)node.Attribute("CompressedSize");
            resource.ViewStateSize = (int?) node.Attribute("ViewStateSize");

            foreach (XElement header in node.Descendants("Header"))
                resource.Headers.Add((string) header.Attribute("Key"), (string) header.Attribute("Value"));

            foreach (XElement meta in node.Descendants("Content"))
            {
                ResourceContentKey? resourceKey = EnumHelper<ResourceContentKey>.TryParse((string) meta.Attribute("Key"));
                resource.Content.Add(new ResourceContent
                {
                    Key = resourceKey ?? ResourceContentKey.Meta,
                    SubKey = (string) meta.Attribute(resourceKey.HasValue ? "SubKey" : "Key"),
                    Value = (string) meta.Attribute("Value")
                });
            }

            foreach (XElement link in node.Descendants("Reference"))
                resource.References.Add(
                    EnumHelper<ResourceReferenceTypes>.Parse((string)link.Attribute("Type")),
                    new Uri((string) link.Attribute("Url"), UriKind.RelativeOrAbsolute),
                    EnumHelper<ReferenceSubType>.TryParse((string)link.Attribute("SubType"), ReferenceSubType.None, true),
                    NumberHelper.TryParseInt32((string) link.Attribute("Count")) ?? 1);

            foreach (XElement error in node.Descendants("Error"))
            {
                resource.Errors.Add(new ResourceError
                {
                    Type = EnumHelper<ResourceErrorType>.Parse((string) error.Attribute("Type")),
                    Message = (string)error.Attribute("Message"),
                    Value = (string)error.Attribute("Value"),
                    Line = (int?)error.Attribute("Line")
                });
            }

            return resource;
        }

        public static CrawlingContext DeserializeContext(XElement resourcesNode)
        {
            CrawlingContext context = new CrawlingContext();
            context.Resources.AddRange(DeserializeResourceCollection(resourcesNode));

            return context;
        }

        public static List<Resource> DeserializeResourceCollection(XElement resourcesNode)
        {
            List<Resource> resources = new List<Resource>();

            foreach (XElement resourceNode in resourcesNode.Elements("Resource"))
            {
                Resource resource = DeserializeResource(resourceNode);
                resources.Add(resource);
            }

            Dictionary<string, Resource> resourcesByUrl = resources.ToDictionary(x => x.Url.AbsoluteUri);
            foreach (Resource resource in resources)
            {
                foreach (ResourceReference link in resource.References)
                {
                    Uri absoluteUri = link.Url.ToAbsolute(resource.Url).WithoutSession();
                    if (absoluteUri != null)
                        link.Target = resourcesByUrl.TryGetValue(absoluteUri.AbsoluteUri);
                }
            }

            return resources;
        }

        private static XAttribute OptionalAttribute(string name, object value)
        {
            return value == null ? null : new XAttribute(name, value);
        }

        private static DateTime TryParseDateTime(XAttribute attribute)
        {
            return attribute == null ? DateTime.MinValue : (DateTime) attribute;
        }
    }
}
