using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace FamilyReporter
{
    class XmlDataModel
    {
        readonly DirectoryInfo _docDirectory;
        readonly string _xmlPath;
        private XDocument _rootDoc;

        public XmlDataModel(DirectoryInfo docDirectory, string projectFileName)
        {
            _docDirectory = docDirectory;
            // create xml file
            _xmlPath = Path.Combine(docDirectory.FullName, Path.ChangeExtension(projectFileName, ".xml"));

            _rootDoc = new XDocument(new XElement("Project", new XAttribute("Name", projectFileName)));
            _rootDoc.Save(_xmlPath);
        }

        public XDocument RootDoc { get { return _rootDoc; } set { _rootDoc = value; } }
        public string ProjectXmlPath { get { return _xmlPath; } }

        public void AggregatePartAtoms()
        {
            foreach(TreeViewItemBase child in UIEventApp.m_ProjectViewModel.Children)
            {
                if (child.GetType() != typeof(DocumentViewModel))
                {
                    continue;
                }
                DocumentViewModel docVm = child as DocumentViewModel;
                _rootDoc.Root.Add(GetXmlFromDocumentViewModel(docVm));
            }

            _rootDoc.Save(_xmlPath);

            FileIOUtils.OpenFolderAndHightlightFile(_xmlPath);
            
        }

        private XElement GetXmlFromDocumentViewModel(DocumentViewModel docVm)
        {
            string xmlFilePath = Path.ChangeExtension(docVm.DocumentItem.FilePath, ".xml");
            XDocument xdoc = XDocument.Load(xmlFilePath);
            XElement main = new XElement("Document", new XAttribute("Name", docVm.ItemName), new XAttribute("UID", docVm.DocumentItem.UniqueId));
            foreach (XElement xElem in xdoc.Elements())
            {
                main.Add(xElem);
            }
            foreach(TreeViewItemBase child in docVm.Children)
            {
                if (child.GetType() != typeof(DocumentViewModel))
                {
                    continue;
                }
                DocumentViewModel childDoc = child as DocumentViewModel;
                XElement childXElement = GetXmlFromDocumentViewModel(childDoc);
                main.Add(childXElement);
            }
            return main;
        }
    }
}
