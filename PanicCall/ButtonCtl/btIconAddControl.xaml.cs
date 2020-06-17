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
using System.Windows.Media.Animation;

namespace PanicCall
{
	/// <summary>
	/// btIconAddControl.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class btIconAddControl : UserControl
	{
		public btIconAddControl()
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

        public void PlayNewStory()
        {
            btBackground_2.Opacity = 100.0;
            Storyboard st = Resources["NewItemStoryboard"] as Storyboard;
            st.Begin();
        }

        public void StopNewStory()
        {
            btBackground_2.Opacity = 0.0;
            Storyboard st = Resources["NewItemStoryboard"] as Storyboard;
            st.Remove();
        }

	}
}