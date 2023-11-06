using FamilyReporter.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FamilyReporter
{
    public class CommandBase
    {
        private ICommand _openDocumentCmd;
        public ICommand OpenDocumentCmd
        {
            get
            {
                if (_openDocumentCmd == null)
                {
                    _openDocumentCmd = new OpenDocumentCmd();
                }
                return _openDocumentCmd;
            }
            set
            {
                _openDocumentCmd = value;
            }
        }

        private ICommand _purgeSelImportedStylesInImportCmd;
        public ICommand PurgeSelImportedStylesInImportCmd
        {
            get
            {
                if (_purgeSelImportedStylesInImportCmd == null)
                {
                    _purgeSelImportedStylesInImportCmd = new PurgeSelImportedStylesInImportCmd();
                }
                return _purgeSelImportedStylesInImportCmd;
            }
            set
            {
                _purgeSelImportedStylesInImportCmd = value;
            }
        }

        private ICommand _purgeAllImportedStylesInImportCmd;
        public ICommand PurgeAllImportedStylesInImportCmd
        {
            get
            {
                if (_purgeAllImportedStylesInImportCmd == null)
                {
                    _purgeAllImportedStylesInImportCmd = new PurgeAllImportedStylesInImportCmd();
                }
                return _purgeAllImportedStylesInImportCmd;
            }
            set
            {
                _purgeAllImportedStylesInImportCmd = value;
            }
        }

        private ICommand _purgeAllImportedStylesInDocCmd;
        public ICommand PurgeAllImportedStylesInDocCmd
        {
            get
            {
                if (_purgeAllImportedStylesInDocCmd == null)
                {
                    _purgeAllImportedStylesInDocCmd = new PurgeAllImportedStylesInDocCmd();
                }
                return _purgeAllImportedStylesInDocCmd;
            }
            set
            {
                _purgeAllImportedStylesInDocCmd = value;
            }
        }

        private ICommand _deleteImportCmd;
        public ICommand DeleteImportCmd
        {
            get
            {
                if (_deleteImportCmd == null)
                {
                    _deleteImportCmd = new DeleteImportCmd();
                }
                return _deleteImportCmd;
            }

            set
            {
                _deleteImportCmd = value;
            }
        }

        private ICommand _deleteChkdImportCmd;
        public ICommand DeleteChkdImportCmd
        {
            get
            {
                if (_deleteChkdImportCmd == null)
                {
                    _deleteChkdImportCmd = new DeleteChkdImportCmd();
                }
                return _deleteChkdImportCmd;
            }

            set
            {
                _deleteChkdImportCmd = value;
            }
        }

        private ICommand _deleteDocumentCmd;
        public ICommand DeleteDocumentCmd
        {
            get
            {
                if (_deleteDocumentCmd == null)
                {
                    _deleteDocumentCmd = new DeleteDocumentCmd();
                }
                return _deleteDocumentCmd;
            }

            set
            {
                _deleteDocumentCmd = value;
            }
        }

        //private ICommand _purgeAllImportsInDocCmd;
        //public ICommand PurgeAllImportsInDocCmd
        //{
        //    get
        //    {
        //        if (_purgeAllImportsInDocCmd == null)
        //        {
        //            _purgeAllImportsInDocCmd = new PurgeAllImportsInDocCmd();
        //        }
        //        return _purgeAllImportsInDocCmd;
        //    }
        //    set
        //    {
        //        _purgeAllImportsInDocCmd = value;
        //    }
        //}
    }
}
