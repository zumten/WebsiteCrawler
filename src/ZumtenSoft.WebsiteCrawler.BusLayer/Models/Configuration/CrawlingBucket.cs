using System;
using System.Diagnostics;
using System.Net;
using ZumtenSoft.WebsiteCrawler.Utils.Events;
using ZumtenSoft.Linq2ObsCollection.Collections;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration
{
    [DebuggerDisplay(@"\{CrawlingBucket Name={Name}, Behavior={Behavior}, NbThreads={NbThreads}, LimitRequests={LimitRequests}\}")]
    public class CrawlingBucket : NotifyObject
    {
        public CrawlingBucket()
        {
            HostMappings = new ObservableCollection<CrawlingHostMapping>();
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

        private int _nbThreads;
        public int NbThreads
        {
            get { return _nbThreads; }
            set { if (_nbThreads != value) { _nbThreads = value; Notify("NbThreads"); } }
        }

        private int _nbRetry;
        public int NbRetry
        {
            get { return _nbRetry; }
            set { if (_nbRetry != value) { _nbRetry = value; Notify("NbRetry"); } }
        }

        private int _limitRequests;
        public int LimitRequests
        {
            get { return _limitRequests; }
            set { if (_limitRequests != value) { _limitRequests = value; Notify("LimitRequests"); } }
        }

        public ObservableCollection<CrawlingHostMapping> HostMappings { get; private set; }
    }

    [DebuggerDisplay(@"\{CrawlingHostMapping Host={Host}, IPAddress={IPAddress}\}")]
    public class CrawlingHostMapping : NotifyObject
    {
        private string _host;
        public string Host
        {
            get { return _host; }
            set { if (_host != value) { _host = value; Notify("Host"); } }
        }

        private IPAddress _iPAddress;
        public IPAddress IPAddress
        {
            get { return _iPAddress; }
            set { if (_iPAddress != value) { _iPAddress = value; Notify("IPAddress"); } }
        }
    }
}
