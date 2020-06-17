using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;


namespace PanicCall
{
    [Serializable]
    public class Maps : ObservableCollection<Map>
    {
        int selectIndex = 0;
        bool iconLock = false;
        bool mapMoveLock = false;
        
        [NonSerialized]
        List<PanicControl> notInsertPanic = new List<PanicControl>();

        Size iconSize = new Size(37.0, 37.0);
        Size viewMapSize = new Size(0, 0);

        public Maps()
        {

        }

        public List<PanicControl> NotInsertPanic
        {
            get { return notInsertPanic; }
            set { notInsertPanic = value; }
        }



        public bool IconLock
        {
            get { return iconLock; }
            set { iconLock = value; }
        }

        public bool MapMoveLock
        {
            get { return mapMoveLock; }
            set { mapMoveLock = value; }
        }

        public int SelectIndex
        {
            get { return selectIndex; }
            set { selectIndex = value; }
        }

        public Size IconSize
        {
            get { return iconSize; }
            set { iconSize = value; }
        }

        public Size ViewMapSize
        {
            get { return viewMapSize; }
            set { viewMapSize = value; }
        }

    }


}
