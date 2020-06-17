using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PanicCall
{
    /// <summary>
    /// PisControl.xaml에 대한 상호 작용 논리
    /// </summary>
    
   
    [Serializable]
    public partial class PisControl : UserControl, System.Runtime.Serialization.ISerializable
    {

        public string pisName;                   //pis name
        public bool isFocused;                   //선택된 그룹
        public string Ip;                        //pis ip
        public int addr;                         //pis addr
        public string serverIp;                  //입구 표시 같은 경우 여러 서버중 pis 에 주차수 합산해서 보내는 메인 서버 ip
        public bool isGatePis = false;           //pis 형태 구분
        public Group group1 = new Group();       //지하1층, 왼쪽
        public Group group2 = new Group();       //지하2층, 오른쪽
        public Group group3 = new Group();       //지하3층, 없음
        double beginX = 0;
        double beginY = 0;
        double pointX = 0;
        double pointY = 0;
        bool isMouseDown = false;
        public int selectedGroup = 0; //지하1 = 1 지하2 = 2   <- 삭제 예정
        public System.Drawing.Rectangle Bounds;

        Canvas IconCanvas;

        public PisControl()
        {
            InitializeComponent();

            isFocused = false;
            Ip = "";
            addr = -1;
            group1 = new Group();
            group2 = new Group();
            group3 = new Group();

        }

        public PisControl(SerializationInfo info, StreamingContext context)
        {
            InitializeComponent();

            try
            {
                this.pisName = info.GetString("Name");
                this.Ip = info.GetString("IP");
                this.addr = info.GetInt32("Addr");
                this.serverIp = info.GetString("ServerIP");
                this.group1 = (Group)info.GetValue("Group1", group1.GetType());
                this.group2 = (Group)info.GetValue("Group2", group2.GetType());
                this.group3 = (Group)info.GetValue("Group3", group3.GetType());

                this.isGatePis = info.GetBoolean("IsGatePis");
                this.SetValue(Canvas.TopProperty, info.GetDouble("Top"));
                this.SetValue(Canvas.LeftProperty, info.GetDouble("Left"));
                this.selectedGroup = info.GetInt32("Line");

                //group1.LoadPanics();
                //group2.LoadPanics();

                Count1.Content = group1.cars.ToString();
                Count2.Content = group2.cars.ToString();
            }
            catch (Exception ex)
            {
                Console.Write(" 로딩 에러 ");
            }
        }
       
        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            info.AddValue("Name", pisName);
            info.AddValue("IP", Ip);
            info.AddValue("Addr", addr);
            info.AddValue("ServerIP", serverIp);
            info.AddValue("Group1", group1);
            info.AddValue("Group2", group2);
            info.AddValue("Group3", group3);
            info.AddValue("IsGatePis", isGatePis);
            info.AddValue("Top", this.GetValue(Canvas.TopProperty));
            info.AddValue("Left", this.GetValue(Canvas.LeftProperty));
            info.AddValue("Line", selectedGroup);
           // info.AddValue("Parent", parent, parent.GetType());
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(pisName))
                PisTitle.Content = "P.I.S";
            else
                PisTitle.Content = pisName;

            
            group1.LoadPanics();
            group2.LoadPanics();
            group3.LoadPanics();
            
            IconCanvas = (Canvas)Application.Current.Properties["IconCanvas"];

           // this.RenderTransform = new ScaleTransform(MainWindow.scale / 2, MainWindow.scale / 2);
            
            this.MouseEnter += new MouseEventHandler(UserControl_MouseEnter);
            this.MouseLeave += new MouseEventHandler(UserControl_MouseLeave);
        }

        public void setValue(string ip, int addr, int cars1, int areas1, int cars2, int areas2, int cars3, int areas3)
        {
            this.Ip = ip;
            this.addr = addr;
            group1.cars = cars1;
            group1.areas = areas1;
            group2.cars = cars2;
            group2.areas = areas2;
            group3.cars = cars3;
            group3.areas = areas3;
        }

       
        //UI 에 주차여유공간 몇개 남았는지 표시
        public void setUICount()
        {
            try
            {

                float num1 = group1.areas - group1.cars;
                float num2 = group2.areas - group2.cars;
                float num3 = group3.areas - group3.cars;


                Count1.Content = num1;
                Count2.Content = num2;
                Count3.Content = num3;


                // 남은 주차 공간 비율 =  남은주차공간 / 전체주차공간  
                float per1 = (group1.cars == 0) ? 1 : (num1 / (float)group1.areas);
                float per2 = (group2.cars == 0) ? 1 : (num2 / (float)group2.areas);



                if (per1 < 0.1)
                {
                    State1.Content = "혼잡";
                    State1.Foreground = Brushes.Red;
                }
                else if (per1 < 0.3)
                {
                    State1.Content = "보통";
                    State1.Foreground = Brushes.Yellow;
                }
                else
                {
                    State1.Content = "여유";
                    State1.Foreground = Brushes.YellowGreen;
                }
                if (per2 < 0.1)
                {
                    State2.Content = "혼잡";
                    State2.Foreground = Brushes.Red;
                }
                else if (per2 < 0.3)
                {
                    State2.Content = "보통";
                    State2.Foreground = Brushes.Yellow;
                }
                else
                {
                    State2.Content = "여유";
                    State2.Foreground = Brushes.YellowGreen;
                }


                if (num1 == 0)          //한자리도 없을때 만차  설정이 없을때는 없음
                {
                    if (group1.areas == 0)
                    {
                        State1.Content = "없음";
                        State1.Foreground = Brushes.White;
                    }
                    else
                    {
                        State1.Content = "만차";
                        State1.Foreground = Brushes.Red;
                    }
                }
                if (num2 == 0)
                {
                    if (group2.areas == 0)
                    {
                        State2.Content = "없음";
                        State2.Foreground = Brushes.White;
                    }
                    else
                    {
                        State2.Content = "만차";
                        State2.Foreground = Brushes.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(delegate()
                {
                    Console.Write("pis err");
                }));
            }
        }

        //그룹1 선택 : 그룹1에 포함된 panic 모두 표시 및 그룹1 포커스 표시
        private void clicked1(object sender, MouseButtonEventArgs e)
        {

            //initSelect();
            //MainWindow.selectedGroup = group1;  //선택 그룹 할때  아래 select 도 변경 
            //select(group1);
            MessageBox.Show("qwe");
            if (isGatePis)
            {

                //focus
                //offFocus();
                Group1Label.Foreground = Brushes.White;
                Group2Label.Foreground = Brushes.Gray;
                Count1.Foreground = Brushes.GreenYellow;
                Count2.Foreground = Brushes.Gray;
            }

            //isFocused = true;
        }

        //그룹2 선택
        private void clicked2(object sender, MouseButtonEventArgs e)
        {
            //if (isgatepis)
            //{
            //    selectedGroup = 2;

            //    group2label.foreground = brushes.white;
            //    group1label.foreground = brushes.gray;
            //    count2.foreground = brushes.greenyellow;
            //    count1.foreground = brushes.gray;
            //}
        }


        //ip 입력 context menu
        private void PisNum_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu menu = new ContextMenu();

            MenuItem pis_setting = new MenuItem();
            MenuItem pis_remove = new MenuItem();

            pis_setting.Header = "P.I.S 설정";
            pis_remove.Header = "P.I.S 삭제";

            menu.Items.Add(pis_setting);
            menu.Items.Add(pis_remove);

            pis_setting.Click += new RoutedEventHandler(MenuItem_Click);
            pis_remove.Click += new RoutedEventHandler(DeletePis_Click);

            menu.IsOpen = true;
        }

        //ip 입력창 팝업
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            InputPisIP input = new InputPisIP(this);
            Point point = this.PointToScreen(new Point(0, 0));

            input.Top = point.Y - input.Height;
            input.Left = point.X;

            input.Show();
        }

        private void DeletePis_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("정말 지우시겠습니까?", "", MessageBoxButton.YesNo))
            {
                removeIcon();
            }
        }
        private void removeIcon()
        {
            (Application.Current.Properties["IconCanvas"] as Canvas).Children.Remove(this);
            MainWindow.maps[MainWindow.maps.SelectIndex].PisList.Remove(this);

            SetMove(false);
        }


        private void PisControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.beginX = e.GetPosition(IconCanvas).X;
            this.beginY = e.GetPosition(IconCanvas).Y;

            this.CaptureMouse();

            isMouseDown = true;

        }

        private void PisControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                // 전체 영역에서 마우스의 현재 위치
                double currX = e.GetPosition(IconCanvas).X;
                double currY = e.GetPosition(IconCanvas).Y;

                // 사각형의 현재위치
                double valueX = double.Parse(this.GetValue(Canvas.LeftProperty).ToString());
                double valueY = double.Parse(this.GetValue(Canvas.TopProperty).ToString());

                valueX += (currX - beginX);
                valueY += (currY - beginY);

                this.SetValue(Canvas.LeftProperty, valueX);
                this.SetValue(Canvas.TopProperty, valueY);


                beginX = currX;
                beginY = currY;
            }
        }

        private void PisControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            pointX = e.GetPosition(IconCanvas).X;
            pointY = e.GetPosition(IconCanvas).Y;

            isMouseDown = false;

            this.ReleaseMouseCapture();

        }
       

        public void SetMove(bool set)
        {
            if (set)
            {
                this.MouseLeftButtonDown += new MouseButtonEventHandler(PisControl_MouseLeftButtonDown);
                this.MouseMove += new MouseEventHandler(PisControl_MouseMove);
                this.MouseLeftButtonUp += new MouseButtonEventHandler(PisControl_MouseLeftButtonUp);
            }
            else
            {
                this.MouseLeftButtonDown -= PisControl_MouseLeftButtonDown;
                this.MouseMove -= PisControl_MouseMove;
                this.MouseLeftButtonUp -= PisControl_MouseLeftButtonUp;
            }
        }


        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Maps maps = MainWindow.maps;
            ((ZoomBoxLibrary.ZoomBoxPanel)Application.Current.Properties["zoomBox"]).MouseMode
                 = ZoomBoxLibrary.ZoomBoxPanel.eMouseMode.None;

        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Maps maps = MainWindow.maps;
            if (!(maps.MapMoveLock))
                ((ZoomBoxLibrary.ZoomBoxPanel)Application.Current.Properties["zoomBox"]).MouseMode
                    = ZoomBoxLibrary.ZoomBoxPanel.eMouseMode.Pan;
        }


        private void maximize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 사각형의 현재위치
            
            if (isGatePis == false)
            {
                this.Height = 65;
                this.Width = 130;
                Group1Label.Visibility = Visibility.Collapsed;
                Group2Label.Visibility = Visibility.Collapsed;
                Group3Label.Visibility = Visibility.Collapsed;
                State1.Visibility = Visibility.Visible;
                State2.Visibility = Visibility.Visible;
                Count1.Visibility = Visibility.Visible;
                Count2.Visibility = Visibility.Visible;
                Minimize.Visibility = Visibility.Visible;
                Maximize.Visibility = Visibility.Collapsed;

                Count1.Margin = new Thickness(10, 40, 0, 0);
                Count2.Margin = new Thickness(80, 40, 0, 0);
            }
            else
            {
                this.Height = 85;
                this.Width = 130;
                Group1Label.Visibility = Visibility.Visible;
                Group1Label.Content = "지하1층";
                Group2Label.Visibility = Visibility.Visible;
                Group2Label.Content = "지하2층";
                Group3Label.Visibility = Visibility.Visible;
                Group3Label.Content = "지하3층";
                State1.Visibility = Visibility.Collapsed;
                State2.Visibility = Visibility.Collapsed;
                Count1.Visibility = Visibility.Visible;
                Count2.Visibility = Visibility.Visible;
                Count3.Visibility = Visibility.Visible;
                Minimize.Visibility = Visibility.Visible;
                Maximize.Visibility = Visibility.Collapsed;

                
                Group2Label.Margin = new Thickness(10, 40, 0, 0);
                Group3Label.Margin = new Thickness(10, 60, 0, 0);

                Count1.Margin = new Thickness(80, 20, 0, 0);
                Count2.Margin = new Thickness(80, 40, 0, 0);
                Count3.Margin = new Thickness(80, 60, 0, 0); 

                
            }
            
        }


        private void minimize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Height = 20;
            this.Width = 60;
            Group1Label.Visibility = Visibility.Collapsed;
            Group2Label.Visibility = Visibility.Collapsed;
            Group3Label.Visibility = Visibility.Collapsed;

            State1.Visibility = Visibility.Collapsed;
            State2.Visibility = Visibility.Collapsed;
            Count1.Visibility = Visibility.Collapsed;
            Count2.Visibility = Visibility.Collapsed;
            Count3.Visibility = Visibility.Collapsed;

            Maximize.Visibility = Visibility.Visible;
            Minimize.Visibility = Visibility.Collapsed;

            double valueX = double.Parse(this.GetValue(Canvas.LeftProperty).ToString());
            double valueY = double.Parse(this.GetValue(Canvas.TopProperty).ToString());

        }

        public void changeTitle()
        {
            if (string.IsNullOrWhiteSpace(pisName))
                PisTitle.Content = "P.I.S";
            else
                PisTitle.Content = pisName;
        }


        //public Rect BoundsRelativeTo(this FrameworkElement element, Visual relativeTo)
        //{
        //    return element.RenderTransform.TransformBounds(new Rect(element.RenderSize));
        //}
        
    }
}
