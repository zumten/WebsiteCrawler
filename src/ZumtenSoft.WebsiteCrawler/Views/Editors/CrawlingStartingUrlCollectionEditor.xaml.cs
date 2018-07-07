using System;
using System.Windows;
using System.Windows.Controls;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;

namespace ZumtenSoft.WebsiteCrawler.Views.Editors
{
    public partial class CrawlingStartingUrlCollectionEditor
    {
        public CrawlingStartingUrlCollectionEditor()
        {
            InitializeComponent();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            CrawlingConfig config = (CrawlingConfig)DataContext;
            CrawlingStartingUrl startingUrl = (CrawlingStartingUrl) ((Control) sender).DataContext;
            config.StartingUrls.Remove(startingUrl);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            CrawlingConfig config = (CrawlingConfig) DataContext;
            config.StartingUrls.Add(new CrawlingStartingUrl
            {
                Guid = Guid.NewGuid(),
                Name = "New URL",
                Value = new Uri("http://www.somedomain.com/")
            });
        }
    }
}
