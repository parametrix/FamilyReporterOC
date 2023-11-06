using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyReporter
{
    public class ImportSubCategoryViewModel : TreeViewItemBase
    {
        readonly ImportSubCategoryItem _importSubCategoryItem;

        internal ImportSubCategoryViewModel(ImportSubCategoryItem importSubCategoryItem, TreeViewItemBase parent) : base(parent)
        {
            _importSubCategoryItem = importSubCategoryItem;
            base.ItemName = importSubCategoryItem.ImportSubCategoryName;
        }

        public ImportSubCategoryItem ImportSubCategoryItem { get { return _importSubCategoryItem; } }


    }
}
