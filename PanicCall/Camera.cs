
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PanicCall
{
    [Serializable]
    public class Camera 
    {
        public string IP = "";
        public string ROI_1_x = "0";
        public string ROI_1_y = "0";
        public string ROI_2_x = "0";
        public string ROI_2_y = "0";
        public string ROI_3_x = "0";
        public string ROI_3_y = "0";
        public string number = "0";
        public bool isfull = false;
        public int areas = 0;
        public int cars = 0;
        public int settingCount = 0;

        public Camera()
        {
 
        }
    }
}