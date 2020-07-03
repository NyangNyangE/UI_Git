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
using System.Windows.Media.Media3D;


namespace PanicCall
{
	/// <summary>
	/// AddIcon.xaml에 대한 상호 작용 논리
	/// </summary>
    ///

	public partial class AddIcon : UserControl
    {
        double beginX = 0;
        double beginY = 0;
        int initAddr;

        bool isHidden = false;
        bool isMouseDown = false;

        Canvas Root;
        Maps maps;
        Image MapImage;
        Canvas IconCanvas;
        PanicControl panic;
        PisControl pis;
        ZoomBoxLibrary.ZoomBoxPanel zoomBox;

        public AddIcon()
		{
			this.InitializeComponent();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Root = (Canvas)(this.Parent);
            maps = MainWindow.maps;
            MapImage = Application.Current.Properties["MapImage"] as Image;
            IconCanvas = Application.Current.Properties["IconCanvas"] as Canvas;
            zoomBox = Application.Current.Properties["zoomBox"] as ZoomBoxLibrary.ZoomBoxPanel;

            NewPanic.TextView.Visibility = Visibility.Collapsed;

            SerchInsertPanic();

           // MainWindow.serialport.SendButtonSacn();
        }

        public void SerchInsertPanic()
        {  
            foreach (PanicControl _panic in maps.NotInsertPanic)
            {
                int addr = _panic.Addr;
                bool _ishave = false;

                foreach (ListBoxItem item in listBox.Items)
                {
                    StackPanel _st = item.Content as StackPanel;

                    if (_st.Children[0].GetType() == _panic.GetType())
                    {
                        if (((PanicControl)(_st.Children[0])).Addr == addr)
                        {
                            _ishave = true;
                        }
                    }
                }

                if (!_ishave)
                {

                    PanicControl _tp = new PanicControl();
                    _tp.TextView.Visibility = Visibility.Collapsed;

                    _tp.Addr = _panic.Addr;
                   
                  //  _tp.Matrix = _panic.Matrix;
                  //  _tp.Preset = _panic.Preset;
                    
                    _tp.TextView.Text = _panic.TextView.Text;


                    if (_tp.IsSleep)
                    {
                        _tp.button.Template = (ControlTemplate)_tp.UserControl.Resources["ButtonControlTemplate2"];
                    }

                    //_tp = _panic;
                    _tp.Width = 20.0;
                    _tp.Height = 20.0;
                    _tp.Margin = new Thickness(5, 0, 0, 0);
                    _tp.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_tp_PreviewMouseLeftButtonDown);

                    StackPanel panel = new StackPanel();
                    panel.Height = 20.0;
                    panel.Orientation = Orientation.Horizontal;

                    TextBlock tb = new TextBlock();
                    tb.Text = "비상버튼  " + addr.ToString();
                    tb.Margin = new Thickness(5, 0, 0, 0);
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.Foreground = Brushes.White;

                    panel.Children.Add(_tp);
                    panel.Children.Add(tb);

                    ListBoxItem listitem = new ListBoxItem();
                    listitem.Content = panel;
                    listBox.Items.Add(listitem);

                    listitem.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PanicItem_PreviewMouseLeftButtonDown);
                    listitem.MouseLeave += new MouseEventHandler(PanicItem_MouseLeave);
                    listitem.MouseEnter += new MouseEventHandler(PanicItem_MouseEnter);
                }
            }
 
        }

       
        void _ts_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        void _tp_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
   
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isHidden = true;
            UserControl.ReleaseMouseCapture();
            this.Visibility = Visibility.Hidden;

            (Application.Current.Properties["btIconAdd"] as btIconAddControl).StopNewStory();


            ((Canvas)this.Parent).Children.Remove(this);
            
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (isHidden)
                return;

            this.beginX = e.GetPosition(null).X;
            this.beginY = e.GetPosition(null).Y;

            isMouseDown = true;

            UserControl.CaptureMouse();
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isMouseDown == true)
            {
                // 전체 영역에서 마우스의 현재 위치
                double currX = e.GetPosition(null).X;
                double currY = e.GetPosition(null).Y;

                // 사각형의 현재위치
                double valueX = Double.Parse(UserControl.GetValue(Canvas.LeftProperty).ToString());
                double valueY = double.Parse(UserControl.GetValue(Canvas.TopProperty).ToString());

                valueX += (currX - beginX);
                valueY += (currY - beginY);

                UserControl.SetValue(Canvas.LeftProperty, valueX);
                UserControl.SetValue(Canvas.TopProperty, valueY);

                beginX = currX;
                beginY = currY;
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.isMouseDown = false;

            UserControl.ReleaseMouseCapture();
        }

        public void SetHedden(bool flag)
        {
            isHidden = flag;
        }
        
       


        private void PanicItem_MouseEnter(object sender, MouseEventArgs e)
        {
            PanicControl tp = (PanicControl)((StackPanel)((ListBoxItem)sender).Content).Children[0];
            Storyboard st = new Storyboard();
            st = (Storyboard)tp.Resources["PanicSwingStory"];
            st.Begin();
        }



        private void PanicItem_MouseLeave(object sender, MouseEventArgs e)
        {
            PanicControl tp = (PanicControl)((StackPanel)((ListBoxItem)sender).Content).Children[0];
            Storyboard st = new Storyboard();
            st = (Storyboard)tp.Resources["PanicSwingStory"];
            st.Remove();
        }




        void Panic_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            panic.MouseMove -= Panic_MouseMove;
            panic.MouseLeftButtonUp -= Panic_MouseLeftButtonUp;

            Root.Children.Remove(panic);

            if (maps.Count == 0)
                return;

            if (e.GetPosition(UserControl).X > 0 && e.GetPosition(UserControl).X < UserControl.ActualWidth
                && e.GetPosition(UserControl).Y > 0 && e.GetPosition(UserControl).Y < UserControl.ActualHeight)
                return;

            if (this.isMousePosition(e))
            {
                double Scale = 100.0 / zoomBox.Zoom;
                ScaleTransform scale = new ScaleTransform(Scale, Scale);

                panic.RenderTransform = scale;
                
                panic.SetValue(Canvas.LeftProperty, (e.GetPosition(IconCanvas).X - ((panic.Width * Scale) / 2)));
                panic.SetValue(Canvas.TopProperty, (e.GetPosition(IconCanvas).Y - ((panic.Height * Scale) / 2)));


                IconCanvas.Children.Add(panic);
                //maps[maps.SelectIndex].PanicList.Add(panic.Addr, panic);

                if (maps.IconLock == false)
                    panic.SetMove(true);

                //panic.PanicName = panic.Addr.ToString();
                panic.ReleaseMouseCapture();
                panic.SetContextMenu();
                panic.SetTooltip();
                panic.SetViewText(Properties.Settings.Default.nViewButtonText);

                if (panic.Addr > -1)
                {
                    foreach (PanicControl _panic in maps.NotInsertPanic)
                    {
                        if (_panic.Addr == panic.Addr)
                        {
                            maps.NotInsertPanic.Remove(_panic);
                            break;
                        }
                            
                    }

                    foreach (ListBoxItem item in listBox.Items)
                    {
                        StackPanel _st = item.Content as StackPanel;

                        if (_st.Children[0].GetType() == panic.GetType())
                        {
                            if (((PanicControl)(_st.Children[0])).Addr == panic.Addr)
                            {
                                listBox.Items.Remove(item);                                
                                break;
                            }
                        }
                    }
                    
                }
  
            }
        }

        void Pis_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            pis.MouseMove -= Pis_MouseMove;
            pis.MouseLeftButtonUp -= Pis_MouseLeftButtonUp;

            Root.Children.Remove(pis);

            if (maps.Count == 0)
                return;

            if (e.GetPosition(UserControl).X > 0 && e.GetPosition(UserControl).X < UserControl.ActualWidth
                && e.GetPosition(UserControl).Y > 0 && e.GetPosition(UserControl).Y < UserControl.ActualHeight)
                return;

            if (this.isMousePosition(e))
            {
                double Scale = 100.0 / zoomBox.Zoom;
                ScaleTransform scale = new ScaleTransform(Scale, Scale);

                pis.RenderTransform = scale;

                pis.SetValue(Canvas.LeftProperty, (e.GetPosition(IconCanvas).X - ((pis.Width * Scale) / 2)));
                pis.SetValue(Canvas.TopProperty, (e.GetPosition(IconCanvas).Y - ((pis.Height * Scale) / 2)));


                IconCanvas.Children.Add(pis);
                maps[maps.SelectIndex].PisList.Add(pis);
                MainWindow.pisIndex++;

                if (maps.IconLock == false)
                    pis.SetMove(true);

                //panic.PanicName = panic.Addr.ToString();
                pis.ReleaseMouseCapture();

            }
        }

       

        bool isMousePosition(MouseButtonEventArgs e)
        {
            if (e.GetPosition(MapImage).X > 0 && e.GetPosition(MapImage).Y > 0
                && e.GetPosition(MapImage).X < MapImage.ActualWidth
                && e.GetPosition(MapImage).Y < MapImage.ActualHeight)
            {
                return true;
            }

            return false;
        }

        void Panic_MouseMove(object sender, MouseEventArgs e)
        {
            panic.SetValue(Canvas.LeftProperty, (e.GetPosition(null).X - (panic.Width / 2)));
            panic.SetValue(Canvas.TopProperty, (e.GetPosition(null).Y - (panic.Height / 2)));
        }
        void Pis_MouseMove(object sender, MouseEventArgs e)
        {
            pis.SetValue(Canvas.LeftProperty, (e.GetPosition(null).X - (pis.Width / 2)));
            pis.SetValue(Canvas.TopProperty, (e.GetPosition(null).Y - (pis.Height / 2)));
        }

      

      

        private void PanicItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PanicControl pPanic = (PanicControl)((StackPanel)((ListBoxItem)sender).Content).Children[0];

            panic = new PanicControl();

            panic.Height = maps.IconSize.Height;
            panic.Width = maps.IconSize.Width;
            
            
            panic.Addr = pPanic.Addr;
            
            //panic.ParentMap =  maps.

            if (panic.IsSleep)
            {
                panic.button.Template = (ControlTemplate)panic.UserControl.Resources["ButtonControlTemplate2"];
            }

            Root.Children.Add(panic);

            panic.SetValue(Canvas.TopProperty, (Mouse.GetPosition(null).Y - panic.Height / 2));
            panic.SetValue(Canvas.LeftProperty, (Mouse.GetPosition(null).X - panic.Width / 2));
            panic.SetValue(Panel.ZIndexProperty, 10);

            panic.CaptureMouse();

            panic.MouseMove += new MouseEventHandler(Panic_MouseMove);
            panic.MouseLeftButtonUp += new MouseButtonEventHandler(Panic_MouseLeftButtonUp);
        }

        private void PisItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PisControl tPis = (PisControl)((StackPanel)((ListBoxItem)sender).Content).Children[0];

            pis = new PisControl();

            Root.Children.Add(pis);

            pis.SetValue(Canvas.TopProperty, (Mouse.GetPosition(null).Y - pis.Height / 2));
            pis.SetValue(Canvas.LeftProperty, (Mouse.GetPosition(null).X - pis.Width / 2));
            pis.SetValue(Panel.ZIndexProperty, 2);
            
            pis.CaptureMouse();

            pis.MouseMove += new MouseEventHandler(Pis_MouseMove);
            pis.MouseLeftButtonUp += new MouseButtonEventHandler(Pis_MouseLeftButtonUp);
        }

     

       

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as TextBlock).Background = Brushes.Orange;
            (sender as TextBlock).Cursor = Cursors.Hand;

        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as TextBlock).Background = Brushes.White;
            (sender as TextBlock).Cursor = Cursors.Arrow;
        }

	}
}