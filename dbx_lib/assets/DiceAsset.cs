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
                        asset.Guid = findSubstring(line, "uid=\"");
                        asset.PrimaryInstance = findSubstring(line, "nce=\"");
                        hasPrimaryInstance = true;
                    }
                    else if (!parsedPrimaryInstance && hasPrimaryInstance && line.Contains(asset.PrimaryInstance))
                    {
                        asset.Type = findSubstring(line, "type=\"", "\"");
                        asset.Name = findSubstring(line, "id=\"", "\"");
                        parsedPrimaryInstance = true;
                    }
                    else if (line.Contains("uid=\""))
                    {
                        var idx = line.IndexOf("uid=\"");
                        var guid = line.Substring(idx + 5, 36);
                        asset.addChildIfUnique(guid);
                    }
                }
            }

            if (!parsedPrimaryInstance)
                throw new Exception("Foobar!");
            return asset;
        }

        private static string findSubstring(string source, string identifier, int count=36)
        {
            var idx = source.IndexOf(identifier);
            return source.Substring(idx + identifier.Length, count);
        }

        private static string findSubstring(string source, string startIdentifier, string endIdentifier)
        {
            var startIdx = source.IndexOf(startIdentifier) + startIdentifier.Length;
            var endIdx = source.IndexOf(endIdentifier, startIdx + 1);
            return source.Substring(startIdx, endIdx-startIdx);
        }

        private static string parseRegexGroup(string line, string regexPattern, int regexGroup)
        {
            var regexGuid = new Regex(regexPattern, RegexOptions.Compiled);
            var guid = regexGuid.Match(line);

            if (!guid.Success)
                throw new Exception("Foobar");
            if (regexGroup >= guid.Groups.Count)
                throw new Exception("foobar!");

            return guid.Groups[regexGroup].Value;
        }

        public DiceAsset()
        {
            //Parents = new List<FBGuid>();
            //Children = new List<FBGuid>();
            Parents = new Dictionary<FBGuid, bool>();
            Children = new Dictionary<FBGuid, bool>();
        }

        public void addChildIfUnique(FBGuid guid)
        {
            //if (!Children.Contains(guid))
            //    Children.Add(guid);
            if (!Children.ContainsKey(guid))
                Children.Add(guid, true);
        }

        public void addParentIfUnique(FBGuid guid)
        {
            //if (!Parents.Contains(guid))
            //    Parents.Add(guid);
            if (!Parents.ContainsKey(guid))
                Parents.Add(guid, true);
        }

        public override string ToString()
        {
            return string.Format("DiceAsset: [Name={3} : FilePath={0} : Guid={1} : PrimaryInstanceGuid={2} : Type={4} : NumParents={5} : NumChildren={6}", FilePath, Guid, PrimaryInstance, Name, this.Type, Parents.Count, Children.Count);
        }
    }
}
