using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace FamilyReporter
{
    public class TaskDialogs
    {
        /// <summary>
        /// Simple Yes/No Dialog Presenter
        /// </summary>
        /// <param name="title"></param>
        /// <param name="bigText"></param>
        /// <param name="content"></param>
        /// <param name="footer"></param>
        public static TaskDialogResult TaskDialogYesNo(string title, string bigText, string smallText, string footer)
        {
            TaskDialog dialog = new TaskDialog(title);
            dialog.MainInstruction = bigText;
            dialog.MainContent = smallText;
            dialog.FooterText = footer;

            dialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            dialog.DefaultButton = TaskDialogResult.No;

            return dialog.Show();
        }
    }
}
