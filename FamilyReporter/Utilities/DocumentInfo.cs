using System;

namespace FamilyReporter
{
    public class DocumentInfo
    {
        //public string DocumentCategory { get; internal set; }
        //public string DocumentName { get; internal set; }
        //public string NumberOfInstances { get; internal set; }
        //public string ParentNodeName { get; internal set; }
        //public string SizeInKb { get; internal set; }

        public string ParentNodeName;
        public string DocumentCategory;
        public string DocumentName;
        public string NumberOfInstances;
        public string SizeInKb;


        public override string ToString()
        {
            return String.Format("{0},{1},{2},{3},{4}", DocumentCategory, DocumentName, NumberOfInstances, ParentNodeName, SizeInKb);
        }
    }
}