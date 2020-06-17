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
    /// Matrix.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Matrix : Window
    {
  //      public static bool IsMatrix = false;
  //      public static int Selection = -1;

        public enum MatrixSelection  { DongYang , SungJin  };

        List<string> MatrixList = new List<string>();
        public Matrix()
        {
            InitializeComponent();

            MatrixList.Add("동양 유니택");
            MatrixList.Add("성진");

            CheckMatrix.IsChecked = Properties.Settings.Default.IsMatrixView;
            SelectMatrix.ItemsSource = MatrixList;

            if (Properties.Settings.Default.nMatrixSelect != -1)
                SelectMatrix.SelectedIndex = Properties.Settings.Default.nMatrixSelect;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (CheckMatrix.IsChecked == true)
            {
                Properties.Settings.Default.nMatrixSelect = SelectMatrix.SelectedIndex;
                Properties.Settings.Default.IsMatrixView = true;
            }
            else
            {
                Properties.Settings.Default.IsMatrixView = false;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SelectMatrix_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
    }
}
