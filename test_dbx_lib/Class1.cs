﻿using System;
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
        public static void Main()
        {
            var lib = new LibMain();
            var dic = new List<DiceAsset>();
            var start = DateTime.Now;
            var files = lib.GetDbxFiles("../../../_dbx_data_examples");
            var time1 = DateTime.Now;
            Console.WriteLine("Finding all dbx-files ={0}ms", (time1 - start).TotalMilliseconds);
            lib.PopulateAssets(files);

            Console.WriteLine("All done in {0}ms", (DateTime.Now - start).TotalMilliseconds);
            Console.ReadLine();
        }
    }
}
