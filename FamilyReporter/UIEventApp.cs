#region NAMESPACES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;




using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Events;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using FamilyReporter.Utils;

#endregion

namespace FamilyReporter
{
    public class UIEventApp : IExternalApplication
    {
        // current database document. Based on :http://forums.autodesk.com/t5/revit-api/accessing-the-document-from-c-form-externalcommanddata-issue/m-p/4781583#M5488
        public static Autodesk.Revit.DB.Document DbDoc { get; set; }

        public static Autodesk.Revit.UI.UIDocument UiDoc { get; set; }

        // class instance
        public static UIEventApp thisApp = null;

        // modeless window instance
        internal static System.Windows.Window m_window;

        // view model
        internal static ProjectViewModel m_ViewModel;

        // passing property for request handler
        internal static List<ElementId> m_ElementIds { get; set; }

        internal static List<TreeViewItemBase> m_TreeNodes { get; set; }

        // passing documents for editing
        internal static List<string> m_DocumentPaths;

        // passing node for editing
        internal static TreeViewItemBase m_treeNode;

        // mothernode of all nodes - this document
        internal static DocumentViewModel m_ProjectViewModel;

        // project folder
        internal static DirectoryInfo m_ProjectDirectory { get; set; }

        // xml data store
        internal static XmlDataModel m_xmlDataModel { get; set; }

        // flag for probing only top-level families
        internal static bool m_ProbleOnlyTopLevelFamilies { get; set; }

        // variable to signal suppression of dialogs in Revit
        internal static bool m_SuppressDialogs { get; set; }




        #region REQUIRED FUNCTIONS

        public Result OnShutdown(UIControlledApplication application)
        {
            if (m_window != null)
            {
                m_window.Close();
                m_window = null;
            }

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            #region INTERFACE
            // dummy panel in case one already exists
            RibbonPanel toolPanel = null;
            PulldownButton pullDownButtonGroup = null;

            List<RibbonPanel> existingPanels = application.GetRibbonPanels();
            foreach (RibbonPanel panel in existingPanels)
            {
                if (panel.Name == AppData.PanelName)
                {
                    toolPanel = panel;
                    // check if pulldown button group exists in panel
                    IList<RibbonItem> panelItems = toolPanel.GetItems();
                    foreach (RibbonItem item in panelItems)
                    {
                        if (item.ItemType == RibbonItemType.PulldownButton && item.Name == AppData.PullDownButtonName)
                        {
                            pullDownButtonGroup = item as PulldownButton;
                            break;
                        }
                    }
                    break;
                }
            }
            if (null == toolPanel)
            {
                // create custom panel
                toolPanel = application.CreateRibbonPanel(AppData.PanelName);
            }

            // create pulldown group if one does not exist.
            if (null == pullDownButtonGroup)
            {
                PulldownButtonData groupData = new PulldownButtonData(AppData.PullDownButtonName, AppData.PullDownButtonName);
                groupData.LargeImage = AppInfrastructureUtils.BmpImageSource("FamilyReporter.Resources.Logo_v3_32.png", this);
                groupData.Image = AppInfrastructureUtils.BmpImageSource("FamilyReporter.Resources.Logo_v3_16.png", this);
                groupData.ToolTip = AppData.GroupButtonToolTip;

                pullDownButtonGroup = toolPanel.AddItem(groupData) as PulldownButton;
            }

            else // add separator in preparation for adding a new button
            {
                pullDownButtonGroup.AddSeparator();
            }

            PushButtonData loadButton = new PushButtonData(AppData.AppName, AppData.AppText, AppInfrastructureUtils.AssemblyFullName, AppData.Command01_Name);
            loadButton.LargeImage = AppInfrastructureUtils.BmpImageSource("FamilyReporter.Resources.familySizeReporter_ICON32.png", this); // function uses png decoder as opposed to BMP decoder cited in the example
            loadButton.Image = AppInfrastructureUtils.BmpImageSource("FamilyReporter.Resources.familySizeReporter_ICON16.png", this);
            loadButton.ToolTip = AppData.ToolTip;
            loadButton.Text = AppData.Btn01_Text;
            ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, AppData.ContextualHelpUrl);
            loadButton.SetContextualHelp(contextualHelp);
            pullDownButtonGroup.AddPushButton(loadButton);

            #endregion

            m_window = null; // no window needed yet. Command will bring it.

            // check for updates
            var checkForUpdates = new CheckForUpdates();

            return Result.Succeeded;
        }

        

        #endregion

        #region FORM UTILITIES

        public static void ShowWindow(UIApplication uiApp)
        {
            // if window does not exist, create window

            // to determine if window is disposed
            // from: http://stackoverflow.com/questions/5802906/how-to-determine-if-window-is-dispose

            if (null == m_window || PresentationSource.FromVisual(m_window).IsDisposed)
            {
                UIEventApp.UiDoc = uiApp.ActiveUIDocument;
                UIEventApp.DbDoc = uiApp.ActiveUIDocument.Document;


                // create a new handler to handle posting by control
                RequestHandler exEventHandler = new RequestHandler();

                // create event for control to post requests
                ExternalEvent exEvent = ExternalEvent.Create(exEventHandler);

                // create project directory
                m_ProjectDirectory = CreateProjectDirectory(UIEventApp.DbDoc);

                //MainCtrl mainControl = new MainCtrl(exEvent, exEventHandler);
                MainWindow mainControl = new MainWindow(exEvent, exEventHandler);
                UIEventApp.m_window = new System.Windows.Window();
                UIEventApp.m_window.Content = mainControl;
                UIEventApp.m_window.Width = 920;
                UIEventApp.m_window.Height = 620;
                UIEventApp.m_window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                UIEventApp.m_window.Title = AppData.MainWindowTitle;
                UIEventApp.m_window.ResizeMode = ResizeMode.CanResizeWithGrip;

                // the static method used earlier does not work.
                // only this method works to retrive image
                try
                {
                    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FamilyReporter.Resources.familySizeReporter_ICON16.png");
                    var decoder = new System.Windows.Media.Imaging.PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                    UIEventApp.m_window.Icon = decoder.Frames[0];
                }
                catch { }

                

                UIEventApp.m_window.Show();
            }
        }

        internal void WakeWindowUp()
        {
            if (null != m_window)
            {
                //UIEventApp.m_window.Activate();
            }
        }

        internal void CloseWindow()
        {
            if (m_window != null )
            {
                m_window.Close();
                m_window = null;
            }
        }

        #endregion


        #region GENERAL UTILITES

        internal static void RefreshDataGridWindow()
        {
            if (UIEventApp.m_window != null)
            {
                MainWindow mainWindow = UIEventApp.m_window.Content as MainWindow;
                if (mainWindow.m_externalView.GetType() == typeof(DataGridCtrl))
                {
                    mainWindow.SetGridControl();
                }
            }
        }


        #region FILEIO

        private static DirectoryInfo CreateProjectDirectory(Document doc)
        {

            try
            {
                string folderName = doc.Title + string.Format("_{0:yyyyMMdd_HH-mm-ss-tt}", DateTime.Now);
                string tempFolder = Path.GetTempPath();
                DirectoryInfo projectDirectory = System.IO.Directory.CreateDirectory(Path.Combine(tempFolder, folderName));
                DirectorySecurity dSecurity = projectDirectory.GetAccessControl();
                dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                projectDirectory.SetAccessControl(dSecurity);
                return projectDirectory;
            }
            catch
            {
                MessageBox.Show("Unable to create Project Folder. Please check your permissions or access to the Windows Temporary Directory", "Error");
            }
            return null;
        }

        #endregion



        #endregion

    }
}
