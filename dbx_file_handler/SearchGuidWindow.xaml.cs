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
using dbx_lib.assets;

namespace dbx_file_handler
{
    /// <summary>
    /// Interaction logic for SearchGuidWindow.xaml
    /// </summary>
    public partial class SearchGuidWindow : UserControl
    {
        public SearchGuidWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            doSearch();
        }

        private void TextBox_KeyUp_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                e.Handled = true;
                doSearch();
            }
        }

        private void doSearch()
        {
            var guid = _guid.Text.ToLower();
            DiceAsset asset = null;
            var usages = new List<DiceAsset>();
            
            //check if guid exists in asset database
            try
            {
                asset = DbxApplication.DBX.GetDiceAsset(guid);
            }
            catch (Exception e)
            {

            }

            //check if any asset has guid as referred child
            foreach (var foo in DbxApplication.DBX.getAllAssets())
            {
                foreach (var child in foo.getChildren())
                {
                    if (child == guid)
                    {
                        usages.Add(foo);
                    }
                }
            }
            presentSearchResult(asset, usages);
        }

        private void presentSearchResult(DiceAsset asset, List<DiceAsset> referenceToGuid)
        {
            if (asset == null && referenceToGuid.Count == 0)
            {
                _content.Text = "Could not find guid in asset database and no asset seems to reference it!";
            }
            else
            {
                var sb = new StringBuilder();
                if(asset == null) {
                    sb.AppendLine("GUID not found in database");
                } else {
                    sb.AppendLine("GUID is referring to asset:\n\t" + asset.FilePath);
                }
                
                if (referenceToGuid.Count == 0) {
                    sb.AppendLine("No asset in database is referring to this GUID");
                } else {
                    sb.AppendLine("Following assets are referring to this GUID:");
                    foreach (var item in referenceToGuid)
                    {
                        sb.AppendLine("\t" + item.FilePath);
                    }
                }

                _content.Text = sb.ToString();
            }
        }

        private void onClose(object sender, RoutedEventArgs e)
        {
            MessageHandler.queueMessage(new CloseOverlayScreen(this));
        }
    }
}
