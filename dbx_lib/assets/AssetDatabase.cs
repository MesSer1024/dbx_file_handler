using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;

namespace dbx_lib.assets
{
    using FBGuid = System.String;

    class AssetDatabaseState {
        public ConcurrentDictionary<FBGuid, DiceAsset> GuidAssetTable = new ConcurrentDictionary<FBGuid, DiceAsset>();
        public ConcurrentDictionary<String, FBGuid> FileGuidTable = new ConcurrentDictionary<String, FBGuid>();
    }
    
    static class AssetDatabase
    {
        private static ConcurrentDictionary<FBGuid, DiceAsset> _guidAssetTable = new ConcurrentDictionary<FBGuid, DiceAsset>();
        private static ConcurrentDictionary<string, FBGuid> _fileGuidTable = new ConcurrentDictionary<string, FBGuid>();
        private static object _lock = new object();

        public static void PopulateAsset(FileInfo[] files)
        {
            Console.WriteLine("Populating database assets: {0} files", files.Length);
            var start = DateTime.Now;
            int count = 0;
            foreach (var file in files)
            {
                doBuildAsset(file);
                count++;
                if (count % 101 == 100)
                {
                    Console.Write(".");
                }
            }
            Console.WriteLine("\nBuilding/Parsing {0} assets took {1}ms", count, (DateTime.Now - start).TotalMilliseconds);
            start = DateTime.Now;
            updateReferencesInDatabase();
            Console.WriteLine("Checking cross-references for {0} assets took {1}ms", _guidAssetTable.Count, (DateTime.Now - start).TotalMilliseconds);
        }

        public static void PopulateAsset(FileInfo file)
        {
            doBuildAsset(file);
            updateReferencesInDatabase();
        }

        internal static void doBuildAsset(FileInfo dbxFile)
        {
            if (containsAsset(dbxFile))
                throw new Exception("Todo? Allow? Replace? Exception? Overwrite?");

            var asset = DiceAsset.Create(dbxFile);
            _guidAssetTable.TryAdd(asset.Guid, asset);
            _fileGuidTable.TryAdd(dbxFile.FullName, asset.Guid);
        }

        private static void updateReferencesInDatabase()
        {
            Console.WriteLine("Setting up parent/child relationships between {0} assets", _guidAssetTable.Count);
            foreach (var asset in _guidAssetTable.Values.ToList())
            {
                foreach (var child in asset.getChildren())
                {
                    if (_guidAssetTable.ContainsKey(child))
                        _guidAssetTable[child].addParentIfUnique(asset.Guid);
                }
            }
        }

        public static bool containsAsset(FileInfo file)
        {
            return _fileGuidTable.ContainsKey(file.FullName);
        }

        public static bool containsAsset(FBGuid guid) {
            return _guidAssetTable.ContainsKey(guid);
        }

        internal static DiceAsset getAsset(FileInfo file)
        {
            return _guidAssetTable[_fileGuidTable[file.FullName]];
        }

        internal static DiceAsset getAsset(FBGuid guid) {
            return _guidAssetTable[guid];
        }

        internal static void loadDatabase(string path) {
            var file = new FileInfo(path);
            if (!file.Exists)
                throw new Exception("file does not exist!");

            var state = JsonConvert.DeserializeObject<AssetDatabaseState>(File.ReadAllText(file.FullName));
            _guidAssetTable = state.GuidAssetTable;
            _fileGuidTable = state.FileGuidTable;
        }

        internal static void saveDatabase(string path) {
            var file = new FileInfo(path);
            if (!file.Directory.Exists)
                file.Directory.Create();

            var state = new AssetDatabaseState() { FileGuidTable = _fileGuidTable, GuidAssetTable = _guidAssetTable };
            var output = JsonConvert.SerializeObject(state);

            using (var sw = new StreamWriter(file.FullName)) {
                sw.Write(output);
                sw.Flush();
            }
        }
    }
}
