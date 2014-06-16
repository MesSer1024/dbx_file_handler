using System;
using System.Collections.Generic;
using System.IO;
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
using dbx_lib.assets;

namespace dbx_file_handler {
    /// <summary>
    /// Interaction logic for ShowDatabaseScreen.xaml
    /// </summary>
    public partial class ShowDatabaseScreen : UserControl, IScreen
    {
        #region HelpClasses
        private class AssetDirectoryEncapsulator : TreeViewItem {
            public bool HaveBuiltChildAssets { get; set; }
            public int IndexEnd { get; set; }
            public int IndexStart { get; set; }
            public string FolderName { get { return base.Header.ToString(); } set { base.Header = value; } }
            public string FullPath { get; set; }
            public List<AssetDirectoryEncapsulator> Children { get; private set; }
            public AssetDirectoryEncapsulator AssetParent { get; set; }
            public int ListIndex { get; set; }

            public AssetDirectoryEncapsulator(string folderName) {
                Children = new List<AssetDirectoryEncapsulator>(0);
                base.Header = folderName;
                base.ExpandSubtree();
                base.IsExpanded = false;
            }

            public void addChild(AssetDirectoryEncapsulator asset) {
                if (asset.AssetParent != null)
                    throw new Exception("Foobar!");

                Children.Add(asset);
                asset.AssetParent = this;
            }
        }
        #endregion

        private List<string> _files;
        private List<AssetDirectoryEncapsulator> _assets;
        private IEnumerable<DiceAsset> _excludedItems;
        private AssetDirectoryEncapsulator _lastSelectedItem;
        private AssetDirectoryEncapsulator SelectedItem { get { return _tree.SelectedItem as AssetDirectoryEncapsulator; } }

        public ShowDatabaseScreen() {
            InitializeComponent();
            _excludedItems = new List<DiceAsset>();
            _files = DbxApplication.DBX.getAllFilePaths();
            _files.Sort();
            _assets = new List<AssetDirectoryEncapsulator>();
            _statusBar.Content = "Assets in Database: " + _files.Count;
            _tree.KeyUp += _tree_KeyUp;
        }

        void _tree_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Return)
            {
                showAdvancedAssetInformation((AssetDirectoryEncapsulator)_tree.SelectedItem);
            }
        }

        void showAdvancedAssetInformation(AssetDirectoryEncapsulator item)
        {
            if (item == null)
                return;
            if (_lastSelectedItem != null)
            {
                _lastSelectedItem.Background = item.Background;
            }

            _lastSelectedItem = item;
            item.Background = Brushes.Yellow;

            var file = new FileInfo(item.FullPath);
            var assets = getAssetsRelatedToItem(item);
            var sbparent = new StringBuilder();
            var sbchild = new StringBuilder();
            var missingAssets = new StringBuilder();

            List<DiceAsset> allParents = new List<DiceAsset>();
            List<DiceAsset> allChildren = new List<DiceAsset>();
            foreach (var foo in assets)
            {
                foreach (var parent in foo.getParents())
                {
                    if (DbxApplication.DBX.HasAsset(parent))
                        allParents.Add(DbxApplication.DBX.GetDiceAsset(parent));
                    else
                        missingAssets.AppendLine(string.Format("{0} :: ref={1}", foo.FilePath, parent));
                }

                foreach (var child in foo.getChildren())
                {
                    if (DbxApplication.DBX.HasAsset(child))
                        allChildren.Add(DbxApplication.DBX.GetDiceAsset(child));
                    else
                        missingAssets.AppendLine(string.Format("{0}: ref={1}", foo.FilePath, child));
                }
            }

            var parents = allParents.Except(_excludedItems).Distinct().OrderBy(a => a.FilePath);
            var children = allChildren.Except(_excludedItems).Distinct().OrderBy(a => a.FilePath);

            foreach (var par in parents)
            {
                sbparent.AppendLine(par.FilePath);
            }
            foreach (var c in children)
            {
                sbchild.AppendLine(c.FilePath);
            }
            _pHeader.Content = String.Format("Parents: ({0})", parents.Count());
            _cHeader.Content = String.Format("Children: ({0})", children.Count());
            var p = new Paragraph();
            p.Inlines.Add(sbparent.ToString());
            p.FontSize = 11;
            p.FontFamily = new FontFamily("segoe ui");
            _parents.Document = new FlowDocument(p);

            p = new Paragraph();
            p.FontFamily = new FontFamily("segoe ui");
            p.Inlines.Add(sbchild.ToString());
            p.FontSize = 11;
            _children.Document = new FlowDocument(p);

            string s = "";
            if (item.IndexStart == item.IndexEnd && DbxApplication.DBX.HasAsset(file)) {
                s += createAssetInfoString(DbxApplication.DBX.GetDiceAsset(file));
            } else {
                s += item.FolderName + Environment.NewLine;
            }

            if (missingAssets.Length > 0)
            {
                s += "\nFiles containing links to items not existing in DB:\n" + missingAssets.ToString();
            }
            _assetInfo.Text = s;
        }

        private string createAssetInfoString(dbx_lib.assets.DiceAsset asset) {
            var sb = new StringBuilder();
            sb.AppendLine("Name: " + asset.Name);
            sb.AppendLine("Guid: " + asset.Guid);
            sb.AppendLine("Type: " + asset.Type);
            sb.AppendLine("Path: " + asset.FilePath);
            sb.AppendLine("Children: " + asset.getChildren().Count);
            sb.AppendLine("Parents: " + asset.getParents().Count);
            return sb.ToString();
        }

        public void onInit() {
            var path1 = _files[0];
            var path2 = _files[_files.Count - 1];

            var samePath = getCommonString(path1, path2);
            samePath = samePath.Substring(0, samePath.LastIndexOf('\\') + 1);
            try
            {
                var asset = new AssetDirectoryEncapsulator(samePath) { IndexStart = 0, IndexEnd = _files.Count - 1, ListIndex = 0, FullPath = samePath };
                _assets.Add(asset);
                SetTreeStyle(asset, true);
                asset.Expanded += item_Expanded;
                _tree.Items.Add(asset);
                asset.Items.Add("");
                _tree.SelectedItemChanged += _tree_SelectedItemChanged;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void _tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //clearInfoText();
        }

        private void clearInfoText()
        {
            _pHeader.Content = "Parents";
            _cHeader.Content = "Children";
            _assetInfo.Text = "";
            _parents.Document = null;
            _children.Document = null;
        }

        private string getCommonString(string s1, string s2) {
            var pathNameEndIdx = Math.Min(s1.Length, s2.Length);

            int i = 0;
            while (i < s1.Length && i < s2.Length) {
                if (s1[i] != s2[i]) {
                    break;
                }
                i++;
            }

            return s1.Substring(0, i);
        }

        private void getExpandedView(ref AssetDirectoryEncapsulator assets) {
            if (assets.HaveBuiltChildAssets)
                return;

            if (assets.IndexStart == assets.IndexEnd) {
                assets.HaveBuiltChildAssets = true;
                var fileName = assets.FullPath.Substring(assets.FullPath.LastIndexOf('\\') + 1);
                var child = new AssetDirectoryEncapsulator(fileName) { HaveBuiltChildAssets = true, ListIndex = _assets.Count, FullPath = assets.FullPath };
                assets.addChild(child);
                _assets.Add(child);
                return;
            }

            var path1 = _files[assets.IndexStart];
            var path2 = _files[assets.IndexEnd];
            var samePath = getCommonString(path1, path2);
            samePath = samePath.Substring(0, samePath.LastIndexOf('\\') + 1);

            int idx = samePath.Length;
            
            string lastFolderName = null;
            AssetDirectoryEncapsulator lastAsset = null;

            for (int i = assets.IndexStart; i <= assets.IndexEnd; ++i) {
                var path = _files[i];
                var endidx = path.IndexOf('\\', idx);
                string folderName = null;
                if (endidx > 0) {
                    folderName = path.Substring(idx, endidx - idx);
                } else {
                    folderName = path.Substring(idx);
                }

                bool workingInsideNewDirectory = folderName != lastFolderName;
                if (workingInsideNewDirectory) {
                    if (lastAsset != null) {
                        if (lastAsset.IndexEnd > lastAsset.IndexStart) {
                            lastAsset.FolderName = String.Format("{0}: ({1})", lastAsset.FolderName, lastAsset.IndexEnd - lastAsset.IndexStart + 1);
                        } else {
                            //case 1: asset contains "awards.dbx"
                            //case 2: asset contains "commander" which in turn contains one file
                            //case 3: asset contains "flow" which contains only one folder, "logic" which contains x files, ends up with middle directory being stripped

                            lastAsset.HaveBuiltChildAssets = true;
                            if (!lastAsset.FolderName.EndsWith(".dbx")) {
                                int fooIndex = lastAsset.FullPath.LastIndexOf('\\');
                                lastAsset.FolderName = lastAsset.FolderName + lastAsset.FullPath.Substring(fooIndex);
                            }

                        }
                    }
                    lastFolderName = folderName;
                    lastAsset = new AssetDirectoryEncapsulator(folderName) { IndexStart = i, IndexEnd = i, ListIndex = _assets.Count, HaveBuiltChildAssets = endidx < 0, FullPath = path };
                    assets.addChild(lastAsset);
                    _assets.Add(lastAsset);
                } else {
                    if (lastAsset != null) {
                        lastAsset.IndexEnd = i;
                    }
                }
            }
            assets.HaveBuiltChildAssets = true;
        }

        void item_Expanded(object sender, RoutedEventArgs e) {
            var item = (AssetDirectoryEncapsulator)sender;

            if (item.Items.Count > 0 && (item.Items[0] is AssetDirectoryEncapsulator == false))
                item.Items.Clear();

            if (item.HaveBuiltChildAssets == false) {
                getExpandedView(ref item);
                foreach (var dataChild in item.Children) {
                    dataChild.Expanded += item_Expanded;
                    item.Items.Add(dataChild);

                    if (dataChild.IndexEnd > dataChild.IndexStart) {
                        dataChild.Items.Add("");
                    }
                }
            }
        }

        public void onDeinit() {
            _tree.KeyUp -= _tree_KeyUp;
            //todo: go through all items and remove event listener for them?
            _tree.Items.Clear();
        }

        private List<DiceAsset> getAssetsRelatedToItem(AssetDirectoryEncapsulator asset)
        {
            List<DiceAsset> assets = new List<DiceAsset>();
            for(int i=asset.IndexStart; i <= asset.IndexEnd; ++i) {
                assets.Add(DbxApplication.DBX.GetDiceAsset(new FileInfo(_files[i])));
            }
            return assets;
        }


        private void onShowAssetInformation(object sender, RoutedEventArgs e)
        {
            showAdvancedAssetInformation(SelectedItem);
        }

        private void onExcludeAssets(object sender, RoutedEventArgs e)
        {
            SetTreeStyle(SelectedItem, false);
            var assets = getAssetsRelatedToItem(SelectedItem);
            _excludedItems = _excludedItems.Concat(assets).Distinct();
        }

        private void onReincludeAssets(object sender, RoutedEventArgs e)
        {
            SetTreeStyle(SelectedItem, true);
            var assets = getAssetsRelatedToItem(SelectedItem);
            _excludedItems = _excludedItems.Except(assets);
        }

        private void SetTreeStyle(AssetDirectoryEncapsulator item, bool included)
        {
            if (item.HaveBuiltChildAssets)
            {
                foreach (var child in item.Children)
                {
                    SetTreeStyle(child, included);
                }
            }
            if (included)
            {
                item.FontStyle = FontStyles.Normal;
                item.FontWeight = FontWeights.SemiBold;
            }
            else
            {
                item.FontStyle = FontStyles.Italic;
                item.FontWeight = FontWeights.Normal;
            }
        }
    }
}
