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
    using System.Threading.Tasks;
    using System.Threading;

    class AssetDatabaseState {
        public ConcurrentDictionary<FBGuid, DiceAsset> GuidAssetTable = new ConcurrentDictionary<FBGuid, DiceAsset>();
        public ConcurrentDictionary<String, FBGuid> FileGuidTable = new ConcurrentDictionary<String, FBGuid>();
    }
    
    static class AssetDatabase
    {
        private static ConcurrentDictionary<FBGuid, DiceAsset> _guidAssetTable = new ConcurrentDictionary<FBGuid, DiceAsset>();
        private static ConcurrentDictionary<string, FBGuid> _fileGuidTable = new ConcurrentDictionary<string, FBGuid>();
        private static object _lock = new object();

        public static void PopulateAsset(FileInfo[] files, LibMain.ProgressCallback progressCallback)
        {
            var MAX_THREADS = 120;

            var start = DateTime.Now;
            int count = 0;
            var tasks = new List<Task>(files.Length);
            var modifiedFiles = files.Distinct().OrderByDescending(a => a.Length);

            var actions = new List<Action>();
            foreach(var file in modifiedFiles) {
                actions.Add(new Action(() => {
                    doBuildAsset(file);
                    var foo = Interlocked.Increment(ref count);
                    if (foo == files.Length)
                    {
                        if (progressCallback != null)
                            progressCallback.Invoke(new ProgressData(files.Length, 0, foo, 0, true));
                    }
                    else if (foo % 100 == 99)
                    {
                        if (progressCallback != null)
                            progressCallback.Invoke(new ProgressData(files.Length, 0, foo, 0, false));
                    }
                }));
            }
            System.Threading.Tasks.Parallel.Invoke(new System.Threading.Tasks.ParallelOptions() { MaxDegreeOfParallelism = MAX_THREADS }, actions.ToArray());

            start = DateTime.Now;
            updateReferencesInDatabase();
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
            _fileGuidTable.TryAdd(dbxFile.FullName.ToLower(), asset.Guid);
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
            return _fileGuidTable.ContainsKey(file.FullName.ToLower());
        }

        public static bool containsAsset(FBGuid guid) {
            return _guidAssetTable.ContainsKey(guid);
        }

        internal static DiceAsset getAsset(FileInfo file)
        {
            return _guidAssetTable[_fileGuidTable[file.FullName.ToLower()]];
        }

        internal static DiceAsset getAsset(FBGuid guid) {
            return _guidAssetTable[guid];
        }

        internal static void loadDatabase(string path) {
            var file = new FileInfo(path);
            if (!file.Exists)
                throw new Exception("file does not exist!");

            var state = JsonConvert.DeserializeObject<AssetDatabaseState>(File.ReadAllText(file.FullName.ToLower()));
            _guidAssetTable = state.GuidAssetTable;
            _fileGuidTable = state.FileGuidTable;
        }

        internal static List<string> getAllFilePaths() {
            return _fileGuidTable.Keys.ToList();
        }

        internal static List<DiceAsset> getAllAssets()
        {
            return _guidAssetTable.Values.ToList();
        }

        internal static void saveDatabase(string path) {
            var file = new FileInfo(path);
            if (!file.Directory.Exists)
                file.Directory.Create();

            var state = new AssetDatabaseState() { FileGuidTable = _fileGuidTable, GuidAssetTable = _guidAssetTable };
            var output = JsonConvert.SerializeObject(state);

            using (var sw = new StreamWriter(file.FullName.ToLower())) {
                sw.Write(output);
                sw.Flush();
            }
        }
    }
}
