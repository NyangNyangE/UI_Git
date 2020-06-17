using System;
using System.IO.Ports;
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
    /// Connect.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Connect : Window
    {
        List<string> PortList = new List<string>();
        List<int> BaudRateList = new List<int>();

   //     public static string Port = "AUTO";
  //      public static int BaudRate = 9600;

        public Connect()
        {
            InitializeComponent();

            SetComboBox();
            cbPort.SelectedItem = Properties.Settings.Default.PortSelect;
            cbBaud.SelectedItem = Properties.Settings.Default.nBaudRate;
            
        }

        private void SetComboBox()
        {
            PortList.Add("AUTO");

            foreach (string com in SerialPort.GetPortNames())
            {
                PortList.Add(com);
            }
            /*
            for (int i = 1; i < 16; i++)
            {
                PortList.Add("COM" + i.ToString());
            }
            */
            BaudRateList.Add(9600);
            BaudRateList.Add(14400);
            BaudRateList.Add(19200);
            BaudRateList.Add(28800);
            BaudRateList.Add(33600);
            BaudRateList.Add(38400);
            BaudRateList.Add(56000);
            BaudRateList.Add(57600);
            BaudRateList.Add(115200);
            BaudRateList.Add(128000);
            BaudRateList.Add(256000);

            cbPort.ItemsSource = PortList;
            cbBaud.ItemsSource = BaudRateList;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PortSelect = (string)cbPort.SelectedItem;
            Properties.Settings.Default.nBaudRate = (int)cbBaud.SelectedItem;

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
