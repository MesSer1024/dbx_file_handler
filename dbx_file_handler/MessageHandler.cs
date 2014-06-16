using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dbx_file_handler {
    public interface IMessage {
    }

    public interface IMessageListener {
        void onMessage(IMessage msg);
    }

    public static class MessageHandler {
        private static List<IMessageListener> _listeners = new List<IMessageListener>();

        public static void addListener(IMessageListener listener) {
            _listeners.Add(listener);
        }

        public static void removeListener(IMessageListener listener) {
            _listeners.Remove(listener);
        }

        public static void queueMessage(IMessage msg) {
            Task.Factory.StartNew(() => executeMessage(msg));
        }

        private static void executeMessage(IMessage msg) {
            var items = _listeners.ToArray();
            foreach (var listener in items) {
                listener.onMessage(msg);
            }
        }
    }
}
