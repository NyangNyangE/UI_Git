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
    /// InputPreset.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InputPreset : Window
    {
        object obj;
        int count = 0;

        public enum PresetSelection { PelcoD, PelcoP, DongYang, SungJin, SCC641 };

        public InputPreset()
        {
            InitializeComponent();
        }

        public InputPreset(object arg)
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

            LoadPreset();
        }

        void LoadPreset()
        {
            count = 0;

            if (obj.GetType().Equals(new PanicControl().GetType()))
            {
                foreach (IntToInt temp in (obj as PanicControl).Preset)
                {
                    Add(temp.Int1.ToString(), temp.Int2.ToString(), temp.Int3);
                }
                
            }
            else if (obj.GetType().Equals(new SensorControl().GetType()))
            {
                foreach (IntToInt temp in (obj as SensorControl).Preset)
                {
                    Add(temp.Int1.ToString(), temp.Int2.ToString(), temp.Int3);
                }
            }

            if (count < 4)
            {
                Add("", "", 0);
            }
        }

        void Add(string arg1, string arg2, int arg3)
        {
            count++;

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(0, 15, 0, 0);
            sp.Orientation = Orientation.Horizontal;
            sp.Height = 20.0;
            sp.Width = 350.0;

            TextBlock tb1 = new TextBlock();
            tb1.Text = count.ToString();
            tb1.Height = 20.0;
            tb1.Width = 15.0;
            tb1.FontSize = 13.0;
            tb1.Margin = new Thickness(5, 2, 0, 0);
            tb1.Foreground = Brushes.White;
            sp.Children.Add(tb1);

            TextBlock tb2 = new TextBlock();
            tb2.Text = "카메라";
            tb2.Height = 20.0;
            tb2.Width = 45.0;
            tb2.FontSize = 13.0;
            tb2.Foreground = Brushes.White;
            tb2.Margin = new Thickness(0, 2, 0, 0);
            sp.Children.Add(tb2);

            NumberTextBox tbx1 = new NumberTextBox();
            tbx1.Height = 20.0;
            tbx1.Width = 40.0;
            tbx1.Text = arg1;
            sp.Children.Add(tbx1);

            TextBlock tb3 = new TextBlock();
            tb3.Text = "프리셋";
            tb3.Height = 20.0;
            tb3.Width = 45.0;
            tb3.FontSize = 13.0;
            tb3.Foreground = Brushes.White;
            tb3.Margin = new Thickness(10, 2, 0, 0);
            sp.Children.Add(tb3);

            NumberTextBox tbx2 = new NumberTextBox();
            tbx2.Height = 20.0;
            tbx2.Width = 40.0;
            tbx2.Text = arg2;
            sp.Children.Add(tbx2);

            ComboBox PresetList = new ComboBox();
            PresetList.Height = 20.0;
            PresetList.Width = 90.0;
            PresetList.Margin = new Thickness(15, 0, 0, 0);

            PresetList.Items.Add("PelcoD");
            PresetList.Items.Add("PelcoP");
            PresetList.Items.Add("동양 유니택");
            PresetList.Items.Add("성진");
            PresetList.Items.Add("SCC-641");

            PresetList.SelectedIndex = arg3;
            sp.Children.Add(PresetList);

            if (arg1 == "")
            {
                Button bt = new Button();
                bt.Height = 20.0;
                bt.Width = 30.0;
                bt.Content = "추가";
                bt.Margin = new Thickness(10, 0, 0, 0);
                sp.Children.Add(bt);

                bt.Click += new RoutedEventHandler(bt_Click);
            }
            else
            {

                tbx1.IsReadOnly = true;
                tbx2.IsReadOnly = true;
                PresetList.IsEnabled = false;

          //      Button change = new Button();
                Button delete = new Button();

           //     change.Content = "수정";
                delete.Content = "삭제";

          //      change.Height = 20.0;
                delete.Height = 20.0;
           //     change.Width = 30.0;
                delete.Width = 30.0;

          //      change.Margin = new Thickness(60, 0, 0, 0);
                delete.Margin = new Thickness(10, 0, 0, 0);

          //      sp.Children.Add(change);
                sp.Children.Add(delete);

          //      change.Click += new RoutedEventHandler(change_Click);
                delete.Click += new RoutedEventHandler(delete_Click);
            }
            

            panel.Children.Add(sp);
        }

        void bt_Click(object sender, RoutedEventArgs e)
        {
            StackPanel parent = (sender as Button).Parent as StackPanel;

            if ((parent.Children[2] as NumberTextBox).Text == ""
                || (parent.Children[4] as NumberTextBox).Text == "")
                return;

            IntToInt tmp = new IntToInt();

            try
            {
                tmp.Int1 = Convert.ToInt32((parent.Children[2] as NumberTextBox).Text);
                tmp.Int2 = Convert.ToInt32((parent.Children[4] as NumberTextBox).Text);
                tmp.Int3 = (parent.Children[5] as ComboBox).SelectedIndex;
            }
            catch
            {
                return;
            }

            // 프리셋은 카메라 사양에 따라 값이 달라지므로 카메라값에만 제한을 둔다..
            if (tmp.Int1 < 1 || tmp.Int1 > 9999)
            {
                MessageBox.Show("카메라의 입력값은 1 ~ 9999번 이내여야 합니다");
                return;
            }

            (parent.Children[2] as NumberTextBox).IsReadOnly = true;
            (parent.Children[4] as NumberTextBox).IsReadOnly = true;
            (parent.Children[5] as ComboBox).IsEnabled = false;

      //      Button change = new Button();
            Button delete = new Button();

      //      change.Content = "수정";
            delete.Content = "삭제";

      //      change.Height = 20.0;
            delete.Height = 20.0;
      //      change.Width = 30.0;
            delete.Width = 30.0;

      //      change.Margin = new Thickness(40, 0, 0, 0);
            delete.Margin = new Thickness(10, 0, 0, 0);

            parent.Children.Remove(sender as Button);
       //     parent.Children.Add(change);
            parent.Children.Add(delete);

       //     change.Click += new RoutedEventHandler(change_Click);
            delete.Click += new RoutedEventHandler(delete_Click);

            if (obj.GetType().Equals(new PanicControl().GetType()))
            {
                (obj as PanicControl).Preset.Add(tmp);
            }
            else if (obj.GetType().Equals(new SensorControl().GetType()))
            {
                (obj as SensorControl).Preset.Add(tmp);
            };

            if (count < 4)
            {
                Add("", "", 0);
            }
        }

        void delete_Click(object sender, RoutedEventArgs e)
        {
            StackPanel parent = (sender as Button).Parent as StackPanel;
            IntToInt tmp = new IntToInt();

            tmp.Int1 = Convert.ToInt32((parent.Children[2] as NumberTextBox).Text);
            tmp.Int2 = Convert.ToInt32((parent.Children[4] as NumberTextBox).Text);

            if (obj.GetType().Equals(new PanicControl().GetType()))
            {
                foreach (IntToInt temp in (obj as PanicControl).Preset)
                {
                    if (temp.Int1 == tmp.Int1 && temp.Int2 == tmp.Int2)
                    {
                        (obj as PanicControl).Preset.Remove(temp);
                        break;
                    }
                }



            }
            else if (obj.GetType().Equals(new SensorControl().GetType()))
            {
                foreach (IntToInt temp in (obj as SensorControl).Preset)
                {
                    if (temp.Int1 == tmp.Int1 && temp.Int2 == tmp.Int2)
                    {
                        (obj as SensorControl).Preset.Remove(temp);
                        break;
                    }
                }

            };

            (parent.Parent as StackPanel).Children.Clear();
            LoadPreset();
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
