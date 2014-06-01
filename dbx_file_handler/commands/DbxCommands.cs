using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbx_file_handler.commands {
    interface ICommand {
        void invoke();
    }

    class PopulateDatabaseCommand : ICommand {
        public delegate void CommandDelegate(string info = "");
        public event CommandDelegate onSuccess;
        public event CommandDelegate onError;
        private string _root;
        private bool _load;

        public PopulateDatabaseCommand(string rootDir, bool load) {
            _root = rootDir;
            _load = load;
        }

        public void invoke() {
            try {
                if (_load) {
                    DbxApplication.DBX.loadDatabase(_root);
                } else {
                    var files = DbxApplication.DBX.GetDbxFiles(_root);
                    DbxApplication.DBX.PopulateAssets(files);
                }
                if (onSuccess != null)
                    onSuccess.Invoke();
            } catch (Exception e) {
                if (onError != null)
                    onError.Invoke(e.ToString());
            }
        }
    }
}
