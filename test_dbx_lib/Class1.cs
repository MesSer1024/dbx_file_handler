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
            var lib = buildDb();
            printDb(lib);
            lib.saveDatabase("db.txt");

            //var lib = loadDb();
            //printDb(lib);
            Console.ReadLine();
        }

        public static LibMain buildDb(string rootFolder = ROOT_FOLDER) {
            var lib = new LibMain();
            var start = DateTime.Now;
            var files = lib.GetDbxFiles(rootFolder);
            var time1 = DateTime.Now;
            Console.WriteLine("Finding all dbx-files ={0}ms", (time1 - start).TotalMilliseconds);
            lib.PopulateAssets(files);
            Console.WriteLine("All done in {0}ms", (DateTime.Now - start).TotalMilliseconds);
            return lib;
        }

        public static LibMain loadDb(string path = "db.txt") {
            var lib = new LibMain();
            lib.loadDatabase(path);
            return lib;
        }

        public static void saveDb(LibMain lib, string path = "db.txt") {
            lib.saveDatabase(path);
        }

        public static void printDb(LibMain lib) {
            var frontendFiles = lib.GetDbxFiles(Path.Combine(ROOT_FOLDER, "frontend"));
            foreach (var file in frontendFiles) {
                var asset = lib.GetDiceAsset(file);
                Console.WriteLine(asset);
                foreach (var child in asset.getChildren()) {
                    if (lib.HasAsset(child)) {
                        var foobar = lib.GetDiceAsset(child);
                        Console.WriteLine("\tchild={0} parents={2} assetRef={1}", child, foobar.Name, foobar.getParents().Count);
                    } else {
                        Console.WriteLine("\tchild={0}", child);
                    }
                }
            }
        }
    }
}
