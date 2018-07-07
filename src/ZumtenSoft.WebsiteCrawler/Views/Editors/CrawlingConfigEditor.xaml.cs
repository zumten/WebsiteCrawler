using System.Windows;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;

namespace ZumtenSoft.WebsiteCrawler.Views.Editors
{
    public partial class CrawlingConfigEditor
    {
        public CrawlingConfig Model
        {
            get { return (CrawlingConfig) GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            "Model", typeof (CrawlingConfig), typeof (CrawlingConfigEditor));

        public bool ApplyChanges { get; private set; }

        public CrawlingConfigEditor(CrawlingConfig config)
        {
            DataContext = config;
            Model = config;
            InitializeComponent();
        }

        public CrawlingConfigEditor()
        {
            
        }

        public static bool StartEditing(Window parent, CrawlingConfig config)
        {
            CrawlingConfigEditor view = new CrawlingConfigEditor(config);
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
    }
}
