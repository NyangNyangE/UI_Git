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
    /// Admin.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Admin : Window
    {

        public static PasswordBox password;

        public Admin()
        {
            InitializeComponent();

            if (password == null)
                password = new PasswordBox();

            Password_1.Password = password.Password;
            Password_2.Password = password.Password;

            check.IsChecked = Properties.Settings.Default.IsAdminLock;
            if (check.IsChecked == false)
            {
                PasswordBoxEnable(false);
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (Password_1.Password != Password_2.Password)
            {
                MessageBox.Show("잘못된 비밀번호 입니다!!", "비밀번호 입력 오류");
                return;
            }

            if (Properties.Settings.Default.IsAdminLock)
            {
                if (Password_1.Password.Length < 6 || Password_2.Password.Length > 20)
                {
                    MessageBox.Show("비밀번호는 6자 이상 20자 이내로 설정되어야합니다!!", "비밀번호 입력 오류");
                    return;
                }
            }

            Properties.Settings.Default.AdminPassword = Password_1.Password.ToString(); 
            
            if (check.IsChecked == true)
            {
                Properties.Settings.Default.IsAdminLock = true;
            }
            else 
            {
                Properties.Settings.Default.IsAdminLock = false;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void check_Checked(object sender, RoutedEventArgs e)
        {
            PasswordBoxEnable(true);
        }

        private void check_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBoxEnable(false);
        }

        private void PasswordBoxEnable(bool _state)
        {
            Properties.Settings.Default.IsAdminLock = _state;
            Password_1.IsEnabled = _state;
            Password_2.IsEnabled = _state;
        }
    }
}
