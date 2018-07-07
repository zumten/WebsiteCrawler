
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.WebsiteCrawler.Utils.Events;
using ZumtenSoft.Linq2ObsCollection.Collections;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models
{
    public enum CrawlingStatus
    {
        [Description("Ready for work")]
        Ready,
        [Description("Loading file...")]
        Loading,
        [Description("Processing URLs...")]
        Processing,
        [Description("Generating report...")]
        GeneratingReport,
        [Description("Stopping work...")]
        Stopping,
        [Description("Saving file...")]
        Saving
    }

    [DebuggerDisplay(@"\{CrawlingContext FullPath={FullPath.FullName}, HasChanged={HasChanged}, Status={Status}, Resources={Resources.Count}\}")]
    public class CrawlingContext : NotifyObject
    {
        public CrawlingContext()
        {
            Resources = new ObservableCollection<Resource>();
        }

        private FileInfo _fullPath;
        public FileInfo FullPath
        {
            get { return _fullPath; }
            set { if (_fullPath != value) { _fullPath = value; Notify("FullPath"); } }
        }

        private bool _hasChanged;
        public bool HasChanged
        {
            get { return _hasChanged; }
            set { if (_hasChanged != value) { _hasChanged = value; Notify("HasChanged"); } }
        }

        private CrawlingStatus _status;
        public CrawlingStatus Status
        {
            get { return _status; }
            set { if (_status != value) { _status = value; Notify("Status"); } }
        }

        public ObservableCollection<Resource> Resources { get; private set; }
        public Crawler Crawler { get; set; }
    }
}
