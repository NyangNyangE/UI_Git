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

namespace PanicCall
{
    /// <summary>
    /// InputSc.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InputSc : Window
    {
        object obj;
        int count = 0;

        public InputSc()
        {
            InitializeComponent();
        }

        public InputSc(object arg)
        {
            InitializeComponent();

            if (arg.GetType().Equals(new PanicControl().GetType()))
            {
                obj = (PanicControl)arg;
                info.Text = "비상버튼 : " + (arg as PanicControl).Addr.ToString();
            }
            else if (arg.GetType().Equals(new SensorControl().GetType()))
            {
                obj = (SensorControl)arg;
                info.Text = "센서 : " + (arg as SensorControl).Addr.ToString();
            }

            LoadSc();
        }

        void LoadSc()
        {
            count = 0;

            if (obj.GetType().Equals(new PanicControl().GetType()))
            {
                foreach (IntToInt temp in (obj as PanicControl).Sc)
                {
                    Add(temp.Int1.ToString(), temp.Int2.ToString(), temp.Int3.ToString());
                }
                
            }
            else if (obj.GetType().Equals(new SensorControl().GetType()))
            {
                foreach (IntToInt temp in (obj as SensorControl).Sc)
                {
                    Add(temp.Int1.ToString(), temp.Int2.ToString(), temp.Int3.ToString());
                }
            }

            if (count < 4)
            {
                Add("", "", "3");
            }
        }

        void Add(string arg1, string arg2, string arg3)
        {
            count++;

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(0, 15, 0, 0);
            sp.Orientation = Orientation.Horizontal;
            sp.Height = 20.0;
            sp.Width = 400.0;

            TextBlock tb1 = new TextBlock();
            tb1.Text = count.ToString();
            tb1.Height = 20.0;
            tb1.Width = 30.0;
            tb1.FontSize = 13.0;
            tb1.Margin = new Thickness(15, 2, 0, 0);
            tb1.Foreground = Brushes.White;
            sp.Children.Add(tb1);

            TextBlock tb2 = new TextBlock();
            tb2.Text = "SC";
            tb2.Height = 20.0;
            tb2.Width = 30.0;
            tb2.FontSize = 13.0;
            tb2.Foreground = Brushes.White;
            tb2.Margin = new Thickness(10, 2, -10, 0);
            sp.Children.Add(tb2);

            NumberTextBox tbx1 = new NumberTextBox();
            tbx1.Height = 20.0;
            tbx1.Width = 30.0;
            tbx1.Text = arg1;
            sp.Children.Add(tbx1);

            TextBlock tb3 = new TextBlock();
            tb3.Text = "채널";
            tb3.Height = 20.0;
            tb3.Width = 40.0;
            tb3.FontSize = 13.0;
            tb3.Foreground = Brushes.White;
            tb3.Margin = new Thickness(20, 2, -10, 0);
            sp.Children.Add(tb3);

            NumberTextBox tbx2 = new NumberTextBox();
            tbx2.Height = 20.0;
            tbx2.Width = 30.0;
            tbx2.Text = arg2;
            sp.Children.Add(tbx2);

            TextBlock tb4 = new TextBlock();
            tb4.Text = "시간";
            tb4.Height = 20.0;
            tb4.Width = 40.0;
            tb4.FontSize = 13.0;
            tb4.Foreground = Brushes.White;
            tb4.Margin = new Thickness(20, 2, -10, 0);
            sp.Children.Add(tb4);

            NumberTextBox tbx3 = new NumberTextBox();
            tbx3.Height = 20.0;
            tbx3.Width = 30.0;
            tbx3.Text = arg3;
            sp.Children.Add(tbx3);

            TextBlock tb5 = new TextBlock();
            tb5.Text = "초";
            tb5.Height = 20.0;
            tb5.Width = 40.0;
            tb5.FontSize = 13.0;
            tb5.Foreground = Brushes.White;
            tb5.Margin = new Thickness(0, 2, 0, 0);
            sp.Children.Add(tb5);

            if (arg1 == "")
            {
                Button bt = new Button();
                bt.Height = 20.0;
                bt.Width = 30.0;
                bt.Content = "추가";
                bt.Margin = new Thickness(15, 0, 0, 0);
                sp.Children.Add(bt);

                bt.Click += new RoutedEventHandler(bt_Click);
            }
            else
            {
          //      Button change = new Button();
                Button delete = new Button();

         //       change.Content = "수정";
                delete.Content = "삭제";

         //       change.Height = 20.0;
                delete.Height = 20.0;
         //       change.Width = 30.0;
                delete.Width = 30.0;

        //        change.Margin = new Thickness(40, 0, 0, 0);
                delete.Margin = new Thickness(15, 0, 0, 0);

        //        sp.Children.Add(change);
                sp.Children.Add(delete);

           //     change.Click += new RoutedEventHandler(change_Click);
                delete.Click += new RoutedEventHandler(delete_Click);
            }
            

            panel.Children.Add(sp);

            

        }

        void bt_Click(object sender, RoutedEventArgs e)
        {
            StackPanel parent = (sender as Button).Parent as StackPanel;

            if ((parent.Children[2] as NumberTextBox).Text == ""
                || (parent.Children[4] as NumberTextBox).Text == ""
                || (parent.Children[6] as NumberTextBox).Text == "")
                return;

            IntToInt tmp = new IntToInt();

            try
            {
                tmp.Int1 = Convert.ToInt32((parent.Children[2] as NumberTextBox).Text);
                tmp.Int2 = Convert.ToInt32((parent.Children[4] as NumberTextBox).Text);
                tmp.Int3 = Convert.ToInt32((parent.Children[6] as NumberTextBox).Text);
            }
            catch
            {
                return;
            }

            if (tmp.Int1 < 0 || tmp.Int1 > 99)
            {
                MessageBox.Show("SC의 어드레스는 0~99 이내여야합니다.");
                return;
            }
            if (tmp.Int2 < 0x01 || tmp.Int2 > 0x10)
            {
                MessageBox.Show("SC의 채널은 1~16 이내여야합니다.");
                return;
            }
            if (tmp.Int3 < 0x00 || tmp.Int3 > 0xFF)
            {
                MessageBox.Show("SC의 대기시간은 0~255초 이내여야합니다.");
                return;
            }

            (parent.Children[2] as NumberTextBox).IsReadOnly = true;
            (parent.Children[4] as NumberTextBox).IsReadOnly = true;
            (parent.Children[6] as NumberTextBox).IsReadOnly = true;

    //        Button change = new Button();
            Button delete = new Button();

    //        change.Content = "수정";
            delete.Content = "삭제";

    //        change.Height = 20.0;
            delete.Height = 20.0;
    //        change.Width = 30.0;
            delete.Width = 30.0;

   //         change.Margin = new Thickness(40, 0, 0, 0);
            delete.Margin = new Thickness(15, 0, 0, 0);

            parent.Children.Remove(sender as Button);
   //        parent.Children.Add(change);
            parent.Children.Add(delete);

   //         change.Click += new RoutedEventHandler(change_Click);
            delete.Click += new RoutedEventHandler(delete_Click);

            

            if (obj.GetType().Equals(new PanicControl().GetType()))
            {
                (obj as PanicControl).Sc.Add(tmp); 
            }
            else if (obj.GetType().Equals(new SensorControl().GetType()))
            {
                (obj as SensorControl).Sc.Add(tmp); 
            };

            if (count < 4)
            {
                Add("", "", "3");
            }
        }
        /*
        void change_Click(object sender, RoutedEventArgs e)
        {
            StackPanel parent = (sender as Button).Parent as StackPanel;
            IntToInt tmp = new IntToInt();

            tmp.Int1 = Convert.ToInt32((parent.Children[2] as TextBox).Text);
            tmp.Int2 = Convert.ToInt32((parent.Children[4] as TextBox).Text);

            if (obj.GetType().Equals(new PanicControl().GetType()))
            {
                (obj as PanicControl).Matrix[count - 1].Int1 = tmp.Int1;
                (obj as PanicControl).Matrix[count - 1].Int2 = tmp.Int2;
            }
            else if (obj.GetType().Equals(new SensorControl().GetType()))
            {
                (obj as SensorControl).Matrix[count - 1].Int1 = tmp.Int1;
                (obj as SensorControl).Matrix[count - 1].Int2 = tmp.Int2;
            };
        }
        */

        void delete_Click(object sender, RoutedEventArgs e)
        {
            StackPanel parent = (sender as Button).Parent as StackPanel;
            IntToInt tmp = new IntToInt();

            tmp.Int1 = Convert.ToInt32((parent.Children[2] as NumberTextBox).Text);
            tmp.Int2 = Convert.ToInt32((parent.Children[4] as NumberTextBox).Text);
            tmp.Int3 = Convert.ToInt32((parent.Children[6] as NumberTextBox).Text);

            if (obj.GetType().Equals(new PanicControl().GetType()))
            {
                foreach (IntToInt temp in (obj as PanicControl).Sc)
                {
                    if (temp.Int1 == tmp.Int1 && temp.Int2 == tmp.Int2 && temp.Int3 == tmp.Int3)
                    {
                        (obj as PanicControl).Sc.Remove(temp);
                        break;
                    }
                }
            }
            else if (obj.GetType().Equals(new SensorControl().GetType()))
            {
                foreach (IntToInt temp in (obj as SensorControl).Sc)
                {
                    if (temp.Int1 == tmp.Int1 && temp.Int2 == tmp.Int2 && temp.Int3 == tmp.Int3)
                    {
                        (obj as SensorControl).Sc.Remove(temp);
                        break;
                    }
                }

            };

            (parent.Parent as StackPanel).Children.Clear();
            LoadSc();
        }

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void image_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void image_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }
    }
}
