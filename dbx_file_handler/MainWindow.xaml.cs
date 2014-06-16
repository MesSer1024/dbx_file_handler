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
using dbx_lib;

namespace dbx_file_handler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMessageListener
    {
        private IScreen _screen;

        public MainWindow()
        {
            InitializeComponent();
            DbxApplication.init();
            MessageHandler.addListener(this);
            _screen = new PopulateDatabaseScreen();
            _content.Children.Add(_screen as PopulateDatabaseScreen);
            _screen.onInit();
        }

        public void onMessage(IMessage msg) {
            if (msg is DatabasePopulatedMessage) {
                this.Dispatcher.Invoke(() => {
                    var databaseMessage = msg as DatabasePopulatedMessage;
                    _screen.onDeinit();
                    _content.Children.Clear();

                    var outputFile = String.Format("output/db.mdb", DateTime.Now.Ticks);
                    DbxApplication.DBX.saveDatabase(System.IO.Path.Combine(Environment.CurrentDirectory, outputFile));

                    _screen = new ShowDatabaseScreen();
                    _screen.onInit();
                    _content.Children.Add(_screen as ShowDatabaseScreen);
                });
            }
        }
    }
}
