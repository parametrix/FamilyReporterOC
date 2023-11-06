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
using System.Threading;
#endregion

namespace FamilyReporter
{
    public class Request
    {
        private int m_request = (int)RequestId.None; // default request

        public RequestId Take()
        {
            return (RequestId)Interlocked.Exchange(ref m_request, (int)RequestId.None); // indicator that the request is passed on

            //var result = RequestId.None;
            //try
            //{
            //    result = (RequestId)Interlocked.Exchange(ref m_request, (int)RequestId.None);
            //}
            //catch(Exception e) { TaskDialog.Show("Error", e.Message); }
            ////return (RequestId)Interlocked.Exchange(ref m_request, (int)RequestId.None); // indicator that the request is passed on
            //return result;
        }

        public void Make(RequestId request)
        {
            Interlocked.Exchange(ref m_request, (int)request); // called when an event is raised in form eg: button-press
        }
    }
}