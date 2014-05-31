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

            Console.WriteLine("Searching for all dbx-files in folder:\n\t{0}", dir.FullName);
            return dir.GetFiles("*.dbx", SearchOption.AllDirectories);
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

        public void PopulateAssets(FileInfo[] files)
        {
            AssetDatabase.PopulateAsset(files);
        }
    }
}
