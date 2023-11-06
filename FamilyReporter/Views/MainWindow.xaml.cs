using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace FamilyReporter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UserControl
    {
        public ContentControl m_externalView { get; set; }
        public ContentControl m_propertyView { get; set; }
        ProjectViewModel viewModel;

        private RequestHandler m_Handler;
        private ExternalEvent m_ExEvent;

        string m_projectWebPage;

        public int m_currentNumberOfLoadingFamily { get; set; }

        public string m_currentlyLoadingFamily { get; set; }

        public MainWindow(ExternalEvent exEvent, RequestHandler handler)
        {

            m_Handler = handler;
            m_ExEvent = exEvent;

            InitializeComponent();

            m_projectWebPage = AppData.ContextualHelpUrl;

            m_currentNumberOfLoadingFamily = 0;

            UIEventApp.m_ViewModel = viewModel = new ProjectViewModel(UIEventApp.DbDoc);
            
            base.DataContext = UIEventApp.m_ViewModel;

            changeCtrl.Visibility = System.Windows.Visibility.Hidden;

            var treeview = m_externalView = new TreeViewCtrl(m_ExEvent, m_Handler);
            m_externalView.DataContext = UIEventApp.m_ViewModel;
            this.ControlCanvas.Content = m_externalView;

            // event handler to activate combobox after the dispatcher queue is complete
            // from: http://stackoverflow.com/questions/23129528/process-dispatcher-queue-background-threads-with-one-call
            LoadFamilyItemsDispatcher.Hooks.DispatcherInactive += DispatcherInactiveHandler;

            this.Unloaded += MainCtrl_Unloaded;

            LoadLevelOneNodes();
        }

        private void CloseWindow()
        {
            //var window = Window.GetWindow(this);
            //window.Close();
            if (UIEventApp.m_window != null)
            {
                UIEventApp.m_window.Close();
                UIEventApp.m_window = null;
            }
        }


        public void SetGridControl()
        {
            UIEventApp.m_ViewModel.FamilyCollectionView = new FamilyCollectionViewModel(UIEventApp.m_ViewModel);
            m_externalView = new DataGridCtrl(viewModel, m_ExEvent, m_Handler);
            m_externalView.DataContext = UIEventApp.m_ViewModel.FamilyCollectionView;
            this.ControlCanvas.Content = m_externalView;
        }

        private void SetTreeControl()
        {
            m_externalView = new TreeViewCtrl(m_ExEvent, m_Handler);
            m_externalView.DataContext = UIEventApp.m_ViewModel;
            this.ControlCanvas.Content = m_externalView;
        }


        #region PROGRESS BAR FUNCTIONS AND PROPERTIES/MEMBERS

        private void DispatcherInactiveHandler(object sender, EventArgs e)
        {
            LoadFamilyItemsDispatcher.Hooks.DispatcherInactive -= DispatcherInactiveHandler;
        }

        private void LoadLevelOneNodes()
        {
            DocumentViewModel projectDocViewModel = UIEventApp.m_ViewModel.DocumentViewModels
                                                                    .OfType<DocumentViewModel>()
                                                                    .FirstOrDefault();

            // Load Imports up Front - they are not included in the Progress Bar Calcs
            projectDocViewModel.LoadImports();

            List<Family> levelOneFamilies = projectDocViewModel
                                                        .GetFamiliesInProject()
                                                        .Cast<Family>()
                                                        .OrderBy(x=>x.Name)
                                                        .ToList();

            pBar.IsIndeterminate = false;
            pBar.Maximum = levelOneFamilies.Count<Family>();

            LastOperations = new List<DispatcherOperation>();
            foreach (Family family in levelOneFamilies)
            {
                LastOperations.Add(LoadFamilyItemsDispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(() =>
                {
                    m_currentlyLoadingFamily = family.Name;
                    projectDocViewModel.LoadFamily(family, projectDocViewModel);
                    (m_externalView as TreeViewCtrl).ExpandProjectTreeViewItems();
                    pBar.Dispatcher.Invoke(new ProgressBarDelegate(UpdateProgress), System.Windows.Threading.DispatcherPriority.Background);
                })));
            }
        }


        public Dispatcher LoadFamilyItemsDispatcher
        {
            get { return Dispatcher.CurrentDispatcher; }
        }

        private ICollection<DispatcherOperation> LastOperations { get; set; }

        private void StopLoadingFamilies()
        {
            foreach (var operation in LastOperations)
            {
                operation.Abort();
            }
            LastOperations.Clear();
        }

        // using dispatcher for progress bar
        // from: http://matteocominetti.com/update-wpf-progessbar-in-revit-loop/
        private delegate void ProgressBarDelegate();

        private void UpdateProgress()
        {
            pBar.Value++;
            m_currentNumberOfLoadingFamily++;
            //tBxNumFamilies.Text = _currentNumberOfLoadingFamily.ToString() + @"/" + pBar.Maximum.ToString() + String.Format(" Loading: {0}", _currentlyLoadingFamily);
            txtBxNumberLoaded.Text = m_currentNumberOfLoadingFamily.ToString() + @"/" + pBar.Maximum.ToString();
            txtBxLoadingFamily.Text = m_currentlyLoadingFamily;
            // this is run after loading is complete
            if (pBar.Maximum == m_currentNumberOfLoadingFamily)
            {
                changeCtrl.Visibility = System.Windows.Visibility.Visible;
            }
        }


        #endregion



        

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.Close();
        }

        private void btnCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            StopLoadingFamilies();
            changeCtrl.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string csvFilePath = FileIOUtils.CreateCSVFile(UIEventApp.m_ProjectViewModel.DocDirectory);
                if (csvFilePath != string.Empty)
                {
                    TaskDialog.Show("CSV File Created", string.Format("File Successfully Created in the following location: {0}.csv", csvFilePath));
                    FileIOUtils.OpenFolderAndHightlightFile(csvFilePath);
                }
                else
                {
                    TaskDialog.Show("File Not Created", "File could not be written, possibly due to user permission restriction");
                }
            }

            // catch all exceptions as I don't know which exception is triggering the problem
            catch (Exception ex)
            {
                TaskDialog.Show("Error", String.Format("Could Not Write to this directory. Please try again and select another directory. \n {0}", ex.Message));
            }

            // bring window up on top
            var window = Window.GetWindow(this);
            BringWindowToFront.BringToFront(window.Title);
            
        }

        private void changeCtrl_Click(object sender, RoutedEventArgs e)
        {
            if (m_externalView.GetType() == typeof(DataGridCtrl))
            {
                SetTreeControl();
            }
            else
            {
                SetGridControl();
            }
        }

        private void helpMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }

        private void helpItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(AppData.ContextualHelpUrl);
        }

        private void MainCtrl_Unloaded(object sender, RoutedEventArgs e)
        {
            StopLoadingFamilies();
            this.CloseWindow();
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

        private void btnOpenDoc_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectedItem.OpenDocumentCmd.Execute(viewModel.SelectedItem);
        }

        private void PurgeImportedStylesInDocCmd_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectedItem.PurgeAllImportedStylesInDocCmd.Execute(viewModel.SelectedItem);
            MakeRequest(RequestId.DeleteAllImportStyles);
        }

        private void DeleteFamily_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectedItem.DeleteDocumentCmd.Execute(viewModel.SelectedItem);
            MakeRequest(RequestId.DeleteSelectedFamily);
        }

        private void DeleteChkdImportsInDoc_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectedItem.DeleteChkdImportCmd.Execute(viewModel.SelectedItem);
            MakeRequest(RequestId.DeleteChkdImports);
        }

        private void DeleteSelImportsInDoc_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectedItem.DeleteImportCmd.Execute(viewModel.SelectedItem);
            MakeRequest(RequestId.DeleteImport);
        }

        private void DeleteAllStyles_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectedItem.PurgeAllImportedStylesInImportCmd.Execute(viewModel.SelectedItem);
            MakeRequest(RequestId.DeleteAllImportStylesInImport);
        }

        private void DeleteChkdStyles_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectedItem.PurgeSelImportedStylesInImportCmd.Execute(viewModel.SelectedItem);
            MakeRequest(RequestId.DeleteElements);
            //MakeRequest(RequestId.DeleteChkdImports);
        }

        /// <summary>
        /// // from: http://stackoverflow.com/questions/16234522/scrollviewer-mouse-wheel-not-working
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void loadXml_Click(object sender, RoutedEventArgs e)
        {
            UIEventApp.m_xmlDataModel.AggregatePartAtoms();
        }
    }
}
