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

namespace PanicCall
{
    /// <summary>
    /// Preset.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SecretOption : Window
    {
        MainWindow Main;
        public SecretOption(MainWindow _Main)
        {
            InitializeComponent();
            Main = _Main;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            //Main.Save();
            //Properties.Settings.Default.Save();

            if (MessageBoxResult.Yes == MessageBox.Show("현재 상태를 저장합니다", "저장", MessageBoxButton.YesNo))
            {
                Main.Save();
                Properties.Settings.Default.Save();
            }

            if (MessageBoxResult.No == MessageBox.Show("프로그램을 종료 하시겠습니까?", "종료", MessageBoxButton.YesNo))
            {
                return;
            }


            Process[] procSystecList_mini = Process.GetProcessesByName("Detector");
            foreach (Process proc in procSystecList_mini)
            {
                if (proc.HasExited == true)
                {                    
                    proc.WaitForExit();
                    continue;
                }
       
                proc.Kill();
                proc.WaitForExit();
            }

            Process[] processList = Process.GetProcessesByName("PGrestart");

            if (processList.Length > 0)
            {
                processList[0].Kill();
            }


            this.DialogResult = true;
            this.Close();

            Main.Exit();
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();

            Main.Minimize();
        }

        private void Admin_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
