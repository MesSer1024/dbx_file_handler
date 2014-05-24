using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dbx_lib;

namespace test_dbx_lib
{
    public class Class1
    {
        public static void Main()
        {
            var lib = new LibMain();

            var dir = new DirectoryInfo("../../../_dbx_data_examples");
            var files = dir.GetFiles("*.dbx");
            foreach (var file in files)
            {
                var foo = lib.GetDiceAsset(file.FullName);
                Console.WriteLine(foo.ToString());
            }

            Console.ReadLine();
        }
    }
}
