using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using FamilyReporter.ViewModel.Commands;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows.Controls;

namespace FamilyReporter
{
    public class TreeViewItemBase : CommandBase,  INotifyPropertyChanged
    {
        

        protected virtual void LoadChildren()
        {
            // this is overridden. -Required function? gives the option to override in ViewModel
        }

        protected TreeViewItemBase(TreeViewItemBase parent)
        {
            this._parent = parent;
            this._children = new ObservableCollection<TreeViewItemBase>();
        }

        private int _level;
        public int Level { get { return _level; } set { _level = value; } }

        private string _ItemName;
        public string ItemName { get { return _ItemName; } set { _ItemName = value; } }

        readonly TreeViewItemBase _parent;
        public TreeViewItemBase Parent { get { return _parent; } }

        readonly ObservableCollection<TreeViewItemBase> _children;
        public ObservableCollection<TreeViewItemBase> Children { get { return _children; } }

        private System.Windows.Media.Brush _nodeColor;
        public System.Windows.Media.Brush NodeColor
        {
            get { return _nodeColor; }
            set
            {
                _nodeColor = value;
                NotifyPropertyChanged("NodeColor");
            }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value != _isChecked)
                {
                    _isChecked = value;
                    NotifyPropertyChanged("IsChecked");
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
                // from: http://stackoverflow.com/questions/32725973/wpf-mvvm-treeview-commands-enabled-based-on-selected-item
                // dunno if this is necessary
                //if (_isSelected)
                //{
                //    UIEventApp.m_ViewModel.SelectedItem = this;
                //    CommandManager.InvalidateRequerySuggested();
                //}
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }

        #region PUBLIC METHODS


        internal List<ImportInstanceViewModel> GetAllImportViewModels()
        {
            List<ImportInstanceViewModel> importVMs = new List<ImportInstanceViewModel>();

            foreach(TreeViewItemBase child in this.Children)
            {
                if(child.GetType() == typeof(ImportInstanceViewModel))
                {
                    importVMs.Add(child as ImportInstanceViewModel);
                }
            }

            return importVMs;
        }


        public List<ImportSubCategoryViewModel> GetAllImportSubCategoryViewModels()
        {
            List<ImportSubCategoryViewModel> subCats = new List<ImportSubCategoryViewModel>();

            foreach (TreeViewItemBase child in this.Children)
            {
                if (child.GetType() == typeof(ImportInstanceViewModel))
                {
                    // this does not seem to work reliably
                    subCats.AddRange(child.Children.Cast<ImportSubCategoryViewModel>().Select(x => x));
                }

                else if (child.GetType() == typeof(DocumentViewModel))
                {
                    subCats.AddRange((child as DocumentViewModel).GetAllImportSubCategoryViewModels());
                }

                else if (child.GetType() == typeof(ImportSubCategoryViewModel))
                {
                    subCats.Add(child as ImportSubCategoryViewModel);
                }
            }

            return subCats;
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
