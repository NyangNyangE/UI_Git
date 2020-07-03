
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace PanicCall
{
    class TestModule
    {
        string protocol;
        string ip;
        WebClient webclient;
        BitmapImage image;
        Uri uri;
        Maps maps;
        public TestModule(Maps maps)
        {
            this.maps = maps;
        }

        public void startTest()
        {
            foreach (Map map in maps)
            {
                foreach (PanicControl panic in map.PanicList.Values)
                {
                    foreach (Camera camera in panic.cameras)
                    {
                        if (string.IsNullOrWhiteSpace(camera.IP))
                            continue;
                        ip = camera.IP;
                        protocol = "http://admin:admin@" + ip + "/still.jpg";

                        using (WebClient client = new WebClient())
                        {
                            client.Credentials = new NetworkCredential("admin", "admin");
                            using (Stream s = client.OpenRead(protocol))
                            {
                                using (Bitmap image = new Bitmap(s))
                                {
                                    int w = image.Width;
                                    int h = image.Height;
                                    Color color;
                                    int count = 0;
                                    int avg = 0;
                                    int sum = 0;
                                    image.Save(".\\downImage\\" + ip + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                                    for (int i = 0; i < w; i+=10)
                                    {
                                        for (int j = 0; j < h; j+=10)
                                        {
                                            color = image.GetPixel(i, j);
                                            sum += (int)((color.R + color.B + color.G) / 3);
                                            count++;
                                        }
                                    }

                                    avg = sum / count;
                                    MainWindow.trace("Avg : " + avg);
                                    if(avg < 40 )
                                        camera.isfull = true;


                                }
                            }
                        }
                    }
                }
            }
        }
    }
}