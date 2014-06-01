using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dbx_lib;

namespace dbx_file_handler {
    interface IScreen {
        void onInit();
        void onDeinit();
    }

    static class DbxApplication {
        public static LibMain DBX { get; private set; }

        public static void init() {
            DBX = new LibMain();
        }
    }
}
