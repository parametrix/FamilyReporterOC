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
    class DeleteDocumentCmd : ICommand
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
            TreeViewItemBase docNode = (TreeViewItemBase)parameter;
            if (null == docNode)
            {
                return true;
            }

            DocumentViewModel docVM = docNode as DocumentViewModel;
            if (docVM.DocumentItem.CategoryName != null)
            {
                return true;
            }

            return false;
        }

        void ICommand.Execute(object parameter)
        {
            TreeViewItemBase familyNode = (TreeViewItemBase)parameter;
            UIEventApp.m_treeNode = familyNode;

            // no need to remove from node as it is done in request handler functions
            return;
        }
    }
}
