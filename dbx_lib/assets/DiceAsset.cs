using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace dbx_lib.assets
{
    using FBGuid = System.String;
    public class DiceAsset
    {
        public string FilePath { get; set; }
        public FBGuid Guid { get; set; } //used for identifying any external links to this asset, usually coupled with ref="internal_guid" which would then refer to a specific element inside asset
        public FBGuid PrimaryInstance { get; set; } //used for identifying what type of asset this is, basically the "root element" of a specific dbx-file.
        public string Name { get; set; }
        public string Type { get; set; }

        private Dictionary<FBGuid, bool> _parents { get; set; }
        private Dictionary<FBGuid, bool> _children { get; set; }

        public List<FBGuid> getChildren() {
            return _children.Keys.ToList();
        }

        public List<FBGuid> getParents() {
            return _parents.Keys.ToList();
        }

        /// <summary>
        /// Parses a dbx-file, looking for information such as PartionGuid, PrimaryInstanceGuid & any references this file might have towards other files [children]
        /// Throws exceptions if file allready exists in database, if file does not exist or if file does not live up to minimum requirements for a dbx-file (partion guid, type and primary instance)
        /// </summary>
        /// <param name="dbxFile"></param>
        /// <returns></returns>
        public static DiceAsset Create(FileInfo dbxFile)
        {
            if (AssetDatabase.containsAsset(dbxFile))
                return AssetDatabase.getAsset(dbxFile);
            if (!dbxFile.Exists)
                throw new Exception("Foobar");

            bool hasPrimaryInstance = false;
            bool parsedPrimaryInstance = false;
            DiceAsset asset = new DiceAsset();
            asset.FilePath = dbxFile.FullName;
            using (var sr = new StreamReader(dbxFile.FullName))
            {
                while (!sr.EndOfStream)
                {
                    //assumes primaryInstance-line exists in file before that element exists

                    var line = sr.ReadLine();
                    if (hasPrimaryInstance == false && line.Contains("primaryInstance"))
                    {
                        asset.Guid = DbxUtils.findSubstring(line, "uid=\"", DbxUtils.GUID_LENGTH);
                        asset.PrimaryInstance = DbxUtils.findSubstring(line, "stance=\"", DbxUtils.GUID_LENGTH);
                        hasPrimaryInstance = true;
                    }
                    else if (!parsedPrimaryInstance && hasPrimaryInstance && line.Contains(asset.PrimaryInstance))
                    {
                        asset.Type = DbxUtils.findSubstring(line, "type=\"", "\"");
                        asset.Name = DbxUtils.findSubstring(line, "id=\"", "\"");
                        parsedPrimaryInstance = true;
                    }
                    else if (line.Contains("partitionGuid=\""))
                    {
                        var guid = DbxUtils.findSubstring(line, "partitionGuid=\"", DbxUtils.GUID_LENGTH);
                        asset.addChildIfUnique(guid);
                    }
                }
            }

            bool validGuid = asset.Guid.Length == DbxUtils.GUID_LENGTH;
            bool validPrimaryInstance = asset.PrimaryInstance.Length == DbxUtils.GUID_LENGTH;
            bool validAssetType = asset.Type.Length > 3;
            if (validGuid && validPrimaryInstance && validAssetType)
                return asset;

            throw new Exception("Foobar!");
        }

        public DiceAsset()
        {
            _parents = new Dictionary<FBGuid, bool>();
            _children = new Dictionary<FBGuid, bool>();
        }

        public void addChildIfUnique(FBGuid guid)
        {
            if (!_children.ContainsKey(guid))
                _children.Add(guid, true);
        }

        public void addParentIfUnique(FBGuid guid)
        {
            if (!_parents.ContainsKey(guid))
                _parents.Add(guid, true);
        }

        public override string ToString()
        {
            return string.Format("DiceAsset: [Name={3} : FilePath={0} : Guid={1} : PrimaryInstanceGuid={2} : Type={4} : NumParents={5} : NumChildren={6}", FilePath, Guid, PrimaryInstance, Name, this.Type, _parents.Count, _children.Count);
        }
    }
}
