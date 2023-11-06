using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using System.Security;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Diagnostics;

namespace FamilyReporter
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        internal static extern uint GetCompressedFileSizeW([In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
           [Out, MarshalAs(UnmanagedType.U4)] out uint lpFileSizeHigh);

        [DllImport("kernel32.dll", SetLastError = true, PreserveSig = true)]
        internal static extern int GetDiskFreeSpaceW([In, MarshalAs(UnmanagedType.LPWStr)] string lpRootPathName,
           out uint lpSectorsPerCluster, out uint lpBytesPerSector, out uint lpNumberOfFreeClusters,
           out uint lpTotalNumberOfClusters);
    }

    internal class FileIOUtils
    {
        internal static void OpenFolderAndHightlightFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                Process.Start("explorer.exe", "/select," + filePath);
            }
            else
            {
                System.Windows.MessageBox.Show("Sorry, the file was not created.", "Error");
            }
        }

        internal static long GetFileSizeOnDisk(string filePath)
        {
            // deal w/ Cloud Models

            FileInfo info = new FileInfo(filePath);
            uint dummy, sectorsPerCluster, bytesPerSector;
            int result = NativeMethods.GetDiskFreeSpaceW(info.Directory.Root.FullName, out sectorsPerCluster, out bytesPerSector, out dummy, out dummy);
            if (result == 0) throw new Win32Exception();
            uint clusterSize = sectorsPerCluster * bytesPerSector;
            uint hosize;
            uint losize = NativeMethods.GetCompressedFileSizeW(filePath, out hosize);
            long size;
            size = (long)hosize << 32 | losize;

            long sizeInBytes = ((size + clusterSize - 1) / clusterSize) * clusterSize;

            return ((size + clusterSize - 1) / clusterSize) * clusterSize;
        }

        // clean inputs
        // from: https://msdn.microsoft.com/en-us/library/844skk0h%28v=vs.110%29.aspx
        // The regular expression pattern [^\w\.@-] matches any character that is not a word character, a period, an @ symbol, or a hyphen. 
        // A word character is any letter, decimal digit, or punctuation connector such as an underscore.
        internal static string RemoveInvalidCharacters(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^\w\.@-]", "");
                //return Regex.Replace(strIn, @"[^\w\.@-]", "", RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters, 
            // we should return Empty.
            catch
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Retrieves DocInfo for JSON translation
        /// </summary>
        /// <param name="docViewModel"></param>
        /// <returns></returns>
        internal static DocumentInfo CreateDocumentInfoFromDocumentViewModel(TreeViewItemBase treeViewModel)
        {
            DocumentInfo docInfo;
            if(null==treeViewModel)
            {
                return null;
            }
            if (treeViewModel.GetType() == typeof(DocumentViewModel))
            {
                DocumentViewModel docViewModel = treeViewModel as DocumentViewModel;
                string parentNodeName = docViewModel.ParentDocumentViewModel == null ? "" : docViewModel.ParentDocumentViewModel.ItemName;
                docInfo = new DocumentInfo()
                {
                    ParentNodeName = parentNodeName,
                    DocumentCategory = docViewModel.DocumentItem.CategoryName,
                    DocumentName = docViewModel.ItemName,
                    NumberOfInstances = docViewModel.DocumentItem.DocumentInstanceCount.ToString(),
                    SizeInKb = docViewModel.DocumentItem.DocumentSize.ToString()
                };
            }
            else
            {
                ImportInstanceViewModel importViewModel = treeViewModel as ImportInstanceViewModel;
                string parentNodeName = importViewModel.Parent == null ? "" : importViewModel.Parent.ItemName;
                docInfo = new DocumentInfo()
                {
                    ParentNodeName = parentNodeName,
                    DocumentCategory = "Import",
                    DocumentName = importViewModel.ItemName,
                    NumberOfInstances = importViewModel.ImportCategoryItem.InstanceCount.ToString(),
                    SizeInKb = "N/A"
                };
            }

            return docInfo;
        }

        /// <summary>
        /// Creates CSV in same folder as document
        /// </summary>
        /// <returns></returns>
        internal static string CreateCSVFile(DirectoryInfo projectDocDirectory)
        {

            //  ask user to select path  
            string csvFileName = UIEventApp.DbDoc.Title;
            //FolderBrowserDialog dialog = new FolderBrowserDialog();
            //if (dialog.ShowDialog() == DialogResult.OK)
            //{
            //    csvFilePath = Path.Combine(dialog.SelectedPath, csvFileName);
            //}

            string csvFilePath = Path.Combine(projectDocDirectory.FullName, csvFileName);
            csvFilePath = Path.ChangeExtension(csvFilePath, ".csv");

            if (File.Exists(csvFilePath))
            {
                DialogResult dlgResult = System.Windows.Forms.MessageBox.Show("A File with the same name already exists at this location. Do you want to replace it?", "Warning", MessageBoxButtons.YesNo);
                if (dlgResult == DialogResult.Yes)
                {
                    File.Delete(csvFilePath);
                }
                else
                {
                    return csvFilePath;
                }
            }


            CsvDestination destination = new CsvDestination(csvFilePath);
            using (var csvFile = new CsvFile<DocumentInfo>(destination))
            {
                List<DocumentInfo> docInfoList = new List<DocumentInfo>();

                foreach (TreeViewItemBase treeViewModel in UIEventApp.m_ViewModel.DocumentViewModels)
                {

                    docInfoList.AddRange(AppendNestedNodeData(treeViewModel));
                }

                foreach(DocumentInfo docInfo in docInfoList)
                {
                    if (docInfo == null)
                    {
                        continue;
                    }
                    csvFile.Append(docInfo);
                }
            }
            return csvFilePath;
        }

        /// <summary>
        /// Recursive function to extract doc info from nodes
        /// </summary>
        /// <param name="treeViewModel"></param>
        /// <returns></returns>
        private static List<DocumentInfo> AppendNestedNodeData(TreeViewItemBase treeViewModel)
        {
            List<DocumentInfo> docInfoList = new List<DocumentInfo>();

            DocumentInfo docInfo = FileIOUtils.CreateDocumentInfoFromDocumentViewModel(treeViewModel);
            if (null != docInfo)
            {
                docInfoList.Add(docInfo);
            }

            if (null == treeViewModel.Parent && treeViewModel.Children.Count > 0)
            {
                foreach (var child in treeViewModel.Children)
                {
                    if(null!= child)
                    {
                        docInfoList.AddRange(AppendNestedNodeData(child));
                    }
                }
            }

            return docInfoList;
        }

        internal static void DeleteFamilyDocuments(DocumentViewModel projectDocViewModel)
        {
            foreach(var item in projectDocViewModel.Children)
            {
                if(item.GetType() == typeof(DocumentViewModel))
                {
                    DocumentViewModel viewModel = item as DocumentViewModel;
                    string filePath = viewModel.DocumentItem.FilePath;
                    string fileExtension = Path.GetExtension(filePath);

                    if(fileExtension == "rfa")
                    {
                        File.Delete(filePath);
                    }
                }
            }
        }
    }
}
