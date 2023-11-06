using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace FamilyReporter
{
    public class DocumentItem
    {
        readonly string _documentName;
        readonly string _filePath;
        readonly Document _document;
        readonly string _categoryName;
        readonly long _documentSize;
        readonly int _documentInstanceCount;
        readonly string _uniqueId;
        readonly ElementId _familyId;

        public DocumentItem(Document document, string filePath, int familyInstanceCount, string uniqueId, ElementId familyId)
        {
            _document = document;
            _documentName = document.Title;
            _filePath = filePath;
            _uniqueId = uniqueId;
            _familyId = familyId;
            

            if (document.IsFamilyDocument)
            {
                _categoryName = document.OwnerFamily.FamilyCategory.Name;
            }
            else
            {
                _categoryName = "Project";
            }

            _documentSize = FileIOUtils.GetFileSizeOnDisk(_filePath);

            _documentInstanceCount = familyInstanceCount;

        }

        public string DocumentName { get { return _documentName; } }
        public string FilePath { get { return _filePath; } }
        public Document Document { get { return _document; } }
        public string CategoryName { get { return _categoryName; } }
        public long DocumentSize { get { return _documentSize/1024; } }
        public int DocumentInstanceCount { get { return _documentInstanceCount; } }
        public string UniqueId { get { return _uniqueId; } }
        public ElementId FamilyId { get { return _familyId; } }
    }
}
