using System;
using System.Collections.Generic;
using System.Text;
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

namespace PanicCall
{
	/// <summary>
	/// AddMapCtl.xaml에 대한 상호 작용 논리
	/// </summary>
    public partial class Support : UserControl
    {
        public bool isSupport;
        //string WanIP;

        public Support()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.No == MessageBox.Show("원격지원 서비스를 종료 하시겠습니까?", "원격 지원", MessageBoxButton.YesNo))
            {
                return;
            }

            this.Visibility = Visibility.Hidden;
            isSupport = false;
        }

        public void Init()
        {
            //isSupport = false;
            //WanIP = (Application.Current.Properties["MainWindow"] as MainWindow).GetWanIP();
            //IP.Content = String.Format("IP 주소 : {0}", WanIP);
        }
    }
}