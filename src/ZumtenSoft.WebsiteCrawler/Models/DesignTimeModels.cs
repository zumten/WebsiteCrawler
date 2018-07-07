using System;
using System.Net;
using System.Windows.Threading;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction.Behaviors;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.Linq2ObsCollection.Collections;
using ZumtenSoft.Linq2ObsCollection.Threading;

namespace ZumtenSoft.WebsiteCrawler.Models
{
    public static class DesignTimeModels
    {
        static DesignTimeModels()
        {
            CrawlingBucket bucket = new CrawlingBucket
            {
                Name = "Example.com",
                Description = "",
                NbThreads = 2,
                NbRetry = 2,
                LimitRequests = 100
            };

            CrawlingCondition condition = new CrawlingCondition
            {
                FieldType = CrawlingConditionFieldType.Host,
                ComparisonType = CrawlingConditionComparisonType.Equals,
                Value = "www.example.com"
            };

            CrawlingRule rule = new CrawlingRule
            {
                Name = "Example.com",
                Description = "",
                Behavior = ResourceBehavior.FollowAllReferences,
                TargetBucket = bucket
            };

            rule.Conditions.Add(condition);

            CrawlingBucket = new CrawlingBucket
            {
                Name = "Bucket name",
                Description = "Bucket description",
                LimitRequests = 0,
                NbThreads = 2,
                NbRetry = 1
            };

            CrawlingBucket.HostMappings.Add(new CrawlingHostMapping
            {
                Host = "www.example.com",
                IPAddress = IPAddress.Parse("127.0.0.1")
            });
            ProcessingBucketCollection = new ObservableCollection<CrawlingBucket> { CrawlingBucket, CrawlingBucket };

            CrawlingCondition = new CrawlingCondition
            {
                ComparisonType = CrawlingConditionComparisonType.Equals,
                FieldType = CrawlingConditionFieldType.Scheme,
                Value = "http"
            };

            ProcessingConditionCollection = new ObservableCollection<CrawlingCondition>
            {
                CrawlingCondition,
                new CrawlingCondition
                {
                    ComparisonType = CrawlingConditionComparisonType.Equals,
                    FieldType = CrawlingConditionFieldType.Host,
                    Value = "www.example.com"
                }
            };

            CrawlingRule = new CrawlingRule
            {
                Name = "Rule name",
                Description = "Rule description",
                Behavior = ResourceBehavior.FollowAllReferences,
                TargetBucket = CrawlingBucket
            };

            CrawlingRule.Conditions.AddRange(ProcessingConditionCollection);

            ProcessingRuleCollection = new ObservableCollection<CrawlingRule> { CrawlingRule, CrawlingRule };

            CrawlingConfig = new CrawlingConfig
            {
                Name = "Processing configuration name",
                Description = "Processing configuration description",
            };

            CrawlingConfig.Buckets.Add(bucket);
            CrawlingConfig.Rules.Add(rule);
            CrawlingConfig.StartingUrls.Add(new CrawlingStartingUrl
            {
                Name = "SiteMap",
                Value = new Uri("http://www.exaple.com/SiteMap.axd?UrlEncode=false")
            });
            ProcessingConfigurationCollection = new ObservableCollection<CrawlingConfig> { CrawlingConfig, CrawlingConfig, CrawlingConfig };

            FileViewModel = new FileViewModel(new CrawlingContext(), new DispatcherQueue(Dispatcher.CurrentDispatcher));
            FileViewModel.Model.Resources.Add(new Resource(new Uri("http://www.google.ca"), ResourceBehavior.FollowAllReferences));


            ReportConfig = new ReportConfig
            {
                Guid = Guid.NewGuid(),
                Name = "Resources",
                Description = "Some description",
                Type = ReportType.ListResources
            };
            ReportConfig.Columns.AddRange(new[]
            {
                new ReportConfigColumn
                {
                    Name = "Starting time",
                    Path = "Resource.TimeStart",
                    Width = 20
                },
                new ReportConfigColumn
                {
                    Name = "Content type",
                    Path = "Resource.Headers.ContentType",
                    Width = 15
                },
                new ReportConfigColumn
                {
                    Name = "Status code",
                    Path = "Resource.HttpStatusCode",
                    Width = 15
                },
                new ReportConfigColumn
                {
                    Name = "URL",
                    Path = "Resource.URL.AbsoluteUri"
                }
            });
            ReportCollection = new ObservableCollection<ReportConfig>
            {
                ReportConfig,
                new ReportConfig { Name = "References", Type = ReportType.ListResources }
            };

            MainViewModel = new MainViewModel();
            MainViewModel.Files.Add(FileViewModel);
        }

        public static CrawlingBucket CrawlingBucket { get; private set; }
        public static ObservableCollection<CrawlingBucket> ProcessingBucketCollection { get; private set; }
        public static CrawlingCondition CrawlingCondition { get; private set; }
        public static ObservableCollection<CrawlingCondition> ProcessingConditionCollection { get; private set; }
        public static CrawlingRule CrawlingRule { get; private set; }
        public static ObservableCollection<CrawlingRule> ProcessingRuleCollection { get; private set; }
        public static CrawlingConfig CrawlingConfig { get; private set; }
        public static ObservableCollection<CrawlingConfig> ProcessingConfigurationCollection { get; private set; }
        public static FileViewModel FileViewModel { get; private set; }
        public static MainViewModel MainViewModel { get; private set; }
        public static ObservableCollection<ReportConfig> ReportCollection { get; private set; }
        public static ReportConfig ReportConfig { get; private set; } 
    }
}
