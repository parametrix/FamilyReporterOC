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
    public enum RequestId : int
    {
        None = 0,
        DeleteElements = 1,
        DeleteImport = 3,
        DeleteSelectedFamily = 4,
        DeepPurgeDocument = 5,
        DeleteSelDocuments = 6,
        DeleteAllImports = 7,
        DeleteChkdImports = 8,
        DeleteAllImportStyles = 9,
        DeleteAllImportStylesInImport = 10
    }
}
