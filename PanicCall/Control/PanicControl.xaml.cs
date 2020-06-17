using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Net.Sockets;

namespace PanicCall
{
    [Serializable]
    public partial class PanicControl : UserControl, System.Runtime.Serialization.ISerializable
    {
        delegate void _delegate();

        const int DEFAULT_ADDR = -1;

        int addr = -1;
        double beginX = 0;
        double beginY = 0;
        bool isMouseDown = false;
        double pointX;
        double pointY;
        string btnText = "";
        public int isRed = 0;
        bool isSelected = false;
        int selectIndex = -1;
        public int pis = -1;

        public Camera[] cameras = new Camera[2];
        public string wisnet_IP;
        


        bool isSleep = false; // false == red

        Canvas IconCanvas;

        Timer timer = new Timer(1000 * 60 * 60);
        DateTime startTime = new DateTime();
        DateTime endTime = new DateTime();


        public PanicControl()
        {
            this.InitializeComponent();

        }

        public PanicControl(SerializationInfo info, StreamingContext context)
        {
            this.InitializeComponent();
     
            try
            {
                this.Height = info.GetDouble("Height");
                this.Width = info.GetDouble("Width");
                this.SetValue(Canvas.TopProperty, info.GetDouble("Top"));
                this.SetValue(Canvas.LeftProperty, info.GetDouble("Left"));


                Addr = info.GetInt32("Addr");
                IsRed = info.GetInt32("IsRed");
                pis = info.GetInt32("PIS");
                
               // camera_IP_L = (Camera)info.GetValue("Camera_IP_L", typeof(Camera));
                //camera_IP_R = (Camera)info.GetValue("Camera_IP_R", typeof(Camera));
                cameras = (Camera[])info.GetValue("Cameras", typeof(Camera[]));
                TextView.Text = info.GetString("TextView");


            }
            catch (Exception ex)
            {
            }
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
           
            info.AddValue("Height", this.Height);
            info.AddValue("Width", this.Width);
            info.AddValue("Top", this.GetValue(Canvas.TopProperty));
            info.AddValue("Left", this.GetValue(Canvas.LeftProperty));

            info.AddValue("Addr", Addr);
            info.AddValue("PIS", pis);
           // info.AddValue("Camera_IP_L", camera_IP_L);
            //info.AddValue("Camera_IP_R", camera_IP_R);
            info.AddValue("Cameras", cameras);
            info.AddValue("IsRed", IsRed);
            info.AddValue("TextView", TextView.Text);
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //wndProc = (WndProc)Application.Current.Properties["wndProc"];
            IconCanvas = (Canvas)Application.Current.Properties["IconCanvas"];

            viewport3D.MouseEnter += new MouseEventHandler(PanicControl_MouseEnter);
            viewport3D.MouseLeave += new MouseEventHandler(PanicControl_MouseLeave);

            IsRed = 0;

            SetTooltip();  
        }

        #region Variable

        public int Addr
        {
            get { return addr; }
            set { addr = value; }
        }

        public int IsRed
        {
            get { return isRed; }
            set { isRed = value; }
        }

        public bool IsSleep
        {
            get { return isSleep; }
            set { isSleep = value; }
        }

        public string BtnText
        {
            get { return btnText; }
            set { btnText = value; }
        }

        
        #endregion

        public void SetTooltip()
        {
            viewport3D.ToolTip = "주소 : " + addr.ToString() + "\n";
            viewport3D.ToolTip += "카메라 IP \n";
            if (cameras[0] != null)
                viewport3D.ToolTip += (string.IsNullOrWhiteSpace(cameras[0].IP)) ? ",  " : cameras[0].IP + ",  ";
            if (cameras[1] != null)
                viewport3D.ToolTip += (string.IsNullOrWhiteSpace(cameras[1].IP)) ? "  " : cameras[1].IP ;
            viewport3D.ToolTip += "\n";
            if (cameras[0] != null && cameras[0].settingCount != 0)
                viewport3D.ToolTip += "왼쪽 : " + cameras[0].settingCount + "\n";
            if (cameras[1] != null && cameras[1].settingCount != 0)
                viewport3D.ToolTip += "오른쪽 : " + cameras[1].settingCount + "\n";
           
        }

        public void SetTooltip2()
        {
            viewport3D.ToolTip = "주소 : " + addr.ToString() + "\n";
            viewport3D.ToolTip += "카메라 IP \n";
            if (cameras[0] != null)
                viewport3D.ToolTip += (string.IsNullOrWhiteSpace(cameras[0].IP)) ? ",  " : cameras[0].IP + ",  ";
            if (cameras[1] != null)
                viewport3D.ToolTip += (string.IsNullOrWhiteSpace(cameras[1].IP)) ? "  " : cameras[1].IP;
            viewport3D.ToolTip += "\n";
            if (cameras[0] != null && cameras[0].settingCount != 0)
                viewport3D.ToolTip += "왼쪽 : " + cameras[0].settingCount + "\n";
            if (cameras[1] != null && cameras[1].settingCount != 0)
                viewport3D.ToolTip += "오른쪽 : " + cameras[1].settingCount + "\n";
        }

        public void SetTooltip3()
        {
            viewport3D.ToolTip = "주소 : " + addr.ToString() + "\n";
            viewport3D.ToolTip += "카메라 IP \n";
            if (cameras[0] != null)
                viewport3D.ToolTip += (string.IsNullOrWhiteSpace(cameras[0].IP)) ? ",  " : cameras[0].IP + ",  " ;
            if (cameras[1] != null)
                viewport3D.ToolTip += (string.IsNullOrWhiteSpace(cameras[1].IP)) ? "  " : cameras[1].IP ;
            viewport3D.ToolTip += "\n";
            if (cameras[0] != null && cameras[0].settingCount != 0)
                viewport3D.ToolTip += "왼쪽 : " + cameras[0].settingCount + "\n";
            if (cameras[1] != null && cameras[1].settingCount != 0)
                viewport3D.ToolTip += "오른쪽 : " + cameras[1].settingCount + "\n";
        }

        void PanicControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.isMouseDown = false;
            pointX = e.GetPosition(IconCanvas).X;
            pointY = e.GetPosition(IconCanvas).Y;
            button.ReleaseMouseCapture();
        }

        void PanicControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isMouseDown == true)
            {
                // 전체 영역에서 마우스의 현재 위치
                double currX = e.GetPosition(IconCanvas).X;
                double currY = e.GetPosition(IconCanvas).Y;

                // 사각형의 현재위치
                double valueX = double.Parse(UserControl.GetValue(Canvas.LeftProperty).ToString());
                double valueY = double.Parse(UserControl.GetValue(Canvas.TopProperty).ToString());

                valueX += (currX - beginX);
                valueY += (currY - beginY);

                UserControl.SetValue(Canvas.LeftProperty, valueX);
                UserControl.SetValue(Canvas.TopProperty, valueY);

                beginX = currX;
                beginY = currY;
            }
        }

        void PanicControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MainWindow.isSelectChecked == false)
            {
                this.beginX = e.GetPosition(IconCanvas).X;
                this.beginY = e.GetPosition(IconCanvas).Y;

                isMouseDown = true;

                button.CaptureMouse();
            }
            else 
            {
                try
                {
                    if (isSelected)
                    {
                        unSelectedPanic();
                        MainWindow.selectedGroup.RemovePanic(this);
                    }
                    else
                    {
                        MainWindow.selectedGroup.AddPanic(this);
                        selectedPanic();
                    }

                    //clickedCount++;
                    //if (clickedCount == 3)
                    //    clickedCount = 0;
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }
            

         
        }

        void PanicControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Maps maps = MainWindow.maps;

            if (!(maps.MapMoveLock))
                ((ZoomBoxLibrary.ZoomBoxPanel)Application.Current.Properties["zoomBox"]).MouseMode
                    = ZoomBoxLibrary.ZoomBoxPanel.eMouseMode.Pan;
            //        zoomBox.MouseMode = ZoomBoxLibrary.ZoomBoxPanel.eMouseMode.Pan;
            (Resources["MouseEnterStory"] as Storyboard).Remove();
        }

        void PanicControl_MouseEnter(object sender, MouseEventArgs e)
        {
            ((ZoomBoxLibrary.ZoomBoxPanel)Application.Current.Properties["zoomBox"]).MouseMode
                 = ZoomBoxLibrary.ZoomBoxPanel.eMouseMode.None;
            // zoomBox.MouseMode = ZoomBoxLibrary.ZoomBoxPanel.eMouseMode.None;
            (Resources["MouseEnterStory"] as Storyboard).Begin();

            try
            {
                _delegate _SetTooltip = new _delegate(SetTooltip3);
                Dispatcher.Invoke(_SetTooltip);
            }
            catch
            { }

        }
        
        public void SetMove(bool set)
        {
            if (set)
            {
                this.button.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PanicControl_MouseLeftButtonDown);
                this.button.PreviewMouseMove += new MouseEventHandler(PanicControl_MouseMove);
                this.button.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(PanicControl_MouseLeftButtonUp);
            }
            else
            {
                this.button.PreviewMouseLeftButtonDown -= PanicControl_MouseLeftButtonDown;
                this.button.PreviewMouseMove -= PanicControl_MouseMove;
                this.button.PreviewMouseLeftButtonUp -= PanicControl_MouseLeftButtonUp;
            }
        }

        public void UnSetContextMenu()
        {
            button.ContextMenu = null;
        }

        public void SetContextMenu()
        {
            ContextMenu menu = new ContextMenu();

            MenuItem item_change = new MenuItem();
            MenuItem item_remove = new MenuItem();
            MenuItem item_setting_L = new MenuItem();
            MenuItem item_setting_R = new MenuItem();

            if (cameras[0] != null && cameras[0].settingCount != 0)
                item_setting_L.Background = new SolidColorBrush(Color.FromArgb(255, 204, 204, 255));
            if (cameras[1] != null && cameras[1].settingCount != 0)
                item_setting_R.Background = new SolidColorBrush(Color.FromArgb(255, 204, 204, 255));

            item_change.Header = "수정";
            item_remove.Header = "삭제";
            item_setting_L.Header = "왼쪽 영역 설정";
            item_setting_R.Header = "오른쪽 영역 설정";

            menu.Items.Add(item_change);
            menu.Items.Add(item_remove);
            menu.Items.Add(item_setting_L);
            menu.Items.Add(item_setting_R);


            item_change.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(item_change_PreviewMouseLeftButtonDown);
            item_remove.Click += new RoutedEventHandler(item_remove_Click);
            item_setting_L.Click += new RoutedEventHandler(item_setting_L_Click);
            item_setting_R.Click += new RoutedEventHandler(item_setting_R_Click);

            button.ContextMenu = menu;
        }

       

        bool CheckAddress(int _addr)
        {
            if (_addr == DEFAULT_ADDR)
            {
                MessageBox.Show("\t버튼의 어드레스를 등록하시기 바랍니다.\t");
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// cameras size, 기본 크기 2에서 실제 null 이 아닌 크기
        /// </summary>
        /// <returns></returns>
        public int camerasCount()
        {
            int count = 0;
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] == null || string.IsNullOrWhiteSpace(cameras[i].IP))
                    continue;
                count++;
            }
            return count;
        }

        /// <summary>
        /// panic button color green
        /// </summary>
        public void No_parking()
        {
            IsSleep = true;

            if(isSelected)
                button.Template = (ControlTemplate)UserControl.Resources["ButtonControlTemplate5"];
            else
                button.Template = (ControlTemplate)UserControl.Resources["ButtonControlTemplate2"];

        }

        /// <summary>
        /// panic button color red
        /// </summary>
        public void Yes_parking()       
        {
            IsSleep = false;
            
            if (isSelected)
                button.Template = (ControlTemplate)UserControl.Resources["ButtonControlTemplate4"];
            else
                button.Template = (ControlTemplate)UserControl.Resources["ButtonControlTemplate1"];
        }

        /// <summary>
        /// 그룹에 선택되는 panic button
        /// </summary>
        public void selectedPanic()
        {
            if (addr != -1)
            {
                if (isSleep)
                    button.Template = (ControlTemplate)UserControl.Resources["ButtonControlTemplate5"];
                else
                    button.Template = (ControlTemplate)UserControl.Resources["ButtonControlTemplate4"];

                isSelected = true;
            }
        }

        /// <summary>
        /// 그룹에서 선택 panic button 제외
        /// </summary>
        public void unSelectedPanic()
        {
            if (isSleep)
                button.Template = (ControlTemplate)UserControl.Resources["ButtonControlTemplate2"];
            else
                button.Template = (ControlTemplate)UserControl.Resources["ButtonControlTemplate1"];

            isSelected = false;
        }

        void item_death_Checked(object sender, RoutedEventArgs e)
        {
            MenuItem item = ((ContextMenu)((MenuItem)sender).Parent).Items[0] as MenuItem;
            item.IsChecked = false;
            button.Template = (ControlTemplate)UserControl.Resources["ButtonControlTemplate2"];
        }

        void item_change_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            InputPanicInfo info = new InputPanicInfo(this);
            Canvas Root = Application.Current.Properties["Root"] as Canvas;

            Point point = new Point();
            point = TranslatePoint(point, Root);

            if ((Root.ActualWidth - point.X) < info.Width)
            {
                info.SetRight();
                info.Left = point.X - info.Width + this.ActualWidth / 2;
            }
            else
            {
                info.Left = point.X + this.ActualWidth / 2;
            }

            info.Top = point.Y + this.ActualHeight / 2 + 10.0;
            info.ShowDialog();
        }

        void item_remove_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("정말 지우시겠습니까?", " 번호 : " + Addr, MessageBoxButton.YesNo))
            {
                RemoveIocn();
            }
        }

        public void RemoveIocn()
        {
            (Application.Current.Properties["IconCanvas"] as Canvas).Children.Remove(this);
            MainWindow.maps[MainWindow.maps.SelectIndex].PanicList.Remove(this);
            MainWindow.maps.NotInsertPanic.Add(this);
            if (Addr > 0)
                (Application.Current.Properties["MainWindow"] as MainWindow).AnswerIcon();

            TextView.Visibility = Visibility.Hidden;
            SetMove(false);
        }

        public void SetViewText(int index)
        {
            if (index == 0)
            {
                BtnText = "";
                TextView.Text = "";
            }
            else if (index == 1)
            {
                BtnText = Addr.ToString();
                TextView.Text = Addr.ToString();
            }
            else if (index == 2)
            {
                BtnText = Addr.ToString();
                TextView.Text = Addr.ToString();
            }
        }

        void item_setting_L_Click(object sender, RoutedEventArgs e)
        {
            if (cameras[0] == null)
            {
                MessageBox.Show("연결된 카메라가 없습니다.");
                return;
            }

            PanicCall.LprSetting Lprset = new LprSetting(cameras[0]);

            Lprset.Top = (Application.Current.Properties["Root"] as Canvas).ActualHeight / 2 - Lprset.Height / 2;
            Lprset.Left = (Application.Current.Properties["Root"] as Canvas).ActualWidth / 2 - Lprset.Width / 2;
            Lprset.ShowDialog();
            SetContextMenu();
        }
        void item_setting_R_Click(object sender, RoutedEventArgs e)
        {
            if (cameras[1] == null)
            {
                MessageBox.Show("연결된 카메라가 없습니다.");
                return;
            }

            PanicCall.LprSetting Lprset = new LprSetting(cameras[1]);

            Lprset.Top = (Application.Current.Properties["Root"] as Canvas).ActualHeight / 2 - Lprset.Height / 2;
            Lprset.Left = (Application.Current.Properties["Root"] as Canvas).ActualWidth / 2 - Lprset.Width / 2;
            Lprset.ShowDialog();
            SetContextMenu();
        }
  
    }
}