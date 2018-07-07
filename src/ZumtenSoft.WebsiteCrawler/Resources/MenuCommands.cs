using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ZumtenSoft.WebsiteCrawler.Resources
{
    /// <summary>
    /// Commande WPF contenant directement un lien vers l'icône à afficher.
    /// </summary>
    [DebuggerDisplay(@"\{Command Text={Text}\}")]
    public class Command : RoutedUICommand
    {
        public BitmapImage Icon { get; private set; }

        public Command(string text, string name, BitmapImage icon, params InputGesture[] keys) : base(text, name, typeof(Window))
        {
            Icon = icon;
            InputGestures.AddRange(keys);
        }

        public Command(string text, string name, params InputGesture[] keys) : base(text, name, typeof(Window))
        {
            InputGestures.AddRange(keys);
        }
    }


    /// <summary>
    /// Liste des commandes utilisées dans l'application
    /// </summary>
    public static class MenuCommands
    {
        public static RoutedUICommand Build(string text, string name, BitmapImage icon, params InputGesture[] keys)
        {
            var command = new Command(text, name, icon);
            command.InputGestures.AddRange(keys);
            return command;
        }

        public static RoutedUICommand Build(string text, string name, params InputGesture[] keys)
        {
            var command = new RoutedUICommand(text, name, typeof(Window));
            command.InputGestures.AddRange(keys);
            return command;
        }

        // File
        public static readonly RoutedUICommand FileNew = Build("New", "FileNew", BitmapIcons.New, new KeyGesture(Key.N, ModifierKeys.Control));
        public static readonly RoutedUICommand FileOpen = Build("Open...", "FileOpen", BitmapIcons.Open, new KeyGesture(Key.O, ModifierKeys.Control));
        public static readonly RoutedUICommand FileClose = Build("Close", "FileClose", new KeyGesture(Key.W, ModifierKeys.Control));
        public static readonly RoutedUICommand FileSave = Build("Save", "FileSave", BitmapIcons.Save, new KeyGesture(Key.S, ModifierKeys.Control));
        public static readonly RoutedUICommand FileSaveAs = Build("Save As...", "FileSaveAs");
        public static readonly RoutedUICommand FileExit = Build("Exit", "FileExit", new KeyGesture(Key.F4, ModifierKeys.Alt));

        // Crawling
        public static readonly RoutedUICommand CrawlingPlay = Build("Play", "CrawlingPlay", BitmapIcons.ControlPlay);
        public static readonly RoutedUICommand CrawlingStop = Build("Stop", "CrawlingStop", BitmapIcons.ControlStop);
        public static readonly RoutedUICommand ImportUrls = Build("Import urls", "ImportUrls");

        // Reporting
        public static readonly RoutedUICommand ReportingGenerate = Build("Generate report", "ReportingGenerate", BitmapIcons.FileExcel);

        // Configuration
        public static readonly RoutedUICommand EditCrawlingConfigs = Build("Edit crawling configuration", "EditCrawlingConfigs");
        public static readonly RoutedUICommand EditReportConfigs = Build("Edit reporting configuration", "EditReportConfigs");

        public static readonly RoutedUICommand ResetResources = Build("Reset resources", "ResetResources");
    }
}