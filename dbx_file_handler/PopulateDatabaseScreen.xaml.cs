using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using dbx_file_handler.commands;
using Microsoft.Win32;

namespace dbx_file_handler {
    /// <summary>
    /// Interaction logic for PopulateDatabaseScreen.xaml
    /// </summary>
    public partial class PopulateDatabaseScreen : UserControl, IScreen {
        PopulateDatabaseCommand _cmd;

        public PopulateDatabaseScreen() {
            InitializeComponent();
            _dbxRoot.Text = DbxApplication.SearchFolder;
        }

        private void clickPopulate(object sender, RoutedEventArgs e) {
            resetState();
            _cmd = new PopulateDatabaseCommand(this._dbxRoot.Text, false);
            _cmd.onSuccess += cmd_onSuccess;
            _cmd.onError += cmd_onError;
            _cmd.onProgress += _cmd_onProgress;

            _progressText.Content = "Searching for all DBX-files in folder: " + _dbxRoot.Text;
            _popBtn.IsEnabled = false;
            _loadBtn.IsEnabled = false;
            _dbxRoot.IsReadOnly = true;
            Task.Factory.StartNew(() => _cmd.invoke());
            DbxApplication.SearchFolder = _dbxRoot.Text;
        }

        void _cmd_onProgress(dbx_lib.ProgressData data)
        {
            this.Dispatcher.Invoke(() =>
            {
                var fileProgress = (data.FilesCompleted / (float)data.FilesTotal);
                _progressText.Content = String.Format("Progress: {0}%", (int)(fileProgress * 100));
            });
        }

        void cmd_onSuccess(string info) {
            MessageHandler.queueMessage(new DatabasePopulatedMessage());
        }

        void cmd_onError(string info) {
            MessageBox.Show("Unknown error, check paths!");
            resetState();
        }

        private void resetState() {
            if (_cmd != null) {
                _cmd.onSuccess -= cmd_onSuccess;
                _cmd.onError -= cmd_onError;
                _cmd = null;
            }
            _popBtn.IsEnabled = true;
            _loadBtn.IsEnabled = true;
            _dbxRoot.IsReadOnly = false;
        }

        public void onInit() {
            
        }

        public void onDeinit() {
            resetState();
            DbxApplication.saveInfo();
        }

        private void clickLoad(object sender, RoutedEventArgs e) {
            var fd = new OpenFileDialog();
            fd.Filter = "DB-Files (.mdb)|*.mdb|All Files (*.*)|*.*";
            fd.FilterIndex = 0;
            fd.Multiselect = false;
            fd.InitialDirectory = Environment.CurrentDirectory;
            if (fd.ShowDialog() == true) {
                resetState();
                _cmd = new PopulateDatabaseCommand(fd.FileName, true);
                _cmd.onSuccess += cmd_onSuccess;
                _cmd.onError += cmd_onError;
                _cmd.invoke();
            }
        }
    }
}
