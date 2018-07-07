using System;
using System.Diagnostics;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction.Behaviors;
using ZumtenSoft.WebsiteCrawler.Utils.Events;
using ZumtenSoft.Linq2ObsCollection.Collections;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration
{
    [DebuggerDisplay(@"\{CrawlingRule Name={Name}, Conditions={Conditions.Count}, Behavior={Behavior}\}")]
    public class CrawlingRule : NotifyObject
    {
        public CrawlingRule()
        {
            Conditions = new ObservableCollection<CrawlingCondition>();
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

        private ResourceBehavior _behavior;
        public ResourceBehavior Behavior
        {
            get { return _behavior; }
            set { if (_behavior != value) { _behavior = value; Notify("Behavior"); } }
        }

        private CrawlingBucket _targetBucket;
        public CrawlingBucket TargetBucket
        {
            get { return _targetBucket; }
            set { if (_targetBucket != value) { _targetBucket = value; Notify("TargetBucket"); } }
        }

        public ObservableCollection<CrawlingCondition> Conditions { get; private set; }
    }
}
