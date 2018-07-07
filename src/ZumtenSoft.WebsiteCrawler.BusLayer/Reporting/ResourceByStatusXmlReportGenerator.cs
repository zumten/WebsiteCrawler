using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.WebsiteCrawler.Utils.Helpers;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Reporting
{
    public class ResourceByStatusXmlReportGenerator
    {
        /// <summary>
        /// Génère un xml avec la liste des ressources.
        /// </summary>
        /// <param name="resources">Liste des resources à sérializer</param>
        /// <example>
        ///   <ResourcesByStatus>
        ///     <Status Code="..." Name="...">
        ///       <Resource Url="..." FinalStatus="..." TimeLoading="..." />
        ///     </Status>
        ///   </ResourcesByStatus>
        /// </example>
        public static XDocument GenerateReport(ICollection<Resource> resources)
        {
            // Récupère toutes les ressources regroupées par code de status.
            var resourcesByStatusCode = resources
                .OrderBy(x => x.Url.AbsoluteUri)
                .GroupBy(x => x.HttpStatus)
                .OrderBy(x => x.Key);

            XElement root = new XElement("ResourcesByStatus",
                from status in resourcesByStatusCode
                select new XElement("Status",
                    new XAttribute("Code", (int)status.Key),
                    new XAttribute("Name", GenerateHttpStatusName(status.Key)),

                    from resource in status
                    select new XElement("Resource",
                        new XAttribute("Url", resource.Url.AbsoluteUri),
                        new XAttribute("FinalStatus", GenerateStatusName(FollowRedirection(resource))),
                        new XAttribute("TimeLoading", resource.TimeLoading == TimeSpan.Zero ? "" : resource.TimeLoading.ToString()))));

                        //from reference in resource.References
                        //select new XElement("Reference",
                        //    new XAttribute("Type", reference.Type),
                        //    new XAttribute("Url", reference.Url),
                        //    new XAttribute("Status", GenerateStatusName(reference.Resource)),

                        //from resourceReferencing in FindReferencedBy(resources, resource)
                        //select new XElement("ReferencedBy",
                        //    new XAttribute("Url", resourceReferencing.Url.AbsoluteUri))
                        

            return new XDocument(root);
        }

        private static Resource FollowRedirection(Resource resource)
        {
            ResourceReference redirection = resource.References.FirstOrDefault(x => x.Type == ResourceReferenceTypes.Redirection);
            while(redirection != null)
            {
                resource = redirection.Target;
                redirection = resource.References.FirstOrDefault(x => x.Type == ResourceReferenceTypes.Redirection);
            }
            return resource;
        }

        private static string GenerateHttpStatusName(HttpStatusCode status)
        {
            switch(status)
            {
                case 0: return "Unknown";
                case HttpStatusCode.Found: return "Redirect";
            }

            return status.ToString();
        }

        private static string GenerateStatusName(Resource resource)
        {
            if (resource.Status == ResourceStatus.Processed)
            {
                return GenerateHttpStatusName(resource.HttpStatus);
            }
            return resource.Status.ToString();
        }
    }
}
