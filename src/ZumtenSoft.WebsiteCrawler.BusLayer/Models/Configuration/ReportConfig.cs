using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;
using ZumtenSoft.WebsiteCrawler.Utils.Events;
using ZumtenSoft.WebsiteCrawler.Utils.Helpers;
using ZumtenSoft.Linq2ObsCollection.Collections;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration
{
    public enum ReportType
    {
        [Description("List resources")]
        ListResources,
        [Description("List references")]
        ListReferences,
        [Description("List resource content")]
        ListContent,
        [Description("List errors")]
        ListErrors,
    }

    [DebuggerDisplay(@"\{ReportConfig Name={Name}, Columns={Columns.Count}\}")]
    public class ReportConfig : NotifyObject
    {
        public ReportConfig()
        {
            Columns = new ObservableCollection<ReportConfigColumn>();
        }

        public Guid Guid { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { if (_name != value) { _name = value; Notify("Name"); } }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { if (_description != value) { _description = value; Notify("Description"); } }
        }

        private ReportType _type;
        public ReportType Type
        {
            get { return _type; }
            set { if (_type != value) { _type = value; Notify("Type"); } }
        }

        public ObservableCollection<ReportConfigColumn> Columns { get; private set; }
    }

    [DebuggerDisplay(@"\{ReportConfigColumn Name={Name}, Path={Path}, Width={Width}\}")]
    public class ReportConfigColumn : NotifyObject
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { if (_name != value) { _name = value; Notify("Name"); } }
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            set { if (_path != value) { _path = value; Notify("Path"); } }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set { if (_width != value) { _width = value; Notify("Width"); } }
        }
    }

    public class ReportArray
    {
        public ReportArray(object[] values)
        {
            Values = values;
        }

        public object[] Values { get; private set; }
    }

    [DebuggerDisplay(@"\{Resource={Resource.Url.AbsoluteUri}, Reference={Reference!=null}, Content={Content!=null}, Error={Error!=null}\}")]
    public class ReportNodeInfos
    {
        public Resource Resource { get; set; }
        public ResourceReference Reference { get; set; }
        public ResourceContent Content { get; set; }
        public ResourceError Error { get; set; }
    }

    [DebuggerDisplay(@"\{ReportFieldNode Name={Name}, Type={Type.Name}, Nodes={Nodes.Count}\}")]
    public class ReportFieldNode
    {
        public ReportFieldNode(Type type, string name, Func<ReportArray, ReportArray> extractor, List<ReportFieldNode> nodes)
        {
            Type = type;
            Name = name;
            Extractor = extractor;
            Nodes = nodes;
        }

        public Type Type { get; private set; }
        public string Name { get; private set; }
        public Func<ReportArray, ReportArray> Extractor { get; private set; }
        public List<ReportFieldNode> Nodes { get; private set; }

        public void AddNodes(params ReportFieldNode[] children)
        {
            Nodes.AddRange(children);
        }
    }

    public static class ReportFieldNodes
    {
        public static List<ReportFieldNode> ExtractNodePath(string path)
        {
            string[] parts = path.Split('.');
            List<ReportFieldNode> nodes = new List<ReportFieldNode>();
            ReportFieldNode currentNode = Root;
            foreach (string part in parts)
            {
                string part1 = part;
                currentNode = currentNode.Nodes.FirstOrDefault(x => String.Equals(x.Name, part1, StringComparison.InvariantCultureIgnoreCase));

                if (currentNode == null)
                    return null;

                nodes.Add(currentNode);
            }

            return nodes;
        }

        public static ReportFieldNode Root { get; private set; }
        public static ReportFieldNode NodeResource { get; private set; }
        public static ReportFieldNode NodeReference { get; private set; }
        public static ReportFieldNode NodeContent { get; private set; }
        public static ReportFieldNode NodeError { get; private set; }

        static ReportFieldNodes()
        {
            var nodeUrl = Create("Uri", (Uri uri) => uri,
                Create("AbsolutePath", (Uri uri) => uri.AbsolutePath),
                Create("AbsoluteUri", (Uri uri) => uri.AbsoluteUri),
                Create("Authority", (Uri uri) => uri.Authority),
                Create("Fragment", (Uri uri) => uri.Fragment),
                Create("Host", (Uri uri) => uri.Host),
                Create("PathAndQuery", (Uri uri) => uri.PathAndQuery),
                Create("Query", (Uri uri) => uri.Query),
                Create("Scheme", (Uri uri) => uri.Scheme));

            var nodeReference =
                Create("References", (ResourceReferenceCollection r) => r,
                    Create("First", (IEnumerable<ResourceReference> lst) => lst.FirstOrDefault()),
                    Create("Count", (IEnumerable<ResourceReference> lst) => lst.Count()));

            NodeResource = Create("Resource", (ReportNodeInfos i) => i.Resource);
            NodeReference = Create("Reference", (ReportNodeInfos i) => i.Reference);
            NodeContent = Create("Content", (ReportNodeInfos i) => i.Content);
            NodeError = Create("Error", (ReportNodeInfos i) => i.Error);

            var referenceCollection = Create("ReferenceCollection", (IEnumerable<ResourceReference> coll) => coll,
                Create("Count", (IEnumerable<ResourceReference> coll) => coll.Count()),
                Create("First", (IEnumerable<ResourceReference> coll) => coll.FirstOrDefault()),
                Create("First5", (IEnumerable<ResourceReference> coll) => new ReportArray(coll.Take(5).ToArray<object>())),
                Create("First10", (IEnumerable<ResourceReference> coll) => new ReportArray(coll.Take(10).ToArray<object>())),
                CreateAlias("Items", (IEnumerable<ResourceReference> coll) => new ReportArray(coll.ToArray<object>()), NodeReference));

            NodeResource.AddNodes(
                Create("ProcessingStatus", (Resource res) => res.Status),
                Create("Status", (Resource res) => DescriptionExtractor.GetDescription(res.Status == ResourceStatus.Processed ? (object)res.HttpStatus : res.Status)),
                Create("Content", (Resource res) => res.Content),
                Create("Errors", (Resource res) => res.Errors),
                Create("Redirections", (Resource res) => res,
                    CreateAlias("First", (Resource res) => res.References.FirstOrDefault(x => x.Type == ResourceReferenceTypes.Redirection), NodeReference),
                    Create<Resource, int>("Count", CountRedirections),
                    CreateAlias<Resource, Resource>("Final", FollowRedirections, NodeResource)),
                Create("References", (Resource res) => res.References,
                    CreateAlias("All", (ResourceReferenceCollection res) => res, referenceCollection),
                    CreateAlias("Anchors", (ResourceReferenceCollection res) => res.Where(x => x.Type == ResourceReferenceTypes.Anchor), referenceCollection),
                    CreateAlias("Images", (ResourceReferenceCollection res) => res.Where(x => x.Type == ResourceReferenceTypes.Image), referenceCollection),
                    Create("Links", (ResourceReferenceCollection res) => res,
                        CreateAlias("Canonicals", (ResourceReferenceCollection res) => res.Where(x => x.SubType == ReferenceSubType.Canonical), referenceCollection),
                        CreateAlias("ShortcutIcons", (ResourceReferenceCollection res) => res.Where(x => x.SubType == ReferenceSubType.ShortcutIcon), referenceCollection),
                        CreateAlias("StyleSheets", (ResourceReferenceCollection res) => res.Where(x => x.SubType == ReferenceSubType.StyleSheet), referenceCollection)),
                    CreateAlias("Scripts", (ResourceReferenceCollection res) => res.Where(x => x.Type == ResourceReferenceTypes.Script), referenceCollection)),
                CreateAlias("Referers", (Resource res) => res.ReferencedBy, referenceCollection),
                Create("Request", (Resource res) => res,
                    Create("Behavior", (Resource res) => res.Behavior),
                    Create("TimeStart", (Resource res) => res.TimeStart),
                    CreateAlias("Url", (Resource res) => res.Url, nodeUrl)),
                Create("Response", (Resource res) => res,
                    Create("CompressedSize", (Resource res) => res.CompressedSize),
                    Create("ContentType", (Resource res) => res.ContentType),
                    Create("Headers", (Resource res) => res.Headers,
                        Create("ContentEncoding", (ResourceHeaders h) => h.TryGetValue("content-encoding")),
                        Create("ContentType", (ResourceHeaders h) => h.TryGetValue("content-type")),
                        Create("CacheControl", (ResourceHeaders h) => h.TryGetValue("cache-control")),
                        Create("HasETAG", (ResourceHeaders h) => !String.IsNullOrWhiteSpace(h.TryGetValue("ETag"))),
                        Create("LastModified", (ResourceHeaders h) => TryParseDate(h.TryGetValue("last-modified"))),
                        Create("Expires", (ResourceHeaders h) => TryParseDate(h.TryGetValue("expires")))),
                    Create("HttpStatus", (Resource res) => res.HttpStatus),
                    Create("Size", (Resource res) => res.Size),
                    Create("CompressedSize", (Resource res) => res.CompressedSize),
                    Create("TimeLoading", (Resource res) => res.TimeLoading),
                    Create("TimeProcessing", (Resource res) => res.TimeProcessing),
                    Create("ViewStateSize", (Resource res) => res.ViewStateSize ?? 0)));
                    
            NodeReference.AddNodes(
                CreateAlias("Source", (ResourceReference r) => r.Source, NodeResource),
                CreateAlias("Target", (ResourceReference r) => r.Target, NodeResource),
                Create("Type", (ResourceReference r) => r.Type),
                Create("SubType", (ResourceReference r) => r.SubType),
                Create("RawUrl", (ResourceReference r) => r.Url.OriginalString),
                Create("Count", (ResourceReference r) => r.Count),
                Create("IsRecursive", (ResourceReference r) => r.Source == r.Target || r.Source == FollowRedirections(r.Target)));

            NodeContent.AddNodes(
                Create("Key", (ResourceContent c) => c.Key),
                Create("Value", (ResourceContent c) => c.Value));

            NodeError.AddNodes(
                Create("Type", (ResourceError e) => e.Type),
                Create("Message", (ResourceError e) => e.Message),
                Create("Value", (ResourceError e) => e.Value),
                Create("Line", (ResourceError e) => e.Line));

            Root = Create("Root", (ReportNodeInfos i) => i, NodeResource, NodeReference, NodeContent, NodeError);
        }

        private static Resource FollowRedirections(Resource resource)
        {
            for (int i = 0; i < 10; i++)
            {
                ResourceReference reference = resource.References.FirstOrDefault(x => x.Type == ResourceReferenceTypes.Redirection);
                if (reference == null || reference.Target == null)
                    return resource;

                resource = reference.Target;
            }

            return resource;
        }

        private static int CountRedirections(Resource resource)
        {
            for (int i = 0; i < 10; i++)
            {
                ResourceReference reference = resource.References.FirstOrDefault(x => x.Type == ResourceReferenceTypes.Redirection);
                if (reference == null || reference.Target == null)
                    return i;

                resource = reference.Target;
            }

            return 10;
        }

        private static ReportFieldNode Create<TSource, T>(string name, Func<TSource, T> extractor, params ReportFieldNode[] children)
        {
            return new ReportFieldNode(typeof (T), name, x =>
            {
                if (x == null || x.Values.Length == 0)
                    return null;

                List<object> results = new List<object>();
                foreach (object result in x.Values.OfType<TSource>().Select(y => (object)extractor(y)))
                {
                    var array = result as ReportArray;
                    if (array != null)
                        results.Add(array.Values);
                    else
                        results.Add(result);
                }

                return new ReportArray(results.ToArray());

            }, children.ToList());
        }

        private static ReportFieldNode CreateAlias<TSource, T>(string name, Func<TSource, T> extractor, ReportFieldNode target)
        {
            return new ReportFieldNode(typeof(T), name, x =>
            {
                if (x == null || x.Values.Length == 0)
                    return null;

                List<object> results = new List<object>();
                foreach (object result in x.Values.OfType<TSource>().Select(y => (object)extractor(y)))
                {
                    var array = result as ReportArray;
                    if (array != null)
                        results.AddRange(array.Values);
                    else
                        results.Add(result);
                }

                return new ReportArray(results.ToArray());

            }, target.Nodes);
        }

        private static DateTime? TryParseDate(string text)
        {
            DateTime date;
            if (DateTime.TryParse(text, out date))
                return date;
            return null;
        }
    }

    public class ExcelFormatter
    {
        public Type SourceType { get; private set; }
        public Type TargetType { get; private set; }
        public object DefaultValue { get; private set; }
        public Func<ReportArray, object> GetValue { get; private set; }
        public string StringFormat { get; private set; }

        private static Dictionary<Type, ExcelFormatter> _formatsByType = new Dictionary<Type, ExcelFormatter>();
        public static ExcelFormatter Add<TSource, TTarget>(Func<TSource[], TTarget> getValue, TTarget defaultValue, string stringFormat)
        {
            ExcelFormatter format = new ExcelFormatter
            {
                SourceType = typeof(TSource),
                TargetType = typeof(TTarget),
                DefaultValue = defaultValue,
                StringFormat = stringFormat,
                GetValue = x => x == null ? defaultValue : getValue(x.Values.OfType<TSource>().ToArray())
            };
            _formatsByType.Add(format.SourceType, format);
            return format;
        }

        static ExcelFormatter()
        {
            Add((string[] x) => String.Join(", ", x), "", null);
            Add((DateTime[] x) => x.FirstOrDefault(), DateTime.MinValue, "yyyy-mm-dd hh:mm:ss");
            Add((DateTime?[] x) => x.FirstOrDefault(y => y.HasValue) ?? DateTime.MinValue, DateTime.MinValue, "yyyy-mm-dd hh:mm:ss");
            Add((int[] x) => x.Sum(), 0, "0");
            Add((bool[] x) => x.All(y => y) ? "Yes" : "No", "No", null);
            Add((float[] x) => x.Sum(), 0f, "0.00");
            Add((double[] x) => x.Sum(), 0d, "0.00");
            Add((TimeSpan[] x) => x.Sum(y => y.TotalSeconds), 0d, "0.00");
        }

        public static ExcelFormatter GetFormat(Type type)
        {
            ExcelFormatter formatter;
            if (!_formatsByType.TryGetValue(type, out formatter))
            {
                if (type.IsEnum)
                {
                    formatter = new ExcelFormatter
                    {
                        SourceType = type,
                        TargetType = typeof (string),
                        DefaultValue = "",
                        GetValue = x => String.Join(", ", x.Values.Select(DescriptionExtractor.GetDescription)),
                        StringFormat = null
                    };
                }
                else
                {
                    formatter = new ExcelFormatter
                    {
                        SourceType = type,
                        TargetType = typeof(string),
                        DefaultValue = "",
                        GetValue = x => "#FORMATERROR#",
                        StringFormat = null
                    };
                }

                _formatsByType.Add(type, formatter);
            }

            return formatter;
        }


    }
}
