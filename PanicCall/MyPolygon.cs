using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace PanicCall
{
    class MyPolygon
    {
        public List<Point> pList;
        public int camera = 0;
        public int dvr = 0;
        public MyPolygon(List<Point> pList)
        {
            this.pList = pList;
        }

        public bool isPointInPolygon(Point point)
        {
            sortPoint();

            bool result = false;
            int j = pList.Count() - 1;
            for (int i = 0; i < pList.Count(); i++)
            {
                if (pList[i].Y < point.Y && pList[j].Y >= point.Y || pList[j].Y < point.Y && pList[i].Y >= point.Y)
                {
                    if (pList[i].X + (point.Y - pList[i].Y) / (pList[j].Y - pList[i].Y) * (pList[j].X - pList[i].X) < point.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
        private void sortPoint() // 왼쪽위, 왼쪽아래, 오른쪽아래 , 오른쪽위
        {
            pList.Sort((a, b) => a.X > b.X ? 1 : -1);

            if (pList[0].Y > pList[1].Y)
            {
                Point temp = pList[0];
                pList[0] = pList[1];
                pList[1] = temp;
            }
            if (pList[2].Y > pList[3].Y)
            {
                Point temp = pList[2];
                pList[2] = pList[3];
                pList[3] = temp;
            }
           
        }
    }
}
