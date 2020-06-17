using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanicCall
{
    public class ReceiveEventArgs : System.EventArgs
    {
        public string receiveData;

        public ReceiveEventArgs(string eventData)
        {
            this.receiveData = eventData;
        }
    }
}
