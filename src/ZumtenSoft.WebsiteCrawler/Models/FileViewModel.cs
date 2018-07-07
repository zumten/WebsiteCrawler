using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Imaging;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;
using ZumtenSoft.WebsiteCrawler.Resources;
using ZumtenSoft.WebsiteCrawler.Utils;
using ZumtenSoft.WebsiteCrawler.Utils.Events;
using ZumtenSoft.WebsiteCrawler.Utils.Helpers;
using ZumtenSoft.Linq2ObsCollection.Collections;
using ZumtenSoft.Linq2ObsCollection.Threading;

namespace ZumtenSoft.WebsiteCrawler.Models
{
    [DebuggerDisplay(@"\{FileViewModel Resources={Resources.Count}\}")]
    public class FileViewModel : NotifyObject
    {
        public FileViewModel(CrawlingContext model, DispatcherQueue dispatcherQueue)
        {
            Model = model;
            Resources = model.Resources
                .Select(x => new ResourceViewModel(x, dispatcherQueue))
                .Dispatch(dispatcherQueue);

            var orderedResources = Resources.OrderBy(x => x.HttpStatus).ThenBy(x => x.URL);
            var processedResources = orderedResources.Where(x => x.Status == ResourceStatus.Processed);

            Nodes = new ObservableCollection<Node>
            {
                new Node("Resources", true, BitmapIcons.Folder, null, 

                    new Node("By processing status", true, BitmapIcons.Folder, orderedResources,
                        EnumHelper<ResourceStatus>.Values
                            .Select(status => CreateNodeSplitByDomain(DescriptionExtractor.GetDescription(status), BitmapIcons.WorkStatuses.TryGetValue(status), orderedResources.Where(x => x.Status == status)))
                            .ToArray()),

                    new Node("By http status", true, BitmapIcons.Folder, processedResources,
                        from resource in processedResources
                        group resource by resource.HttpStatus into grp
                        orderby grp.Key
                        select new Node(DescriptionExtractor.GetDescription(grp.Key), false, BitmapIcons.GetImageFromHttpStatus(grp.Key), grp, SplitByDomain(grp))),

                    new Node("By bucket", true, BitmapIcons.Folder, null,
                        from resource in Resources
                        group resource by resource.CurrentBucket ?? "" into grp
                        where grp.Key != ""
                        orderby grp.Count descending, grp.Key
                        select new Node(grp.Key, false, BitmapIcons.Folder, grp)))
            };

            Model.PropertyChanged += Model_PropertyChanged;
        }

        private Node CreateNodeSplitByDomain(string name, BitmapImage icon, IObservableCollection<ResourceViewModel> resources)
        {
            return new Node(name, false, icon, resources, SplitByDomain(resources));
        } 

        private IObservableCollection<Node> SplitByDomain(IObservableCollection<ResourceViewModel> resources)
        {
            return from resource in resources
                   group resource by resource.Model.Url.Host.EmptyAsNull() ?? "(None)" into grp
                   orderby grp.Count descending, grp.Key
                   select new Node(grp.Key, false, BitmapIcons.DomainName, grp);
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "FullPath":
                    Notify("Name");
                    break;

                case "Status":
                    Notify("Status");
                    break;
            }
        }

        public string Name
        {
            get { return Model.FullPath == null ? "<new file>" : Model.FullPath.Name; }
        }

        public CrawlingStatus Status
        {
            get { return Model.Status; }
        }

        public CrawlingContext Model { get; private set; }
        public IObservableCollection<ResourceViewModel> Resources { get; private set; }
        public IObservableCollection<Node> Nodes { get; private set; }
    }
}
