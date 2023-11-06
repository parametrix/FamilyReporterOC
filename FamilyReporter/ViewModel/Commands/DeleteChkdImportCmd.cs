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
    class DeleteChkdImportCmd : ICommand
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
            // get all checked imports in project node
            TreeViewItemBase rootNode = (TreeViewItemBase)parameter;

            if (rootNode == null)
            {
                rootNode = UIEventApp.m_ProjectViewModel as TreeViewItemBase;
            }

            List<TreeViewItemBase> nodesToTransfer = GetCheckedNodes(rootNode);

            UIEventApp.m_TreeNodes = nodesToTransfer;

            // remove checked nodes
            foreach(var item in nodesToTransfer)
            {
                var parent = item.Parent;
                parent.Children.Remove(item);
            }

        }

        private List<TreeViewItemBase> GetCheckedNodes(TreeViewItemBase parentNode)
        {
            List<TreeViewItemBase> checkedNodes = new List<TreeViewItemBase>();
            foreach(var child in parentNode.Children)
            {
                if (child.IsChecked && child.GetType()==typeof(ImportInstanceViewModel))
                {
                    checkedNodes.Add(child);
                }
                else
                {
                    checkedNodes.AddRange(GetCheckedNodes(child));
                }
            }
            return checkedNodes;
        }

        // from: http://stackoverflow.com/questions/20800310/wpf-treeview-with-checkbox-how-to-get-the-list-of-checked
        private List<TreeViewItemBase> GetCheckedItems(TreeViewItemBase node)
        {
            var checkedItems = new List<TreeViewItemBase>();

            ProcessNode(node, ref checkedItems);

            return checkedItems;
        }

        // from: http://stackoverflow.com/questions/20800310/wpf-treeview-with-checkbox-how-to-get-the-list-of-checked
        private void ProcessNode(TreeViewItemBase node, ref List<TreeViewItemBase> checkedItems)
        {
            foreach (var child in node.Children)
            {
                if (child.IsChecked)
                    checkedItems.Add(child);

                ProcessNode(child, ref checkedItems);
            }
        }
    }
}
