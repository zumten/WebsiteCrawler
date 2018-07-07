//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Text.RegularExpressions;
//using ZumtenSoft.WebsiteCrawler.BusLayer.Models;
//using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;

//namespace ZumtenSoft.WebsiteCrawler.BusLayer.Reporting
//{
//    public static class ResourceCsvMetasReportGenerator
//    {
//        /// <summary>
//        /// Génère le contenu d'un fichier CSV qui sera ouvert par excel.
//        /// </summary>
//        /// <param name="resources">Liste des resources à sérializer</param>
//        /// <returns>Contenu du fichier CSV</returns>
//        public static string GenerateReport(ICollection<Resource> resources)
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.AppendLine("URL", "Title", "Description", "Keywords");

//            foreach (Resource resource in resources.Where(x => x.ContentType == ResourceContentType.Html && x.Status == ResourceStatus.Processed && x.HttpStatus == HttpStatusCode.OK))
//                sb.AppendLine(resource.Url.PathAndQuery, Reformat(resource.Content.TryGetValue("title")), Reformat(resource.Content.TryGetValue("description")), Reformat(resource.Content.TryGetValue("keywords")));

//            return sb.ToString();
//        }
        
//        private static readonly Regex _regex = new Regex("[\r\n\t]+", RegexOptions.Multiline);
//        private static string Reformat(string text)
//        {
//            return text == null ? "" : _regex.Replace(text, " ");
//        }

//        private static void AppendLine(this StringBuilder sb, params string[] parts)
//        {
//            sb.AppendLine(String.Join("\t", parts));
//        }
//    }
//}
