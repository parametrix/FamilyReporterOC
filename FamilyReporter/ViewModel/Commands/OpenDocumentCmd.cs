using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FamilyReporter.ViewModel.Commands
{
    class OpenDocumentCmd : ICommand
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

        void ICommand.Execute(object parameter)
        {
            DocumentViewModel selectedViewModel = (DocumentViewModel)parameter;
            string filepath = selectedViewModel.DocumentItem.FilePath;
            OpenOptions fileOpenOptions = new OpenOptions();
            fileOpenOptions.Audit = true;
            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filepath);

            UIEventApp.UiDoc.Application.OpenAndActivateDocument(modelPath, fileOpenOptions, false);
        }
    }
}
