using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.UI;

namespace FamilyReporter
{
    /// <summary>
    /// Interaction logic for TreeViewCtrl.xaml
    /// </summary>
    public partial class TreeViewCtrl : UserControl
    {
        private ExternalEvent m_ExEvent;
        private RequestHandler m_Handler;


        public TreeViewCtrl(ExternalEvent exEvent, RequestHandler handler)
        {
            InitializeComponent();

            this.m_ExEvent = exEvent;
            this.m_Handler = handler;
        }

        #region REQUEST FUNCTIONS

        private void MakeRequest(RequestId request)
        {
            m_Handler.Request.Make(request);
            m_ExEvent.Raise();
            //DozeOff();
        }

        private void DozeOff()
        {
            //EnableCommands(false);
            //treeViewCtrl.IsEnabled = false;
        }

        // not required as this is controlled from view model
        //public void WakeUp()
        //{
        //    //EnableCommands(true);
        //    treeViewCtrl.IsEnabled = true;
        //}

        #endregion

        #region TREEVIEW EXPAND
        private void btnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            // from: http://stackoverflow.com/questions/834081/wpf-treeview-where-is-the-expandall-method
            foreach (object item in trView.Items)
            {
                TreeViewItem treeItem = trView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeItem != null)
                {
                    treeItem.IsExpanded = true;
                    ExpandAll(treeItem, true);
                }

            }
        }

        private void ExpandAll(ItemsControl items, bool expand)
        {
            foreach (object obj in items.Items)
            {
                ItemsControl childControl = items.ItemContainerGenerator.ContainerFromItem(obj) as ItemsControl;
                if (childControl != null)
                {
                    ExpandAll(childControl, expand);

                    TreeViewItem item = childControl as TreeViewItem;
                    if (item != null)
                    {
                        item.IsExpanded = true;
                    }
                }

            }

        }

        public void ExpandProjectTreeViewItems()
        {
            // from: http://stackoverflow.com/questions/834081/wpf-treeview-where-is-the-expandall-method
            foreach (object item in trView.Items)
            {
                TreeViewItem treeItem = trView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeItem != null)
                {
                    ExpandProjectTree(treeItem, true);
                }
                treeItem.IsExpanded = true;
            }
        }

        private void ExpandProjectTree(ItemsControl items, bool expand)
        {
            foreach (object obj in items.Items)
            {
                ItemsControl childControl = items.ItemContainerGenerator.ContainerFromItem(obj) as ItemsControl;
                if (childControl != null && (childControl as TreeViewItem).GetType() == typeof(DocumentViewModel))
                {
                    ExpandProjectTree(childControl, expand);

                    TreeViewItem item = childControl as TreeViewItem;
                    if (item != null)
                    {
                        item.IsExpanded = true;
                    }
                }

            }

        }

        #endregion

        private void PurgeImportedStylesInDocCmd_Click(object sender, RoutedEventArgs e)
        {
            //MakeRequest(RequestId.DeleteElements);
            MakeRequest(RequestId.DeleteAllImportStyles);
        }

        private void DeleteSelectedDocument_Click(object sender, RoutedEventArgs e)
        {
            MakeRequest(RequestId.DeleteSelectedFamily);
        }

        private void PurgeSelectedObjectStyles_Click(object sender, RoutedEventArgs e)
        {
            // request has to be sent from there after element ids are transferred over
            // using Command
            MakeRequest(RequestId.DeleteElements);
        }

        private void PurgeAllObjectStyles_Click(object sender, RoutedEventArgs e)
        {
            //MakeRequest(RequestId.DeleteElements);
            MakeRequest(RequestId.DeleteAllImportStylesInImport);
        }

        private void DeleteImportCmd_Click(object sender, RoutedEventArgs e)
        {
            MakeRequest(RequestId.DeleteImport);
        }

        private void chkBxImportedObjectStyle_Checked(object sender, RoutedEventArgs e)
        {
            MakeRequest(RequestId.DeleteChkdImports);
        }

        private void DeleteChkdImport_Click(object sender, RoutedEventArgs e)
        {
            MakeRequest(RequestId.DeleteChkdImports);
        }

        #region TREE VIEW EXPANSION CONTROLS

        //private void SetTreeViewIsExpanded(bool isExpanded)
        //{
        //    if (trView.HasItems)
        //    {
        //        foreach (var item in trView.Items)
        //        {
        //            TreeViewItemBase treeNode = item as TreeViewItemBase;
        //            ExpandChildren(treeNode, isExpanded);
        //        }
        //    }
        //}

        /// <summary>
        /// Expands tree of only documents
        /// </summary>
        /// <param name="treeViewItem"></param>
        /// <param name="isExpanded"></param>
        private void ExpandDocumentChildren(TreeViewItemBase treeViewItem, bool isExpanded)
        {
            treeViewItem.IsExpanded = isExpanded;
            if (treeViewItem.Children.Count > 0)
            {
                foreach (TreeViewItemBase child in treeViewItem.Children)
                {
                    ExpandDocumentChildren(child, isExpanded);
                }
            }
        }

        #endregion

        private void trView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            UIEventApp.m_ViewModel.SelectedItem = (TreeViewItemBase)e.NewValue;

            if (UIEventApp.m_ViewModel.SelectedItem.GetType() != typeof(DocumentViewModel))
            {
                return;
            }
            DocumentViewModel docVm = UIEventApp.m_ViewModel.SelectedItem as DocumentViewModel;

            if (docVm.IsProjectDoc)
            {
                return;
            }

            string xmlPath = System.IO.Path.ChangeExtension(docVm.DocumentItem.FilePath, ".xml");

            // show family properties
            // find the content control and assign the user control to it.
            // from: http://stackoverflow.com/questions/26433985/how-to-replace-the-content-of-the-parent-control-from-child-user-control-in-code
            ContentControl parentCtrl = (ContentControl)this.Parent;
            Border border = (Border)parentCtrl.Parent;
            Grid grid = (Grid)border.Parent;
            Grid parentGrid = (Grid)grid.Parent;
            MainWindow mainWindow = (MainWindow)parentGrid.Parent;
            ContentControl propertyContentCtrl = mainWindow.PropertyCanvas;

            ContentControl newCtrl = new FamilyPropertiesCtrl(xmlPath);
            newCtrl.DataContext = mainWindow.DataContext;
            propertyContentCtrl.Content = newCtrl;
        }
    }
}
