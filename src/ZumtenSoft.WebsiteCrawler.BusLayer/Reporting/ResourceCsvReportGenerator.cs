using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Reporting
{
    public static class ResourceCsvReportGenerator
    {
        /// <summary>
        /// Génère le contenu d'un fichier CSV qui sera ouvert par excel.
        /// </summary>
        /// <param name="resources">Liste des resources à sérializer</param>
        /// <returns>Contenu du fichier CSV</returns>
        public static string GenerateReport(ICollection<Resource> resources)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ContentType", "Status", "Status after redirection", "URL",
                "ReferencedByType", "Loading time (s)", "Size", "Redirected URL", "ReferencedByUrl");

            foreach (Resource resource in resources)
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

                sb.AppendLine(
                    resource.ContentType.ToString(),
                    GenerateStatusName(resource),
                    GenerateStatusName(redirectedResource),
                    resource.Url.AbsoluteUri,
                    String.Join(",", referencedByContent),
                    resource.TimeLoading.TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    resource.Size.ToString(CultureInfo.InvariantCulture),
                    redirectedUrl,
                    referencedByUrl);
            }

            return sb.ToString();
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

        private static void AppendLine(this StringBuilder sb, params string[] parts)
        {
            sb.AppendLine(String.Join(",", parts.Select(x => "\"" + (x ?? "").Replace("\"", "\"\"").Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r") + "\"")));
        }
    }
}
