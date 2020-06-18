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

namespace PanicCall
{
	/// <summary>
	/// btSMSControl.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class btSmartControl : UserControl
	{
        public btSmartControl()
		{
			this.InitializeComponent();
		}
		
		private void LayoutRoot_MouseEnter(object sender, MouseEventArgs e)
        {
         //   BitmapImage tmp = new BitmapImage(new Uri("Pack://application:,,,/Images/intro.bmp"));
            btBackground.Source = new BitmapImage(new Uri("Pack://application:,,,/Images/b3.png"));
            this.Cursor = Cursors.Hand;

        }

        private void LayoutRoot_MouseLeave(object sender, MouseEventArgs e)
        {
            btBackground.Source = new BitmapImage(new Uri("Pack://application:,,,/Images/b1.png"));
            this.Cursor = Cursors.Arrow;
        }

        private void LayoutRoot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            btBackground.Source = new BitmapImage(new Uri("Pack://application:,,,/Images/b2.png"));
        }
		
	}
}