using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// This is a class that helps with the Window_Closing event within "EditLoadData.cs"
    /// </summary>
    public class WindowClosingEventType
    {
        private object sender;
        private CancelEventArgs cea;

        public WindowClosingEventType(object sender, CancelEventArgs cea)
        {
            this.Sender = sender;
            this.Cea = cea;
        }

        public object Sender
        {
            get { return sender; }
            set { sender = value; }
        }

        public CancelEventArgs Cea
        {
            get { return cea; }
            set { cea = value; }
        }

    }
}
