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
    public partial class SMS : Window
    {
        Sms sms;

        public SMS(ref Sms _sms)
        {
            InitializeComponent();

            sms = _sms;

            Account.Text = sms.Account;
            Password.Password = sms.Password;
            SendName.Text = sms.SendName;
            SendAddress.Text = sms.SendAddress;
            SendNumber.Text = sms.SendNumber;
            ReciveNumber.Text = sms.ReciveNumber;
            ReciveAsNumber.Text = sms.ReciveAsNumber;
            ReciveAsNumber2.Text = sms.ReciveAsNumber2;
            BtnDelayTime.Text = sms.BtnDelayTime;
            PowerDelayTime.Text = sms.PowerDelayTime;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (Account.Text == "" || Password.Password == "")
            {
                Account.Focus();
                MessageBox.Show("\tSMS 계정과 비밀번호를 설정하시기 바랍니다.\t");
                return;
            }

            if (SendName.Text == "")
            {
                SendName.Focus();
                MessageBox.Show("\t현장명을 설정하시기 바랍니다.\t");
                return;
            }
            if (SendAddress.Text == "")
            {
                SendAddress.Focus();
                MessageBox.Show("\t현장주소를 설정하시기 바랍니다.\t");
                return;
            }

            if (SendNumber.Text == "")
            {
                SendNumber.Focus();
                MessageBox.Show("\t현장연락처를 설정하시기 바랍니다.\t");
                return;
            }
            if (ReciveNumber.Text == "")
            {
                ReciveNumber.Focus();
                MessageBox.Show("\tSMS 수신 번호를 설정하시기 바랍니다.\t");
                return;
            }

            if (BtnDelayTime.Text == "" || PowerDelayTime.Text == "")
            {
                BtnDelayTime.Focus();
                MessageBox.Show("재발송 대기시간을 설정하시기 바랍니다.\n\n대기시간을 설정하지 않는 경우엔 0을 입력해주시기 바랍니다.");
                return;
            }

            sms.Account = Account.Text;
            sms.Password = Password.Password;
            sms.SendName = SendName.Text;
            sms.SendAddress = SendAddress.Text;
            sms.SendNumber = SendNumber.Text;
            sms.ReciveNumber = ReciveNumber.Text;
            sms.ReciveAsNumber = ReciveAsNumber.Text;
            sms.ReciveAsNumber2 = ReciveAsNumber2.Text;
            sms.BtnDelayTime = BtnDelayTime.Text;
            sms.PowerDelayTime = PowerDelayTime.Text;

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
