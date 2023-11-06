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
    class PurgeAllImportedStylesInDocCmd : ICommand
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
            TreeViewItemBase docNode = (TreeViewItemBase)parameter;

            // passing on for processing at request handler
            UIEventApp.m_treeNode = docNode;


            if (null == docNode)
            {
                docNode = UIEventApp.m_ViewModel.DocumentViewModels.CurrentItem as TreeViewItemBase;
                // if the command is coming from the datagrid
                // look for node in listcollectionview
                if (docNode == null)
                {
                    docNode = UIEventApp.m_ViewModel.SelectedItem as TreeViewItemBase;
                }
            }

            UIEventApp.m_ElementIds = new List<ElementId>();

            List<ImportSubCategoryViewModel> subCatviewModels = docNode.GetAllImportSubCategoryViewModels();

            foreach(ImportSubCategoryViewModel subCat in subCatviewModels)
            {
                UIEventApp.m_ElementIds.Add(subCat.ImportSubCategoryItem.SubCategory.Id);
                subCat.IsChecked = true;
            }

            // remove checked Items
            // this has to be done in separate steps - app crashes otherwise -
            List<TreeViewItemBase> checkedItems = GetCheckedItems(docNode);
            foreach (TreeViewItemBase child in checkedItems)
            {
                TreeViewItemBase parentNode = child.Parent;
                parentNode.Children.Remove(child);
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
