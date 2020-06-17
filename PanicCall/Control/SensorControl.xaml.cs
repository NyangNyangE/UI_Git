using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PanicCall
{
    [Serializable]
    public partial class SensorControl : UserControl, System.Runtime.Serialization.ISerializable
    {
        int voltage = -1;
        int addr = -1;
        double beginX = 0;
        double beginY = 0;
        bool isMouseDown = false;
        string sensorName = "";
        string alramPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\wav\\Alram.wav";

        bool isSleep = false;
        bool isPhoneSleep = false;

        bool isInterphone2 = false;
        bool isInterphone3 = false;
        bool isInterphone4 = false;

        WndProc wndProc;
        Canvas IconCanvas;
        DateTime startTime = new DateTime();
        DateTime endTime = new DateTime();
        ZoomBoxLibrary.ZoomBoxPanel zoomBox;

        List<IntToInt> matrix = new List<IntToInt>();
        List<IntToInt> preset = new List<IntToInt>();
        List<IntToInt> sc = new List<IntToInt>();

        List<CameraControl> cameras = new List<CameraControl>();

		public SensorControl()
		{
			this.InitializeComponent();
		}

        public SensorControl(SerializationInfo info, StreamingContext context) 
        {
            this.InitializeComponent();

            this.Height = info.GetDouble("Height");
            this.Width = info.GetDouble("Width");
            this.SetValue(Canvas.TopProperty, info.GetDouble("Top"));
            this.SetValue(Canvas.LeftProperty, info.GetDouble("Left"));

            Addr = info.GetInt32("Addr");
            Voltage = info.GetInt32("Voltage");
            IsSleep = info.GetBoolean("IsSleep");
            IsPhoneSleep = info.GetBoolean("IsPhoneSleep");
            SensorName = info.GetString("PanicName");
            TextView.Text = info.GetString("TextView");
            AlramPath = info.GetString("AlramPath");

            isInterphone2 = info.GetBoolean("isInterphone2");
            isInterphone3 = info.GetBoolean("isInterphone3");
            isInterphone4 = info.GetBoolean("isInterphone4");

            Matrix = (List<IntToInt>)info.GetValue("Matrix", typeof(List<IntToInt>));
            Preset = (List<IntToInt>)info.GetValue("Preset", typeof(List<IntToInt>));
            Cameras = (List<CameraControl>)info.GetValue("Cameras", typeof(List<CameraControl>));
            Sc = (List<IntToInt>)info.GetValue("Sc", typeof(List<IntToInt>));

            startTime = (DateTime)info.GetValue("StartTime", typeof(DateTime));
            EndTime = (DateTime)info.GetValue("EndTime", typeof(DateTime));

            if (IsSleep)
            {
                button.Template = (ControlTemplate)UserControl.Resources["ButtonControlTemplate2"];
            }

            SetContextMenu();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Height", this.Height);
            info.AddValue("Width", this.Width);
            info.AddValue("Top", this.GetValue(Canvas.TopProperty));
            info.AddValue("Left", this.GetValue(Canvas.LeftProperty));

            info.AddValue("Addr", Addr);
            info.AddValue("Voltage", Voltage);
            info.AddValue("IsSleep", IsSleep);
            info.AddValue("IsPhoneSleep", IsPhoneSleep);
            info.AddValue("PanicName", SensorName);
            info.AddValue("TextView", TextView.Text);
            info.AddValue("AlramPath", AlramPath);

            info.AddValue("Sc", Sc);
            info.AddValue("Matrix", Matrix);
            info.AddValue("Preset", Preset);
            info.AddValue("Cameras", Cameras);

            info.AddValue("isInterphone2", isInterphone2);
            info.AddValue("isInterphone3", isInterphone3);
            info.AddValue("isInterphone4", isInterphone4);

            info.AddValue("StartTime", startTime);
            info.AddValue("EndTime", endTime);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            wndProc = (WndProc)Application.Current.Properties["wndProc"];
            IconCanvas = (Canvas)Application.Current.Properties["IconCanvas"];
            zoomBox = (ZoomBoxLibrary.ZoomBoxPanel)Application.Current.Properties["zoomBox"];

            viewport3D.MouseEnter += new MouseEventHandler(SensorControl_MouseEnter);
            viewport3D.MouseLeave += new MouseEventHandler(SensorControl_MouseLeave);

            SetTooltip();
         }

        public int Addr
        {
            get { return addr; }
            set { addr = value; }
        }

        public int Voltage
        {
            get { return voltage; }
            set { voltage = value; }
        }

        public bool IsSleep
        {
            get { return isSleep; }
            set { isSleep = value; }
        }
        
        public bool IsPhoneSleep
        {
            get { return isPhoneSleep; }
            set { isPhoneSleep = value; }
        }

        public string SensorName
        {
            get { return sensorName; }
            set { sensorName = value; }
        }

        public string AlramPath
        {
            get { return alramPath; }
            set { alramPath = value; }
        }

        public DateTime StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        public DateTime EndTime
        {
            get { return endTime; }
            set { endTime = value; }
        }

        public List<IntToInt> Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

        public List<IntToInt> Preset
        {
            get { return preset; }
            set { preset = value; }
        }

        public List<CameraControl> Cameras
        {
            get { return cameras; }
            set { cameras = value; }
        }

        public List<IntToInt> Sc
        {
            get { return sc; }
            set { sc = value; }
        }

        public void SetTooltip()
        {
            viewport3D.ToolTip = "주소 : " + addr.ToString();
        }

        void SensorControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.isMouseDown = false;

            button.ReleaseMouseCapture();
        }

        void SensorControl_MouseMove(object sender, MouseEventArgs e)
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

        void SensorControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.beginX = e.GetPosition(IconCanvas).X;
            this.beginY = e.GetPosition(IconCanvas).Y;

            isMouseDown = true;

            button.CaptureMouse();
        }

        void SensorControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Maps maps = MainWindow.maps;

            if (!(maps.MapMoveLock))
                zoomBox.MouseMode = ZoomBoxLibrary.ZoomBoxPanel.eMouseMode.Pan;

            (Resources["MouseEnterStory"] as Storyboard).Remove();
        }

        void SensorControl_MouseEnter(object sender, MouseEventArgs e)
        {
            zoomBox.MouseMode = ZoomBoxLibrary.ZoomBoxPanel.eMouseMode.None;
            (Resources["MouseEnterStory"] as Storyboard).Begin();
        }

        public void SetMove(bool set)
        {
            if (set)
            {
                this.button.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(SensorControl_MouseLeftButtonDown);
                this.button.PreviewMouseMove += new MouseEventHandler(SensorControl_MouseMove);
                this.button.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(SensorControl_MouseLeftButtonUp);
            }
            else
            {
                this.button.PreviewMouseLeftButtonDown -= SensorControl_MouseLeftButtonDown;
                this.button.PreviewMouseMove -= SensorControl_MouseMove;
                this.button.PreviewMouseLeftButtonUp -= SensorControl_MouseLeftButtonUp;
            }
        }

        public void SetContextMenu()
        {
            ContextMenu menu = new ContextMenu();

            MenuItem item_life = new MenuItem();
            MenuItem item_death = new MenuItem();
            MenuItem item_selftest = new MenuItem();
            MenuItem item_change = new MenuItem();
            MenuItem item_remove = new MenuItem();
            MenuItem item_sc = new MenuItem();
            MenuItem item_dvr = new MenuItem();
            MenuItem item_matrix = new MenuItem();
            MenuItem item_preset = new MenuItem();
            MenuItem item_interphone = new MenuItem();
            MenuItem item_alrampath = new MenuItem();

            item_life.Header = "사용";
            item_death.Header = "중지";
            item_selftest.Header = "경보 테스트";
            item_change.Header = "수정";
            item_remove.Header = "삭제";
            item_sc.Header = "SC 연동";
            item_dvr.Header = "DVR";
            item_matrix.Header = "매트릭스";
            item_preset.Header = "프리셋";
            item_interphone.Header = "인터폰";
            item_alrampath.Header = "경보음 설정";

            menu.Items.Add(item_life);
            menu.Items.Add(item_death);
            menu.Items.Add(new Separator());
            menu.Items.Add(item_sc);
            menu.Items.Add(item_dvr);
            menu.Items.Add(item_matrix);
            menu.Items.Add(item_preset);
            menu.Items.Add(item_interphone);
            menu.Items.Add(new Separator());
            menu.Items.Add(item_alrampath);
            menu.Items.Add(item_selftest);
            menu.Items.Add(new Separator());

            menu.Items.Add(item_change);
            menu.Items.Add(item_remove);
      
            item_life.IsCheckable = true;
            item_death.IsCheckable = true;


            if (IsSleep)
            {
                item_life.IsChecked = false;
                item_death.IsChecked = true;
            }
            else
            {
                item_life.IsChecked = true;
                item_death.IsChecked = false;
            }

            item_life.Checked += new RoutedEventHandler(item_life_Checked);
            item_life.Unchecked += new RoutedEventHandler(item_life_Unchecked);
            item_death.Checked += new RoutedEventHandler(item_death_Checked);
            item_death.Unchecked += new RoutedEventHandler(item_death_Unchecked);
            //item_alrampath.Click += new RoutedEventHandler(item_alrampath_Click);
            //item_selftest.Click += new RoutedEventHandler(item_selftest_Click);

            MenuItem item_phone_life = new MenuItem();
            MenuItem item_phone_death = new MenuItem();
            MenuItem item_interphone1 = new MenuItem();
            MenuItem item_interphone2 = new MenuItem();
            MenuItem item_interphone3 = new MenuItem();
            MenuItem item_interphone4 = new MenuItem();

            item_phone_life.Header = "On";
            item_phone_death.Header = "Off";
            item_interphone1.Header = "인터폰1";
            item_interphone2.Header = "인터폰2";
            item_interphone3.Header = "인터폰3";
            item_interphone4.Header = "인터폰4";

            item_interphone.Items.Add(item_phone_life);
            item_interphone.Items.Add(item_phone_death);
            item_interphone.Items.Add(new Separator());
            item_interphone.Items.Add(item_interphone1);
            item_interphone.Items.Add(item_interphone2);
            item_interphone.Items.Add(item_interphone3);
            item_interphone.Items.Add(item_interphone4);

            item_phone_life.IsCheckable = true;
            item_phone_death.IsCheckable = true;
            item_interphone1.IsCheckable = true;
            item_interphone2.IsCheckable = true;
            item_interphone3.IsCheckable = true;
            item_interphone4.IsCheckable = true;

            if (IsPhoneSleep)
            {
                item_phone_life.IsChecked = false;
                item_phone_death.IsChecked = true;
            }
            else
            {
                item_phone_life.IsChecked = true;
                item_phone_death.IsChecked = false;
            }

            item_interphone1.IsChecked = true;
            item_interphone2.IsChecked = isInterphone2;
            item_interphone3.IsChecked = isInterphone3;
            item_interphone4.IsChecked = isInterphone4;

            item_phone_life.Checked += new RoutedEventHandler(item_phone_life_Checked);
            item_phone_life.Unchecked += new RoutedEventHandler(item_phone_life_Unchecked);
            item_phone_death.Checked += new RoutedEventHandler(item_phone_death_Checked);
            item_phone_death.Unchecked += new RoutedEventHandler(item_phone_death_Unchecked);
            item_interphone1.Unchecked += new RoutedEventHandler(item_interphone1_Unchecked);
            item_interphone2.Checked += new RoutedEventHandler(item_interphone2_Checked);
            item_interphone2.Unchecked += new RoutedEventHandler(item_interphone2_Unchecked);
            item_interphone3.Checked += new RoutedEventHandler(item_interphone3_Checked);
            item_interphone3.Unchecked += new RoutedEventHandler(item_interphone3_Unchecked);
            item_interphone4.Checked += new RoutedEventHandler(item_interphone4_Checked);
            item_interphone4.Unchecked += new RoutedEventHandler(item_interphone4_Unchecked);

            button.ContextMenu = menu;

            item_change.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(item_change_PreviewMouseLeftButtonDown);
            item_remove.Click += new RoutedEventHandler(item_remove_Click);
            item_matrix.Click += new RoutedEventHandler(item_matrix_Click);
            item_preset.Click += new RoutedEventHandler(item_preset_Click);
            item_sc.Click += new RoutedEventHandler(item_sc_Click);
        }

        void item_alrampath_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.Filter = "Wav files |*.wav"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string strMapPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                //  strMapPath += FileName.Text;

                strMapPath = strMapPath.Substring(6) + @"\wav\Sensor" + Addr.ToString() + ".wav";
                AlramPath = strMapPath;
                FileInfo s_file = new FileInfo(dlg.FileName);
                FileInfo c_file = s_file.CopyTo(strMapPath, true);
            }
        }

        

        void item_interphone2_Checked(object sender, RoutedEventArgs e)
        {
            isInterphone2 = true;
        }

        void item_interphone2_Unchecked(object sender, RoutedEventArgs e)
        {
            isInterphone2 = false;
        }

        void item_interphone3_Checked(object sender, RoutedEventArgs e)
        {
            isInterphone3 = true;
        }

        void item_interphone3_Unchecked(object sender, RoutedEventArgs e)
        {
            isInterphone3 = false;
        }

        void item_interphone4_Checked(object sender, RoutedEventArgs e)
        {
            isInterphone4 = true;
        }

        void item_interphone4_Unchecked(object sender, RoutedEventArgs e)
        {
            isInterphone4 = false;
        }

        void item_phone_death_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsPhoneSleep)
            {
                (sender as MenuItem).IsChecked = true;
            }
        }

        void item_phone_death_Checked(object sender, RoutedEventArgs e)
        {
            IsPhoneSleep = true;
            MenuItem item = ((MenuItem)((MenuItem)sender).Parent).Items[0] as MenuItem;
            item.IsChecked = false;
        }

        void item_phone_life_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!IsPhoneSleep)
            {
                (sender as MenuItem).IsChecked = true;
            }
        }

        void item_phone_life_Checked(object sender, RoutedEventArgs e)
        {
            IsPhoneSleep = false;
            MenuItem item = ((MenuItem)((MenuItem)sender).Parent).Items[1] as MenuItem;
            item.IsChecked = false;
        }

        void item_interphone1_Unchecked(object sender, RoutedEventArgs e)
        {
            (sender as MenuItem).IsChecked = true;
        }

        void item_preset_Click(object sender, RoutedEventArgs e)
        {
            InputPreset preset = new InputPreset(this);
            preset.Top = (Application.Current.Properties["Root"] as Canvas).ActualHeight / 2 - preset.Height;
            preset.Left = (Application.Current.Properties["Root"] as Canvas).ActualWidth / 2 - preset.Width / 2;
            preset.ShowDialog();
        }

        void item_sc_Click(object sender, RoutedEventArgs e)
        {
            InputSc sc = new InputSc(this);
            sc.Top = (Application.Current.Properties["Root"] as Canvas).ActualHeight / 2 - sc.Height / 2;
            sc.Left = (Application.Current.Properties["Root"] as Canvas).ActualWidth / 2 - sc.Width / 2;
            sc.ShowDialog();
        }

        void item_matrix_Click(object sender, RoutedEventArgs e)
        {
            InputMatrix matix = new InputMatrix(this);
            matix.Top = (Application.Current.Properties["Root"] as Canvas).ActualHeight / 2 - matix.Height;
            matix.Left = (Application.Current.Properties["Root"] as Canvas).ActualWidth / 2 - matix.Width / 2;
            matix.ShowDialog();
        }

        void item_death_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsSleep)
            {
                (sender as MenuItem).IsChecked = true;
            }
        }

        void item_life_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!IsSleep)
            {
                (sender as MenuItem).IsChecked = true;
            }
        }

        void item_life_Checked(object sender, RoutedEventArgs e)
        {
            IsSleep = false;
            MenuItem item = ((ContextMenu)((MenuItem)sender).Parent).Items[1] as MenuItem;
            item.IsChecked = false;

            button.Template = (ControlTemplate)UserControl.Resources["ButtonControlTemplate1"];
        }

        void item_death_Checked(object sender, RoutedEventArgs e)
        {
            IsSleep = true;
            MenuItem item = ((ContextMenu)((MenuItem)sender).Parent).Items[0] as MenuItem;
            item.IsChecked = false;

            button.Template = (ControlTemplate)UserControl.Resources["ButtonControlTemplate2"];
            
        }

        void item_change_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            InputSensorInfo info = new InputSensorInfo(this);
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
            if (MessageBoxResult.Yes == MessageBox.Show("정말 지우시겠습니까?", " 번호 : " + Addr + ", 이름 : " + SensorName, MessageBoxButton.YesNo))
            {
                (Application.Current.Properties["IconCanvas"] as Canvas).Children.Remove(this);
                MainWindow.maps[MainWindow.maps.SelectIndex].SensorList.Remove(this);

            }
        }

        public void SetPath()
        {
            string geo = "M0,";
            geo += this.Height.ToString();
            geo += " L";
            geo += this.Height.ToString();
            geo += " ,";
            geo += this.Height.ToString();
            geo += "  ";
            geo += (this.Width / 2).ToString();
            geo += " ,0 z";

            path.Data = Geometry.Parse(geo);
        }

        public void SetViewText(int index)
        {
            if (index == 0)
            {
                TextView.Text = "";
            }
            else if (index == 1)
            {
                TextView.Text = Addr.ToString();
            }
            else if (index == 2)
            {
                TextView.Text = SensorName;
            }
        }

	}
}