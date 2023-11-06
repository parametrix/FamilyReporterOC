using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml;

namespace FamilyReporter.Utils
{
    /// <summary>
    /// based on: https://www.youtube.com/watch?v=XnMgjsizad8
    /// </summary>
    class CheckForUpdates
    {
        const string _xmlPath = @"https://s3.amazonaws.com/spicetools/updates.xml";
        Version newVersion;
        string downloadUrl;

        public CheckForUpdates()
        {
            try
            {
                // check when the last check time
                var lastCheckedTime = FamilyReporter.user_settings.Default.lastUpdateCheck;
                if (null != lastCheckedTime)
                {
                    if (lastCheckedTime.AddDays(7) > DateTime.Now) // less than seven days since last check
                    {
                        return;
                    }
                }
                // else it is going to make one check now
                FamilyReporter.user_settings.Default.lastUpdateCheck = DateTime.Now;
                FamilyReporter.user_settings.Default.Save();
            }
            catch { }
            

            XmlTextReader reader = null;

            string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            try
            {
                reader = new XmlTextReader(_xmlPath);
                reader.MoveToContent();
                string elementName = "";
                

                if((reader.NodeType==XmlNodeType.Element) && (reader.Name==assemblyName))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            elementName = reader.Name;
                        }
                        else
                        {
                            if((reader.NodeType==XmlNodeType.Text) && (reader.HasValue))
                            {
                                switch (elementName)
                                {
                                    case "version":
                                        newVersion = new Version(reader.Value);
                                        break;
                                    case "url":
                                        downloadUrl = reader.Value;
                                        break;
                                }

                            }
                        }
                    }
                }
            }
            catch
            {

            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            // open page
            if (null == downloadUrl)
            {
                return;
            }

            Version applicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            //TaskDialog.Show("Check", $"{applicationVersion} \n {newVersion}");

            if (applicationVersion.CompareTo(newVersion) < 0)
            {
                // else ask user if he or she wants to update
                //DialogResult dialogResult = System.Windows.Forms.MessageBox.Show($"An update to {assemblyName} is available. Would you like to download?", "Update", MessageBoxButtons.YesNo);
                //if (dialogResult == DialogResult.No)
                //{
                //    return;
                //}

                // using task dialog
                // from: https://stackoverflow.com/questions/28829232/prompt-user-to-answer-boolean-choice-using-revit-api-in-c-sharp
                TaskDialog td = new TaskDialog("Update Available");
                td.MainContent = $"An update to {assemblyName} is available. Would you like to download?";
                td.AllowCancellation = false;
                td.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;

                TaskDialogResult result = td.Show();
                if (result == TaskDialogResult.Yes)
                {
                    // else open the webpage
                    System.Diagnostics.Process.Start(downloadUrl);
                }

            }
        }

    }
}
