using System;
using System.Windows;
using System.Windows.Controls;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;

namespace ZumtenSoft.WebsiteCrawler.Views.Editors
{
    public partial class CrawlingBucketCollectionEditor
    {
        public CrawlingBucketCollectionEditor()
        {
            InitializeComponent();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            CrawlingConfig config = (CrawlingConfig) DataContext;

            config.Buckets.Add(new CrawlingBucket
            {
                Guid = Guid.NewGuid(),
                Name = "New bucket",
                Description = "",
                NbThreads = 1,
                NbRetry = 2,
                LimitRequests = 0
            });
        }

        private void BtnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            CrawlingConfig config = (CrawlingConfig)DataContext;
            CrawlingBucket bucket = (CrawlingBucket)((Control)sender).DataContext;
            int index = config.Buckets.IndexOf(bucket);

            if (index > 0)
                config.Buckets.Move(index, index - 1);
        }

        private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            CrawlingConfig config = (CrawlingConfig)DataContext;
            CrawlingBucket bucket = (CrawlingBucket)((Control)sender).DataContext;
            int index = config.Buckets.IndexOf(bucket);

            if (index < config.Buckets.Count - 1)
                config.Buckets.Move(index, index + 1);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            CrawlingConfig config = (CrawlingConfig)DataContext;
            CrawlingBucket bucket = (CrawlingBucket)((Control) sender).DataContext;
            config.Buckets.Remove(bucket);

            foreach (CrawlingRule rule in config.Rules)
                if (rule.TargetBucket == bucket)
                    rule.TargetBucket = null;
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            CrawlingConfig config = (CrawlingConfig)DataContext;
            CrawlingBucket bucket = (CrawlingBucket)((Control)sender).DataContext;
            CrawlingBucket clone = CrawlingConfigurationSerializer.Clone(bucket);
            if (CrawlingBucketEditor.StartEditing(Window.GetWindow(this), clone))
            {
                int index = config.Buckets.IndexOf(bucket);
                config.Buckets[index] = clone;

                foreach (CrawlingRule rule in config.Rules)
                    if (rule.TargetBucket == bucket)
                        rule.TargetBucket = clone;
            }
        }


    }
}
