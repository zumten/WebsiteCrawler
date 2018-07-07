using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.Windows.Input;
using ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction.Behaviors;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.WebsiteCrawler.Models;

namespace ZumtenSoft.WebsiteCrawler.Views.Controls
{
    /// <summary>
    /// Interaction logic for FileView.xaml
    /// </summary>
    public partial class FileView
    {
        public FileView()
        {
            InitializeComponent();
        }

        private void ResetResources_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ListView resourcesView = (ListView)sender;
            List<Resource> resources = resourcesView.SelectedItems
                .Cast<ResourceViewModel>()
                .Select(x => x.Model)
                .Where(x => x.Status == ResourceStatus.Processed)
                .ToList();

            foreach (Resource resource in resources)
            {
                while (resource.References.Count > 0)
                {
                    ResourceReference reference = resource.References[resource.References.Count - 1];
                    resource.References.RemoveAt(resource.References.Count - 1);

                    lock (((ICollection)reference.Target.ReferencedBy).SyncRoot)
                        reference.Target.ReferencedBy.Remove(reference);
                }

                resource.Headers.Clear();
                resource.Content.Clear();
                resource.Errors.Clear();
                resource.Behavior = ResourceBehavior.Ignore;
                resource.Status = ResourceStatus.Ignored;
                resource.Size = 0;
                resource.CompressedSize = 0;
                resource.HttpStatus = 0;
                resource.ViewStateSize = 0;
                resource.ContentType = ResourceContentType.Unknown;
                resource.TimeLoading = TimeSpan.Zero;
                resource.TimeProcessing = TimeSpan.Zero;
                resource.TimeStart = new DateTime();
            }
        }

        private void ResetResources_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ListView resourcesView = (ListView) sender;
            e.CanExecute = resourcesView.SelectedItems.Count > 0
                && resourcesView.SelectedItems.Cast<ResourceViewModel>().All(x => x.Model.Status == ResourceStatus.Processed);
        }
    }
}
