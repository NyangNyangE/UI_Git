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
    /// SMS.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Smartphone : Window
    {
        PanicSmartSystem PSS;
        private const string IP_NOT_FOUND = "해당 IP를 확인할 수 없습니다.";

        public Smartphone(ref PanicSmartSystem _PSS)
        {
            InitializeComponent();

            PSS = _PSS;

            Account.Text = PSS.Account;
            Password.Password = PSS.Password;
            Address.Text = NetworkManager.GetIP();
            if (Address.Text == "")
            {
                Address.Text = IP_NOT_FOUND;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (Account.Text == "" || Password.Password == "")
            {
                Account.Focus();
                MessageBox.Show("\tSMS 계정과 비밀번호를 설정하시기 바랍니다.\t");
                return;
            }

            PSS.Account = Account.Text;
            PSS.Password = Password.Password;
            if (Address.Text == IP_NOT_FOUND)
            {
                PSS.Address = "";
            }
            {
                PSS.Address = Address.Text;
            }

            Properties.Settings.Default.IsPanicSmartphone = true;

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
