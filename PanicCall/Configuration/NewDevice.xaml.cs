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

namespace PanicCall.Configuration
{
    /// <summary>
    /// Device.xaml에 대한 상호 작용 논리
    /// </summary>

    public partial class NewDevice : Window
    {
   //     public static bool IsSendSms = false;

        static NewDevice device;

        List<PowerDevice> DeviceList;

        private NewDevice(ref List<PowerDevice> _device)
        {
            InitializeComponent();

            DeviceList = _device;

            DeviceListView.ItemsSource = DeviceList;

            if (Properties.Settings.Default.IsNetwork)
            {
                IsSendSms.IsChecked = Properties.Settings.Default.IsDeviceSms;
            }
            else
            {
                IsSendSms.IsChecked = false;
            }

            InitRack();
        }

        public static bool IsDevice() 
        {
            if (device == null)
                return false;

            return true;
        }

        public static NewDevice GetDevice()
        {
            if (device == null)
            {
                return null;
            }

            return device;
        }

        public static NewDevice GetDevice(ref List<PowerDevice> _device)
        {
            if (device == null)
            {
                device = new NewDevice(ref _device);
            }

            return device;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.IsNetwork == true)
            {
                if (IsSendSms.IsChecked == true)
                {
                    Properties.Settings.Default.IsDeviceSms = true;
                }
                else
                {
                    Properties.Settings.Default.IsDeviceSms = false;
                }
            }
            device = null;
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            device = null;
            this.DialogResult = false;
            this.Close();
        }

        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            if (tbDeviceNum.Text == "")
            {
                MessageBox.Show("장치 번호를 입력해 주세요", "입력오류");
                return;
            }

            if (tbDeviceName.Text == "")
            {
                MessageBox.Show("장치 이름을 입력해 주세요", "입력오류");
                return;
            }

            foreach (PowerDevice pd in DeviceList)
            {
                if (pd.DeviceNum == Convert.ToInt32(tbDeviceNum.Text))
                {
                    MessageBox.Show("이미 등록된 장치 번호 입니다","입력오류");
                    return;
                }
            }

            PowerDevice device = new PowerDevice();
            device.DeviceName = tbDeviceName.Text;

            try
            {
                device.DeviceNum = Convert.ToInt32(tbDeviceNum.Text);
            }
            catch
            {
                MessageBox.Show("장치번호가 잘못 되었습니다", "입력오류");
                return;
            }
               
            DeviceList.Add(device);
            DeviceListView.Items.Refresh();
          //  DeviceListView.Items.Add(device);
          //  MainWindow.serialport.SendDeviceScan(device.DeviceNum);

            tbDeviceName.Text = "";
            tbDeviceNum.Text = "";
        }

        public void Refresh()
        {
            DeviceListView.Items.Refresh();
        }

        private void DeleteDevice_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceListView.SelectedItem != null)
            {
                PowerDevice devic = DeviceListView.SelectedItem as PowerDevice;
                DeviceList.Remove(devic);
                Refresh();
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(MainWindow.IsNetwork())
            {
                IsSendSms.IsChecked = false;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void ModifyDevice_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceListView.SelectedItem == null)
                return;

            if (tbDeviceNum.Text == "")
            {
                MessageBox.Show("장치 번호를 입력해 주세요", "입력오류");
                return;
            }

            if (tbDeviceName.Text == "")
            {
                MessageBox.Show("장치 이름을 입력해 주세요", "입력오류");
                return;
            }

            foreach (PowerDevice pd in DeviceList)
            {
                if (pd.DeviceNum != (DeviceListView.SelectedItem as PowerDevice).DeviceNum
                     && pd.DeviceNum == Convert.ToInt32(tbDeviceNum.Text))
                {
                    MessageBox.Show("이미 등록된 장치 번호 입니다", "입력오류");
                    return;
                }
            }

            try
            {
                (DeviceListView.SelectedItem as PowerDevice).DeviceNum = Convert.ToInt32(tbDeviceNum.Text);
            }
            catch
            {
                MessageBox.Show("장치번호가 잘못 되었습니다", "입력오류");
                return;
            }
             
            (DeviceListView.SelectedItem as PowerDevice).DeviceName = tbDeviceName.Text;

            Refresh();
            RefreshRack();
        }

        private void DeviceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeviceListView.SelectedItem == null)
            {
                tbDeviceNum.Text = "";
                tbDeviceName.Text = "";

                return;
            }

            tbDeviceNum.Text = (DeviceListView.SelectedItem as PowerDevice).DeviceNum.ToString();
            tbDeviceName.Text = (DeviceListView.SelectedItem as PowerDevice).DeviceName;
        }

        private void InitRack()
        {
            List<Rack> RackList = Application.Current.Properties["RackList"] as List<Rack>;
            int count = RackList.Count;

            ContextMenu RackMenu = new ContextMenu();

            MenuItem AddRackItem    = new MenuItem();
            AddRackItem.Header      = "랙 추가";
            AddRackItem.ToolTip     = "랙을 추가합니다.";
            AddRackItem.Click       += new RoutedEventHandler(AddRack_Click);

            RackMenu.Items.Add(AddRackItem);

            ListRack.ContextMenu = RackMenu;

            for (int i = 0; i < count; i++)
            {
                AddListItemRack(false);

                for (int j = 0; j < RackList[i].RackList.Count; j++ )
                {
                    ListBoxItem listitem = ListRack.Items[i] as ListBoxItem;
                    Canvas can = listitem.Content as Canvas;
                    ListBox lb = can.Children[1] as ListBox;

                    Label tb = new Label();
                    tb.Width = 94;
                    tb.Height = 12 * RackList[i].RackList[j].DeviceType;
                    tb.FontSize = 8;
                    tb.Margin = new Thickness(0, 1, 0, 0);

                    if (RackList[i].RackList[j].DeviceType == 1)
                    {
                        tb.Height = 17;
                        tb.FontSize = 6;
                    }

                    tb.BorderBrush = Brushes.Black;
                    tb.BorderThickness = new Thickness(1, 1, 1, 1);

                    foreach (PowerDevice pd in DeviceList)
                    {
                        if (pd.DeviceNum == RackList[i].RackList[j].DeviceNum)
                        {
                            tb.Content = pd.DeviceName;
                            
                            if (pd.Flag)
                                tb.Background = Brushes.Green;
                            else
                                tb.Background = Brushes.Red;

                            break;
                        }
                    }

                    tb.HorizontalContentAlignment = HorizontalAlignment.Center;
                    tb.VerticalContentAlignment = VerticalAlignment.Center;
                    
                    ContextMenu menu = new ContextMenu();
                    menu.Tag = i;

                    MenuItem DeleteMenu = new MenuItem();
                    DeleteMenu.Header = "선택한 아이템 삭제";
                    DeleteMenu.ToolTip = "현재 선택된 아이템을 제거합니다";
                    DeleteMenu.Click += new RoutedEventHandler(DeleteMenuClick);
                    menu.Items.Add(DeleteMenu);

                    menu.Items.Add(new Separator());

                    MenuItem SetItem = new MenuItem();
                    SetItem.Header = "장치 설정";
                    SetItem.ToolTip = "현재 선택된 아이템에 장치를 설정합니다";

                    for (int index = 0; DeviceListView.Items.Count > index; index++)
                    {
                        MenuItem item = new MenuItem();
                        item.Header = (DeviceListView.Items[index] as PowerDevice).DeviceNum + ". " + (DeviceListView.Items[index] as PowerDevice).DeviceName;
                        item.ToolTip = "현재위치에 \"" + (DeviceListView.Items[index] as PowerDevice).DeviceName.ToString() + "\" 을 설정합니다";
                        item.Tag = (DeviceListView.Items[index] as PowerDevice).DeviceNum;
                        item.Click += new RoutedEventHandler(ContextMenuClick);
                        SetItem.Items.Add(item);
                    }

                    menu.Items.Add(SetItem);

                    tb.ContextMenu = menu;
                    lb.Items.Add(tb);
                }
            }
        }

        private void DeleteMenuClick(object sender, RoutedEventArgs e)
        {
            string RackNumber = (((sender as MenuItem).Parent as ContextMenu).Tag.ToString());
            ListRack.SelectedIndex = Convert.ToInt32(RackNumber);
            ListRack.Focus();

            ListBoxItem listitem = ListRack.SelectedItem as ListBoxItem;
            Canvas can = listitem.Content as Canvas;
            ListBox lb = can.Children[1] as ListBox;

            if (lb.SelectedIndex < 0)
                return;

            List<Rack> RackList = Application.Current.Properties["RackList"] as List<Rack>;
            Rack rac = RackList[ListRack.SelectedIndex];

            rac.RackList.RemoveAt(lb.SelectedIndex);
            lb.Items.RemoveAt(lb.SelectedIndex);
        }

        private void ContextMenuClick(object sender, RoutedEventArgs e)
        {
            string RackNumber = ((((sender as MenuItem).Parent as MenuItem).Parent as ContextMenu).Tag.ToString());
            ListRack.SelectedIndex = Convert.ToInt32(RackNumber);
            ListRack.Focus();

            if (ListRack.SelectedIndex < 0)
            {
                return;
            }

            MenuItem mi = sender as MenuItem;
            ListBoxItem listitem = ListRack.SelectedItem as ListBoxItem;
            Canvas can = listitem.Content as Canvas;
            ListBox lb = can.Children[1] as ListBox;

            List<Rack> RackList = Application.Current.Properties["RackList"] as List<Rack>;
            Rack rac = RackList[ListRack.SelectedIndex];

            foreach (PowerDevice pd in DeviceList)
            {
                if (pd.DeviceNum == Convert.ToInt32(mi.Tag))
                {
                    rac.RackList[lb.SelectedIndex].DeviceNum = pd.DeviceNum;
                    tbRackDeviceName.Text = pd.DeviceName;
                    (lb.SelectedItem as Label).Content = pd.DeviceName;

                    if (pd.Flag)
                        (lb.SelectedItem as Label).Background = Brushes.Green;
                    else
                        (lb.SelectedItem as Label).Background = Brushes.Red;

                    (lb.SelectedItem as Label).ToolTip = "장치번호 : " + pd.DeviceNum.ToString();
                    (lb.SelectedItem as Label).ToolTip += "\n";
                    (lb.SelectedItem as Label).ToolTip += "장치이름 : " + pd.DeviceName;

                    break;
                }
            }
        }

        private void AddRack_Click(object sender, RoutedEventArgs e)
        {
            AddListItemRack(true);
        }

        private void AddListItemRack(bool flag)
        {
            if (flag)
            {
                List<Rack> RackList = Application.Current.Properties["RackList"] as List<Rack>;
                RackList.Add(new Rack());
            }

            Canvas can = new Canvas();

            can.Background = Brushes.LightGray;
            
            can.Margin = new Thickness(0, 0, 2, 0);
            can.Width = 140;
            can.Height = 400;

            ContextMenu menu = new ContextMenu();

            MenuItem RackDeleteItem = new MenuItem();
            RackDeleteItem.Header = "랙 제거";
            RackDeleteItem.ToolTip = "현재 랙을 제거합니다";

            MenuItem RackItemAdd = new MenuItem();
            RackItemAdd.Header = "장치 추가";
            RackItemAdd.ToolTip = "현재 랙에 장치를 추가합니다";

            MenuItem Item1U = new MenuItem();
            MenuItem Item2U = new MenuItem();
            MenuItem Item4U = new MenuItem();
            MenuItem ItemMonitor = new MenuItem();
            MenuItem ItemDvr = new MenuItem();

            Item1U.Header = "1U";
            Item2U.Header = "2U";
            Item4U.Header = "4U";
            ItemMonitor.Header = "Monitor";
            ItemDvr.Header = "Dvr";

            RackDeleteItem.Click    += new RoutedEventHandler(DeleteRack_Click);
            Item1U.Click            += new RoutedEventHandler(RackType1U_Click);
            Item2U.Click            += new RoutedEventHandler(RackType2U_Click);
            Item4U.Click            += new RoutedEventHandler(RackType4U_Click);
            ItemMonitor.Click       += new RoutedEventHandler(RackTypeMonitor_Click);
            ItemDvr.Click           += new RoutedEventHandler(RackTypeDVR_Click);

            RackItemAdd.Items.Add(Item1U);
            RackItemAdd.Items.Add(Item2U);
            RackItemAdd.Items.Add(Item4U);
            RackItemAdd.Items.Add(ItemMonitor);
            RackItemAdd.Items.Add(ItemDvr);

            menu.Items.Add(RackDeleteItem);
            menu.Items.Add(RackItemAdd);

            can.ContextMenu = menu;

            TextBlock tb = new TextBlock();
            tb.Text = "CCTV SYSTEM";
            tb.Margin = new Thickness(30, 0, 0, 0);
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.Foreground = Brushes.White;

            can.Children.Add(tb);

            ListBox lb = new ListBox();

            lb.Width = 120;
            lb.Height = 370;
            lb.Background = Brushes.White;
            lb.BorderBrush = Brushes.LightGray;
            lb.Margin = new Thickness(19, 20, 0, 0);
            lb.VerticalAlignment = VerticalAlignment.Center;

            lb.PreviewMouseDoubleClick += new MouseButtonEventHandler(lb_PreviewMouseDoubleClick);

            can.Children.Add(lb);

            ListBox lb2 = new ListBox();

            lb2.Width = 20;
            lb2.Height = 370;
            lb2.Margin = new Thickness(120, 20, 0, 0);
            lb2.Background = Brushes.LightGray;
            lb2.BorderBrush = Brushes.LightGray;

            can.Children.Add(lb2);

            Border bdr = new Border();
            bdr.Width = 140;
            bdr.Height = 400;
            bdr.BorderThickness = new Thickness(1, 1, 1, 1);
            bdr.BorderBrush = Brushes.LightGoldenrodYellow;

            can.Children.Add(bdr);

            ListBoxItem listitem = new ListBoxItem();
            listitem.Margin = new Thickness(18, 0, 0, 0);
            listitem.Content = can;
            ListRack.Items.Add(listitem);
        }
    

        void lb_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListRack.SelectedIndex < 0)
            {
                ListRack.SelectedItem = ((sender as ListBox).Parent as Canvas).Parent as ListBoxItem;
            }

            List<Rack> RackList = Application.Current.Properties["RackList"] as List<Rack>;
            ListBoxItem listitem = ListRack.SelectedItem as ListBoxItem;
            Canvas can = listitem.Content as Canvas;

            ListBox lb = can.Children[1] as ListBox;
            if (lb.SelectedIndex < 0)
                return;

            Rack rac = RackList[ListRack.SelectedIndex];

            tbRackDeviceNum.Text = "-";
            tbRackDeviceName.Text = "-";

            foreach (PowerDevice pd in DeviceList)
            {
                if (pd.DeviceNum == rac.RackList[lb.SelectedIndex].DeviceNum)
                {
                    tbRackDeviceNum.Text = pd.DeviceNum.ToString();
                    tbRackDeviceName.Text = pd.DeviceName.ToString();
                    break;
                }
            }
        }

        private void AddRackItem(int size, bool type1u)
        {
            if (ListRack.SelectedIndex < 0)
            {
                return;
            }

            ListBoxItem listitem = ListRack.SelectedItem as ListBoxItem;
            Canvas can = listitem.Content as Canvas;
            ListBox lb = can.Children[1] as ListBox;
            List<Rack> RackList = Application.Current.Properties["RackList"] as List<Rack>;
            
            Label tb = new Label();
            tb.Width = 94;
            tb.Height = 12 * size;
            tb.BorderBrush = Brushes.Black;
            tb.BorderThickness = new Thickness(1, 1, 1, 1);
            tb.Margin = new Thickness(0, 1, 0, 0);

            if (type1u)
            {
                tb.Height = 17;
                tb.FontSize = 6;
            }
            else
            {
                tb.FontSize = 8;
                tb.VerticalContentAlignment = VerticalAlignment.Center;
            }
            tb.HorizontalContentAlignment = HorizontalAlignment.Center;

            ContextMenu menu = new ContextMenu();
            menu.Tag = ListRack.SelectedIndex.ToString();

            MenuItem DeleteMenu = new MenuItem();
            DeleteMenu.Header   = "선택한 아이템 삭제";
            DeleteMenu.ToolTip  = "현재 선택된 아이템을 제거합니다";
            menu.Items.Add(DeleteMenu);

            menu.Items.Add(new Separator());

            MenuItem SetItem = new MenuItem();
            SetItem.Header = "장치 설정";
            SetItem.ToolTip = "현재 선택된 아이템에 장치를 설정합니다";

            for (int index = 0; DeviceListView.Items.Count > index; index++)
            {
                MenuItem item = new MenuItem();
                item.Header = (DeviceListView.Items[index] as PowerDevice).DeviceNum + ". " + (DeviceListView.Items[index] as PowerDevice).DeviceName;
                item.ToolTip = "현재위치에 \"" + (DeviceListView.Items[index] as PowerDevice).DeviceName.ToString() + "\" 을 설정합니다";
                item.Tag = (DeviceListView.Items[index] as PowerDevice).DeviceNum;
                item.Click += new RoutedEventHandler(ContextMenuClick);
                SetItem.Items.Add(item);
            }

            menu.Items.Add(SetItem);

            tb.ContextMenu = menu;
            lb.Items.Add(tb);
            RackList[ListRack.SelectedIndex].Add(size);
        }


        private void btModify_Click(object sender, RoutedEventArgs e)
        {
            if (ListRack.SelectedIndex < 0)
                return;

            ListBoxItem listitem = ListRack.SelectedItem as ListBoxItem;
            Canvas can = listitem.Content as Canvas;
            ListBox lb = can.Children[1] as ListBox;
           
            if (lb.SelectedIndex < 0)
                return;

            bool isCatch = false;
            List<Rack> RackList = Application.Current.Properties["RackList"] as List<Rack>;
            Rack rac = RackList[ListRack.SelectedIndex];
            foreach (PowerDevice pd in DeviceList)
            {
                if (pd.DeviceNum == Convert.ToInt32(tbRackDeviceNum.Text))
                {
                    rac.RackList[lb.SelectedIndex].DeviceNum = pd.DeviceNum;
                    tbRackDeviceName.Text = pd.DeviceName;
                    (lb.SelectedItem as Label).Content = pd.DeviceName;

                    if (pd.Flag)
                        (lb.SelectedItem as Label).Background = Brushes.Green;
                    else
                        (lb.SelectedItem as Label).Background = Brushes.Red;

                    (lb.SelectedItem as Label).ToolTip = "장치번호 : " + pd.DeviceNum.ToString();
                    (lb.SelectedItem as Label).ToolTip += "\n";
                    (lb.SelectedItem as Label).ToolTip += "장치이름 : " + pd.DeviceName;

                    isCatch = true;
                    break;
                }
            }

            if (!isCatch)
            {
                MessageBox.Show("입력한 장치 번호를 찾을 수 없습니다.", "오류");
            }
        }

        private void RackTypeMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (ListRack.SelectedItem == null)
                return;

            AddRackItem(8, false);

        }

        private void RackTypeDVR_Click(object sender, RoutedEventArgs e)
        {
            if (ListRack.SelectedItem == null)
                return;

            AddRackItem(4, false);
        }

        private void RackType4U_Click(object sender, RoutedEventArgs e)
        {
            if (ListRack.SelectedItem == null)
                return;

            AddRackItem(4, false);
        }

        private void RackType2U_Click(object sender, RoutedEventArgs e)
        {
            if (ListRack.SelectedItem == null)
                return;

            AddRackItem(2, false);
        }

        private void RackType1U_Click(object sender, RoutedEventArgs e)
        {
            if (ListRack.SelectedItem == null)
                return;

            AddRackItem(1, true);
        }

        private void RackTypeDel_Click(object sender, RoutedEventArgs e)
        {
            if (ListRack.SelectedIndex < 0)
                return;

            ListBoxItem listitem = ListRack.SelectedItem as ListBoxItem;
            Canvas can = listitem.Content as Canvas;
            ListBox lb = can.Children[1] as ListBox;

            if (lb.SelectedIndex < 0)
                return;

            List<Rack> RackList = Application.Current.Properties["RackList"] as List<Rack>;
            Rack rac = RackList[ListRack.SelectedIndex];

            rac.RackList.RemoveAt(lb.SelectedIndex);
            lb.Items.RemoveAt(lb.SelectedIndex);
        }

        private void DeleteRack_Click(object sender, RoutedEventArgs e)
        {
            if (ListRack.SelectedIndex < 0)
                return;

            List<Rack> RackList = Application.Current.Properties["RackList"] as List<Rack>;

            RackList.RemoveAt(ListRack.SelectedIndex);
            ListRack.Items.RemoveAt(ListRack.SelectedIndex);
        }

        public void RefreshRack()
        {
            ListRack.Items.Clear();
            InitRack();
        }
    }
}
