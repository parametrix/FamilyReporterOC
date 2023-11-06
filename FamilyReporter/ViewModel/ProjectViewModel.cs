using System;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using System.IO;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;

namespace FamilyReporter
{
    public class ProjectViewModel : INotifyPropertyChanged
    {
        readonly ListCollectionView _documentViewModels;

        readonly Document _projectDoc;

        private DocumentViewModel _currentDocumentViewModel;

        private System.Windows.Visibility _loadCompleteVisibility;

        public FamilyCollectionViewModel _familyCollectionView;

        private TreeViewItemBase _selectedItem;


        public ProjectViewModel(Document projectDoc)
        {
            _projectDoc = projectDoc;

            _documentViewModels = GetFamilyDocumentViewModels();

            this.FamilyLoadComplete = System.Windows.Visibility.Hidden;
        }

        public ListCollectionView DocumentViewModels { get { return _documentViewModels; } }

        public TreeViewItemBase SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_familyCollectionView != null)
                {
                    _familyCollectionView.SelectedItem = value;
                }
                
                SetField<TreeViewItemBase>(ref _selectedItem, value, "SelectedItem");
            }
        }


        public System.Windows.Visibility FamilyLoadComplete { get { return _loadCompleteVisibility; } set { SetField<System.Windows.Visibility>(ref _loadCompleteVisibility, value, "FamilyLoadComplete"); } }

        public FamilyCollectionViewModel FamilyCollectionView { get { return _familyCollectionView; } set { SetField<FamilyCollectionViewModel>(ref _familyCollectionView, value, "FamilyCollectionView"); } }



        /// <summary>
        /// Creates Collection of Document ViewModel for Families containing import instances
        /// </summary>
        /// <returns></returns>
        private ListCollectionView GetFamilyDocumentViewModels()
        {
            IList<DocumentViewModel> docViewModelList = new List<DocumentViewModel>();

            string docPathName = string.Empty;

            try
            {
                docPathName = _projectDoc.PathName;
            }
            catch { }

            // check if file exists (to hand BIM360 models)
            if (!File.Exists(docPathName))
            {
                // find the model in the collaboration cache
                docPathName = GetDocPathFromCollaborationCache(_projectDoc);
            }

            _currentDocumentViewModel = new DocumentViewModel(new DocumentItem(_projectDoc, docPathName, 1, null, null), null, UIEventApp.m_ProjectDirectory);

            docViewModelList.Add(_currentDocumentViewModel);

            return new ListCollectionView(docViewModelList.OfType<DocumentViewModel>().ToList());
        }

        /// <summary>
        /// Retrive model from local cache
        /// </summary>
        /// <param name="projectDoc"></param>
        /// <returns></returns>
        private string GetDocPathFromCollaborationCache(Document projectDoc)
        {
# if (v2018 || v2017 || v2016 || v2015)

            // alternate way to retrive local cache
            TaskDialog td = new TaskDialog("BIM360 Models");
            td.MainInstruction = "Please save this model to a local disk location before running this tool";
            td.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
            td.Show();
            return _projectDoc.PathName;

#else

            var modelPath = _projectDoc.GetCloudModelPath();
            var modelGuid =  modelPath.GetModelGUID();
            var projectGuid = modelPath.GetProjectGUID();
            string versionNumber = _projectDoc.Application.VersionNumber;

            //var userVisiblePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);

            var cachePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Autodesk",
                "Revit",
                "Autodesk Revit "+versionNumber,
                "CollaborationCache"
                );

            var projectFolderPath = Directory.GetDirectories(cachePath, projectGuid.ToString(), SearchOption.AllDirectories).FirstOrDefault();

            var modelFile = Directory.GetFiles(projectFolderPath, "*.rvt").Where(x=>x.Contains(modelGuid.ToString())).FirstOrDefault();

            return modelFile;
#endif
        }

#region PROPERTY CHANGED NOTIFICATION
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

#endregion
    }
}
