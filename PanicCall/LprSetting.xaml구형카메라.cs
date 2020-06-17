using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PanicCall
{
    /// <summary>
    /// LprSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LprSetting : Window
    {
        private delegate void Main_del(BitmapImage Bimg);
        PanicControl panic = new PanicControl();
        WebClient webClient;
        Uri uri;
        Camera camera;
        HttpWebRequest request;

        string connectprotocol = "";
        int count = 0;
        bool isInit = false;
        
        public LprSetting(Camera camera)
        {
            this.camera = camera;
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            //settingROI();
            camera.settingCount = count;
            this.Close();
        }

        //~수정: roi 지역 설정 및 기타
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            isInit = initWebClient();
            downloadImage();
        }

        private bool initWebClient()
        {
            try
            {
                connectprotocol = "http://admin:admin@" + camera.IP + "/still.jpg";
            }
            catch (Exception eee)
            {
                Console.WriteLine("a1");
                return false;
            }

            try
            {
                uri = new Uri(connectprotocol);
            }
            catch (Exception ex)
            {
                return false;
            }

            try
            {
                //if (webClient == null)
                //{
                    //webClient = new WebClient();
                    //webClient.UseDefaultCredentials = false;
                    //webClient.Credentials = new NetworkCredential("admin", "admin");
                    //webClient.DownloadDataCompleted += new DownloadDataCompletedEventHandler(webClient_DownloadDataCompleted);
                    //화성 동탄 
                    try
                    {
                        request = (HttpWebRequest)HttpWebRequest.Create(uri);
                        request.Method = "Get";
                        request.ProtocolVersion = HttpVersion.Version10;
                        request.Credentials = new NetworkCredential("admin", "admin");
                        request.KeepAlive = false;
                        request.Timeout = 300;

                        //request.BeginGetResponse(new AsyncCallback(finishWebRequest), request);
                    }
                    catch (Exception ee)
                    { }
                //}
            }
            catch (ArgumentException ex)
            {
                return false;
            }

            return true;
        }



        //void finishWebRequest(IAsyncResult result)
        //{
        //    try
        //    {
                
        //        HttpWebRequest req = (HttpWebRequest)result.AsyncState;
        //        req.Method = "Get";
        //        req.ProtocolVersion = HttpVersion.Version10;
        //        req.Credentials = new NetworkCredential("admin", "admin");
        //        req.KeepAlive = false;
        //        req.Timeout = 2000;
        //        using (HttpWebResponse response = (HttpWebResponse)req.EndGetResponse(result))
        //        {
        //            System.Drawing.Image image = System.Drawing.Image.FromStream(response.GetResponseStream());

        //            if (image != null)
        //            {
        //                BitmapImage bitmap = ToImage(image);
        //                Getimg(bitmap);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Write(ex.Message);
        //    }
        //}

        //void downloadImage()
        //{
        //    try
        //    {
        //        webClient.DownloadDataAsync(uri);
        //    }
        //    catch (Exception ee)
        //    {
        //        Console.WriteLine(ee.Message);
        //    }
        //}
        void downloadImage()
        {

            System.Drawing.Image image;

            try
            {
                HttpWebResponse res = (HttpWebResponse)request.GetResponse();

                image = System.Drawing.Image.FromStream(res.GetResponseStream());
                BitmapImage bitmap = ToImage(image);
                Getimg(bitmap);
                settingROI();
            }
            catch (Exception ex)
            {
                Console.Write("Qwe");
            }
        }

        void settingROI ()
        {
            try
            {
                Cnv.Children.Clear();
                count = 0;

                if (camera.ROI_1_x != "0" || camera.ROI_1_y != "0")
                {
                    Rectangle Rectangle = new Rectangle();          //가운데점
                    Rectangle.Fill = Brushes.Transparent;
                    Rectangle.Width = 6;
                    Rectangle.Height = 6;
                    Rectangle.Stroke = Brushes.Red;
                    Rectangle.StrokeThickness = 4;

                    Rectangle Rectangle2 = new Rectangle();         //테두리
                    Rectangle2.Fill = Brushes.Transparent;
                    Rectangle2.Width = 180;
                    Rectangle2.Height = 140;
                    Rectangle2.Stroke = Brushes.Red;
                    Rectangle2.StrokeThickness = 2;

                    Cnv.Children.Add(Rectangle);

                    Canvas.SetLeft(Rectangle, Convert.ToDouble(camera.ROI_1_x) / 2);
                    Canvas.SetTop(Rectangle, Convert.ToDouble(camera.ROI_1_y) / 2);

                    Cnv.Children.Add(Rectangle2);

                    Canvas.SetLeft(Rectangle2, Convert.ToDouble(camera.ROI_1_x) / 2 - 90);
                    Canvas.SetTop(Rectangle2, Convert.ToDouble(camera.ROI_1_y) / 2 - 70);
                    Console.WriteLine("ROI 1 " + Convert.ToDouble(camera.ROI_1_x) / 2 + "," + Convert.ToDouble(camera.ROI_1_y) / 2);
                    ///
                    count++;
                }

                if (camera.ROI_2_x != "0" || camera.ROI_2_y != "0")
                {
                    Rectangle Rectangle = new Rectangle();
                    Rectangle.Fill = Brushes.Transparent;
                    Rectangle.Width = 6;
                    Rectangle.Height = 6;
                    Rectangle.Stroke = Brushes.Red;
                    Rectangle.StrokeThickness = 4;

                    Rectangle Rectangle2 = new Rectangle();         //테두리
                    Rectangle2.Fill = Brushes.Transparent;
                    Rectangle2.Width = 180;
                    Rectangle2.Height = 140;
                    Rectangle2.Stroke = Brushes.Red;
                    Rectangle2.StrokeThickness = 2;

                    Cnv.Children.Add(Rectangle);

                    Canvas.SetLeft(Rectangle, Convert.ToDouble(camera.ROI_2_x) / 2);
                    Canvas.SetTop(Rectangle, Convert.ToDouble(camera.ROI_2_y) / 2);

                    Cnv.Children.Add(Rectangle2);

                    Canvas.SetLeft(Rectangle2, Convert.ToDouble(camera.ROI_2_x) / 2 - 90);
                    Canvas.SetTop(Rectangle2, Convert.ToDouble(camera.ROI_2_y) / 2 - 70);
                    Console.WriteLine("ROI 2 " + Convert.ToDouble(camera.ROI_2_x) / 2 + "," + Convert.ToDouble(camera.ROI_2_y) / 2);
                    ///
                    count++;
                }

                if (camera.ROI_3_x != "0" || camera.ROI_3_y != "0")
                {
                    Rectangle Rectangle = new Rectangle();
                    Rectangle.Fill = Brushes.Transparent;
                    Rectangle.Width = 6;
                    Rectangle.Height = 6;
                    Rectangle.Stroke = Brushes.Red;
                    Rectangle.StrokeThickness = 4;

                    Rectangle Rectangle2 = new Rectangle();         //테두리
                    Rectangle2.Fill = Brushes.Transparent;
                    Rectangle2.Width = 180;
                    Rectangle2.Height = 140;
                    Rectangle2.Stroke = Brushes.Red;
                    Rectangle2.StrokeThickness = 2;

                    Cnv.Children.Add(Rectangle);

                    Canvas.SetLeft(Rectangle, Convert.ToDouble(camera.ROI_3_x) / 2);
                    Canvas.SetTop(Rectangle, Convert.ToDouble(camera.ROI_3_y) / 2);

                    Cnv.Children.Add(Rectangle2);

                    Canvas.SetLeft(Rectangle2, Convert.ToDouble(camera.ROI_3_x) / 2 - 90);
                    Canvas.SetTop(Rectangle2, Convert.ToDouble(camera.ROI_3_y) / 2 - 70);
                    Console.WriteLine("ROI 3 " + Convert.ToDouble(camera.ROI_3_x) / 2 + "," + Convert.ToDouble(camera.ROI_3_y) / 2);

                    count++;
                }
            }
            catch (Exception eeee)
            {
                Dispatcher.BeginInvoke(new Action(delegate()
                {
                    MainWindow.trace2("주차면 설정 저장 실패");
                }));
            }
        }


        //void webClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        //{
        //    try
        //    {
        //        //~수정: 비트맵 이미지 저장

        //        System.IO.Directory.CreateDirectory(".\\downImage");
        //        BitmapImage downImage = ToImage(e.Result);

        //        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
        //        encoder.Frames.Add(BitmapFrame.Create(downImage));

        //        string filePath = ".\\downImage\\" + camera.IP + ".jpg";
        //        using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
        //        {
        //            encoder.Save(fileStream);
        //        }

        //        Getimg(downImage);

        //    }
        //    catch (Exception ee)
        //    {
        //        Console.WriteLine(ee.Message);
        //    }
        //    settingROI();

        //}

        //public BitmapImage ToImage(byte[] array)
        //{
        //    using (var ms = new System.IO.MemoryStream(array))
        //    {
        //        var image = new BitmapImage();
        //        image.BeginInit();
        //        image.CacheOption = BitmapCacheOption.OnLoad; // here
        //        image.StreamSource = ms;
        //        image.EndInit();
        //        return image;
        //    }
        //}

        public BitmapImage ToImage(System.Drawing.Image img)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        private static bool InvokeRequired
        {
            get { return System.Windows.Threading.Dispatcher.CurrentDispatcher != Application.Current.Dispatcher; }
        }

        public void Getimg(BitmapImage Bimg)
        {
            LprImage.Source = Bimg;
            //if (InvokeRequired)
            //{
            //    Console.WriteLine("이미지를 가져오는동안 에러");
            //    Dispatcher.BeginInvoke(new Main_del(Getimg), Bimg);
            //}
            //else
            //{
            //    LprImage.Source = Bimg;
            //    // Cnv.Children.Add(LprImage);
            //}
        }


        PointCollection collection;
        List<Point> pList = new List<Point>();
        
        private void LprImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Rectangle Rectangle = new Rectangle();
                Rectangle.Fill = Brushes.Transparent;
                Rectangle.Width = 6;
                Rectangle.Height = 6;
                Rectangle.Stroke = Brushes.Red;
                Rectangle.StrokeThickness = 4;

                Rectangle Rectangle2 = new Rectangle();
                Rectangle2.Fill = Brushes.Transparent;
                Rectangle2.Width = 180;
                Rectangle2.Height = 140;
                Rectangle2.Stroke = Brushes.Red;
                Rectangle2.StrokeThickness = 2;


                if (count == 0)
                {
                    camera.ROI_1_x = (e.GetPosition(LprImage).X * 2).ToString();
                    camera.ROI_1_y = (e.GetPosition(LprImage).Y * 2).ToString();
                }
                else if (count == 1)
                {
                    camera.ROI_2_x = (e.GetPosition(LprImage).X * 2).ToString();
                    camera.ROI_2_y = (e.GetPosition(LprImage).Y * 2).ToString();
                }
                else if (count == 2)
                {
                    camera.ROI_3_x = (e.GetPosition(LprImage).X * 2).ToString();
                    camera.ROI_3_y = (e.GetPosition(LprImage).Y * 2).ToString();
                }
                else if (count == 3)
                {
                    Cnv.Children.Clear();
                    count = 0;
                    camera.ROI_1_x = "0";
                    camera.ROI_1_y = "0";
                    camera.ROI_2_x = "0";
                    camera.ROI_2_y = "0";
                    camera.ROI_3_x = "0";
                    camera.ROI_3_y = "0";
                    return;
                }

                Cnv.Children.Add(Rectangle);

                Canvas.SetLeft(Rectangle, e.GetPosition(LprImage).X );
                Canvas.SetTop(Rectangle, e.GetPosition(LprImage).Y );

                Cnv.Children.Add(Rectangle2);

                Canvas.SetLeft(Rectangle2, e.GetPosition(LprImage).X - 90);
                Canvas.SetTop(Rectangle2, e.GetPosition(LprImage).Y - 70);

                Console.WriteLine(e.GetPosition(LprImage).X + "," + e.GetPosition(LprImage).Y);



                count++;
            }
            catch (Exception eee)
            {
                Console.WriteLine(eee.Message);
            }


        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                if (isInit == false)
                    isInit = initWebClient();
                downloadImage();
            }
        }   
        
    }
}
