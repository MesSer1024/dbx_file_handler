using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dbx_lib.assets;

namespace dbx_file_handler
{
    class SelectAssetMessage : IMessage
    {
        public DiceAsset Asset { get; private set; }
        public List<DiceAsset> ReferencedFiles { get; private set; }

        public SelectAssetMessage(dbx_lib.assets.DiceAsset asset, List<DiceAsset> filesReferringGuid)
        {
            Asset = asset;
            ReferencedFiles = filesReferringGuid;
        }


    }
}
