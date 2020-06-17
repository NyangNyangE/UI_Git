using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PanicCall
{
    [Serializable]
    public class Dvr
    {
        string ip = "";
        string id = "";
        string password = "";
        int port = 0;

        public string Ip
        {
            get { return ip; }
            set { ip = value; }
        }

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public int Port
        {
            get { return port; }
            set { port = value; }
        }
    }

     

}
