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
        }

        private void clickPopulate(object sender, RoutedEventArgs e) {
            clearCommand();
            _cmd = new PopulateDatabaseCommand(this._dbxRoot.Text, false);
            _cmd.onSuccess += cmd_onSuccess;
            _cmd.onError += cmd_onError;
            _cmd.invoke();
        }

        void cmd_onSuccess(string info) {
            MessageHandler.queueMessage(new DatabasePopulatedMessage());
        }

        void cmd_onError(string info) {
            MessageBox.Show("Unknown error, check paths!");
            clearCommand();
        }

        private void clearCommand() {
            if (_cmd != null) {
                _cmd.onSuccess -= cmd_onSuccess;
                _cmd.onError -= cmd_onError;
                _cmd = null;
            }
        }

        public void onInit() {
            
        }

        public void onDeinit() {
            clearCommand();
        }

        private void clickLoad(object sender, RoutedEventArgs e) {
            var fd = new OpenFileDialog();
            fd.Filter = "DB-Files (.mdb)|*.mdb|All Files (*.*)|*.*";
            fd.FilterIndex = 0;
            fd.Multiselect = false;
            fd.InitialDirectory = Environment.CurrentDirectory;
            if (fd.ShowDialog() == true) {
                clearCommand();
                _cmd = new PopulateDatabaseCommand(fd.FileName, true);
                _cmd.onSuccess += cmd_onSuccess;
                _cmd.onError += cmd_onError;
                _cmd.invoke();
            }
        }
    }
}
