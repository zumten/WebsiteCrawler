using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ZumtenSoft.WebsiteCrawler.Utils.Events;
using ZumtenSoft.Linq2ObsCollection.Collections;

namespace ZumtenSoft.WebsiteCrawler.Models
{
    [DebuggerDisplay(@"\{Node Name={Name}, IsExpanded={IsExpanded}, Nodes={Nodes.Count}\}")]
    public class Node : NotifyObject, IDisposable
    {
        private readonly string _name;
        private ActionDeferrer _propChangedName;

        public Node(string name, bool isExpanded, BitmapImage icon, IObservableCollection<ResourceViewModel> resources, params Node[] children)
        {
            _name = name;
            _isExpanded = isExpanded;
            _icon = icon;
            Nodes = new ObservableCollection<Node>(children);

            if (resources != null)
            {
                Resources = resources;
                Resources.PropertyChanged += ResourcesOnPropertyChanged;
                _propChangedName = new ActionDeferrer(() => Notify("Name"), TimeSpan.FromSeconds(0.5), Dispatcher.CurrentDispatcher);
            }
        }

        public Node(string name, bool isExpanded, BitmapImage icon, IObservableCollection<ResourceViewModel> resources = null, IObservableCollection<Node> children = null)
        {
            _name = name;
            _isExpanded = isExpanded;
            _icon = icon;
            Nodes = children ?? new ObservableCollection<Node>();

            if (resources != null)
            {
                Resources = resources;
                Resources.PropertyChanged += ResourcesOnPropertyChanged;
                _propChangedName = new ActionDeferrer(() => Notify("Name"), TimeSpan.FromSeconds(0.5), Dispatcher.CurrentDispatcher);
            }
        }

        private void ResourcesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Count":
                    //Notify("Name");
                    _propChangedName.Raise();
                    break;
            }
        }

        public string Name { get { return Resources == null ? _name : _name + " (" + Resources.Count + ")"; } }
        public IObservableCollection<Node> Nodes { get; private set; }
        public IObservableCollection<ResourceViewModel> Resources { get; private set; }
        
        private BitmapImage _icon;
        public BitmapImage Icon
        {
            get { return _icon; }
            set { if (_icon != value) { _icon = value; Notify("Icon"); } }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { if (_isSelected != value) { _isSelected = value; Notify("IsSelected"); } }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { if (_isExpanded != value) { _isExpanded = value; Notify("IsExpanded"); } }
        }

        public void Dispose()
        {
            _propChangedName = null;
            Nodes.PropertyChanged -= ResourcesOnPropertyChanged;
        }
    }
}
