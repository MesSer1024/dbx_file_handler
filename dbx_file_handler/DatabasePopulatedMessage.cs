using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbx_file_handler {
    class DatabasePopulatedMessage : IMessage {
        public int MessageType {
            get { return 0; }
        }

        public object Content {
            get { return null; }
        }
    }
}
