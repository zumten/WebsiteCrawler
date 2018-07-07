using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;
using ZumtenSoft.WebsiteCrawler.Utils.Helpers;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Reporting
{
    public static class ReportGenerator
    {
        class Column
        {
            public ReportConfigColumn Config { get; set; }
            public ExcelFormatter Formatter { get; set; }
            public Func<ReportNodeInfos, ReportArray> Extactor { get; set; }
        }

        public static DataTable GenerateReport(ReportConfig config, ICollection<Resource> resources)
        {
            List<Column> columns = new List<Column>();
            foreach (ReportConfigColumn configColumn in config.Columns)
            {
                List<ReportFieldNode> nodes = ReportFieldNodes.ExtractNodePath(configColumn.Path);
                if (nodes != null && nodes.Count > 0)
                {
                    Func<ReportArray, ReportArray>[] functions = nodes.Select(x => x.Extractor).ToArray();
                    Func<ReportNodeInfos, ReportArray> extractor = x => functions.Aggregate(new ReportArray(new object[] { x }), (current, function) => function(current));

                    columns.Add(new Column
                    {
                        Config = configColumn,
                        Extactor = extractor,
                        Formatter = ExcelFormatter.GetFormat(nodes.Last().Type)
                    });
                }
                else
                {
                    throw new Exception("Path " + configColumn.Path + " not found");
                }
            }

            DataTable table = new DataTable(config.Name);
            foreach (Column column in columns)
                table.AddColumn(column.Config.Name, column.Formatter.TargetType, column.Config.Width, column.Formatter.StringFormat);

            switch (config.Type)
            {
                case ReportType.ListResources:
                    foreach (Resource resource in resources)
                    {
                        ReportNodeInfos infos = new ReportNodeInfos
                        {
                            Resource = resource
                        };

                        AddRow(table, columns, infos);
                    }
                    break;
                case ReportType.ListReferences:
                    foreach (Resource resource in resources)
                    {
                        foreach (ResourceReference reference in resource.References)
                        {
                            ReportNodeInfos infos = new ReportNodeInfos
                            {
                                Resource = resource,
                                Reference = reference
                            };

                            AddRow(table, columns, infos);
                        }
                    }
                    break;
                case ReportType.ListContent:
                    foreach (Resource resource in resources)
                    {
                        foreach (ResourceContent content in resource.Content)
                        {
                            ReportNodeInfos infos = new ReportNodeInfos
                            {
                                Resource = resource,
                                Content = content
                            };

                            AddRow(table, columns, infos);
                        }
                    }
                    break;
                case ReportType.ListErrors:
                    foreach (Resource resource in resources)
                    {
                        foreach (ResourceError error in resource.Errors)
                        {
                            ReportNodeInfos infos = new ReportNodeInfos
                            {
                                Resource = resource,
                                Error = error
                            };

                            AddRow(table, columns, infos);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return table;
        }

        private static void AddRow(DataTable table, List<Column> columns, ReportNodeInfos infos)
        {
            object[] rowValues = new object[columns.Count];
            for (int i = 0; i < columns.Count; i++)
            {
                Column column = columns[i];
                rowValues[i] = column.Formatter.GetValue(column.Extactor(infos));
            }

            table.Rows.Add(rowValues);
        }

        public static DataTable GenerateRequestsReport(ICollection<Resource> resources)
        {
            DataTable table = new DataTable("Requests");
            table.AddColumn("Starting time", typeof(DateTime), 20, "yyyy-mm-dd hh:mm:ss");
            table.AddColumn("Content type", typeof(string), 11);
            table.AddColumn("Status code", typeof (string), 11);
            table.AddColumn("URL", typeof(string), 85);
            table.AddColumn("Loading time (s)", typeof(double), 11).AddProperty("Format", "0.00");
            table.AddColumn("Mime type", typeof(string), 11);
            table.AddColumn("Compression", typeof(string), 11);
            table.AddColumn("Size (bytes)", typeof(int), 11, "0");
            table.AddColumn("CompressedSize (bytes)", typeof(int), 11, "0");
            table.AddColumn("ViewStateSize (bytes)", typeof(int), 12);


            foreach (Resource resource in resources)
            {
                table.Rows.Add(
                    resource.TimeStart,
                    resource.ContentType.ToString(),
                    GenerateStatusName(resource),
                    resource.Url.AbsoluteUri,
                    resource.TimeLoading.TotalSeconds,
                    resource.Headers.TryGetValue("content-type"),
                    resource.Headers.TryGetValue("content-encoding"),
                    resource.Size,
                    resource.CompressedSize,
                    resource.ViewStateSize ?? 0);
            }

            return table;
        }

        public static DataTable GenerateCachingReport(ICollection<Resource> resources)
        {
            DataTable table = new DataTable("Caching");
            table.AddColumn("Content type", typeof(string), 11);
            table.AddColumn("URL", typeof(string), 85);
            table.AddColumn("Cache control", typeof(string), 13);
            table.AddColumn("Has ETAG", typeof(string), 10);
            table.AddColumn("Last modified", typeof(DateTime), 20, "yyyy-mm-dd hh:mm:ss");
            table.AddColumn("Expires", typeof(DateTime), 20, "yyyy-mm-dd hh:mm:ss");

            foreach (Resource resource in resources.Where(x => x.HttpStatus == HttpStatusCode.OK))
            {
                table.Rows.Add(
                    resource.ContentType.ToString(),
                    resource.Url.AbsoluteUri,
                    resource.Headers.TryGetValue("cache-control"),
                    String.IsNullOrWhiteSpace(resource.Headers.TryGetValue("ETag")) ? "No" : "Yes",
                    TryParseDate(resource.Headers.TryGetValue("last-modified")),
                    TryParseDate(resource.Headers.TryGetValue("expires")));
            }

            return table;
        }

        public static DataTable GenerateReferenceSummaryReport(ICollection<Resource> resources)
        {
            DataTable table = new DataTable("ReferenceSummary");
            table.AddColumn("Content type", typeof(string), 11);
            table.AddColumn("Status code", typeof(string), 11);
            table.AddColumn("URL", typeof(string), 85);
            table.AddColumn("# images", typeof (int), 12);
            table.AddColumn("# scripts", typeof (int), 12);
            table.AddColumn("# css", typeof (int), 12);
            table.AddColumn("# hyperlinks", typeof (int), 12);
            table.AddColumn("# source references", typeof (int), 12);

            foreach (Resource resource in resources.Where(x => x.Status == ResourceStatus.Processed))
            {
                table.Rows.Add(
                    resource.ContentType.ToString(),
                    GenerateStatusName(resource),
                    resource.Url.AbsoluteUri,
                    resource.References.Count(x => x.Type == ResourceReferenceTypes.Image),
                    resource.References.Count(x => x.Type == ResourceReferenceTypes.Script),
                    resource.References.Count(x => x.Type == ResourceReferenceTypes.Link && x.SubType == ReferenceSubType.StyleSheet),
                    resource.References.Count(x => x.Type == ResourceReferenceTypes.Anchor),
                    resource.ReferencedBy.Count);
            }

            return table;
        }

        public static DataTable GenerateReferencesReport(ICollection<Resource> resources)
        {
            DataTable table = new DataTable("References");
            table.AddColumn("Source content type", typeof(string), 11);
            table.AddColumn("Source URL", typeof(string), 70);
            table.AddColumn("Reference type", typeof (string), 11);
            table.AddColumn("Sub type", typeof (string), 11);
            table.AddColumn("Raw value", typeof (string), 50);
            table.AddColumn("Target URL", typeof (string), 50);
            table.AddColumn("Target status", typeof (string), 15);
            table.AddColumn("Final status", typeof (string), 15);
            table.AddColumn("Is recursive", typeof (string), 15);
            table.AddColumn("Count", typeof (int), 10);

            foreach (Resource resource in resources)
            {
                foreach (ResourceReference reference in resource.References)
                {
                    table.Rows.Add(
                        resource.ContentType.ToString(),
                        resource.Url.AbsoluteUri,
                        reference.Type.ToString(),
                        reference.SubType.ToString(),
                        reference.Url.OriginalString,
                        reference.Target != null ? reference.Target.Url.AbsoluteUri : null,
                        reference.Target != null ? GenerateStatusName(reference.Target) : null,
                        reference.Target != null ? GenerateStatusName(FollowRedirection(reference.Target)) : null,
                        reference.Target == resource ? "Yes" : "No",
                        reference.Count);
                }
            }

            return table;
        }

        public static DataTable GenerateRedirectionReport(ICollection<Resource> resources)
        {
            DataTable table = new DataTable("Redirection");
            table.AddColumn("Status code", typeof(string), 11);
            table.AddColumn("Source URL", typeof(string), 85);
            table.AddColumn("Target URL", typeof(string), 50);
            table.AddColumn("Final status code", typeof(string), 11);
            table.AddColumn("Referenced by types", typeof(string), 14);
            table.AddColumn("Referenced by URLs", typeof(string), 85);

            foreach (Resource resource in resources.Where(x => x.Status == ResourceStatus.Processed && x.References.Any(y => y.Type == ResourceReferenceTypes.Redirection)))
            {
                Resource redirectedResource = FollowRedirection(resource);

                // Récupère la liste de tous les types de contenus qui pointent sur cette resource
                var referencedByContent = resource.ReferencedBy
                    .Select(x => x.Source.ContentType) // On récupère le type de contenu de chaque référence
                    .Distinct()                        // Élimination des doublons
                    .Where(x => x != ResourceContentType.Unknown) // On ne veut pas avoir le type Unknown
                    .OrderBy(x => x);                  // Mettre en ordre (de code)

                string redirectedUrl = (redirectedResource == resource) ? "" : redirectedResource.Url.AbsoluteUri;

                var referencedByUrl = String.Join(", ", resource.ReferencedBy.Take(9).Select(x => x.Source.Url.AbsoluteUri)) + (resource.ReferencedBy.Count > 9 ? ", ..." : "");

                table.Rows.Add(
                    GenerateStatusName(resource),
                    resource.Url.AbsoluteUri,
                    redirectedUrl,
                    GenerateStatusName(redirectedResource),
                    String.Join(",", referencedByContent),
                    referencedByUrl);
            }

            return table;
        }

        public static DataTable GenerateContentReport(ICollection<Resource> resources)
        {
            DataTable table = new DataTable("Content");
            table.AddColumn("URL", typeof(string), 80);
            table.AddColumn("Key", typeof(string), 15);
            table.AddColumn("SubKey", typeof(string), 15);
            table.AddColumn("Value", typeof(string), 120);

            foreach (Resource resource in resources.Where(x => x.HttpStatus == HttpStatusCode.OK))
            {
                foreach (ResourceContent content in resource.Content)
                {
                    table.Rows.Add(
                        resource.Url.AbsoluteUri,
                        content.Key.ToString(),
                        content.SubKey ?? "",
                        content.Value);
                }
            }

            return table;
        }

        public static DataTable GenerateErrorsReport(ICollection<Resource> resources)
        {
            DataTable table = new DataTable("Errors");
            table.AddColumn("URL", typeof(string), 80);
            table.AddColumn("Type", typeof(string), 15);
            table.AddColumn("Line number", typeof(int), 15);
            table.AddColumn("Message", typeof(string), 80);
            table.AddColumn("Value", typeof(string), 80);

            foreach (Resource resource in resources)
            {
                foreach (ResourceError error in resource.Errors)
                {
                    table.Rows.Add(
                        resource.Url.AbsoluteUri,
                        DescriptionExtractor.GetDescription(error.Type),
                        error.Line,
                        error.Message,
                        error.Value);
                }
            }

            return table;
        }

        private static DataColumn AddColumn(this DataTable table, string columnName, Type type, double width, string format = null)
        {
            DataColumn column = table.Columns.Add(columnName, type);
            column.ExtendedProperties.Add("Width", width);

            if (format != null)
                column.ExtendedProperties.Add("Format", format);
            return column;
        }

        private static DataColumn AddProperty(this DataColumn column, string key, object value)
        {
            column.ExtendedProperties.Add(key, value);
            return column;
        }

        private static Resource FollowRedirection(Resource resource)
        {
            int i = 10;
            ResourceReference redirection = resource.References.FirstOrDefault(x => x.Type == ResourceReferenceTypes.Redirection);
            while (redirection != null && i > 0)
            {
                resource = redirection.Target;
                redirection = resource.References.FirstOrDefault(x => x.Type == ResourceReferenceTypes.Redirection);
                i--;
            }
            return resource;
        }

        private static string GenerateStatusName(Resource resource)
        {
            // Si la ressource a déjà été processé, on préfère utiliser le HttpStatus.
            if (resource.Status == ResourceStatus.Processed)
            {
                return GenerateHttpStatusName(resource.HttpStatus);
            }

            // Dans le cas contraire, on veut savoir pourquoi la ressource n'a jamais été traitée.
            return "0 - " + resource.Status.ToString();
        }

        private static string GenerateHttpStatusName(HttpStatusCode status)
        {
            switch (status)
            {
                // Si le HttpStatus n'a jamais été assigné et n'a encore aucun nom.
                case 0: return "0 - Unknown";

                // Found et Redirect ont la même valeur, mais Found est utilisé par défaut.
                // Le nom n'est pas très bon alors on préfère utiliser Redirect.
                case HttpStatusCode.Found: return "302 - Redirect";
            }

            return (int)status + " - " + status.ToString();
        }

        private static DateTime? TryParseDate(string text)
        {
            DateTime date;
            if (DateTime.TryParse(text, out date))
                return date;
            return null;
        }

        private static int? TryParseInt(string text)
        {
            int number;
            if (Int32.TryParse(text, out number))
                return number;
            return null;
        }
    }
}
