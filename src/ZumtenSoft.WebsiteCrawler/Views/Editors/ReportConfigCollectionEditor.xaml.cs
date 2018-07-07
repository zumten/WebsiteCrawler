using System;
using System.Windows;
using System.Windows.Controls;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;
using ZumtenSoft.WebsiteCrawler.Controls;
using ZumtenSoft.Linq2ObsCollection.Collections;

namespace ZumtenSoft.WebsiteCrawler.Views.Editors
{
    public partial class ReportConfigCollectionEditor : Window
    {
        public ObservableCollection<ReportConfig> Model { get; private set; }
        public bool ApplyChanges { get; private set; }

        public ReportConfigCollectionEditor()
        {
            InitializeComponent();
        }

        public static bool StartEditing(Window owner, ObservableCollection<ReportConfig> items)
        {
            ReportConfigCollectionEditor editor = new ReportConfigCollectionEditor();
            editor.Model = items;
            editor.DataContext = items;
            editor.Owner = owner;
            editor.ShowDialog();

            return editor.ApplyChanges;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ReportConfig config = new ReportConfig
            {
                Guid = Guid.NewGuid(),
                Name = "New report",
                Description = "",
                Type = ReportType.ListResources,
            };

            if (ReportConfigEditor.StartEditing(this, config))
                Model.Add(config);
        }

        private void BtnMoveUp_CLick(object sender, RoutedEventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            ReportConfig config = (ReportConfig)button.DataContext;
            int index = Model.IndexOf(config);

            if (index > 0)
                Model.Move(index, index - 1);
        }

        private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            ReportConfig config = (ReportConfig)button.DataContext;
            int index = Model.IndexOf(config);

            if (index < Model.Count - 1)
                Model.Move(index, index + 1);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            ReportConfig config = (ReportConfig)button.DataContext;

            Model.Remove(config);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            ReportConfig config = (ReportConfig) button.DataContext;
            ReportConfig clone = ReportConfigSerializer.Clone(config);

            if (ReportConfigEditor.StartEditing(this, clone))
            {
                int index = Model.IndexOf(config);
                Model[index] = clone;
            }
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
