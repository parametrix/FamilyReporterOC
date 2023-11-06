using Autodesk.Revit.UI;
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

namespace FamilyReporter
{
    /// <summary>
    /// Interaction logic for TreeViewCtrl.xaml
    /// </summary>
    public partial class TreeViewCtrl : UserControl
    {
        private RequestHandler m_Handler;
        private ExternalEvent m_ExEvent;

        public TreeViewCtrl(ExternalEvent exEvent, RequestHandler handler)
        {
            InitializeComponent();

            m_ExEvent = exEvent;
            m_Handler = handler;
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

        internal void ExpandProjectTreeViewItems()
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

        private void PurgeImportedStylesInDocCmd_Click(object sender, RoutedEventArgs e)
        {
            MakeRequest(RequestId.DeleteElements);
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
            MakeRequest(RequestId.DeleteElements);
        }

        private void DeleteImportCmd_Click(object sender, RoutedEventArgs e)
        {
            MakeRequest(RequestId.DeleteImport);
        }

        private void chkBxImportedObjectStyle_Checked(object sender, RoutedEventArgs e)
        {
            // blank
        }
    }
}
