using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Printing;

namespace PanicCall.Configuration
{
    /// <summary>
    /// Print.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Print : Window
    {
        string FilePath = "";
        StreamReader sr;
        PrintDocument document = new PrintDocument();
        static int PageNum = 1;

        public Print()
        {
            InitializeComponent();

            calendea.SelectedDate = DateTime.Now.Date;
       //     MarkCalendea();

            document.PrintPage += new PrintPageEventHandler(document_PrintPage);
        }

        void MarkCalendea()
        {
            DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\Log");
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                string year = file.Name[0].ToString() + file.Name[1].ToString() + file.Name[2].ToString() + file.Name[3].ToString();
                string month = file.Name[5].ToString() + file.Name[6].ToString();
                string day = file.Name[8].ToString() + file.Name[9].ToString();

                DateTime date = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));

                calendea.SelectedDates.Add(date);
          
                //Microsoft.Windows.Controls.CalendarDateRange range = new Microsoft.Windows.Controls.CalendarDateRange(date);
                //calendea.BlackoutDates.Add(range);

                //aa.Replace(".txt", "");
            }
  
        }
           
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void calendea_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {   
            FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Log\\" + calendea.SelectedDate.Value.ToShortDateString() + ".txt";
            FilePath = FilePath.Remove(0, 6);
            //    Directory.GetCurrentDirectory() + "\\Log\\" + calendea.SelectedDate.Value.ToShortDateString() + ".txt";

            if (File.Exists(FilePath))
            {
                
                print.IsEnabled = true;
            }
            else
            {
                print.IsEnabled = false;
            }

           // MarkCalendea();
        }

        private void print_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.PrintDialog pd = new System.Windows.Forms.PrintDialog();
            PrinterSettings setting = new PrinterSettings();
            pd.PrinterSettings = setting;

            if (pd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                sr = new StreamReader(FilePath);
                PageNum = 1;
                document.PrinterSettings = setting;
                document.Print();

                sr.Close();
            }
            /*
            
            
            
            if (pd.ShowDialog() == true)
            {
                string FileName = Directory.GetCurrentDirectory() + "\\Log\\" + calendea.SelectedDate.Value.ToShortDateString() + ".txt";
             //   FileInfo file = new FileInfo(FileName);

                TextBlock tb = new TextBlock();
                tb.Height = 500.0;
                tb.Width = 500.0;
                tb.Foreground = Brushes.Black;
                tb.Text = "testawetjasdlfkjasldkjglasjkdgljiwaelhgilashdlgkjasljk";
                sr.Close();

                FixedDocument document = new FixedDocument();
                document.
                pd.PrintDocument();
                pd.PrintVisual(tb, "로그인쇄");
            }
             */
        }

        void document_PrintPage(object sender, PrintPageEventArgs e)
        {
            float yPos = 0f;
            int count = 0;
            float LeftMargin = e.MarginBounds.Left;
            float TopMargin = e.MarginBounds.Top;
            string line = null;
            System.Drawing.Font font = new Font("Arial", 10);

            float LinesPerPage = e.MarginBounds.Height / font.GetHeight(e.Graphics);

            string title = calendea.SelectedDate.Value.ToShortDateString();
            e.Graphics.DrawString(title, font, System.Drawing.Brushes.Black, LeftMargin, 50f, new StringFormat());
       

            while (count < LinesPerPage - 4) //3줄 정도 여백
            {
                line = sr.ReadLine();

                if (line == null)
                {
                    break;
                }

                yPos = TopMargin + count * font.GetHeight(e.Graphics);
                e.Graphics.DrawString(line, font, System.Drawing.Brushes.Black, LeftMargin, yPos, new StringFormat());
                count++;
            }

            string page =  PageNum.ToString() + "페이지";
            e.Graphics.DrawString(page, font, System.Drawing.Brushes.Black, LeftMargin, e.MarginBounds.Height + 90, new StringFormat());
            PageNum++;

            if (line != null)
            {
                e.HasMorePages = true;
            }
        }
    }
}
