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
    public partial class ShowDatabaseScreen : UserControl, IScreen {
        private class DiceAssetTreeItem : TreeViewItem {
            public int ListIndex { get; private set; }
            public string Path { get; private set; }

            public DiceAssetTreeItem(string path, int listIndex) {
                base.Header = path;
                Path = path;
                ListIndex = listIndex;
                base.ExpandSubtree();
                base.IsExpanded = false;
            }
        }

        private class AssetDirectoryEncapsulator {
            public bool Expanded { get; set; }
            public int IndexEnd { get; set; }
            public int IndexStart { get; set; }
            public string FolderName { get; set; }
            public string FullPath { get; set; }
            public List<AssetDirectoryEncapsulator> Children { get; private set; }
            public AssetDirectoryEncapsulator Parent { get; set; }
            public int ListIndex { get; set; }

            public AssetDirectoryEncapsulator() {
                Children = new List<AssetDirectoryEncapsulator>(0);
            }

            public void addChild(AssetDirectoryEncapsulator asset) {
                if (asset.Parent != null)
                    throw new Exception("Foobar!");

                Children.Add(asset);
                asset.Parent = this;
            }
        }
        
        private List<string> _files;
        private TreeView _tree;
        private List<AssetDirectoryEncapsulator> _assets;

        public ShowDatabaseScreen() {
            InitializeComponent();
            _tree = new TreeView();
            _left.Children.Add(_tree);
            _files = DbxApplication.DBX.getAllFilePaths();
            _files.Sort();
            _assets = new List<AssetDirectoryEncapsulator>();
            _tree.SelectedItemChanged += _tree_SelectedItemChanged;
            _statusBar.Content = "Assets in Database: " + _files.Count;
            _tree.MouseRightButtonUp += _tree_MouseRightButtonUp;
        }

        void _tree_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = (DiceAssetTreeItem)_tree.SelectedItem;
            if (item == null)
                return;

            var data = _assets[item.ListIndex];
            var file = new FileInfo(data.FullPath);
            var asset = DbxApplication.DBX.GetDiceAsset(file);
            var assets = getAssetsRelatedToItem(data);
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
                        missingAssets.AppendLine(string.Format("{0}: ref={1}", foo.FilePath, parent));
                }

                foreach (var child in foo.getChildren())
                {
                    if (DbxApplication.DBX.HasAsset(child))
                        allChildren.Add(DbxApplication.DBX.GetDiceAsset(child));
                    else
                        missingAssets.AppendLine(string.Format("{0}: ref={1}", foo.FilePath, child));
                }
            }

            var parents = allParents.Distinct().ToList().OrderBy(a => a.FilePath);
            var children = allChildren.Distinct().ToList().OrderBy(a => a.FilePath);

            foreach (var par in parents)
            {
                sbparent.AppendLine(par.FilePath);
            }
            foreach (var c in children)
            {
                sbchild.AppendLine(c.FilePath);
            }

            var p = new Paragraph();
            p.Inlines.Add(sbparent.ToString());
            _parents.Document = new FlowDocument(p);

            p = new Paragraph();
            p.Inlines.Add(sbchild.ToString());
            _children.Document = new FlowDocument(p);

            string s = "";
            if (data.IndexStart == data.IndexEnd && DbxApplication.DBX.HasAsset(file))
            {
                s += createAssetInfoString(asset);
            }

            if (missingAssets.Length > 0)
            {
                s += "\nMissing Assets:\n" + missingAssets.ToString();
            }
            _assetInfo.Text = s;            
        }

        void _tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            _assetInfo.Text = "";
            _parents.Document = null;
            _children.Document = null;
            var item = (DiceAssetTreeItem)_tree.SelectedItem;
            if (item != null) {
                var data = _assets[item.ListIndex];
                var file = new FileInfo(data.FullPath);
                if (data.IndexStart == data.IndexEnd && DbxApplication.DBX.HasAsset(file)) {
                    var asset = DbxApplication.DBX.GetDiceAsset(file);
                    _assetInfo.Text = createAssetInfoString(asset);
                }
            } else {
                _assetInfo.Text = "Null value selected in TreeView!";
            }
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

            var asset = new AssetDirectoryEncapsulator() { IndexStart = 0, IndexEnd = _files.Count - 1, FolderName = samePath, FullPath = samePath, ListIndex = 0 };
            _assets.Add(asset);
            var item = new DiceAssetTreeItem(asset.FolderName, asset.ListIndex);
            item.Expanded += item_Expanded;
            _tree.Items.Add(item);
            item.Items.Add("");
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
            if (assets.Expanded)
                return;

            if (assets.IndexStart == assets.IndexEnd) {
                assets.Expanded = true;
                var fileName = assets.FullPath.Substring(assets.FullPath.LastIndexOf('\\') + 1);
                var child = new AssetDirectoryEncapsulator() { Expanded = true, FolderName = fileName, ListIndex = _assets.Count, FullPath = assets.FullPath };
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

                            lastAsset.Expanded = true;
                            if (!lastAsset.FolderName.EndsWith(".dbx")) {
                                int fooIndex = lastAsset.FullPath.LastIndexOf('\\');
                                lastAsset.FolderName = lastAsset.FolderName + lastAsset.FullPath.Substring(fooIndex);
                            }

                        }
                    }
                    lastFolderName = folderName;
                    lastAsset = new AssetDirectoryEncapsulator() { FolderName = folderName, IndexStart = i, IndexEnd = i, FullPath = path, ListIndex = _assets.Count, Expanded = endidx < 0 };
                    assets.addChild(lastAsset);
                    _assets.Add(lastAsset);
                } else {
                    if (lastAsset != null) {
                        lastAsset.IndexEnd = i;
                    }
                }
            }
            assets.Expanded = true;
        }

        void item_Expanded(object sender, RoutedEventArgs e) {
            var item = (DiceAssetTreeItem)sender;
            var data = _assets[item.ListIndex];

            if (item.Items.Count > 0 && (item.Items[0] is DiceAssetTreeItem == false))
                item.Items.Clear();

            if (data.Expanded == false) {
                getExpandedView(ref data);
                foreach (var dataChild in data.Children) {
                    var child = new DiceAssetTreeItem(dataChild.FolderName, dataChild.ListIndex);
                    child.Expanded += item_Expanded;
                    item.Items.Add(child);

                    if (dataChild.IndexEnd > dataChild.IndexStart) {
                        child.Items.Add("");
                    }
                }
            }
        }

        public void onDeinit() {
            //throw new NotImplementedException();
        }

        private List<DiceAsset> getAssetsRelatedToItem(AssetDirectoryEncapsulator asset)
        {
            List<DiceAsset> assets = new List<DiceAsset>();
            for(int i=asset.IndexStart; i <= asset.IndexEnd; ++i) {
                assets.Add(DbxApplication.DBX.GetDiceAsset(new FileInfo(_files[i])));
            }
            return assets;
        }
    }
}
