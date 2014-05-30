using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbx_lib.assets
{
    using System.IO;
    using System.Text.RegularExpressions;
    using FBGuid = System.String;
    public class DiceAsset
    {
        public string FilePath { get; set; }
        public FBGuid Guid { get; set; }
        public FBGuid PrimaryInstance { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public Dictionary<FBGuid, bool> Parents { get; set; }
        public Dictionary<FBGuid, bool> Children { get; set; }

        public static DiceAsset Create(FileInfo dbxFile)
        {
            if (AssetDatabase.containsAsset(dbxFile))
                return AssetDatabase.getAsset(dbxFile);
            if (!dbxFile.Exists)
                throw new Exception("Foobar");

            bool hasPrimaryInstance = false;
            bool parsedPrimaryInstance = false;
            DiceAsset asset = new DiceAsset();
            using (var sr = new StreamReader(dbxFile.FullName))
            {
                while (!sr.EndOfStream)
                {
                    //assumes primaryInstance-line exists in file before that element exists

                    var line = sr.ReadLine();
                    if (hasPrimaryInstance == false && line.Contains("primaryInstance"))
                    {
                        asset.Guid = DbxUtils.findSubstring(line, "uid=\"", 36);
                        asset.PrimaryInstance = DbxUtils.findSubstring(line, "nce=\"", 36);
                        hasPrimaryInstance = true;
                    }
                    else if (!parsedPrimaryInstance && hasPrimaryInstance && line.Contains(asset.PrimaryInstance))
                    {
                        asset.Type = DbxUtils.findSubstring(line, "type=\"", "\"");
                        asset.Name = DbxUtils.findSubstring(line, "id=\"", "\"");
                        parsedPrimaryInstance = true;
                    }
                    else if (line.Contains("uid=\""))
                    {
                        asset.addChildIfUnique(DbxUtils.findSubstring(line, "uid=\"", 36));
                    }
                }
            }

            if (!parsedPrimaryInstance)
                throw new Exception("Foobar!");
            return asset;
        }

        public DiceAsset()
        {
            Parents = new Dictionary<FBGuid, bool>();
            Children = new Dictionary<FBGuid, bool>();
        }

        public void addChildIfUnique(FBGuid guid)
        {
            if (!Children.ContainsKey(guid))
                Children.Add(guid, true);
        }

        public void addParentIfUnique(FBGuid guid)
        {
            if (!Parents.ContainsKey(guid))
                Parents.Add(guid, true);
        }

        public override string ToString()
        {
            return string.Format("DiceAsset: [Name={3} : FilePath={0} : Guid={1} : PrimaryInstanceGuid={2} : Type={4} : NumParents={5} : NumChildren={6}", FilePath, Guid, PrimaryInstance, Name, this.Type, Parents.Count, Children.Count);
        }
    }
}
