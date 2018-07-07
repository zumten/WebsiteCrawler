using System.Net;
using System.Windows;
using System.Windows.Controls;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;

namespace ZumtenSoft.WebsiteCrawler.Views.Editors
{
    public partial class CrawlingBucketEditor : Window
    {
        public bool ApplyChanges { get; private set; }

        public CrawlingBucketEditor(CrawlingBucket bucket)
        {
            DataContext = bucket;
            InitializeComponent();
        }

        public static bool StartEditing(Window parent, CrawlingBucket bucket)
        {
            CrawlingBucketEditor view = new CrawlingBucketEditor(bucket);
            view.Owner = parent;
            view.ShowDialog();

            return view.ApplyChanges;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges = false;
            Close();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            CrawlingBucket bucket = (CrawlingBucket)DataContext;

            bucket.HostMappings.Add(new CrawlingHostMapping
            {
                Host = "www.example.com",
                IPAddress = IPAddress.Parse("127.0.0.1")
            });
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            CrawlingRule rule = (CrawlingRule)DataContext;
            CrawlingCondition condition = (CrawlingCondition)((Control)sender).DataContext;

            rule.Conditions.Remove(condition);
        }
    }
}
