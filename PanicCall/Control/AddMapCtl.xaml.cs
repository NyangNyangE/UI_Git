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
using System.IO;

namespace PanicCall
{
	/// <summary>
	/// AddMapCtl.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class AddMapCtl : UserControl
	{
        public bool isview;
        String FilePath;

		public AddMapCtl()
		{
			this.InitializeComponent();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            isview = false;
            FilePath = "";
        }

        private void btOpenDlg_Click(object sender, RoutedEventArgs e)
        {
            isview = true;
            
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
    //        dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Image files |*.JPG;*.GIF;*.PNG;*.TIF;*.BMP"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                FilePath = dlg.FileName;
                FileName.Text = dlg.SafeFileName;

                imgFile.Source = new BitmapImage(new Uri(dlg.FileName));
            }

            isview = false;

        }

        private void btMapAdd_Click(object sender, RoutedEventArgs e)
        {
            Maps maps = MainWindow.maps;

            if (FileName.Text != "" && MapName.Text != "")
            {
                foreach (Map m in (Maps)maps)
                {
                    if (m.MapName == MapName.Text)
                    {
                        MessageBox.Show("이름이 중복 되었습니다!!");
                        return;
                    }
                }

                //C# base
                // System.Environment.CurrentDirectory
                //CF
                //System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                String strMapPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                //  strMapPath += FileName.Text;

                strMapPath = strMapPath.Substring(6) + @"\Maps\";
                DirectoryInfo dir = new DirectoryInfo(strMapPath);

                if (!dir.Exists)
                    dir.Create();

                string tmp = FileName.Text;
                FileInfo[] files = dir.GetFiles();

                for (int i = 0, j = 0; i < files.Length; i++)
                {
                    if (files[i].Name == FileName.Text)
                    {
                        FileName.Text = "[" + j.ToString() + "]" + tmp;
                        j++;
                        i = 0;
                    }
                }

                strMapPath += FileName.Text;

                try
                {
                    FileInfo s_file = new FileInfo(FilePath);
                    FileInfo c_file = s_file.CopyTo(strMapPath, true);

                    //   Map.ListMap.Add(FilePath.Text);
                    Map map = new Map(MapName.Text, strMapPath, FileName.Text);
                    maps.Add(map);

                    FileName.Text = "";
                    MapName.Text = "";
                    imgFile.Source = null;
                }
                catch
                {
                    MessageBox.Show("맵추가 실패!! ", "오류");
                }
            }

        }
	}
}