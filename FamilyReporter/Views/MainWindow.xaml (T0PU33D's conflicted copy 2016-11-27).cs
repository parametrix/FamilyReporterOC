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
        ContentControl m_externalView;

        private RequestHandler m_Handler;
        private ExternalEvent m_ExEvent;

        string m_projectWebPage;

        int _currentNumberOfLoadingFamily;


        private DocumentViewModel _selDocViewModel;
        private ImportInstanceViewModel _selImportViewModel;

        public MainWindow(ExternalEvent exEvent, RequestHandler handler)
        {

            m_Handler = handler;
            m_ExEvent = exEvent;

            InitializeComponent();

            m_projectWebPage = AppData.ContextualHelpUrl;

            _currentNumberOfLoadingFamily = 0;

            UIEventApp.m_ViewModel = new ProjectViewModel(UIEventApp.DbDoc);
            
            base.DataContext = UIEventApp.m_ViewModel;

            changeCtrl.Visibility = UIEventApp.m_ViewModel.LoadCompleteVisibility;

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
                                                        .ToList();

            pBar.IsIndeterminate = false;
            pBar.Maximum = levelOneFamilies.Count<Family>();

            LastOperations = new List<DispatcherOperation>();
            foreach (Family family in levelOneFamilies)
            {
                LastOperations.Add(LoadFamilyItemsDispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(() =>
                {
                    projectDocViewModel.LoadFamily(family, projectDocViewModel);
                    (m_externalView as TreeViewCtrl).ExpandProjectTreeViewItems();
                    //ExpandProjectTreeViewItems();
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
            _currentNumberOfLoadingFamily++;
            //tBxNumFamilies.Text = _currentNumberOfLoadingFamily.ToString() + " of " + pBar.Maximum.ToString() + " Families Loaded";
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
        }

        private void btnExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string csvFilePath = FileIOUtils.CreateCSVFile();
                if (csvFilePath != string.Empty)
                {
                    TaskDialog.Show("CSV File Created", string.Format("File Successfully Created in the following location: {0}.csv", csvFilePath));
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
        }

        private void changeCtrl_Click(object sender, RoutedEventArgs e)
        {
            if (changeCtrl.IsChecked==false)
            {
                changeCtrl.Content = "Show Table View";
            }
            else
            {
                changeCtrl.Content = "Show Tree View";
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
            string queryString = "\"" + @"http://spicetools.co/AppHelpDocs/html/getHelp.php?productId=" + @"ADSK610200205" + "\"";
            ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe", queryString);
            Process.Start(startInfo);
        }

        private void MainCtrl_Unloaded(object sender, RoutedEventArgs e)
        {
            StopLoadingFamilies();
            this.CloseWindow();
        }

        
    }
}
