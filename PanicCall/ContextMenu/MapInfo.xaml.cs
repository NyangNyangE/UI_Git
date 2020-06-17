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
    /// MapInfo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MapInfo : Window
    {
        MainWindow parent;

        public MapInfo()
        {
            InitializeComponent();
        }

        public MapInfo(MainWindow _parent)
        {
            InitializeComponent();

            parent = _parent;

            Maps maps = MainWindow.maps;
            name.Text = maps[maps.SelectIndex].MapName;
        }

        private void comfirm_Click(object sender, RoutedEventArgs e)
        {
            Maps maps = MainWindow.maps;
            Map map = maps[maps.SelectIndex];

            if (name.Text == "")
                MessageBox.Show("변경할 이름을 입력하세요", map.MapName);

            foreach (Map m in (Maps)maps)
            {
                if (m != map && m.MapName == name.Text)
                {
                    MessageBox.Show("이미 사용중인 이름 입니다");
                    return;
                }
            }
            map.MapName = name.Text;
            (Application.Current.Properties["cbMapList"] as ComboBox).SelectedIndex = -1;

            this.DialogResult = true;
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
