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
    public class ProgressData
    {
        public int FilesTotal { get; private set; }
        public int BytesTotal { get; private set; }
        public int FilesCompleted { get; private set; }
        public int BytesCompleted { get; private set; }
        public bool Finished { get; private set; }

        private ProgressData()
        {
        }

        internal ProgressData(int files, int bytes, int filesDone, int bytesDone, bool finished)
        {
            
            FilesTotal = files;
            BytesTotal = bytes;
            BytesCompleted = bytesDone;
            FilesCompleted = filesDone;
            Finished = finished;
        }
    }

    public class LibMain
    {
        public delegate void ProgressCallback(ProgressData data);

        public FileInfo[] GetDbxFiles(string rootFolder)
        {
            var dir = new DirectoryInfo(rootFolder.ToLower());
            if (!dir.Exists)
                throw new ArgumentException("Folder does not exist");
            Console.WriteLine("Searching for all dbx-files in folder:\n\t{0}", dir.FullName);
            return dir.GetFiles("*.dbx", SearchOption.AllDirectories);
        }

        public bool HasAsset(string guid) {
            return AssetDatabase.containsAsset(guid);
        }

        public bool HasAsset(FileInfo file) {
            return AssetDatabase.containsAsset(file);
        }

        public DiceAsset GetDiceAsset(string guid)
        {
            return AssetDatabase.getAsset(guid);
        }

        public DiceAsset GetDiceAsset(FileInfo file)
        {
            return AssetDatabase.getAsset(file);
        }

        public void PopulateAssets(FileInfo[] files, ProgressCallback progressCallback = null)
        {
            AssetDatabase.PopulateAsset(files, progressCallback);
        }

        public void saveDatabase(string path) {
            AssetDatabase.saveDatabase(path);
        }

        public void loadDatabase(string path) {
            AssetDatabase.loadDatabase(path);
        }

        public List<string> getAllFilePaths() {
            return AssetDatabase.getAllFilePaths();
        }

        public List<DiceAsset> getAllAssets()
        {
            return AssetDatabase.getAllAssets();
        }
    }
}
