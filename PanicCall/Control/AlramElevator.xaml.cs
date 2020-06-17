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
	/// AlramStop.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class AlramElevator : UserControl
	{
        public AlramElevator()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Original.Opacity = 0;
            Visibility = Visibility.Hidden;
        }

        private void LayoutRoot_MouseEnter(object sender, MouseEventArgs e)
        {
            Original.Opacity = 1.0;
            Cursor = Cursors.Hand;
        }

        private void LayoutRoot_MouseLeave(object sender, MouseEventArgs e)
        {
            Original.Opacity = 0;
            Cursor = Cursors.Arrow;
        }

        public void Stop()
        {
            Visibility = Visibility.Hidden;
        }
	}
}