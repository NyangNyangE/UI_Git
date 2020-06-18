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
    /// Preset.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Preset : Window
    {
     //   public static bool IsPreset = false;
     //   public static int Selection = -1;

        List<string> PresetList = new List<string>();
        public enum PresetSelection { PelcoD, PelcoP, DongYang, SungJin, SCC641 };

        public Preset()
        {
            InitializeComponent();

            PresetList.Add("PelcoD");
            PresetList.Add("PelcoP");
            PresetList.Add("동양 유니택");
            PresetList.Add("성진");
            PresetList.Add("SCC-641");

            CheckPreset.IsChecked = Properties.Settings.Default.IsPresetMove;
            SelectPreset.ItemsSource = PresetList;

            if (Properties.Settings.Default.nPresetSelect != -1)
                SelectPreset.SelectedIndex = Properties.Settings.Default.nPresetSelect;
              
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (CheckPreset.IsChecked == true)
            {
                Properties.Settings.Default.nPresetSelect = SelectPreset.SelectedIndex;
                Properties.Settings.Default.IsPresetMove = true;
            }
            else
            {
                Properties.Settings.Default.IsPresetMove = false;
            }

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
