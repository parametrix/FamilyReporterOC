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
    /// Interaction logic for DataGridCtrl.xaml
    /// </summary>
    public partial class DataGridCtrl : UserControl
    {
        private ExternalEvent m_ExEvent;
        private RequestHandler m_Handler;

        ProjectViewModel m_viewModel;
        
        public DataGridCtrl(ProjectViewModel viewModel, ExternalEvent exEvent, RequestHandler handler)
        {
            InitializeComponent();

            this.m_ExEvent = exEvent;
            this.m_Handler = handler;

            this.DataContext = m_viewModel = viewModel;
        }

        // from: http://stackoverflow.com/questions/16234522/scrollviewer-mouse-wheel-not-working
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // using this to set initial state
            this.expandAll.IsChecked = false;
            this.removeGroups.IsChecked = false;
        }

        private void searchBx_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox textBx = sender as System.Windows.Controls.TextBox;
            string text = textBx.Text;
            if (text != "")
            {
                expandAll.IsChecked = true;
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

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count<1)
            {
                return;
            }

            UIEventApp.m_ViewModel.SelectedItem = (TreeViewItemBase)e.AddedItems[0];

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
            if (parentCtrl == null)
            {
                return;
            }
            Border border = (Border)parentCtrl.Parent;
            Grid parentGrid = (Grid)border.Parent;
            Grid grid = (Grid)parentGrid.Parent;
            MainWindow mainWindow = (MainWindow)grid.Parent;
            ContentControl propertyContentCtrl = mainWindow.PropertyCanvas;

            ContentControl newCtrl = new FamilyPropertiesCtrl(xmlPath);
            newCtrl.DataContext = mainWindow.DataContext;
            propertyContentCtrl.Content = newCtrl;
        }
    }
}
