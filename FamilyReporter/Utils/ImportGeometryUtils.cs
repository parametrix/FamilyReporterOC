using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FamilyReporter
{
    internal class ImportGeometryUtils
    {
        public static void DeleteImportStylesInImport(UIApplication uiApp, Document doc, ImportInstance import)
        {
            var subCategories = import.Category.SubCategories;
            List<ElementId> importSubCategoryIds = new List<ElementId>();
            if (subCategories != null && subCategories.Size > 0)
            {
                foreach (Category subCategory in subCategories)
                {
                    importSubCategoryIds.Add(subCategory.Id);
                }
            }

            DeleteElements(importSubCategoryIds, doc);
        }

        /// <summary>
        /// DELETES ALL  IMPORT SUBCATEGORIES IN DOCUMENT AND NESTED DOCUMENTS
        /// </summary>
        /// <param name="uiApp"></param>
        /// <param name="doc"></param>
        public static void DeleteImportStylesInDocument(UIApplication uiApp, Document doc)
        {
            Categories docCategories = doc.Settings.Categories;
            List<ElementId> importSubCategoryIds = new List<ElementId>();

            var imports = GetImportInstancesFromDocument(doc);

            // get sub categories from project
            if(imports!=null && imports.Count<Element>() > 0)
            {
                foreach(var import in imports)
                {
                    var subCategories = import.Category.SubCategories;
                    if(subCategories!=null && subCategories.Size > 0)
                    {
                        foreach(Category subCategory in subCategories)
                        {
                            importSubCategoryIds.Add(subCategory.Id);
                        }
                    }
                }
            }

            // get subcategories from families
            Category importObjectStyleCategory = docCategories.get_Item(BuiltInCategory.OST_ImportObjectStyles);
            if (null != importObjectStyleCategory && importObjectStyleCategory.SubCategories.Size>0)
            {
                importSubCategoryIds.AddRange(importObjectStyleCategory.SubCategories.Cast<Category>().Select(x => x.Id));
            }

            // delete elementIds
            DeleteElements(importSubCategoryIds, doc);
        }

        /// <summary>
        /// Deletion
        /// </summary>
        /// <param name="list"></param>
        /// <param name="doc"></param>
        private static void DeleteElements(List<ElementId> list, Document doc)
        {
            // delete elementIds
            using (Transaction t = new Transaction(doc, "Deleting all imported styles"))
            {
                t.Start();
                doc.Delete(list);
                t.Commit();
            }
        }

        /// <summary>
        /// Gets Elements of Import Instances not linked in a project
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static IEnumerable<Element> GetImportInstancesFromDocument(Document doc)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(ImportInstance)).Cast<ImportInstance>().Where(x => x.IsLinked == false);
        }
    }
}
