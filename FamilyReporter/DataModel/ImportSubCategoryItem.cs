using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace FamilyReporter
{
    public class ImportSubCategoryItem
    {
        readonly string _importSubCategoryName;
        readonly Category _subCategory;
        readonly ElementId _subCategoryId;

        public ImportSubCategoryItem(Category subCategory)
        {
            _importSubCategoryName = subCategory.Name;
            _subCategory = subCategory;
            _subCategoryId = subCategory.Id;
        }

        public string ImportSubCategoryName { get { return _importSubCategoryName; } }
        public Category SubCategory { get { return _subCategory; } }
        public ElementId SubCategoryId { get { return _subCategoryId; } }
    }
}
