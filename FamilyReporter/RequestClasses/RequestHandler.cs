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
    public sealed class RequestHandler : Autodesk.Revit.UI.IExternalEventHandler
    {
        string IExternalEventHandler.GetName()
        {
            // UPDATE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            return "App Interaction Control";
        }

        private Request m_request = new Request();

        public Request Request
        { get { return m_request; } }

        void IExternalEventHandler.Execute(UIApplication app)
        {
            try
            {
                switch (Request.Take())
                {
                    case RequestId.None:
                        {
                            return; // no pending request
                        }
                    case RequestId.DeleteElements:
                        {
                            RequestHandlerFunctions.DeleteElements(app);
                            return;
                        }
                    case RequestId.DeleteImport:
                        {
                            RequestHandlerFunctions.DeleteImport(app);
                            return;
                        }
                    case RequestId.DeleteSelectedFamily:
                        {
                            RequestHandlerFunctions.DeleteFamilyDocInProject(app);
                            return;
                        }
                    case RequestId.DeepPurgeDocument:
                        {
                            RequestHandlerFunctions.DeleteSelDocuments(app);
                            return;
                        }
                    case RequestId.DeleteChkdImports:
                        {
                            RequestHandlerFunctions.TraverseTreeAndDeleteSelImports(app);
                            return;
                        }
                    case RequestId.DeleteAllImports:
                        {
                            RequestHandlerFunctions.DeleteAllImportsInDocument(app);
                            return;
                        }
                    case RequestId.DeleteAllImportStyles:
                        {
                            RequestHandlerFunctions.DeleteAllImportsStylesInDocument(app);
                            return;
                        }
                    case RequestId.DeleteAllImportStylesInImport:
                        {
                            RequestHandlerFunctions.DeleteAllImportStylesInImport(app);
                            return;
                        }
                    default:
                        {
                            // warn of unexpected results?
                            break;
                        }
                }
            }
            finally
            {
                //UIEventApp.thisApp.WakeWindowUp();
            }
            return;
        }
    }
}
