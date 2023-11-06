using FamilyReporter.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace FamilyReporter
{
    public class FamilyCollectionViewModel : CommandBase, INotifyPropertyChanged
    {
        ICollectionView _familyCollection;
        // filtering functionality from: http://stackoverflow.com/questions/17099042/filter-wpf-datagrid-values-from-a-textbox
        private string _filterString;
        private bool _isExpanded;
        private bool _isGrouped;

        private GroupDescription _categoryGroupDescription;
        private int _numberOfFamilies;

        private TreeViewItemBase _selectedItem;
        private ProjectViewModel _projectViewModel;

        public FamilyCollectionViewModel(ProjectViewModel projectData)
        {
            _projectViewModel = projectData;
            _categoryGroupDescription = new PropertyGroupDescription("DocumentItem.CategoryName");
            _familyCollection = GetGroupedCollection(projectData.DocumentViewModels);
            _familyCollection.Filter = new Predicate<object>(Filter); // part of filtering functionality
        }

        public TreeViewItemBase SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetField<TreeViewItemBase>(ref _selectedItem, value, "SelectedItem");
                _projectViewModel.DocumentViewModels.MoveCurrentTo(_selectedItem); // update the document models so that context menu works
            }
        }

        public ICollectionView FamilyCollection
        {
            get { return _familyCollection; }
            set { SetField<ICollectionView>(ref _familyCollection, value, "FamilyCollection"); }
        }

        public int NumberOfFamilies
        {
            get { return _numberOfFamilies; }
            set { SetField<int>(ref _numberOfFamilies, value, "NumberOfFamilies"); }
        }

        public string FilterString
        {
            get { return _filterString; }
            set
            {
                SetField<string>(ref _filterString, value, "FilterString");
                FilterCollection();
            }
        }

        public bool IsExpanded { get { return _isExpanded; } set { SetField<bool>(ref _isExpanded, value, "IsExpanded"); } }

        public bool IsGrouped
        {
            get { return _isGrouped; }
            set
            {
                if (value)
                {
                    FamilyCollection.GroupDescriptions.Remove(_categoryGroupDescription);
                }
                else
                {
                    FamilyCollection.GroupDescriptions.Add(_categoryGroupDescription);
                }
                SetField<bool>(ref _isGrouped, value, "IsGrouped");
            }
        }

        private ICollectionView GetGroupedCollection(ListCollectionView documentViewModels)
        {
            List<DocumentViewModel> familyList = new List<DocumentViewModel>();

            foreach (var item in documentViewModels)
            {
                if (item.GetType() != typeof(DocumentViewModel)) // this needs to be checked at every stage
                {
                    continue;
                }

                DocumentViewModel project = item as DocumentViewModel;
                familyList.Add(project);
                foreach (var subItem in project.Children)
                {
                    if (subItem.GetType() != typeof(DocumentViewModel))
                    {
                        continue;
                    }

                    DocumentViewModel levelOneFamily = subItem as DocumentViewModel;

                    familyList.Add(levelOneFamily);
                }
            }

            ICollectionView collection = new ListCollectionView(familyList);

            IEnumerable<GroupDescription> groupDescriptions = familyList.Select(x => x.DocumentItem.CategoryName).Distinct().Select(y => new PropertyGroupDescription(y));

            collection.GroupDescriptions.Concat(groupDescriptions);



            return collection;
        }

        #region FILTERING METHODS
        // from: http://stackoverflow.com/questions/17099042/filter-wpf-datagrid-values-from-a-textbox

        private void FilterCollection()
        {
            if (_familyCollection != null)
            {
                _familyCollection.Refresh();
            }
        }

        public bool Filter(object obj)
        {
            var data = obj as TreeViewItemBase;
            if (data != null)
            {
                if (!string.IsNullOrEmpty(_filterString))
                {
                    // original method was case sensitive
                    // this snippet from: http://stackoverflow.com/questions/8494703/find-a-substring-in-a-case-insensitive-way-c-sharp
                    int index = data.ItemName.IndexOf(_filterString, StringComparison.CurrentCultureIgnoreCase);
                    if (index > -1)
                    {
                        return true;
                    }

                    // this rule from the original seems to be need for reliable functioning
                    return data.ItemName.Contains(_filterString);
                }
                return true;
            }
            return false;
        }


        #endregion


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
