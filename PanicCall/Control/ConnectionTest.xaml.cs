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
using System.Threading;
using System.Windows.Threading;

namespace PanicCall.Control
{
    /// <summary>
    /// ConnectionTest.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConnectionTest : Window
    {
        MainWindow Main;

        public ConnectionTest(MainWindow _Main)
        {
            InitializeComponent();
            Main = _Main;
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            if (AcuNumber.Text == "" || AcuNumber.Text == string.Empty)
            {
                MessageBox.Show("ACU 번호를 입력하시기 바랍니다.");
                return;
            }

            int Number = 0;
            try
            {
                Number = Convert.ToInt32(AcuNumber.Text);
            }
            catch
            {
                MessageBox.Show("ACU 번호는 숫자만 입력하시기 바랍니다.");
                return;
            }

            if (Number <= 0 || Number > 999)
            {
                MessageBox.Show("Acu 번호는 0 ~ 999 입니다.");
                return;
            }

            Test.IsEnabled = false;
            ConnectionState.Foreground = Brushes.Black;
            ConnectionState.Text = "대기";

            Main.isConnectionState = ACU_STATE.WAIT;
            Main.ConnectionTesting(Number);

            ThreadPool.QueueUserWorkItem(new WaitCallback(CheckState), null);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public void CheckState(object obj)
        {
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(1000);
                    if (Main.isConnectionState != ACU_STATE.WAIT)
                    {
                        break;
                    }
                }
            }
            finally
            {
                this.Dispatcher.BeginInvoke
                (
                    DispatcherPriority.Normal, (ThreadStart)delegate()
                    {
                        Test.IsEnabled = true;

                        switch (Main.isConnectionState)
                        {
                            case ACU_STATE.OK:
                                ConnectionState.Foreground = Brushes.DarkGreen;
                                ConnectionState.Text = "정상";
                                break;

                            case ACU_STATE.ERROR:
                                ConnectionState.Foreground = Brushes.Red;
                                ConnectionState.Text = "이상";
                                break;

                            case ACU_STATE.WAIT:
                                ConnectionState.Foreground = Brushes.Red;
                                ConnectionState.Text = "시간초과";
                                break;
                        }
                    }
                );
            }
        }
    }
}
