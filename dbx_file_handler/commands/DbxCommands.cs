using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dbx_lib;

namespace dbx_file_handler.commands {
    interface ICommand {
        void invoke();
    }

    class PopulateDatabaseCommand : ICommand {
        public delegate void CommandDelegate(string info = "");
        public delegate void CommandProgress(ProgressData data);
        public event CommandDelegate onSuccess;
        public event CommandDelegate onError;
        public event CommandProgress onProgress;

        private string _root;
        private bool _load;

        public PopulateDatabaseCommand(string rootDir, bool load) {
            _root = rootDir;
            _load = load;
        }

        public void invoke() {
            var start = DateTime.Now;
            try {
                if (_load) {
                    DbxApplication.DBX.loadDatabase(_root);
                } else {                    
                    var files = DbxApplication.DBX.GetFilesOfType(_root, "dbx");
                    Console.WriteLine("Finding all DBX-files in folder {0} took: {1}ms", _root, (DateTime.Now - start).TotalMilliseconds);
                    DbxApplication.DBX.PopulateAssets(files, reportedProgress);
                    Console.WriteLine("All done in {0}ms", (DateTime.Now - start).TotalMilliseconds);
                }
                if (onSuccess != null)
                    onSuccess.Invoke();
            } catch (Exception e) {
                if (onError != null)
                    onError.Invoke(e.ToString());
            }
        }

        private void reportedProgress(dbx_lib.ProgressData data)
        {
            Console.WriteLine("onProgress, {0}", (data.FilesCompleted / (float)data.FilesTotal));
            if (onProgress != null)
            {
                onProgress.Invoke(data);
            }
        }
    }
}
