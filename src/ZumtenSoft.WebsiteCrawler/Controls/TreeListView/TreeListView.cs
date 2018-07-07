using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ZumtenSoft.WebsiteCrawler.Controls.TreeListView
{
    public class TreeListView : TreeView
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

        private GridViewColumnCollection _columns;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public GridViewColumnCollection Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = new GridViewColumnCollection();
                    //_columns.SetProperty("Owner", this);
                    //_columns.Owner = this;
                    //_columns.InViewMode = true;
                }
                return _columns;
            }
        }
    }

    public class TreeListViewItem : TreeViewItem
    {
        public ItemsControl ParentTree
        {
            get
            {
                DependencyObject parent = VisualTreeHelper.GetParent(this);
                while (!(parent is TreeView))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                return parent as ItemsControl;
            }
        }

        /// <summary>
        /// Item's hierarchy in the tree
        /// </summary>
        public int Level
        {
            get
            {
                if (_level == -1)
                {
                    TreeListViewItem parent =
                        ItemsControl.ItemsControlFromItemContainer(this)
                            as TreeListViewItem;
                    _level = (parent != null) ? parent.Level + 1 : 0;
                }
                return _level;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

        private int _level = -1;
    }

}
