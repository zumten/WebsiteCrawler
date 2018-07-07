using System;
using System.Windows;
using System.Windows.Controls;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;
using ZumtenSoft.WebsiteCrawler.Controls;
using ZumtenSoft.Linq2ObsCollection.Collections;

namespace ZumtenSoft.WebsiteCrawler.Views.Editors
{
    /// <summary>
    /// Interaction logic for ProcessingConfigurationListingView.xaml
    /// </summary>
    public partial class CrawlingConfigCollectionEditor
    {
        public ObservableCollection<CrawlingConfig> Model { get; private set; }

        public bool ApplyChanges { get; private set; }

        public CrawlingConfigCollectionEditor()
        {
            InitializeComponent();
        }

        public static bool StartEditing(Window owner, ObservableCollection<CrawlingConfig> items)
        {
            CrawlingConfigCollectionEditor editor = new CrawlingConfigCollectionEditor();
            editor.Model = items;
            editor.DataContext = items;
            editor.Owner = owner;
            editor.ShowDialog();

            return editor.ApplyChanges;
        }

        private void BtnMoveUp_CLick(object sender, RoutedEventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            CrawlingConfig config = (CrawlingConfig)button.DataContext;
            int index = Model.IndexOf(config);

            if (index > 0)
                Model.Move(index, index - 1);
        }

        private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            CrawlingConfig config = (CrawlingConfig)button.DataContext;
            int index = Model.IndexOf(config);

            if (index < Model.Count - 1)
                Model.Move(index, index + 1);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            CrawlingConfig config = (CrawlingConfig)button.DataContext;
            CrawlingConfig clone = CrawlingConfigurationSerializer.Clone(config);

            if (CrawlingConfigEditor.StartEditing(this, clone))
            {
                int index = Model.IndexOf(config);
                Model[index] = clone;
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            CrawlingConfig config = (CrawlingConfig)button.DataContext;

            Model.Remove(config);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            CrawlingConfig config = new CrawlingConfig
            {
                Guid = Guid.NewGuid(),
                Name = "New configuration",
                Description = ""
            };

            config.Buckets.Add(new CrawlingBucket
            {
                Guid = Guid.NewGuid(),
                Name = "Bucket",
                Description = "",
                NbThreads = 1,
                NbRetry = 2
            });

            if (CrawlingConfigEditor.StartEditing(this, config))
                Model.Add(config);
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges = false;
            Close();
        }
    }
}
