using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using dbx_lib.assets;

namespace dbx_lib
{
    public class LibMain
    {
        public FileInfo[] GetDbxFiles(string rootFolder)
        {
            var dir = new DirectoryInfo(rootFolder);
            if (!dir.Exists)
                throw new ArgumentException("Folder does not exist");

            return dir.GetFiles("*.dbx");
        }

        public DiceAsset GetDiceAsset(string filePath)
        {
            var file = new FileInfo(filePath);
            return GetDiceAsset(file);
        }

        public DiceAsset GetDiceAsset(FileInfo file)
        {
            if (AssetDatabase.containsAsset(file))
                return AssetDatabase.getAsset(file);
            if (!file.Exists)
                throw new ArgumentException("File does not exist");

            var asset = DiceAsset.Create(file);
            asset.FilePath = file.FullName;
            return asset;
        }

        public void checkFileForReferences(FileInfo file, ref List<DiceAsset> dic)
        {
            DbxUtils.updateAssetsRelatedToFile(file, ref dic);
        }

        public void PopulateAssets(FileInfo[] files)
        {
            AssetDatabase.PopulateAsset(files);
        }
    }
}
