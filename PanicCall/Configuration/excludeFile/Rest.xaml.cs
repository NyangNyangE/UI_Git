using System;
using System.IO;
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
    /// Rest.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Rest : Window
    {
    //    public static int TextViewSelection = 1;
    //    public static bool IsAutoStart = true;

        public Rest()
        {
            InitializeComponent();

            if (Properties.Settings.Default.nViewButtonText == 0)
            {
                RadioNon.IsChecked = true;
            }
            else if (Properties.Settings.Default.nViewButtonText == 1)
            {
                RadioAddress.IsChecked = true;
            }
            else if (Properties.Settings.Default.nViewButtonText == 2)
            {
                RadioName.IsChecked = true;
            }

            PcPanicStop.IsChecked       = Properties.Settings.Default.IsPcPanicStop;
            AutoStart.IsChecked         = Properties.Settings.Default.IsAutoStart;
            InterphoneChar.IsChecked    = Properties.Settings.Default.IsInterphoneCharComplete;
            DataReverse.IsChecked       = Properties.Settings.Default.IsReverse;

            if (Properties.Settings.Default.IsNetwork)
            {
                SendPanicSms.IsChecked      = Properties.Settings.Default.IsPanicSms;
                SendCameraSms.IsChecked     = Properties.Settings.Default.IsCameraSms;
                SendSmartphone.IsChecked    = Properties.Settings.Default.IsPanicSmartphone;
            }
            else
            {
                SendPanicSms.IsChecked = false;
                SendCameraSms.IsChecked = false;
                SendSmartphone.IsChecked = false;
            }
        }

        //---------------------------------------------------------------------------------------
        // ● Description   : 네트워크 연결여부를 판단하여 SMS 상태 저장
        // ● Date          : 2011년 1월
        // ● Name          : 김지우
        //---------------------------------------------------------------------------------------
        // ○ 네트워크에 연결되어있지 않으면 SMS서비스는 작동되지 않아야한다.
        //---------------------------------------------------------------------------------------
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (RadioNon.IsChecked == true)
            {
                Properties.Settings.Default.nViewButtonText = 0;
            }
            else if (RadioAddress.IsChecked == true)
            {
                Properties.Settings.Default.nViewButtonText = 1;
            }
            else if (RadioName.IsChecked == true)
            {
                Properties.Settings.Default.nViewButtonText = 2;
            }

            if (AutoStart.IsChecked == true)
            {
                Properties.Settings.Default.IsAutoStart = true;
            }
            else 
            {
                Properties.Settings.Default.IsAutoStart = false;
            }

            if (PcPanicStop.IsChecked == true)
            {
                Properties.Settings.Default.IsPcPanicStop = true;
            }
            else
            {
                Properties.Settings.Default.IsPcPanicStop = false;
            }

            // 데이터 반전 여부
            if (DataReverse.IsChecked == true)
            {
                Properties.Settings.Default.IsReverse = true;
            }
            else
            {
                Properties.Settings.Default.IsReverse = false;
            }

            // 인터폰 전송 데이터 완성형 셋
            if (InterphoneChar.IsChecked == true)
            {
                Properties.Settings.Default.IsInterphoneCharComplete = true;
            }
            else
            {
                Properties.Settings.Default.IsInterphoneCharComplete = false;
            }

            if (Properties.Settings.Default.IsNetwork == true)
            {
                if (SendPanicSms.IsChecked == true)
                {
                    Properties.Settings.Default.IsPanicSms = true;
                }
                else
                {
                    Properties.Settings.Default.IsPanicSms = false;
                }

                if (SendCameraSms.IsChecked == true)
                {
                    Properties.Settings.Default.IsCameraSms = true;
                }
                else
                {
                    Properties.Settings.Default.IsCameraSms = false;
                }

                if (SendSmartphone.IsChecked == true)
                {
                    Properties.Settings.Default.IsPanicSmartphone = true;
                }
                else
                {
                    Properties.Settings.Default.IsPanicSmartphone = false;
                }
            }

            foreach (Map map in MainWindow.maps)
            {
                foreach (PanicControl panic in map.PanicList)
                {
                    panic.SetViewText(Properties.Settings.Default.nViewButtonText);
                }

                foreach (SensorControl sensor in map.SensorList)
                {
                    sensor.SetViewText(Properties.Settings.Default.nViewButtonText);
                }

                foreach (CameraControl camera in map.CameraList)
                {
                    camera.SetViewText(Properties.Settings.Default.nViewButtonText);
                }
            }

            //---------------------------------------------------------------------------------------
            // ● Description   : 예약작업을 이용한 윈도우시작시 관리자권한 프로그램 실행
            // ● Date          : 2011년 3월
            // ● Name          : 김지우
            //---------------------------------------------------------------------------------------
            // ○ 관리자권한 프로그램을 윈도우 시작시 구동시키기 위한 방법
            //---------------------------------------------------------------------------------------
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            startInfo.FileName = "CMD.exe";
            startInfo.WorkingDirectory = @"c:\";

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;

            process.EnableRaisingEvents = false;
            process.StartInfo = startInfo;
            process.Start();

            if (AutoStart.IsChecked == true)
            {
                Properties.Settings.Default.IsAutoStart = true;
                string FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                FilePath = FilePath.Remove(0, 6);
                StringBuilder ShortPath = new StringBuilder(FilePath);
                string FileName = "\\PanicCall.exe";

                Win32APIStorage.GetShortPathName(FilePath, ShortPath, FilePath.Length);

                process.StandardInput.WriteLine("schtasks /Create /tn TWS_PANIC /tr " + ShortPath + FileName +" /sc onlogon /rl highest /f");
            }
            else
            {
                Properties.Settings.Default.IsAutoStart = false;
                process.StandardInput.WriteLine("schtasks /Delete /tn TWS_PANIC /f");
            }

            process.StandardInput.Close();
            process.WaitForExit();
            process.Close();

            Properties.Settings.Default.Save();

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SendCameraSms_Checked(object sender, RoutedEventArgs e)
        {
            if (MainWindow.IsNetwork())
            {
                SendCameraSms.IsChecked = false;
            }
        }

        private void SendPanicSms_Checked(object sender, RoutedEventArgs e)
        {
            if (MainWindow.IsNetwork())
            {
                SendPanicSms.IsChecked = false;
            }
        }

        private void SendSmartphone_Checked(object sender, RoutedEventArgs e)
        {
            if (MainWindow.IsNetwork())
            {
                SendSmartphone.IsChecked = false;
            }
        }
        /*
        private void RadioNon_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.nViewButtonText = 0;
        }

        private void RadioAddress_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.nViewButtonText = 1;
        }

        private void RadioName_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.nViewButtonText = 2;
        }

        private void AutoStart_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsAutoStart = true;
        }

        private void AutoStart_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsAutoStart = false;
        }
        */
    }
}
