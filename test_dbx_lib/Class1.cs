using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dbx_lib;
using dbx_lib.assets;

namespace test_dbx_lib
{
    public class Class1
    {
        const string ROOT_FOLDER = "../../../_dbx_data_examples";

        public static void Main()
        {
            var lib = new LibMain();
            var dic = new List<DiceAsset>();
            var start = DateTime.Now;
            var files = lib.GetDbxFiles(ROOT_FOLDER);
            var time1 = DateTime.Now;
            Console.WriteLine("Finding all dbx-files ={0}ms", (time1 - start).TotalMilliseconds);
            lib.PopulateAssets(files);

            Console.WriteLine("All done in {0}ms", (DateTime.Now - start).TotalMilliseconds);

            files = lib.GetDbxFiles(Path.Combine(ROOT_FOLDER, "FrontEnd"));
            foreach (var file in files) {
                var asset = lib.GetDiceAsset(file);
                Console.WriteLine(asset);
                foreach (var child in asset.getChildren()) {
                    if(lib.HasAsset(child))
                    {
                        var foobar = lib.GetDiceAsset(child);
                        Console.WriteLine("child={0} assetRef={1}", child, foobar.Name);
                    }
                    else {
                        Console.WriteLine("child={0}", child);
                    }
                }
            }

            Console.ReadLine();
        }
    }
}
