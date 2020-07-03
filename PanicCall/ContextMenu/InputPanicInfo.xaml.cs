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
    /// InputPanicInfo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InputPanicInfo : Window
    {
        PanicControl panic = new PanicControl();

        public InputPanicInfo()
        {
            InitializeComponent();
        }

        public InputPanicInfo(PanicControl _panic)
        {
            InitializeComponent();

            panic = _panic;

            address.Text = panic.Addr.ToString();

            if(panic.cameras[0] != null)
                cameraIP_L.Text = panic.cameras[0].IP;
            if(panic.cameras[1] != null)
                cameraIP_R.Text = panic.cameras[1].IP;

        }
        private void comfirm_Click(object sender, RoutedEventArgs e)
        {
            confirm();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public void SetRight()
        {
            right.Visibility = Visibility.Visible;
        }

        private void onEnterKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                confirm();
        }
        private void confirm()
        {
            if (address.Text == "")
            {
                MessageBox.Show("번호를 입력해 주세요", "비상버튼 : " + panic.Addr.ToString());
                return;
            }

            Maps maps = MainWindow.maps;

            try
            {
                int AddressNumber = Convert.ToInt32(address.Text);
                if (AddressNumber < 1 && AddressNumber > 999)
                {
                    MessageBox.Show("버튼의 번호는 1 ~ 999까지 설정 가능합니다.", "비상버튼 : " + panic.Addr.ToString());
                    return;
                }
            }
            catch
            {
                MessageBox.Show("잘못된 번호 입니다", "비상버튼 : " + address.Text);
                return;
            }

            if (string.IsNullOrWhiteSpace(cameraIP_L.Text) == false && string.IsNullOrWhiteSpace(cameraIP_R.Text) == false && cameraIP_L.Text == cameraIP_R.Text)
            {
                MessageBox.Show("카메라 IP 정보가 중복 되었습니다.\n IP 정보 : " + cameraIP_R.Text);
               // return
            }

            try
            {
                foreach (Map map in maps)
                {
                    foreach (PanicControl pc in map.PanicList.Values)
                    {
                        if (panic.Addr == pc.Addr)
                            continue;

                        if (pc.Addr.ToString() == address.Text)
                        {
                            MessageBox.Show("비상버튼 번호가 중복 되었습니다", "비상버튼 : " + panic.Addr.ToString());
                            return;
                        }


                        for (int i = 0; i < pc.camerasCount(); i++)
                        {
                            if (string.IsNullOrWhiteSpace(cameraIP_L.Text) == false && pc.cameras[i].IP == cameraIP_L.Text)
                            {
                                MessageBox.Show("카메라 IP 정보가 중복 되었습니다.\n IP 정보 : " + cameraIP_L.Text);
                               // return;
                            }
                            if (string.IsNullOrWhiteSpace(cameraIP_R.Text) == false && pc.cameras[i].IP == cameraIP_R.Text)
                            {
                                MessageBox.Show("카메라 IP 정보가 중복 되었습니다.\n IP 정보 : " + cameraIP_R.Text);
                               // return;
                            }

                        }

                    }
                }
            }
            catch (Exception ee)
            {
                MainWindow.WriteException(ee.Message);
            }

            try
            {
                int prevAddr = panic.Addr;
                try
                {
                    panic.Addr = Convert.ToInt32(address.Text);
                }
                catch
                {
                    MessageBox.Show("잘못된 번호 입니다", "비상버튼 : " + panic.Addr.ToString());
                    return;
                }



                Camera cam = new Camera();
                cam.IP = cameraIP_L.Text;
                panic.cameras[0] = cam;

                cam = new Camera();
                cam.IP = cameraIP_R.Text;
                panic.cameras[1] = cam;



                panic.SetViewText(Properties.Settings.Default.nViewButtonText);
                panic.SetTooltip();


                Dictionary<int, PanicControl> panics = maps[maps.SelectIndex].PanicList;

                if (panics.ContainsValue(panic))
                    panics.Remove(prevAddr);
                if(panic.Addr != -1)
                    panics.Add(panic.Addr, panic);

            }
            catch (Exception ex)
            {
                MessageBox.Show("잘못된 IP 입력입니다");
                return;
            }

            this.DialogResult = true;
            this.Close();

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }

    

}
