using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace dbx_file_handler
{
    class CloseOverlayScreen : IMessage
    {
        public UIElement Screen { get; private set; }

        public CloseOverlayScreen(UIElement screen)
        {
            Screen = screen;
        }
    }
}
