using System.Collections.Generic;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Processing;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction.Behaviors
{
    /// <summary>
    /// Règle qui filtre en fonction de l'URL d'une resource.
    /// </summary>
    public class UrlFilterBehaviorRule : IBehaviorRule
    {
        private readonly ICollection<CrawlingCondition> _conditions;
        public string Name { get; private set; }
        public int NbThreads { get; private set; }
        public ResourceBehavior Behavior { get; private set; }
        public WorkBucket<Resource, BucketContext> TargetBucket { get; private set; }

        public UrlFilterBehaviorRule(string name, ResourceBehavior behavior, WorkBucket<Resource, BucketContext> targetBucket, ICollection<CrawlingCondition> conditions)
        {
            _conditions = conditions;
            Name = name;
            Behavior = behavior;
            TargetBucket = targetBucket;
            
        }

        public bool Validate(Resource resource)
        {
            foreach (CrawlingCondition condition in _conditions)
            {
                string resourceValue = condition.FieldType.ExtractValue(resource.Url, null);
                if (!condition.ComparisonType.Condition(resourceValue, condition.Value))
                    return false;
            }
            return true;
        }
    }
}
