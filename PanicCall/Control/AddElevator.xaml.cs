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
using System.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PanicCall
{
    /// <summary>
    /// AddElevator.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AddElevator : UserControl
    {
        bool AlwaysViewChekState;
        List<Elevator> ElevatorList;
        List<Elevator> TempElevatorList;
        List<ElevatorCamera> CameraList;
        WndProc wndProc;
        int PanicCount;
        SoundPlayer Sound;

        public AddElevator()
        {
            InitializeComponent();
            AlwaysViewChekState = Properties.Settings.Default.IsEvPanicAlways;
            CameraList = new List<ElevatorCamera>();
            Sound = new SoundPlayer();
            Sound.LoadAsync();

            PanicCount = 0;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            wndProc = (WndProc)Application.Current.Properties["wndProc"];
        }

        public void InitElevator()
        {
         
            ElevatorList = Application.Current.Properties["ElevatorList"] as List<Elevator>;

            foreach (Elevator Ev in ElevatorList)
            {
                AddListItemEvevator(Ev, false);
            }

            CheckAlwaysVisible.IsChecked = true;
            ElevatorCount.DataContext = ElevatorList.Count();

            ElevatorListBox.SelectedIndex = 0;

            TempElevatorList = new List<Elevator>();

            foreach (Dvr Temp in Application.Current.Properties["DvrList"] as List<Dvr>)
            {
                for (int i = 0; i < 16; i++)
                {
                    ElevatorCamera Cam = new ElevatorCamera();
                    Cam.Channel = i + 1;
                    Cam.DVR = Temp;

                    CameraList.Add(Cam);
                }
            }

            
        }

        private void AddElevatorButton_Click(object sender, RoutedEventArgs e)
        {
            CheckAlwaysVisible.IsChecked = true;

            if (ElevatorList.Count == 0 && TempElevatorList.Count > 0)
            {
                if (MessageBoxResult.Yes == MessageBox.Show("이전에 모두 삭제한 내역이 있습니다. 잘못 누르신 경우 데이터를 복구하시겠습니까?", "엘리베이터 복구", MessageBoxButton.YesNo))
                {
                    foreach (Elevator Ev in TempElevatorList)
                    {
                        AddListItemEvevator(Ev, false);
                        ElevatorList.Add(Ev);
                    }

                    ElevatorCount.DataContext = ElevatorList.Count();
                    ElevatorListBox.SelectedIndex = 0;

                    return;
                }
            }

            Elevator EV = new Elevator();
            AddListItemEvevator(EV, true);
        }

        private void AddListItemEvevator(Elevator _Item, bool _New)
        {
            if (_New)
            {
                ElevatorList.Add(_Item);
                ElevatorCount.DataContext = ElevatorList.Count();
            }

            Canvas Can = new Canvas();

            ContextMenu PopupMenu = new ContextMenu();

            MenuItem ElevatorSetting = new MenuItem();
            ElevatorSetting.Header = "엘리베이터 설정";
            ElevatorSetting.ToolTip = "현재 승강기의 번호를 설정합니다.";

            MenuItem PanicSelfTest = new MenuItem();
            PanicSelfTest.Header = "비상 테스트";
            PanicSelfTest.ToolTip = "현재 승강기의 비상상황을 테스트합니다.";

            MenuItem DeleteElevator = new MenuItem();
            DeleteElevator.Header = "삭제";
            DeleteElevator.ToolTip = "엘리베이터 아이콘을 삭제합니다.";

            MenuItem DeleteElevatorAll = new MenuItem();
            DeleteElevatorAll.Header = "모두 삭제";
            DeleteElevatorAll.ToolTip = "엘리베이터 아이콘을 모두 삭제합니다. (복구 불가능)";

            //ElevatorSetting.Click += new RoutedEventHandler(ElevatorSetting_Click);
            //PanicSelfTest.Click += new RoutedEventHandler(PanicSelfTest_Click);
            //DeleteElevator.Click += new RoutedEventHandler(DeleteElevator_Click);
            DeleteElevatorAll.Click += new RoutedEventHandler(DeleteElevatorAll_Click);

            PopupMenu.Items.Add(ElevatorSetting);
            PopupMenu.Items.Add(PanicSelfTest);
            PopupMenu.Items.Add(new Separator());
            PopupMenu.Items.Add(DeleteElevator);
            PopupMenu.Items.Add(DeleteElevatorAll);

            Can = CreateElevatorIcon(_Item.GetElevatorNumberHo());
            Can.ContextMenu = PopupMenu;

            ListBoxItem ListItem = new ListBoxItem();
            ListItem.Margin = new Thickness(14, 0, 0, 0);
            ListItem.Content = Can;

            ElevatorListBox.Items.Add(ListItem);

            if (_New)
            {
                ElevatorListBox.SelectedIndex = ElevatorListBox.Items.Count - 1;
                ElevatorListBox.ScrollIntoView(ElevatorListBox.SelectedItem);
            }
        }


        /* 0 : Rectangle Background
         * 1 : Image Icon
         * 2 : TextBlock ElevatorText
         * 3 : TextBlock ElevatorNumber */
        private Canvas CreateElevatorIcon(int _Ho)
        {
            Canvas Can = new Canvas();
            Can.Width = 80;
            Can.Height = 170;

            Rectangle Background = new Rectangle();
            Background.RadiusX = 15;
            Background.RadiusY = 15;
            Background.Fill = new SolidColorBrush(Color.FromRgb(35, 35, 35));
            Background.StrokeThickness = 3;
            //Background.Stroke           = new SolidColorBrush(Color.FromRgb(115, 188, 55));
            Background.Stroke = new SolidColorBrush(Color.FromRgb(48, 48, 48));
            Background.Width = 80;
            Background.Height = 140;

            Can.Children.Add(Background);

            BitmapImage IconImage = new BitmapImage();
            IconImage.BeginInit();
            IconImage.UriSource = new Uri(@"pack://application:,,,/PanicCall;component/Images/EVIcon.png");
            IconImage.CacheOption = BitmapCacheOption.OnLoad;
            IconImage.EndInit();
            IconImage.Freeze();

            Image Icon = new Image();
            Icon.Source = IconImage;
            Icon.Width = 59;
            Icon.Height = 114;
            Icon.SetValue(Canvas.TopProperty, 2.0);
            Icon.SetValue(Canvas.LeftProperty, 10.0);

            Can.Children.Add(Icon);

            TextBlock ElevatorText = new TextBlock();
            ElevatorText.Text = "ELEVATOR";
            ElevatorText.Foreground = new SolidColorBrush(Color.FromRgb(175, 175, 175));
            ElevatorText.SetValue(Canvas.TopProperty, 117.0);
            ElevatorText.SetValue(Canvas.LeftProperty, 11.0);

            Can.Children.Add(ElevatorText);

            TextBlock ElevatorNumber = new TextBlock();
            ElevatorNumber.Width = 80;
            ElevatorNumber.TextAlignment = TextAlignment.Center;
            ElevatorNumber.FontSize = 14;
            ElevatorNumber.Text = string.Format("{0}호", _Ho.ToString());
            ElevatorNumber.Foreground = new SolidColorBrush(Color.FromRgb(115, 188, 55));
            ElevatorNumber.SetValue(Canvas.TopProperty, 145.0);

            Can.Children.Add(ElevatorNumber);

            Can.MouseEnter += new MouseEventHandler(Can_MouseEnter);
            Can.MouseLeave += new MouseEventHandler(Can_MouseLeave);


            return Can;
        }

        private Canvas CreatePanicElevatorIcon(int _Ho)
        {
            Canvas Can = new Canvas();
            Can.Width = 80;
            Can.Height = 170;

            Rectangle Background = new Rectangle();
            Background.RadiusX = 15;
            Background.RadiusY = 15;
            Background.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            Background.StrokeThickness = 3;
            Background.Stroke = new SolidColorBrush(Color.FromRgb(89, 0, 0));
            Background.Width = 80;
            Background.Height = 140;

            Can.Children.Add(Background);

            BitmapImage IconImage = new BitmapImage();
            IconImage.BeginInit();
            IconImage.UriSource = new Uri(@"pack://application:,,,/PanicCall;component/Images/EVPanicIcon.png");
            IconImage.CacheOption = BitmapCacheOption.OnLoad;
            IconImage.EndInit();
            IconImage.Freeze();

            Image Icon = new Image();
            Icon.Source = IconImage;
            Icon.Width = 55;
            Icon.Height = 105;
            Icon.SetValue(Canvas.TopProperty, 10.0);
            Icon.SetValue(Canvas.LeftProperty, 15.0);

            Can.Children.Add(Icon);

            TextBlock ElevatorText = new TextBlock();
            ElevatorText.Text = "ELEVATOR";
            ElevatorText.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            ElevatorText.SetValue(Canvas.TopProperty, 117.0);
            ElevatorText.SetValue(Canvas.LeftProperty, 11.0);

            Can.Children.Add(ElevatorText);

            TextBlock ElevatorNumber = new TextBlock();
            ElevatorNumber.Width = 80;
            ElevatorNumber.TextAlignment = TextAlignment.Center;
            ElevatorNumber.FontSize = 14;
            ElevatorNumber.Text = string.Format("{0}호", _Ho.ToString());
            ElevatorNumber.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            ElevatorNumber.SetValue(Canvas.TopProperty, 145.0);

            Can.Children.Add(ElevatorNumber);

            Rectangle PanicMessageBox = new Rectangle();
            PanicMessageBox.RadiusX = 10;
            PanicMessageBox.RadiusY = 10;
            PanicMessageBox.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            PanicMessageBox.StrokeThickness = 1;
            PanicMessageBox.Stroke = new SolidColorBrush(Color.FromRgb(89, 0, 0));
            PanicMessageBox.Width = 80;
            PanicMessageBox.Height = 25;
            PanicMessageBox.SetValue(Canvas.TopProperty, 70.0);

            Can.Children.Add(PanicMessageBox);

            TextBlock PanicMessage = new TextBlock();
            PanicMessage.Width = 80;
            PanicMessage.TextAlignment = TextAlignment.Center;
            PanicMessage.FontSize = 14;
            PanicMessage.FontWeight = FontWeights.Bold;
            PanicMessage.Text = string.Format("비상 발생");
            PanicMessage.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            PanicMessage.SetValue(Canvas.TopProperty, 72.0);

            Can.Children.Add(PanicMessage);

            Can.MouseEnter += new MouseEventHandler(PanicCan_MouseEnter);
            Can.MouseLeave += new MouseEventHandler(PanicCan_MouseLeave);


            return Can;
        }

        private void PanicCan_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ElevatorListBox.SelectedIndex < 0)
            {
                return;
            }

            Canvas Can = sender as Canvas;
            ((Rectangle)Can.Children[0]).Stroke = new SolidColorBrush(Color.FromRgb(48, 48, 48));
        }

        private void PanicCan_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ElevatorListBox.SelectedIndex < 0)
            {
                return;
            }

            Canvas Can = sender as Canvas;
            ((Rectangle)Can.Children[0]).Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }

        private void Can_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ElevatorListBox.SelectedIndex < 0)
            {
                return;
            }

            Canvas Can = sender as Canvas;
            ((Rectangle)Can.Children[0]).Stroke = new SolidColorBrush(Color.FromRgb(48, 48, 48));

        }

        private void Can_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ElevatorListBox.SelectedIndex < 0)
            {
                return;
            }

            Canvas Can = sender as Canvas;
            ((Rectangle)Can.Children[0]).Stroke = new SolidColorBrush(Color.FromRgb(115, 188, 55));
        }


        private void CameraSetting_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

       

        private void CheckAlwaysVisible_Checked(object sender, RoutedEventArgs e)
        {
            AlwaysViewChekState = true;
            Properties.Settings.Default.IsEvPanicAlways = true;
            this.Height = 300;
        }

        private void CheckAlwaysVisible_Unchecked(object sender, RoutedEventArgs e)
        {
            AlwaysViewChekState = false;
            Properties.Settings.Default.IsEvPanicAlways = false;
            this.Height = 55;
        }

        private void DeleteElevatorAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("등록된 모든 승강기가 삭제됩니다. 한번 삭제되면 복구가 불가능합니다 \n정말 지우시겠습니까?", "엘리베이터 삭제", MessageBoxButton.YesNo))
            {
                TempElevatorList.Clear();

                foreach (Elevator EV in ElevatorList)
                {
                    TempElevatorList.Add(EV);
                }

                ElevatorList.Clear();
                ElevatorListBox.Items.Clear();

                ElevatorCount.DataContext = ElevatorList.Count();
                ElevatorListBox.SelectedIndex = ElevatorListBox.Items.Count - 1;
            }
        }

        private void ElevatorListBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ElevatorList.Count == 0 && TempElevatorList.Count > 0)
            {
                if (MessageBoxResult.Yes == MessageBox.Show("실수로 모두 삭제하신경우 이전 작업을 되돌리시겠습니까?", "엘리베이터 복구", MessageBoxButton.YesNo))
                {
                    foreach (Elevator Ev in TempElevatorList)
                    {
                        AddListItemEvevator(Ev, false);
                        ElevatorList.Add(Ev);
                    }

                    ElevatorCount.DataContext = ElevatorList.Count();
                    ElevatorListBox.SelectedIndex = 0;
                }
            }
        }
    }
}
