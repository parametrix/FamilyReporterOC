using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FamilyReporter.ViewModel.Commands
{
    class DeleteImportCmd : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }


        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        // this command can only manipulate the database in terms of deleting the objects from the viewModel.
        // the actual style deletion in the project is done via the request handler after the elementids are 
        // sent over from to the UIEventApp.m_ElementIds
        void ICommand.Execute(object parameter)
        {
            TreeViewItemBase importNode = (TreeViewItemBase)parameter;
            TreeViewItemBase parentNode = importNode.Parent;
            UIEventApp.m_ElementIds = new List<ElementId>();

            DocumentViewModel parentDocNode = parentNode as DocumentViewModel;

            List<ElementId> idsToTransfer = new List<ElementId>();

            if (null!=parentDocNode.DocumentItem.CategoryName && parentDocNode.Level>0)
            {
                ElementClassFilter filter = new ElementClassFilter(typeof(FamilyInstance));
                FilteredElementCollector collector = new FilteredElementCollector(UIEventApp.DbDoc).WherePasses(filter);
                ElementId familyId = collector.Cast<FamilyInstance>().Where(x => x.Symbol.Family.UniqueId == parentDocNode.DocumentItem.UniqueId).Select(x => x.Symbol.Family.Id).FirstOrDefault();

                //if familyId == null, then the family Must be nested or could be project
                if (null == familyId)
                {
                    List<string> docPaths = new List<string>();
                    docPaths.Add(parentDocNode.DocumentItem.FilePath);

                    DocumentViewModel anscestor = parentDocNode.ParentDocumentViewModel;
                    int counter = 0;
                    while (anscestor.DocumentItem.CategoryName != null)
                    {
                        if (counter > 99)
                        {
                            TaskDialog.Show("Error", "Deletion Failed");
                            return;
                        }

                        docPaths.Add(anscestor.DocumentItem.FilePath);
                        anscestor = anscestor.ParentDocumentViewModel;
                    }
                    UIEventApp.m_DocumentPaths = docPaths;
                }

                // send in null for now (if nested Family) and then check with request handler
                idsToTransfer.Add(familyId);
            }

            ElementId importId = (importNode as ImportInstanceViewModel).ImportCategoryItem.InstanceId;
            idsToTransfer.Add(importId);

            UIEventApp.m_ElementIds = idsToTransfer;

            parentNode.Children.Remove(importNode);
        }

        // from: http://stackoverflow.com/questions/20800310/wpf-treeview-with-checkbox-how-to-get-the-list-of-checked
        private List<TreeViewItemBase> GetCheckedItems(TreeViewItemBase node)
        {
            var checkedItems = new List<TreeViewItemBase>();

            ProcessNode(node, checkedItems);

            return checkedItems;
        }

        // from: http://stackoverflow.com/questions/20800310/wpf-treeview-with-checkbox-how-to-get-the-list-of-checked
        private void ProcessNode(TreeViewItemBase node, List<TreeViewItemBase> checkedItems)
        {
            foreach (var child in node.Children)
            {
                if (child.IsChecked)
                    checkedItems.Add(child);

                ProcessNode(child, checkedItems);
            }
        }
    }
}
