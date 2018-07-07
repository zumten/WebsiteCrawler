using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Processing;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction.Behaviors
{
    public interface IBehaviorRule
    {
        string Name { get; }
        int NbThreads { get; }
        ResourceBehavior Behavior { get; }
        WorkBucket<Resource, BucketContext> TargetBucket { get; } 
        bool Validate(Resource resource);
    }
}