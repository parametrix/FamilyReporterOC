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
    class PurgeSelImportedStylesInImportCmd : ICommand
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
            List<TreeViewItemBase> checkedItems = GetCheckedItems(importNode);
            UIEventApp.m_ElementIds = new List<ElementId>();
            foreach(TreeViewItemBase childNode in checkedItems)
            {
                ElementId id = (childNode as ImportSubCategoryViewModel).ImportSubCategoryItem.SubCategoryId;
                UIEventApp.m_ElementIds.Add(id);
                importNode.Children.Remove(childNode);
            }
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
