using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using dbx_lib;

namespace dbx_file_handler {
    interface IScreen {
        void onInit();
        void onDeinit();
    }

    static class DbxApplication {
        public static LibMain DBX { get; private set; }
        public static string SearchFolder { get; set; }

        public static void init() {
            DBX = new LibMain();
            ScreenStack = new Stack<IScreen>();
            loadInfo();
        }

        private static void loadInfo()
        {
            SearchFolder = @"E:\repositories\Tunguska\FutureData\Source";
            var file = new FileInfo("./config.ini");
            if(!file.Exists)
                return;

            using (var sr = new StreamReader(file.FullName))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var parts = line.Split('|');
                    if (parts.Length != 2)
                        MessageBox.Show("Unable to parse config line: " + line);

                    if (parts[0].ToLower().Contains("folder"))
                        SearchFolder = parts[1].Trim();
                }
            }
        }

        public static void saveInfo()
        {
            var file = new FileInfo("./config.ini");
            using (var sw = new StreamWriter(file.FullName))
            {
                sw.WriteLine("folder | {0}", SearchFolder);
                sw.Flush();
            }
        }

        public static Stack<IScreen> ScreenStack { get; private set; }
    }
}
