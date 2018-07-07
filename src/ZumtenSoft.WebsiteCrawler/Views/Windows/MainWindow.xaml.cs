using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using Microsoft.Win32;
using OfficeOpenXml;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction.Behaviors;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Networking;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Processing;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.WebsiteCrawler.BusLayer.Reporting;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;
using ZumtenSoft.WebsiteCrawler.Models;
using ZumtenSoft.WebsiteCrawler.Utils;
using ZumtenSoft.WebsiteCrawler.Utils.Helpers;
using ZumtenSoft.WebsiteCrawler.Views.Editors;
using ZumtenSoft.Linq2ObsCollection.Collections;
using ZumtenSoft.Linq2ObsCollection.Threading;

namespace ZumtenSoft.WebsiteCrawler.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainViewModel Model { get; private set; }

        public MainWindow(string[] args)
        {
            Model = new MainViewModel();
            InitializeComponent();
            DataContext = Model;

            if (args.Length > 0)
            {
                foreach (string arg in args)
                {
                    if (File.Exists(arg))
                        LoadFile(new FileInfo(arg));
                    else
                        MessageBox.Show("Could not find file " + arg, "File load error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                AddFile(new CrawlingContext());
            }
            

            Model.CrawlingConfigs.AddRange(ConfigurationHelper.LoadCrawlingConfigs());
            Model.ReportConfigs.AddRange(ConfigurationHelper.LoadReportConfigurations());
            Model.SelectedConfig = Model.CrawlingConfigs.FirstOrDefault();
            //WebSiteCrawlerConfigSection configSection = (WebSiteCrawlerConfigSection)ConfigurationManager.GetSection("webSiteCrawler");
            //Model.CrawlingConfigs.AddRange(configSection.CrawlingConfigs.Cast<CrawlingConfigElement>());
        }

        #region Command bindings

        private void FileNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddFile(new CrawlingContext());
        }

        private void FileClose_Executed(object sender, RoutedEventArgs e)
        {
            FileViewModel file = GetSelectedFile(sender, e);
            if (file.Status == CrawlingStatus.Ready)
            {
                int index = Model.Files.IndexOf(file);
                if (Model.SelectedFile == file)
                {
                    Model.SelectedFile = Model.Files.ElementAtOrDefault(index + 1) ?? Model.Files.ElementAtOrDefault(index - 1);
                }
                Model.Files.RemoveAt(index);
            }
        }

        private void FileAction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            FileViewModel file = GetSelectedFile(sender, e);
            e.CanExecute =  file != null && file.Status == CrawlingStatus.Ready;
        }


        private void CrawlingStop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            FileViewModel file = GetSelectedFile(sender, e);
            e.CanExecute = file != null && file.Status == CrawlingStatus.Processing;
        }

        private void FileOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Configure open file dialog box 
            OpenFileDialog dlg = new OpenFileDialog(); 
            dlg.FileName = "CrawlingResult.cwl"; // Default file name 
            dlg.DefaultExt = ".cwl"; // Default file extension 
            dlg.Filter = "Crawling result (.cwl)|*.cwl"; // Filter files by extension 

            // Show open file dialog box 
            if (dlg.ShowDialog() == true)
            {
                LoadFile(new FileInfo(dlg.FileName));
            }
        }

        private void FileSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileViewModel file = GetSelectedFile(sender, e);
            SaveFile(file.Model, false);
        }

        private void FileSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileViewModel file = GetSelectedFile(sender, e);
            SaveFile(file.Model, true);
        }

        private void FileExit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }



        private void CrawlingPlay_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Model.SelectedConfig == null)
                return;

            CrawlingContext context = Model.SelectedFile.Model;
            int nbRetry = Int32.Parse(ConfigurationManager.AppSettings["NbRetry"]);
            context.Crawler = new Crawler(context, null);

            context.Crawler.OnCompleted += (_, __) =>
            {
                context.Status = CrawlingStatus.Ready;
                context.Crawler = null;
                RefreshActions();
            };

            Dictionary<CrawlingBucket, WorkBucket<Resource, BucketContext>> bucketMapping = new Dictionary<CrawlingBucket, WorkBucket<Resource, BucketContext>>();
            foreach (CrawlingBucket bucket in Model.SelectedConfig.Buckets)
            {
                BucketContext bucketContext = new BucketContext(nbRetry);
                foreach (CrawlingHostMapping mapping in bucket.HostMappings)
                    bucketContext.Hosts[mapping.Host] = mapping.IPAddress;

                WorkBucket<Resource, BucketContext> workBucket = context.Crawler.AddBucket(bucket.Name, bucket.NbThreads, bucketContext);
                bucketMapping.Add(bucket, workBucket);
            }

            foreach (CrawlingRule rule in Model.SelectedConfig.Rules)
            {
                WorkBucket<Resource, BucketContext> workBucket = bucketMapping[rule.TargetBucket];
                context.Crawler.AddBehaviorRule(rule.Name, rule.Behavior, workBucket, rule.Conditions);
            }

            context.Crawler.Reprocess();
            //foreach (ResourceToProcess resourceToProcess in config.)
            //    context.Crawler.AddUrlToProcess(resourceToProcess.Url);
            foreach (CrawlingStartingUrl startingUrl in Model.SelectedConfig.StartingUrls)
                context.Crawler.AddUrlToProcess(startingUrl.Value);

            if (context.Crawler.WorkDispatcher.IsWorking)
            {
                context.Status = CrawlingStatus.Processing;
                RefreshActions();
            }
            else
            {
                MessageBox.Show(this, "Nothing to do", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CrawlingStop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CrawlingContext context = Model.SelectedFile.Model;

            if (context.Crawler.WorkDispatcher.IsWorking)
            {
                context.Status = CrawlingStatus.Stopping;
                Crawler crawler = context.Crawler;
                crawler.Stop();
                RefreshActions();
            }
        }

        private void ReportingGenerate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CrawlingContext context = Model.SelectedFile.Model;

            string defaultFileName = context.FullPath != null ? Path.GetFileNameWithoutExtension(context.FullPath.FullName) : "Report";

            // Configure save file dialog box
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = defaultFileName; // Default file name 
            dlg.DefaultExt = ".xlsx"; // Default file extension 
            dlg.Filter = "Excel document (.xlsx)|*.xlsx"; // Filter files by extension 

            // Process save file dialog box results
            if (dlg.ShowDialog() == true)
            {
                context.Status = CrawlingStatus.GeneratingReport;

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    ExcelPackage pkg = new ExcelPackage();
                    ReportSerializer.AddReport(pkg, ReportGenerator.GenerateRequestsReport(context.Resources));
                    ReportSerializer.AddReport(pkg, ReportGenerator.GenerateCachingReport(context.Resources));
                    ReportSerializer.AddReport(pkg, ReportGenerator.GenerateReferenceSummaryReport(context.Resources));
                    ReportSerializer.AddReport(pkg, ReportGenerator.GenerateReferencesReport(context.Resources));
                    ReportSerializer.AddReport(pkg, ReportGenerator.GenerateRedirectionReport(context.Resources));
                    ReportSerializer.AddReport(pkg, ReportGenerator.GenerateContentReport(context.Resources));
                    ReportSerializer.AddReport(pkg, ReportGenerator.GenerateErrorsReport(context.Resources));
                    //foreach (ReportConfig reportConfig in Model.ReportConfigs)
                    //    ReportSerializer.AddReport(pkg, ReportGenerator.GenerateReport(reportConfig, context.Resources));

                    pkg.SaveAs(new FileInfo(dlg.FileName));

                    context.Status = CrawlingStatus.Ready;
                    RefreshActions();
                });
            }
        }

        #endregion Command bindings

        private void SaveFile(CrawlingContext file, bool saveAs)
        {
            FileInfo target = null;
            if (file.FullPath == null || saveAs)
            {
                // Configure save file dialog box
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = file.FullPath == null ? "CrawlingResult.cwl" : file.FullPath.FullName; // Default file name 
                dlg.DefaultExt = ".cwl"; // Default file extension 
                dlg.Filter = "Crawling result (.cwl)|*.cwl"; // Filter files by extension 

                // Process save file dialog box results
                if (dlg.ShowDialog() == true)
                {
                    target = new FileInfo(dlg.FileName);
                }
                else
                {
                    return;
                }
            }
            else
            {
                target = file.FullPath;
            }

            if (target != null)
            {
                file.Status = CrawlingStatus.Saving;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        XElement element = ResourcesSerializer.SerializeResourceCollection(file.Resources);
                        element.Save(target.FullName);
                        file.FullPath = target;
                        file.HasChanged = false;
                    }
                    catch (Exception e)
                    {
                        Dispatcher.Invoke((Action) (() => MessageBox.Show("Failed to save file " + target.Name + ": " + e.Message)));
                    }
                    finally
                    {
                        file.Status = CrawlingStatus.Ready;
                        RefreshActions();
                    }
                });
            }
        }

        private void AddFile(CrawlingContext model)
        {
            DispatcherQueue queue = new DispatcherQueue(Dispatcher);
            FileViewModel viewModel = new FileViewModel(model, queue);
            Model.Files.Add(viewModel);
            Model.SelectedFile = viewModel;
        }

        private void LoadFile(FileInfo file)
        {
            CrawlingContext context = new CrawlingContext();
            context.FullPath = new FileInfo(file.FullName);
            context.HasChanged = false;
            context.Status = CrawlingStatus.Loading;
            AddFile(context);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                XDocument doc = XDocument.Load(file.FullName);
                List<Resource> resources = ResourcesSerializer.DeserializeResourceCollection(doc.Root);
                foreach (Resource resource in resources)
                    context.Resources.Add(resource);

                context.Status = CrawlingStatus.Ready;
                RefreshActions();
            });
        }

        private FileViewModel GetSelectedFile(object sender, RoutedEventArgs e)
        {
            UIElement element = sender as UIElement;
            if (element != null)
            {
                foreach (TabItem tabItem in element.Ancestors<TabItem>())
                {
                    FileViewModel viewModel = (FileViewModel) tabItem.Header;
                    if (viewModel != null)
                        return viewModel;
                }
            }

            return Model.SelectedFile;
        }

        private void EditCrawlingConfigs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObservableCollection<CrawlingConfig> clone = new ObservableCollection<CrawlingConfig>(Model.CrawlingConfigs.Select(CrawlingConfigurationSerializer.Clone));
            if (CrawlingConfigCollectionEditor.StartEditing(this, clone))
            {
                CrawlingConfig previousSelectedConfig = Model.SelectedConfig;
                Model.CrawlingConfigs.Clear();
                Model.CrawlingConfigs.AddRange(clone);
                ConfigurationHelper.SaveCrawlingConfigs(Model.CrawlingConfigs);

                CrawlingConfig newSelectedConfig = previousSelectedConfig == null ? null :
                    Model.CrawlingConfigs.FirstOrDefault(x => x.Guid == previousSelectedConfig.Guid);

                Model.SelectedConfig = newSelectedConfig ?? Model.CrawlingConfigs.FirstOrDefault();
            }
        }

        private void EditReportConfigs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObservableCollection<ReportConfig> clone = new ObservableCollection<ReportConfig>(Model.ReportConfigs.Select(ReportConfigSerializer.Clone));
            if (ReportConfigCollectionEditor.StartEditing(this, clone))
            {
                Model.ReportConfigs.Clear();
                Model.ReportConfigs.AddRange(clone);
                ConfigurationHelper.SaveReportConfigs(Model.ReportConfigs);
            }
        }

        private void ImportUrls_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CrawlingContext context = Model.SelectedFile.Model;
            if (context.Status == CrawlingStatus.Ready)
            {
                List<Uri> urls = ImportUrlsWindow.PromptUrls(this);
                HashSet<string> existingUrls = new HashSet<string>(context.Resources.Select(x => x.Url.AbsoluteUri));
                foreach (Uri url in urls)
                {
                    Uri urlWithoutSessionId = url.WithoutSession();
                    if (!existingUrls.Contains(urlWithoutSessionId.AbsoluteUri))
                    {
                        Resource resource = new Resource(urlWithoutSessionId, ResourceBehavior.Ignore);
                        resource.Status = ResourceStatus.ReadyToProcess;
                        context.Resources.Add(resource);
                        existingUrls.Add(urlWithoutSessionId.AbsoluteUri);
                    }
                }
            }
        }

        private void RefreshActions()
        {
            if (Dispatcher == Dispatcher.CurrentDispatcher)
                CommandManager.InvalidateRequerySuggested();
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Send, (Action)(CommandManager.InvalidateRequerySuggested));
            }
        }
    }
}
