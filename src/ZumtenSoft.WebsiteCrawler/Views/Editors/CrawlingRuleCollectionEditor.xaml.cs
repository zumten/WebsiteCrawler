using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;

namespace ZumtenSoft.WebsiteCrawler.Views.Editors
{
    public partial class CrawlingRuleCollectionEditor
    {
        public CrawlingRuleCollectionEditor()
        {
            InitializeComponent();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            CrawlingConfig config = (CrawlingConfig)DataContext;
            CrawlingRule rule = new CrawlingRule
            {
                Guid = Guid.NewGuid(),
                Name = "New rule",
                Description = "",
                TargetBucket = config.Buckets.FirstOrDefault()
            };

            if (CrawlingRuleEditor.StartEditing(Window.GetWindow(this), config, rule))
                config.Rules.Add(rule);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            CrawlingConfig config = (CrawlingConfig)DataContext;
            CrawlingRule rule = (CrawlingRule)((Control)sender).DataContext;
            CrawlingRule clone = CrawlingConfigurationSerializer.Clone(rule);

            if (CrawlingRuleEditor.StartEditing(Window.GetWindow(this), config, clone))
            {
                int index = config.Rules.IndexOf(rule);
                config.Rules[index] = clone;
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            CrawlingConfig config = (CrawlingConfig)DataContext;
            CrawlingRule rule = (CrawlingRule) ((Control) sender).DataContext;

            config.Rules.Remove(rule);
        }

        private void BtnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            CrawlingConfig config = (CrawlingConfig)DataContext;
            CrawlingRule rule = (CrawlingRule)((Control)sender).DataContext;
            int index = config.Rules.IndexOf(rule);

            if (index > 0)
                config.Rules.Move(index, index - 1);
        }

        private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            CrawlingConfig config = (CrawlingConfig)DataContext;
            CrawlingRule rule = (CrawlingRule)((Control)sender).DataContext;
            int index = config.Rules.IndexOf(rule);

            if (index < config.Rules.Count - 1)
                config.Rules.Move(index, index + 1);
        }
    }
}
