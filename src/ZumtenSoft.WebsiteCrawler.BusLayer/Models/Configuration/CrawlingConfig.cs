using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ZumtenSoft.WebsiteCrawler.Utils.Events;
using ZumtenSoft.Linq2ObsCollection.Collections;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration
{
    [DebuggerDisplay(@"\{ProcessingConfiguration Name={Name}, Rules={Rules.Count}\}")]
    public class CrawlingConfig : NotifyObject
    {
        public CrawlingConfig()
        {
            Rules = new ObservableCollection<CrawlingRule>();
            Buckets = new ObservableCollection<CrawlingBucket>();
            StartingUrls = new ObservableCollection<CrawlingStartingUrl>();
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

        public ObservableCollection<CrawlingBucket> Buckets { get; private set; }
        public ObservableCollection<CrawlingRule> Rules { get; private set; }
        public ObservableCollection<CrawlingStartingUrl> StartingUrls { get; private set; }
    }
}
