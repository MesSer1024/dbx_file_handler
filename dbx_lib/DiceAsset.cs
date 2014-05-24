using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbx_lib
{
    public class DiceAsset
    {
        public string FilePath { get; set; }
        public string Guid { get; set; }
        public string PrimaryInstance { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public List<string> Parents { get; set; }
        public List<string> Children { get; set; }

        public DiceAsset()
        {
            Parents = new List<string>();
            Children = new List<string>();
        }

        public bool addChild(DiceAsset asset)
        {
            if (Children.Contains(asset.Guid))
                return false;
            Children.Add(asset.Guid);
            return true;
        }

        public bool addParent(DiceAsset asset)
        {
            if (Parents.Contains(asset.Guid))
                return false;
            Parents.Add(asset.Guid);
            return true;
        }

        public override string ToString()
        {
            return string.Format("DiceAsset: [Name={3} : FilePath={0} : Guid={1} : PrimaryInstanceGuid={2} : Type={4} : NumParents={5} : NumChildren={6}", FilePath, Guid, PrimaryInstance, Name, this.Type, Parents.Count, Children.Count);
        }
    }
}
