using System.Collections.Generic;
using System.Linq;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Processing;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction.Behaviors
{
    public class BehaviorRuleCollection : List<IBehaviorRule>
    {
        /// <summary>
        /// Récupère le Behavior pour une ressource en fonction des règles qui ont été définies.
        /// </summary>
        /// <param name="resource">Resource à analyser</param>
        /// <returns>Behavior à associer à cette ressource</returns>
        public IBehaviorRule GetBehaviorRule(Resource resource)
        {
            return this.FirstOrDefault(x => x.Validate(resource));
        }

        /// <summary>
        /// Ajouter un filtre par URL
        /// </summary>
        /// <param name="url">Filtre d'URL (avec des * pour wildcard)</param>
        /// <param name="behavior">Comportement à associer à la nouvelle règle</param>
        /// <returns></returns>
        public UrlFilterBehaviorRule AddUrlFilterRule(string name, ResourceBehavior behavior, WorkBucket<Resource, BucketContext> targetBucket, ICollection<CrawlingCondition> conditions)
        {
            UrlFilterBehaviorRule urlFilterBehaviorRule = new UrlFilterBehaviorRule(name, behavior, targetBucket, conditions);
            Add(urlFilterBehaviorRule);
            return urlFilterBehaviorRule;
        }
    }
}