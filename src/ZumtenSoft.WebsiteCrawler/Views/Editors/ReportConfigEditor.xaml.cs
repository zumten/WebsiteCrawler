using System.Windows;
using System.Windows.Controls;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;

namespace ZumtenSoft.WebsiteCrawler.Views.Editors
{
    public partial class ReportConfigEditor
    {
        public ReportConfig Model { get; private set; }
        public bool ApplyChanges { get; private set; }

        public ReportConfigEditor()
        {
            InitializeComponent();
        }

        public static bool StartEditing(Window owner, ReportConfig items)
        {
            ReportConfigEditor editor = new ReportConfigEditor();
            editor.Model = items;
            editor.DataContext = items;
            editor.Owner = owner;
            editor.ShowDialog();

            return editor.ApplyChanges;
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

        private void BtnMoveUp_CLick(object sender, RoutedEventArgs e)
        {
            ReportConfigColumn column = (ReportConfigColumn)((Control)sender).DataContext;
            int index = Model.Columns.IndexOf(column);

            if (index > 0)
                Model.Columns.Move(index, index - 1);
        }

        private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            ReportConfigColumn column = (ReportConfigColumn)((Control)sender).DataContext;
            int index = Model.Columns.IndexOf(column);

            if (index < Model.Columns.Count - 1)
                Model.Columns.Move(index, index + 1);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            ReportConfigColumn column = (ReportConfigColumn)((Control)sender).DataContext;
            Model.Columns.Remove(column);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Model.Columns.Add(new ReportConfigColumn
            {
                Name = "New column",
                Path = "",
                Width = 15
            });
        }
    }
}
