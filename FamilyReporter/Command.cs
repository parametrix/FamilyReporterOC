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
#endregion

namespace FamilyReporter
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class Command : Autodesk.Revit.UI.IExternalCommand
    {
        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            #region PROCEED WITH APP

            UIApplication uiApp = commandData.Application;

            UIEventApp.UiDoc = uiApp.ActiveUIDocument;
            UIEventApp.DbDoc = uiApp.ActiveUIDocument.Document;


            ElementClassFilter familyFilter = new ElementClassFilter(typeof(Family));
            FilteredElementCollector familyCollector = new FilteredElementCollector(UIEventApp.DbDoc).WherePasses(familyFilter);

            ElementClassFilter importFilter = new ElementClassFilter(typeof(ImportInstance));
            FilteredElementCollector importCollector = new FilteredElementCollector(UIEventApp.DbDoc).WherePasses(importFilter);

            TaskDialog td = new TaskDialog("Proceed?");
            td.Title = "Do You Want To Proceed?";
            string numFamilies = familyCollector.Count().ToString();
            string numImports = importCollector.Count().ToString();
            td.MainInstruction = string.Format("{0}  Families and {1} Import Instances (excluding nested Families and Imports) were found in {2} Project", numFamilies, numImports, UIEventApp.DbDoc.Title);

            td.MainContent = "Depending on the Size of the project and the speed of your computer, "
                                    + "this operation will consume a considerable amount of disk space. "
                                    + "Please ensure that you have sufficient space in your Temporary Directory. "
                                    + "What do you want to do?";

            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Continue", "Only top-level families (visible to the project) are probed");
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Continue [Deep Scan]", "This process takes longer as all nested families are probed");
            td.FooterText = "Note: Sizes of In-place Families are not computed.";
            td.CommonButtons = TaskDialogCommonButtons.Cancel;

            var tdResult = td.Show();
            if (tdResult == TaskDialogResult.Cancel)
            {
                return Result.Cancelled;
            }


            // 1001 for CommandLink1
            // 1002 for CommandLink2
            // set flag for top level families
            uiApp.Application.FailuresProcessing += Application_FailuresProcessing;
            if (tdResult.GetHashCode() == 1001)
            {
                UIEventApp.m_ProbleOnlyTopLevelFamilies = true;
            }
            try
            {
                UIEventApp.ShowWindow(uiApp);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            finally
            {
                //uiApp.Application.FailuresProcessing -= Application_FailuresProcessing;
            }


            #endregion
            
        }

        /// <summary>
        /// https://forums.autodesk.com/t5/revit-api-forum/delete-warning-worksharingfailures-duplicatenameschanged-when/td-p/6873330
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            FailuresAccessor fa = e.GetFailuresAccessor();
            IList<FailureMessageAccessor> failList = new List<FailureMessageAccessor>();
            failList = fa.GetFailureMessages(); // Inside event handler, get all warnings
            if (failList.Count < 1) { return; }
            fa.DeleteAllWarnings();
            foreach (FailureMessageAccessor failure in failList)
            {

                // check FailureDefinitionIds against ones that you want to dismiss, FailureDefinitionId failID = failure.GetFailureDefinitionId();
                // prevent Revit from showing Unenclosed room warnings
                FailureDefinitionId failID = failure.GetFailureDefinitionId();
                //if (failID == BuiltInFailures.WorksharingFailures.DuplicateNamesChanged)
                //{
                //    fa.DeleteWarning(failure);
                //}
                if (failID.Guid == Guid.Parse("{1cc7cec7-c708-4346-a7df-6218c3485271}"))
                {
                    // this is newly created model - warn user
                    TaskDialog.Show("Error", "Newly created file. Please save the file and run this tool again.");
                    UIEventApp.thisApp.CloseWindow();
                }
            }
        }
    }
}
