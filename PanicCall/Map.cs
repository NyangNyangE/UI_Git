using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoomBoxLibrary;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PanicCall
{
    [Serializable]
    public class Map
    {
        string mapName = "";
        string fileName = "";
        string imagePath = "";
        double posX = 0;
        double posY = 0;
        double zoom = 0;
        int cars = 0;
        int areas = 0;

        List<PanicControl> panicList = new List<PanicControl>();
        List<PisControl> pisList = new List<PisControl>();

        public Map()
        { 
        
        }
        
        public Map(string mapname, string filepath, string filename)
        {
            MapName = mapname;
            FileName = filename;
            imagePath = filepath;
         }

        public string MapName
        {
            get { return mapName; }
            set { mapName = value; }
        }

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public string ImagePath
        {
            get { return imagePath; }
            set { imagePath = value; }
        }

        public double PosX
        {
            get { return posX; }
            set { posX = value; }
        }

        public double PosY
        {
            get { return posY; }
            set { posY = value; }
        }

        public double Zoom
        {
            get { return zoom; }
            set { zoom = value; }
        }

        public int Cars
        {
            get { return cars; }
            set { cars = value; }
        }

        public int Areas
        {
            get { return areas; }
            set { areas = value; }
        }

        public List<PanicControl> PanicList
        {
            get { return panicList; }
            set { panicList = value; }
        }

        public List<PisControl> PisList
        {
            get { return pisList; }
            set { pisList = value; }
        }

    }
}
