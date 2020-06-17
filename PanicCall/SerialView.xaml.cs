using System;
using System.IO;
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
	/// SerialView.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class SerialView : Window
	{
        public static SerialView instance;
        public static ListBox SerialData = new ListBox();
        static string FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\DataLog";

        static bool IsStart = true;

        private SerialView()
		{
			this.InitializeComponent();

            Topmost = true;
            SerialData.Items.Clear();
            SerialData.Margin = new Thickness(10, 40, 10, 10);
            SerialData.SelectionMode = SelectionMode.Single;
            LayoutRoot.Children.Add(SerialData);

            FilePath = FilePath.Remove(0, 6);
            DirectoryInfo dir = new DirectoryInfo(FilePath);

            if (!dir.Exists)
                dir.Create();
		}

        public static SerialView getInstance()
        {
            if (instance == null)
                instance = new SerialView();

            return instance;
        }
        private void loadedSerialView(object sender, RoutedEventArgs e)
        {

            //DataBase db = (PanicCall.DataBase)Application.Current.Properties["DB"];
            //List<Dvr> dvrList = (List<Dvr>)Application.Current.Properties["DvrList"];
            //List<Wisnet> wisnetList = (List<PanicCall.Wisnet>)Application.Current.Properties["Wisnet"];

            //try
            //{
            //    foreach (Dvr dvr in dvrList)
            //    {
            //        label1.Content += dvr.Ip + "\n";
            //    }

            //    foreach (Wisnet wis in wisnetList)
            //    {
            //        label2.Content += wis.wisnetIP + "\n";
            //    }
            //    label3.Content += db.database + "\n";
            //}
            //catch (NullReferenceException ex)
            //{
 
            //}
        }

        public static void Recive(object arg)
        {
            try
            {
                if (instance == null)
                    return;

                if (!IsStart)
                    return;

                string strtmp = "";
                strtmp += "Recive";
                strtmp += "\t";
                DateTime now = DateTime.Now;
                strtmp += now.ToShortTimeString();
                strtmp += ":";
                strtmp += now.Second;
                strtmp += ":";
                strtmp += now.Millisecond;
                strtmp += "\t\t";
                strtmp += arg;

                TextBlock tb = new TextBlock();
                tb.Text = strtmp;
                tb.Foreground = Brushes.Red;
                SerialData.Items.Insert(0, tb);

                string filename = FilePath + "\\" + now.ToShortDateString() + ".txt";
                FileStream file = new FileStream(filename, FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(file);
                sw.WriteLine(strtmp);
                sw.Close();

                if (SerialData.Items.Count > 1000)
                    SerialData.Items.Clear();
            }
            catch (Exception eee)
            { 
            }
        }

        public static void Send(object arg)
        {
            
            if (instance == null)
                return;

            
            if (!IsStart)
                return;

            string packet = "";

            //MessageBox.Show("ewq");
            //foreach (byte _byte in arg as byte[])
            //{
            //    packet += ((byte)_byte).ToString("X2") + " ";
            //}
            packet = (string)arg;
            
            string strtmp = "";
            DateTime now = DateTime.Now;
            strtmp += "Send";
            strtmp += "\t";
            strtmp += now.ToShortTimeString();
            strtmp += ":";
            strtmp += now.Second;
            strtmp += ":";
            strtmp += now.Millisecond;
            strtmp += "\t\t";
            strtmp += packet;

            TextBlock tb = new TextBlock();
            tb.Text = strtmp;
            tb.Foreground = Brushes.Blue;
            SerialData.Items.Insert(0, tb);

            //string filename = FilePath + "\\" + now.ToShortDateString() + ".txt";
            //FileStream file = new FileStream(filename, FileMode.Append, FileAccess.Write);
            //StreamWriter sw = new StreamWriter(file);
            //sw.WriteLine(strtmp);
            //sw.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            instance = null;
            SerialData = new ListBox();
            IsStart = true;
            FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\DataLog";
            base.OnClosed(e);
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            IsStart = true;
            start.IsEnabled = false;
            stop.IsEnabled = true;

        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            IsStart = false;
            stop.IsEnabled = false;
            start.IsEnabled = true;
        }

        public static void CloseInstance()
        {
            if (instance != null)
                instance.Close();
        }

        
	}
}