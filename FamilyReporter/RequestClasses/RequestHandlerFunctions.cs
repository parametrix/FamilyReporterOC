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
    internal class RequestHandlerFunctions
    {
        internal static void DeleteElements(UIApplication uiApp)
        {
            if(UIEventApp.m_ElementIds == null || UIEventApp.m_ElementIds.Count < 1)
            {
                return;
            }

            foreach (ElementId id in UIEventApp.m_ElementIds)
            {
                if (null == id || null== UIEventApp.DbDoc.GetElement(id))
                {
                    continue;
                }

                string elementName = "Element: ";
                try
                {
                    elementName += UIEventApp.DbDoc.GetElement(id).Name;
                }
                catch { }

                using (Transaction t = new Transaction(UIEventApp.DbDoc, string.Format("Deleting {0}", elementName)))
                {
                    t.Start();
                    UIEventApp.DbDoc.Delete(id);
                    t.Commit();
                }
            }
        }

        internal static void DeleteImport(UIApplication app)
        {
            Document familyDoc = null;

            // check if we are dealing with a nested document
            List<ElementId> transferredIds = UIEventApp.m_ElementIds;
            
            // incase of nested import - first element is null
            if(null==transferredIds.First() && transferredIds.Count > 1)
            {
                // remove null value
                transferredIds.RemoveAt(0);

                Autodesk.Revit.ApplicationServices.Application appSvcs = UIEventApp.DbDoc.Application;

                List<string> docPaths = UIEventApp.m_DocumentPaths;
                Document childDoc = appSvcs.OpenDocumentFile(docPaths.First());
                string childPath = DeleteElementsAndSaveDocument(childDoc, docPaths.First(), transferredIds, null);
                docPaths.Remove(docPaths.First());
                Document parentDoc;

                while (docPaths.Count > 0)
                {
                    parentDoc = appSvcs.OpenDocumentFile(docPaths.First());
                    LoadChildIntoParent(childPath, parentDoc);
                    childPath = docPaths.First();
                    docPaths.Remove(docPaths.First());
                    parentDoc.Close(true);
                }

                LoadChildIntoParent(childPath, UIEventApp.DbDoc);
                return;
            }

            // it is a level 1 or level 2 import
            if (null != transferredIds.First())
            {
                Element firstElement = UIEventApp.DbDoc.GetElement(transferredIds.FirstOrDefault());

                // if level 1 family
                if (firstElement.GetType() == typeof(Family))
                {
                    Family parentFamily = firstElement as Family;
                    familyDoc = UIEventApp.DbDoc.EditFamily(parentFamily);

                    // remove family document at first location
                    transferredIds.RemoveAt(0);
                }
            }

            if (null == familyDoc)
            {
                familyDoc = UIEventApp.DbDoc;
            }


            if (null != UIEventApp.m_ElementIds)
            {
                string filePath = DeleteElementsAndSaveDocument(familyDoc, null, UIEventApp.m_ElementIds, null);
                LoadChildIntoParent(filePath, UIEventApp.DbDoc);
            }

        }



        internal static void DeleteAllImportStylesInImport(UIApplication app)
        {
            ImportInstanceViewModel importViewModel = UIEventApp.m_treeNode as ImportInstanceViewModel;
            DocumentViewModel parentDocVm = importViewModel.ParentViewModel as DocumentViewModel;
            if (parentDocVm.IsProjectDoc)
            {
                ImportGeometryUtils.DeleteImportStylesInImport(app, UIEventApp.DbDoc, UIEventApp.DbDoc.GetElement(importViewModel.ImportCategoryItem.InstanceId) as ImportInstance);
                return;
            }

            // else open family, delete styles and reload
            else
            {
                Autodesk.Revit.ApplicationServices.Application appSvcs = UIEventApp.DbDoc.Application;
                Document familyDoc = appSvcs.OpenDocumentFile(parentDocVm.DocumentItem.FilePath);
                ImportGeometryUtils.DeleteImportStylesInImport(app, familyDoc, familyDoc.GetElement(importViewModel.ImportCategoryItem.InstanceId) as ImportInstance);
                familyDoc.Save();
                familyDoc.Close();

                // reload upstream
                ReloadFamilyUpstream(parentDocVm);
            }


        }

        internal static void DeleteAllImportsStylesInDocument(UIApplication app)
        {
            DocumentViewModel docVm = UIEventApp.m_treeNode as DocumentViewModel;
            Document doc = docVm.DocumentItem.Document;

            if (docVm.IsProjectDoc)
            {
                ImportGeometryUtils.DeleteImportStylesInDocument(app, doc);
            }
            // else open family, delete all import styles and reload
            // TODO: Additional actions for families
            else
            {
                Autodesk.Revit.ApplicationServices.Application appSvcs = UIEventApp.DbDoc.Application;
                // open the parent and delete the import
                Document familyDoc = appSvcs.OpenDocumentFile(docVm.DocumentItem.FilePath);
                ImportGeometryUtils.DeleteImportStylesInDocument(app, familyDoc);
                familyDoc.Save();
                familyDoc.Close();

                // reload upstream
                ReloadFamilyUpstream(docVm);
            }



        }

        internal static void TraverseTreeAndDeleteSelImports(UIApplication app)
        {
            Autodesk.Revit.ApplicationServices.Application appSvcs = UIEventApp.DbDoc.Application;

            List<TreeViewItemBase> importNodes = UIEventApp.m_TreeNodes;

            foreach(var item in importNodes)
            {
                ImportInstanceViewModel importVm = item as ImportInstanceViewModel;
                TreeViewItemBase parentNode = item.Parent;
                DocumentViewModel parentDoc = parentNode as DocumentViewModel;
                
                // if the parent is project, then simply delete it
                if (parentDoc.IsProjectDoc)
                {
                    DeleteElement(UIEventApp.DbDoc, importVm.ImportCategoryItem.InstanceId, item.ItemName);
                }

                else
                {
                    // open the parent and delete the import
                    Document doc = appSvcs.OpenDocumentFile(parentDoc.DocumentItem.FilePath);
                    DeleteElementsAndSaveDocument(doc, parentDoc.DocumentItem.FilePath, null, importVm.ImportCategoryItem.InstanceId);

                    // reload upstream
                    ReloadFamilyUpstream(parentDoc);
                }

            }
        }

        /// <summary>
        /// Reloads Upstream by tracking the parent nodes
        /// </summary>
        /// <param name="docVM"></param>
        private static void ReloadFamilyUpstream(DocumentViewModel docVM)
        {
            DocumentViewModel child = docVM;
            DocumentViewModel parent = docVM.ParentDocumentViewModel;
            LoadChildIntoParent(child.DocumentItem.FilePath, parent.DocumentItem.Document);

            while (parent.IsProjectDoc == false)
            {
                child = parent;
                parent = parent.ParentDocumentViewModel;
                LoadChildIntoParent(child.DocumentItem.FilePath, parent.DocumentItem.Document);
            }
        }

        internal static void DeleteAllImportsInDocument(UIApplication app)
        {
            Autodesk.Revit.ApplicationServices.Application appSvcs = UIEventApp.DbDoc.Application;

            List<TreeViewItemBase> importNodes = UIEventApp.m_TreeNodes;

            foreach(TreeViewItemBase node in importNodes)
            {
                ImportInstanceViewModel importVM = node as ImportInstanceViewModel;

                if (node.Parent == null)
                {
                    // it is directly in the project
                    DeleteElement(UIEventApp.DbDoc, importVM.ImportCategoryItem.InstanceId, importVM.ItemName);
                }

                else // it is within a family
                {
                    DocumentViewModel docVM = node.Parent as DocumentViewModel;
                    string filePath = docVM.DocumentItem.FilePath;
                    Document doc = appSvcs.OpenDocumentFile(filePath);
                    DeleteElement(doc, importVM.ImportCategoryItem.InstanceId, importVM.ItemName);
                    SaveOptions options = new SaveOptions();
                    doc.Save();

                    DocumentViewModel parentVM = docVM.ParentDocumentViewModel;
                    Document childDoc = doc;
                    while (parentVM.DocumentItem.CategoryName != null)
                    {
                        Document parentDoc = appSvcs.OpenDocumentFile(parentVM.DocumentItem.FilePath);
                        LoadChildIntoParent(childDoc.PathName, parentDoc);

                        parentVM = parentVM.ParentDocumentViewModel;
                        childDoc = parentDoc;
                    }
                }
            }
        }

        private static void DeleteElement(Document doc, ElementId id, string itemName)
        {
            using (Transaction t = new Transaction(doc))
            {
                t.Start(string.Format("Deleting {0}", itemName));
                doc.Delete(id);
                t.Commit();
            }
        }


        internal static void DeleteSelDocuments(UIApplication app)
        {
            // if selected node is main document, return
            DocumentViewModel docNode = UIEventApp.m_treeNode as DocumentViewModel;

            // The objective here is to delete all empty children 
            // don't delete selected node even if it is empty

            // purge selected node
            PurgeDownstream(docNode, app);

            // reload document into parent all the upto project
            //ReloadDocUpstream(docNode);
            ReloadFamilyUpstream(docNode);
        }



        /// <summary> - FUNCTIONALITY DISABLED!!!!!!!!!!!!!!!!!!!!!
        /// RECURSIVE FUNCTION: Recursively checks for empty instance children and deletes them
        /// </summary>
        /// <param name="docNode"></param>
        private static void PurgeDownstream(DocumentViewModel docNode, UIApplication app)
        {
            List<DocumentViewModel> childNodes = GetChildDocs(docNode);
            string filePath = docNode.DocumentItem.FilePath;
            Document doc = app.Application.OpenDocumentFile(filePath);
            foreach (DocumentViewModel child in childNodes)
            {
                if (child.DocumentItem.DocumentInstanceCount == 0)
                {
                    ElementId id = child.DocumentItem.FamilyId;
                    if(null!= DeleteElementsAndSaveDocument(doc, filePath, null, id))
                    {
                        docNode.Children.Remove(child);
                    }
                    else
                    {
                        // indicate that some families could not be deleted
                    }
                }

                else
                {
                    PurgeDownstream(child, app);
                }
            }
        }


        /// <summary>
        /// Gets Child DocumentViewModel
        /// From Argumment DocumentViewModel
        /// </summary>
        /// <param name="selDocNode"></param>
        /// <returns></returns>
        private static List<DocumentViewModel> GetChildDocs(DocumentViewModel selDocNode)
        {
            if(selDocNode.Children.Count < 1)
            {
                return null;
            }

            List<DocumentViewModel> childNodes = new List<DocumentViewModel>();

            foreach(TreeViewItemBase node in selDocNode.Children)
            {
                if (node.GetType() == typeof(DocumentViewModel))
                {
                    childNodes.Add(node as DocumentViewModel);
                }
            }

            if(childNodes.Count < 1)
            {
                return null;
            }

            return childNodes;
        }

        internal static void DeleteFamilyDocInProject(UIApplication app)
        {
            DocumentViewModel familyDoc = UIEventApp.m_treeNode as DocumentViewModel;

            DocumentViewModel parentDocVm = familyDoc.ParentDocumentViewModel;

            Document parentDoc = null;

            if (parentDocVm.IsProjectDoc)
            {
                parentDoc = UIEventApp.DbDoc;

                ElementId idToDelete = new FilteredElementCollector(parentDoc)
                        .OfClass(typeof(Family))
                        .Where(x => x.Id == familyDoc.DocumentItem.FamilyId)
                        .Select(y => y.Id)
                        .FirstOrDefault();

                DeleteElementsAndSaveDocument(parentDoc, null, null, idToDelete);
            }
            // nested family
            else
            {
                Autodesk.Revit.ApplicationServices.Application appSvcs = UIEventApp.DbDoc.Application;
                string filePath = parentDocVm.DocumentItem.FilePath;
                parentDoc = appSvcs.OpenDocumentFile(filePath);
                

                string familyName = familyDoc.ItemName.Substring(0, familyDoc.ItemName.LastIndexOf('.'));

                // get instances of the family if any exist

                List<Element> elements = new FilteredElementCollector(parentDoc).OfClass(typeof(FamilyInstance)).ToList();
                elements.AddRange(new FilteredElementCollector(parentDoc).OfClass(typeof(Family)).ToList());



                List<ElementId> idsToDelete = new List<ElementId>();

                foreach(Element e in elements)
                {
                    if (e.GetType() == typeof(FamilyInstance))
                    {
                        FamilyInstance fi = e as FamilyInstance;
                        if (fi.Symbol.FamilyName == familyName)
                        {
                            using(Transaction t = new Transaction(parentDoc, "Delete Family"))
                            {
                                t.Start();
                                parentDoc.Delete(fi.Id);
                                t.Commit();
                            }
                        }
                    }
                    else if (e.GetType() == typeof(Family))
                    {
                        Family family = e as Family;
                        if (family.Name == familyName)
                        {
                            using (Transaction t = new Transaction(parentDoc, "Delete Family"))
                            {
                                t.Start();
                                parentDoc.Delete(family.Id);
                                t.Commit();
                            }
                        }
                    }
                }

                parentDoc.Save();
                parentDoc.Close();

                // reload upstream if parent is not project
                if (parentDocVm.IsProjectDoc == false)
                {
                    ReloadFamilyUpstream(parentDocVm);
                }
            }

            

        }


        /// <summary>
        /// Deletes either list or single element
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="filePath"></param>
        /// <param name="elementsToBeDeleted"></param>
        /// <param name="elementId"></param>
        /// <returns></returns>
        private static string DeleteElementsAndSaveDocument(Document doc, string filePath, List<ElementId> elementsToBeDeleted, ElementId elementId)
        {
            if (null==elementsToBeDeleted && null==elementId)
            {
                return null;
            }

            using (Transaction t = new Transaction(doc, "Delete Objects in List"))
            {
                t.Start();

                try
                {
                    if (null != elementsToBeDeleted && elementsToBeDeleted.Count > 0)
                    {
                        foreach(ElementId id in elementsToBeDeleted)
                        {
                            doc.GetElement(id).Pinned = false;
                        }

                        doc.Delete(elementsToBeDeleted);
                    }
                    else
                    {
                        doc.GetElement(elementId).Pinned = false;
                        doc.Delete(elementId);
                    }

                    // remove node from parent
                    if (null != UIEventApp.m_treeNode)
                    {
                        TreeViewItemBase parentNode = (UIEventApp.m_treeNode as DocumentViewModel).ParentDocumentViewModel;
                        parentNode.Children.Remove(UIEventApp.m_treeNode);
                    }
                }
                catch(Autodesk.Revit.Exceptions.ArgumentException argEx)
                {
                    // element cannot be deleted
                    Console.WriteLine(argEx.Message);
                    return null;
                }
                

                t.Commit();
            }

            

            if (null == filePath)
            {
                string docName = doc.Title;
                filePath = Path.Combine(Path.GetTempPath(), docName);
            }

            SaveAsOptions saveAsOptions = new SaveAsOptions();
            saveAsOptions.OverwriteExistingFile = true;

            doc.SaveAs(filePath, saveAsOptions);

            doc.Close(true);

            return filePath;
        }


        private static void LoadChildIntoParent(string childFilePath, Document parentDoc)
        {
            FamilyLoadOptions familyLoadOptions = new FamilyLoadOptions();
            using (Transaction t2 = new Transaction(parentDoc, "Reload Modified Family"))
            {
                t2.Start();

                Family family = null;

                FilteredElementCollector familyCollector = new FilteredElementCollector(UIEventApp.DbDoc).OfClass(typeof(Family));

                parentDoc.LoadFamily(childFilePath, familyLoadOptions, out family);

                t2.Commit();
            }
        }

        /// <summary>
        /// Required FamilyLoadOptions for Loading modified family back into project
        /// </summary>
        public class FamilyLoadOptions : IFamilyLoadOptions
        {
            #region IFamilyLoadOptions Members

            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                if (!familyInUse)
                {
                    //TaskDialog.Show("SampleFamilyLoadOptions", "The family has not been in use and will keep loading.");

                    overwriteParameterValues = true;
                    return true;
                }
                else
                {
                    //TaskDialog.Show("SampleFamilyLoadOptions", "The family has been in use but will still be loaded with existing parameters overwritten.");

                    overwriteParameterValues = true;
                    return true;
                }
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                if (!familyInUse)
                {
                    //TaskDialog.Show("SampleFamilyLoadOptions", "The shared family has not been in use and will keep loading.");

                    source = FamilySource.Family;
                    overwriteParameterValues = true;
                    return true;
                }
                else
                {
                    //TaskDialog.Show("SampleFamilyLoadOptions", "The shared family has been in use but will still be loaded from the FamilySource with existing parameters overwritten.");

                    source = FamilySource.Family;
                    overwriteParameterValues = true;
                    return true;
                }
            }

            #endregion
        }
    }
}
