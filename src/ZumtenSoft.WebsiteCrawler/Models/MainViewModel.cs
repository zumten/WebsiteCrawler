using System.Diagnostics;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;
using ZumtenSoft.WebsiteCrawler.Utils.Events;
using ZumtenSoft.Linq2ObsCollection.Collections;

namespace ZumtenSoft.WebsiteCrawler.Models
{
    [DebuggerDisplay(@"\{MainViewModel Files=Files.Count\}")]
    public class MainViewModel : NotifyObject
    {
        public MainViewModel()
        {
            Files = new ObservableCollection<FileViewModel>();
            CrawlingConfigs = new ObservableCollection<CrawlingConfig>();
            ReportConfigs = new ObservableCollection<ReportConfig>();
        }

        private FileViewModel _selectedFile;
        public FileViewModel SelectedFile
        {
            get { return _selectedFile; }
            set { if (_selectedFile != value) { _selectedFile = value; Notify("SelectedFile"); } }
        }

        private CrawlingConfig _selectedConfig;
        public CrawlingConfig SelectedConfig
        {
            get { return _selectedConfig; }
            set { if (_selectedConfig != value) { _selectedConfig = value; Notify("SelectedConfig"); } }
        }

        public IObservableCollection<CrawlingConfig> CrawlingConfigs { get; private set; }
        public IObservableCollection<ReportConfig> ReportConfigs { get; private set; }
        public IObservableCollection<FileViewModel> Files { get; private set; }
    }
}
