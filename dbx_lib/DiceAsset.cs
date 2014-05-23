using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbx_lib
{
    class DiceAsset
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

        public bool addChildUnique(DiceAsset asset)
        {
            if (Children.Contains(asset.Guid))
                return false;
            Children.Add(asset.Guid);
            return true;
        }

        public bool addParentUnique(DiceAsset asset)
        {
            if (Parents.Contains(asset.Guid))
                return false;
            Parents.Add(asset.Guid);
            return true;
        }
    }
}
