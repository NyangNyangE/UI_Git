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
using System.Windows.Shapes;

namespace PanicCall
{
	/// <summary>
	/// MenuFrame.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MenuFrame : Window
	{
		public MenuFrame()
		{
			this.InitializeComponent();
			
			// 개체 만들기에 필요한 코드를 이 지점 아래에 삽입하십시오.
		}

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
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