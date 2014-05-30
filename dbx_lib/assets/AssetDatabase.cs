using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.IO;


namespace dbx_lib.assets
{
    using FBGuid = System.String;
    
    static class AssetDatabase
    {
        private static ConcurrentDictionary<FBGuid, DiceAsset> _guidAssetTable = new ConcurrentDictionary<FBGuid, DiceAsset>();
        private static ConcurrentDictionary<string, FBGuid> _fileGuidTable = new ConcurrentDictionary<string, FBGuid>();
        private static object _lock = new object();

        public static void PopulateAsset(FileInfo[] files)
        {
            foreach (var file in files)
            {
                doBuildAsset(file);
            }
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
            _guidAssetTable.TryAdd(asset.PrimaryInstance, asset);
            _fileGuidTable.TryAdd(dbxFile.FullName, asset.PrimaryInstance);
        }

        private static void updateReferencesInDatabase()
        {
            foreach (var asset in _guidAssetTable.Values.ToList())
            {
                foreach (var child in asset.Children)
                {
                    if (_guidAssetTable.ContainsKey(child.Key))
                        _guidAssetTable[child.Key].addParentIfUnique(asset.PrimaryInstance);
                }
            }
        }

        public static bool containsAsset(FileInfo file)
        {
            return _fileGuidTable.ContainsKey(file.FullName);
        }

        internal static DiceAsset getAsset(FileInfo file)
        {
            return _guidAssetTable[_fileGuidTable[file.FullName]];
        }
    }
}
