using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ZumtenSoft.WebsiteCrawler.Views.Windows
{
    /// <summary>
    /// Interaction logic for ImportUrlsWindow.xaml
    /// </summary>
    public partial class ImportUrlsWindow : Window
    {
        public ImportUrlsWindow()
        {
            InitializeComponent();
            txtUrls.Focus();
        }

        public static List<Uri> PromptUrls(Window owner)
        {
            ImportUrlsWindow window = new ImportUrlsWindow();
            window.Owner = owner;
            window.ShowDialog();

            List<Uri> urls = new List<Uri>();
            if (window.ApplyChanges)
            {
                foreach (string line in window.txtUrls.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Uri url;
                    if (Uri.TryCreate(line, UriKind.Absolute, out url))
                        urls.Add(url);
                }
            }

            return urls;
        }

        protected bool ApplyChanges { get; private set; }

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
