using Autodesk.Revit.DB;
using FamilyReporter.OLEStructuredStorage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace FamilyReporter
{
    public class DocumentViewModel : TreeViewItemBase
    {
        // variable to limit recursion per: https://msdn.microsoft.com/en-us/library/system.stackoverflowexception%28v=vs.110%29.aspx
        private const int MAX_RECURSIVE_CALLS = 9;
        static int cntr = 0;


        readonly DocumentItem _documentItem;
        readonly DocumentViewModel _parentDocumentViewModel;

        readonly DirectoryInfo _docDirectory;

        readonly bool _isProjectDoc;

        internal DocumentViewModel(DocumentItem documentItem, DocumentViewModel parentDocumentViewModel, DirectoryInfo docDirectory) : base(null)
        {
            _documentItem = documentItem;

            _parentDocumentViewModel = parentDocumentViewModel;

            _docDirectory = docDirectory;

            base.ItemName = documentItem.DocumentName;
            base.NodeColor = System.Windows.Media.Brushes.Black;

            // set static Project Node if this is the project node
            if(documentItem.Document.Title == UIEventApp.DbDoc.Title)
            {
                UIEventApp.m_ProjectViewModel = this;
                _isProjectDoc = true;
                // create xml data model
                UIEventApp.m_xmlDataModel = new XmlDataModel(_docDirectory, documentItem.Document.Title);
            }

            if (null == parentDocumentViewModel)
            {
                base.Level = 0;
                _parentDocumentViewModel = UIEventApp.m_ProjectViewModel;
            }
            else
            {
                // loading level two families
                if (parentDocumentViewModel.DocumentItem.Document.IsFamilyDocument)
                {
                    base.NodeColor = System.Windows.Media.Brushes.DarkSlateGray;
                }
                
                base.Level = parentDocumentViewModel.Level++;
                // load only nested families
                // level 1 families will be loaded by dispatcher
                if (!UIEventApp.m_ProbleOnlyTopLevelFamilies && base.Level != 1)
                {
                    LoadChildren();
                }
            }
        }


        

        public DocumentItem DocumentItem { get { return _documentItem; } }

        public DocumentViewModel ParentDocumentViewModel { get { return _parentDocumentViewModel; } }

        public DirectoryInfo DocDirectory { get { return _docDirectory; } }

        public bool IsProjectDoc { get { return _isProjectDoc; } }

        /// <summary>
        /// Gets Level 1 Families
        /// </summary>
        /// <returns></returns>
        public FilteredElementCollector GetFamiliesInProject()
        {
            if (base.Level != 0)
            {
                return null;
            }

            ElementClassFilter familyFilter = new ElementClassFilter(typeof(Family));
            return new FilteredElementCollector(this.DocumentItem.Document).WherePasses(familyFilter);
        }

        /// <summary>
        /// Load Specified Family
        /// </summary>
        /// <param name="family"></param>
        /// <param name="projectDocViewModel"></param>
        public void LoadFamily(Family family, DocumentViewModel projectDocViewModel)
        {
            DocumentViewModel tempDocViewModel = CreateDocumentViewModelFromNestedFamily(family, GetFamilyInstanceCount(projectDocViewModel.DocumentItem.Document, family));
            // HasImports shows only those families with imports in them.
            // OPTION: Enable user controllable switch - but switching here only works for families that already have an import
            //if (null != tempDocViewModel && tempDocViewModel.HasImports)
            if (null != tempDocViewModel)
            {
                projectDocViewModel.Children.Add(tempDocViewModel);
            }
        }

        /// <summary>
        /// Factored Out for Public Access
        /// </summary>
        public void LoadImports()
        {
            // get import category view model
            List<ImportInstanceItem> importInstanceItemList = GetImportInstanceItemsFromDocument(_documentItem.Document);


            if (null != importInstanceItemList && importInstanceItemList.Count > 0)
            {
                foreach (ImportInstanceItem item in importInstanceItemList)
                {
                    if (item != null)
                    {

                        try
                        {
                            base.Children.Add(new ImportInstanceViewModel(item, this));
                        }
                        catch (Exception e) { Console.WriteLine(e.Message); }
                    }
                }
            }
        }


        protected override void LoadChildren()
        {
            LoadImports();

            this.AddNestedFamilyDocumentViewModelsToBaseClass(_documentItem.Document);
        }


        /// <summary>
        /// Import Instances from Document
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private List<ImportInstanceItem> GetImportInstanceItemsFromDocument(Document doc)
        {
            ElementClassFilter importInstanceFilter = new ElementClassFilter(typeof(ImportInstance));

            List<ImportInstance> importInstanceList = new FilteredElementCollector(doc)
                                                        .WherePasses(importInstanceFilter)
                                                        .Cast<ImportInstance>()
                                                        .ToList();

            List<ImportInstanceItem> importItems = new List<ImportInstanceItem>();
            // get count
            foreach(ImportInstance import in importInstanceList)
            {
                int instanceCount = 0;
                instanceCount = new FilteredElementCollector(doc)
                                    .WherePasses(importInstanceFilter)
                                    .Where(x => { try { return x.Category.Name == import.Category.Name; } catch { return false; } })
                                    .ToArray()
                                    .Count();


                string importName = "";
                ElementId id = null;
                try
                {
                    importName = import.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString();
                    id = import.Id;
                }
                catch (Exception e) { Console.WriteLine(e.Message); }

                importItems.Add(new ImportInstanceItem(importName, id, instanceCount));
            }

            return importItems;
        }


        private void AddNestedFamilyDocumentViewModelsToBaseClass(Document doc)
        {
            if (null!=this._parentDocumentViewModel && !this._parentDocumentViewModel._documentItem.Document.IsFamilyDocument)
            {
                cntr = 0; //reset counter if this is not a nested family
                
            }
            cntr++;

            // check for nestded families and import instances within them - RECURSIVE
            bool hasImports = this.Children.Count > 0;        // should work because imports are added first

            ElementClassFilter familyFilter = new ElementClassFilter(typeof(Family));
            var nestedFamilyCollector = new FilteredElementCollector(doc).WherePasses(familyFilter).OrderBy(x => (x as Family).Name).ToList();

            var families = nestedFamilyCollector.Where(x => x.Name != "");

            // figure out if there are nested families


#if v2015
            bool hasNestedFamilies = nestedFamilyCollector.Count() > 0;

#else
            bool hasNestedFamilies = nestedFamilyCollector.Count > 0;

#endif

            // check if this is the last (nested) family in the node
            bool isLastDocumentNode = false;
            if (hasNestedFamilies)
            {
                isLastDocumentNode = nestedFamilyCollector.FirstOrDefault().Name == string.Empty && this.DocumentItem.Document.IsFamilyDocument;
            }

            if ((hasNestedFamilies || hasImports || isLastDocumentNode) && cntr <= MAX_RECURSIVE_CALLS)
            {
                foreach (Family nestedFamily in families)
                {
                    DocumentViewModel tempDocViewModel = CreateDocumentViewModelFromNestedFamily(nestedFamily, GetFamilyInstanceCount(doc, nestedFamily));
                    // HasImports shows only those families with imports in them.
                    // OPTION: Enable user controllable switch - but switching here only works for families that already have an import
                    //if (null != tempDocViewModel && tempDocViewModel.HasImports)
                    if (null != tempDocViewModel)
                    {
                        base.Children.Add(tempDocViewModel);
                    }
                }
            }
        }

        /// <summary>
        /// Counts Number of instances in a given family
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="family"></param>
        /// <returns></returns>
        private int GetFamilyInstanceCount(Document doc, Family family)
        {
            int count = 0;
            ISet<ElementId> familySymbolIds = family.GetFamilySymbolIds();
            if (familySymbolIds.Count < 1)
                return count;
            foreach(ElementId symbolId in familySymbolIds)
            {
                FamilyInstanceFilter instancefilter = new FamilyInstanceFilter(doc, symbolId);


#if v2015
                count += new FilteredElementCollector(doc).WherePasses(instancefilter).Count();
#else
                count += new FilteredElementCollector(doc).WherePasses(instancefilter).GetElementCount();
#endif

            }
            return count;
        }

        /// <summary>
        /// Create View Model from Family
        /// </summary>
        /// <param name="family"></param>
        /// <returns></returns>
        private DocumentViewModel CreateDocumentViewModelFromNestedFamily(Family family, int familyInstanceCount)
        {
            DocumentViewModel docViewModel = null;

            try
            {
                Document familyDoc = _documentItem.Document.EditFamily(family);

                //string correctedFileName = FileIOUtils.RemoveInvalidCharacters(familyDoc.Title); - CAUSING PROBLEMS WHEN RE-LOADING FILE

                //string filePath = Path.Combine(UIEventApp.m_ProjectDirectory.FullName, familyDoc.Title);

                //// if file exists, delete file
                //if (File.Exists(filePath))
                //{
                //    File.Delete(filePath);
                //}

                DirectoryInfo familyDirectory = null;

                string docFileName = Path.GetFileNameWithoutExtension(_documentItem.Document.Title);

                // if this is the Project, then place it in the project directory
                DirectoryInfo nestedDirectory = nestedDirectory = new DirectoryInfo(Path.Combine(DocDirectory.FullName, docFileName));
                
                if (nestedDirectory.Exists == false)
                {
                    nestedDirectory = DocDirectory.CreateSubdirectory(docFileName);
                }
                familyDirectory = nestedDirectory;

                SaveAsOptions saveAsOptions = new SaveAsOptions();
                saveAsOptions.OverwriteExistingFile = true;

                string filePath = Path.Combine(familyDirectory.FullName, familyDoc.Title);

                familyDoc.SaveAs(filePath, saveAsOptions);

                docViewModel = new DocumentViewModel(new DocumentItem(familyDoc, filePath, familyInstanceCount, family.UniqueId, family.Id), this, familyDirectory);


                #region xML OPERATIONS

                string xmlPath = Path.ChangeExtension(filePath, ".xml");
                try
                {
                    // get xmlfile
                    family.ExtractPartAtom(xmlPath);

                    string previewImagePath = GetPreviewImagePath(filePath);
                    List<XElement> stplElements = new List<XElement>();
                    stplElements.Add(new XElement("sizeOnDisk", docViewModel.DocumentItem.DocumentSize, new XAttribute("displayName", "Size In Kb")));
                    stplElements.Add(new XElement("numberOfInstances", familyInstanceCount, new XAttribute("displayName", "Number Of Instances")));
                    stplElements.Add(new XElement("category", familyDoc.OwnerFamily.FamilyCategory.Name, new XAttribute("displayName", "Project Category")));
                    stplElements.Add(new XElement("updated", DateTime.Now.ToString(), new XAttribute("displayName", "Last Updated")));
                    stplElements.Add(new XElement("UID", family.UniqueId, new XAttribute("displayName", "UID")));
                    stplElements.Add(new XElement("parentDocName", this.ItemName, new XAttribute("displayName", "Parent Name")));
                    stplElements.Add(new XElement("parentDocUID", this.DocumentItem.UniqueId, new XAttribute("displayName", "Parent UID")));

                    XmlUtils.InsertSTPLElements(xmlPath, stplElements.ToArray());
                }
                catch { }

                #endregion

                familyDoc.Close(false);

            }
            //catch (Autodesk.Revit.Exceptions.ArgumentException argEx) { Console.WriteLine(argEx.Message); } - Catch all exception. Selective handling fails at times
            catch(Exception ex) { Console.WriteLine(ex.Message); }

            return docViewModel;
        }

        

        private Dictionary<string, string> GetInstanceParametersFromFamily(Document familyDoc)
        {
            Dictionary<string, string> paramDic = new Dictionary<string, string>();

            FamilyManager familyManager = familyDoc.FamilyManager;
            FamilyParameterSet paramSet = familyManager.Parameters;

            var itr = paramSet.GetEnumerator();
            while (itr.MoveNext())
            {
                Parameter param = itr.Current as Parameter;
                Definition def = param.Definition;
                paramDic.Add(def.Name, param.AsValueString());
            }

            return paramDic;
            
        }

        /// <summary>
        /// Create preview image along with xml in the structured storage file
        /// </summary>
        /// <param name="familyFilePath"></param>
        /// <returns></returns>
        private string GetPreviewImagePath(string familyFilePath)
        {
            Storage storage = new Storage(familyFilePath);
            string imagePath = Path.ChangeExtension(familyFilePath, ".png");

            Image image = storage.ThumbnailImage.SavePreviewAsImage(imagePath);


            // gets relative path to the project directory
            return imagePath;
        }

        /// <summary>
        /// Adding to the composite xml
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <param name="docVm"></param>
        private void AppendXmlToParent(string xmlPath, DocumentViewModel docVm)
        {
            XElement familyXml = XDocument.Load(xmlPath).Root;
            XElement familyXmlDataModel = new XElement("Document", new XAttribute("Name", this.ItemName), new XAttribute("UID", this.DocumentItem.UniqueId));
            familyXmlDataModel.Add(familyXml);

            XDocument xDoc = new XDocument(familyXmlDataModel);
            xDoc.Save(xmlPath);
        }
    }
}
