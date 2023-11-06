using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace FamilyReporter

{
    public class ImportInstanceItem
    {
        readonly string _importInstanceName;
        readonly ElementId _instanceId;
        readonly int _instanceCount;

        public ImportInstanceItem(string importInstanceName, ElementId instanceId, int instanceCount)
        {
            _importInstanceName = importInstanceName;
            _instanceId = instanceId;
            _instanceCount = instanceCount;
        }

        public string ImportInstanceName { get { return _importInstanceName; } }

        public ElementId InstanceId
        {
            get { return _instanceId; }
        }

        public int InstanceCount { get { return _instanceCount; } }
    }
}
