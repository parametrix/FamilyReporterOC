using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace FamilyReporter
{
    public class ImportInstanceViewModel : TreeViewItemBase
    {
        readonly ImportInstanceItem _importInstanceItem;
        readonly TreeViewItemBase _parentViewModel;

        public ImportInstanceViewModel(ImportInstanceItem importInstanceItem, TreeViewItemBase parentViewModel) : base(parentViewModel)
        {
            _importInstanceItem = importInstanceItem;
            _parentViewModel = parentViewModel;

            base.ItemName = importInstanceItem.ImportInstanceName;
            LoadChildren();
        }

        public ImportInstanceItem ImportCategoryItem { get { return _importInstanceItem; } }
        public TreeViewItemBase ParentViewModel { get { return _parentViewModel; } }

        protected override void LoadChildren()
        {

            foreach (ImportSubCategoryItem importSubCategoryItem in GetImportSubCategoriesFromDocument())
            {
                base.Children.Add(new ImportSubCategoryViewModel(importSubCategoryItem, this));
            }
        }

        /// <summary>
        /// Get SubCategories from Import Instance View Model
        /// </summary>
        /// <param name="importInstanceViewModel"></param>
        /// <returns></returns>
        private IEnumerable<ImportSubCategoryItem> GetImportSubCategoriesFromDocument()
        {
            DocumentViewModel documentViewModel = this.Parent as DocumentViewModel;
            Document doc = documentViewModel.DocumentItem.Document;

            try
            {
                ImportInstance importInstance = doc.GetElement(_importInstanceItem.InstanceId) as ImportInstance;

                if (null != importInstance.Category.SubCategories)
                {
                    return importInstance.Category.SubCategories.Cast<Category>().Select(x => new ImportSubCategoryItem(x)).ToList();
                }
            }
            catch (NullReferenceException nullRef) { Console.WriteLine(nullRef.Message); }

            return null;
        }
    }
}
