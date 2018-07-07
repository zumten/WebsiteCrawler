using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction.Behaviors;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Networking;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;
using ZumtenSoft.WebsiteCrawler.Utils.Helpers;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction
{
    public static class UrlExtractors
    {
        /// <summary>
        /// Permet d'extraire tous les liens à partir d'un fichier HTML ou CSS
        /// </summary>
        /// <param name="resource">Ressource à laquelle les références devront être ajoutées.</param>
        /// <param name="response">Réponse de la requête qui doit être analyée</param>
        /// <returns>Liste de tous les liens trouvés</returns>
        public static void ExtractLinks(Resource resource, IHttpResponse response)
        {
            switch (resource.ContentType)
            {
                case ResourceContentType.Robots:
                    if (resource.Behavior >= ResourceBehavior.FollowAllReferences)
                    {
                        ExtractRobotsLinks(resource, response.ReadContent());
                    }
                    break;
                case ResourceContentType.SiteMap:
                    if (resource.Behavior >= ResourceBehavior.FollowHtmlReferences)
                    {
                        ExtractSitemapLinks(resource, response.ReadContent());
                    }
                    break;
                case ResourceContentType.Html:
                    if (resource.Behavior >= ResourceBehavior.FollowHtmlReferences)
                    {
                        ExtractHtmlLinks(resource, response.ReadContent());
                    }
                    break;
                case ResourceContentType.Css:
                    if (resource.Behavior >= ResourceBehavior.FollowAllReferences)
                    {
                        ExtractCssLinks(resource, response.ReadContent());
                    }
                    break;
            }
        }

        private static void ExtractRobotsLinks(Resource resource, string content)
        {
            using (StringReader reader = new StringReader(content))
            {
                int index = 1;
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine(), index++)
                {
                    if (line.StartsWith("Sitemap:", StringComparison.OrdinalIgnoreCase))
                    {
                        string url = line.Substring(8).Trim();
                        AddReference(resource, url, ResourceReferenceTypes.Anchor, index, ReferenceSubType.None);
                    }
                }
            }
        }

        /// <summary>
        /// Extait tous les liens d'un sitemap à partir des éléments /urlset/url/loc
        /// </summary>
        /// <param name="resource">Ressource à laquelle les références devront être ajoutées.</param>
        /// <param name="content">Contenu du fichier CSS</param>
        private static void ExtractSitemapLinks(Resource resource, string content)
        {
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XDocument document = XDocument.Parse(content);
            foreach (XElement page in document.Descendants(ns + "url"))
            {
                XElement locElement = page.Element(ns + "loc");
                string url = CleanUrl((string)locElement);
                Uri uri;
                if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
                {
                    resource.References.Add(ResourceReferenceTypes.Anchor, uri, ReferenceSubType.Sitemap);
                }
                else
                {
                    IXmlLineInfo lineInfo = locElement;
                    
                    resource.Errors.Add(new ResourceError
                    {
                        Type = ResourceErrorType.ParseURL,
                        Message = "Failed to parse URL",
                        Value = url,
                        Line = lineInfo == null ? (int?)null : lineInfo.LineNumber
                    });
                }
            }
        }

        /// <summary>
        /// Permet d'extraire tous les liens d'une page à partir des éléments
        /// LINK, SCRIPT, IMG, A.
        /// </summary>
        /// <param name="resource">Ressource à laquelle les références devront être ajoutées.</param>
        /// <param name="content">Contenu du fichier HTML</param>
        /// <returns>Liste de tous les liens trouvés</returns>
        public static void ExtractHtmlLinks(Resource resource, string content)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(content);

            foreach (HtmlParseError parseError in document.ParseErrors.Where(x => x.Code != HtmlParseErrorCode.EndTagNotRequired))
            {
                resource.Errors.Add(new ResourceError
                {
                    Type = ResourceErrorType.ParseHTML,
                    Message = parseError.Reason,
                    Value = content.ExtractSurroundingText(parseError.StreamPosition, 100, 100).RemoveLineBreaks(),
                    Line = parseError.Line
                });
            }
            
            

            FindAndAddResource(resource, document, "//link[@href]", "href", ResourceReferenceTypes.Link, ReferenceSubType.None);
            FindAndAddResource(resource, document, "//script[@src]", "src", ResourceReferenceTypes.Script, ReferenceSubType.None);
            FindAndAddResource(resource, document, "//img[@src]", "src", ResourceReferenceTypes.Image, ReferenceSubType.None, "data", "//:0");
            FindAndAddResource(resource, document, "//image[@src]", "src", ResourceReferenceTypes.Image, ReferenceSubType.None, "data", "//:0");
            FindAndAddResource(resource, document, "//a[@href]", "href", ResourceReferenceTypes.Anchor, ReferenceSubType.None, "javascript", "#", "mailto", "{{");
            FindAndAddResource(resource, document, "//frame[@src]", "src", ResourceReferenceTypes.Frame, ReferenceSubType.Frame, "javascript", "#", "mailto");
            FindAndAddResource(resource, document, "//iframe[@src]", "src", ResourceReferenceTypes.Frame, ReferenceSubType.IFrame, "javascript", "#", "mailto");

            AddContentFromMetas(resource, document);
            AddContentFromXPath(resource, document, "//title", ResourceContentKey.Title, null);
            //AddContentFromXPath(resource, document, "//h1", ResourceContentKey.Content, "H1");
            //AddContentFromXPath(resource, document, "//h2", ResourceContentKey.Content, "H2");
            AddContentFromPlainSearch(resource, content, "Object reference not set");

            HtmlNode viewStateNode = document.GetElementbyId("__VIEWSTATE");
            if (viewStateNode != null)
            {
                string value = viewStateNode.GetAttributeValue("value", "");
                resource.ViewStateSize = value.Length;
            }
        }

        private static void FindAndAddResource(Resource resource, HtmlDocument document, string xpath, string propertyName, ResourceReferenceTypes referenceType, ReferenceSubType defaultSubType, params string[] exclusions)
        {
            HtmlNodeCollection nodeCollection = document.DocumentNode.SelectNodes(xpath);
            if (nodeCollection != null)
            {
                foreach (HtmlNode node in nodeCollection)
                {
                    HtmlAttribute attribute = node.Attributes[propertyName];
                    if (attribute != null && !String.IsNullOrWhiteSpace(attribute.Value))
                    {
                        ReferenceSubType subType = defaultSubType;
                        if (referenceType == ResourceReferenceTypes.Image)
                        {
                            HtmlAttribute href = node.Attributes["xlink:href"];
                            if (href != null)
                            {
                                string hrefUrl = CleanUrl(href.Value);
                                if (exclusions.All(x => !hrefUrl.StartsWith(x)))
                                {
                                    AddReference(resource, hrefUrl, referenceType, node.Line, subType);
                                    subType = ReferenceSubType.Fallback;
                                }
                            }
                        }

                        string url = CleanUrl(node.Attributes[propertyName].Value);
                        if (exclusions.All(x => !url.StartsWith(x)))
                        {
                            HtmlAttribute rel = node.Attributes["rel"];
                            if (rel != null)
                                defaultSubType = EnumHelper<ReferenceSubType>.TryParse(rel.Value.Replace(" ", ""), ReferenceSubType.None, true);

                            HtmlAttribute type = node.Attributes["type"];
                            if (defaultSubType == ReferenceSubType.None && type != null)
                            {
                                switch (type.Value.ToLower())
                                {
                                    case "text/javascript": defaultSubType = ReferenceSubType.JavaScript; break;
                                    case "text/css": defaultSubType = ReferenceSubType.StyleSheet; break;
                                }
                            }

                            AddReference(resource, url, referenceType, node.Line, subType);
                        }
                    }
                }
            }
        }

        private static void AddContentFromMetas(Resource resource, HtmlDocument document)
        {
            HashSet<string> filter = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "keywords", "description" };

            HtmlNodeCollection metaCollection = document.DocumentNode.SelectNodes("//meta[@name]");
            if (metaCollection != null)
            {
                foreach (HtmlNode meta in metaCollection)
                {
                    HtmlAttribute attrName = meta.Attributes["name"];
                    HtmlAttribute attrContent = meta.Attributes["content"];
                    if (attrName != null && attrContent != null && filter.Contains(attrName.Value))
                    {
                        resource.Content.Add(new ResourceContent
                            {
                                Key = ResourceContentKey.Meta,
                                SubKey = attrName.Value,
                                Value = attrContent.Value
                            });
                    }
                }
            }
        }

        private static void AddContentFromXPath(Resource resource, HtmlDocument document, string xPath, ResourceContentKey key, string subKey)
        {
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes(xPath);
            if (nodes != null)
                foreach (HtmlNode node in nodes)
                    resource.Content.Add(new ResourceContent { Key = key, SubKey = subKey, Value = node.InnerText.Trim() });
        }

        private static void AddContentFromPlainSearch(Resource resource, string content, string textToSearch)
        {
            foreach (Match match in Regex.Matches(content, textToSearch, RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                resource.Content.Add(new ResourceContent
                {
                    Key = ResourceContentKey.Search,
                    SubKey = textToSearch,
                    Value = content.ExtractSurroundingText(match.Index, 100, 100).RemoveLineBreaks(),
                });
            }
        }

        private static void AddReference(Resource resource, string url, ResourceReferenceTypes referenceType, int line, ReferenceSubType subType)
        {


            Uri uri;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
            {
                resource.References.Add(referenceType, uri, subType);

                if (uri.IsAbsoluteUri
                    && (referenceType == ResourceReferenceTypes.Script || referenceType == ResourceReferenceTypes.Link || referenceType == ResourceReferenceTypes.Image)
                    && (resource.Url.Scheme == "https" && uri.Scheme == "http"))
                {
                    resource.Errors.Add(new ResourceError
                    {
                        Type = ResourceErrorType.ResolveURL,
                        Message = "Target URL is not secure",
                        Value = url,
                        Line = line,
                    });
                }
            }
            else
            {
                resource.Errors.Add(new ResourceError
                {
                    Type = ResourceErrorType.ParseURL,
                    Message = "Failed to parse URL",
                    Value = url,
                    Line = line,
                });
            }
        }

        private static readonly Regex FindUrl = new Regex(@"url\(('[^']*'|""[^""]*""|[^)]*)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Permet d'extraire tous les liens dans un fichier CSS à partir du pattern url(...)
        /// </summary>
        /// <param name="css">Contenu du fichier CSS</param>
        /// <returns>Liste de tous les liens trouvés</returns>
        public static void ExtractCssLinks(Resource resource, string css)
        {
            foreach (Match match in FindUrl.Matches(css))
            {
                string cleanPath = CleanUrl(match.Groups[1].Value.Trim('\'', '"'));
                bool isStylesheet = cleanPath.Contains(".css");

                if (!cleanPath.StartsWith("data"))
                {
                    AddReference(resource, cleanPath,
                    isStylesheet ? ResourceReferenceTypes.Link : ResourceReferenceTypes.Image, 0,
                    isStylesheet ? ReferenceSubType.StyleSheet : ReferenceSubType.None);
                }
            }
        }

        

        private static string CleanUrl(string input)
        {
            // &#xD; et &#xA; sont des sauts de ligne provenant d'une génération XSLT.
            // Cela corrompt l'URL pour les objets de type Uri mais les browsers les ignorent.
            //return input.Trim().Replace("&amp;", "&").Replace("&#xD;", "").Replace("&#xA;", "");
            return input.Replace("&amp;", "&");
        }
    }
}
