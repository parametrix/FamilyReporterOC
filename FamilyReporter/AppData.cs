using Autodesk.Revit.DB;
using System;

namespace FamilyReporter
{
    public class AppData
    {
        public static AddInId addinId = new AddInId(new Guid("C31AA4E9-F99C-4046-AD50-75303E897864"));
        public static string AppVersion = "v2.1.4";
        public static string AppVendorId = "STPL";
        public static string AppID = @"ADSK170200301";
        public static string AppName = "FamilyReporter";
        public static string AppText = "Family Reporter";
        public static string PanelName = "SPICETOOLS";
        public static string PanelText = PanelName;
        public static string PullDownButtonName = "RUTILITIES";
        public static string PullDownButtonText = PullDownButtonName;
        public static string Btn01_Text = "Family Reporter";
        public static string ContextualHelpUrl = @"https://spicetools.blogspot.com/p/family-reporter-200.html";
        public static string ToolTip = "Reports on Families, Nested Families, and Imports in Projects";
        public static string Command01_Name = "FamilyReporter.Command";
        public static string GroupButtonToolTip = "A Set of Useful Tools to Manage Your Project";
        public static string MainWindowTitle = AppText + " " + AppVersion;
    }
}
