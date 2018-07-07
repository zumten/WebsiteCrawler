using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.WebsiteCrawler.BusLayer.Reporting;
using ZumtenSoft.WebsiteCrawler.Models;
using ZumtenSoft.WebsiteCrawler.Views;
using ZumtenSoft.WebsiteCrawler.Views.Windows;

namespace ZumtenSoft.WebsiteCrawler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow window = new MainWindow(e.Args);
            window.Show();
        }
    }
}
