using System;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using System.IO;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using System.ComponentModel;
using System.Windows.Data;

namespace FamilyReporter
{
    internal class ProjectViewModel : INotifyPropertyChanged
    {
        readonly ListCollectionView _documentViewModels;

        readonly Document _projectDoc;

        private DocumentViewModel _currentDocumentViewModel;

        private System.Windows.Visibility _loadCompleteVisibility;


        public ProjectViewModel(Document projectDoc)
        {
            _projectDoc = projectDoc;

            _documentViewModels = GetFamilyDocumentViewModels();

            _loadCompleteVisibility = System.Windows.Visibility.Hidden;

        }

        public ListCollectionView DocumentViewModels { get { return _documentViewModels; } }

        public TreeViewItemBase SelectedItem { get; set; }

        public System.Windows.Visibility LoadCompleteVisibility { get { return _loadCompleteVisibility; } set { SetField<System.Windows.Visibility>(ref _loadCompleteVisibility, value, "FamilyLoadComplete"); } }


        /// <summary>
        /// Creates Collection of Document ViewModel for Families containing import instances
        /// </summary>
        /// <returns></returns>
        private ListCollectionView GetFamilyDocumentViewModels()
        {
            IList<DocumentViewModel> docViewModelList = new List<DocumentViewModel>();

            _currentDocumentViewModel = new DocumentViewModel(new DocumentItem(_projectDoc, _projectDoc.PathName, 1, null, null), null);

            docViewModelList.Add(_currentDocumentViewModel);

            return new ListCollectionView(docViewModelList.OfType<DocumentViewModel>().ToList());
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
