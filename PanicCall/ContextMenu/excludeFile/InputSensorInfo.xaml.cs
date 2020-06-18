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
    /// InputSensorInfo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InputSensorInfo : Window
    {
        SensorControl sensor = new SensorControl();

        public InputSensorInfo()
        {
            InitializeComponent();
        }

        public InputSensorInfo(SensorControl _sensor)
        {
            InitializeComponent();

            sensor = _sensor;

            address.Text        = _sensor.Addr.ToString();
            name.Text           = sensor.SensorName;
            StartHour.Text      = sensor.StartTime.Hour.ToString();
            StartMinute.Text    = sensor.StartTime.Minute.ToString();
            EndHour.Text        = sensor.EndTime.Hour.ToString();
            EndMinute.Text      = sensor.EndTime.Minute.ToString();
        }

        private void comfirm_Click(object sender, RoutedEventArgs e)
        {
            if (address.Text == "")
            {
                MessageBox.Show("번호를 입력해 주세요", "센서 : " + sensor.Addr.ToString());
                return;
            }

            Maps maps = MainWindow.maps;

            foreach (Map map in maps)
            {
                foreach (PanicControl pc in map.PanicList)
                {
                    if (sensor.Addr != pc.Addr && pc.Addr.ToString() == address.Text)
                    {
                        MessageBox.Show("센서 번호가 중복 되었습니다", "센서 : " + sensor.Addr.ToString());
                        return;
                    }
                }

                foreach (SensorControl sc in map.SensorList)
                {
                    if (sensor.Addr != sc.Addr && sc.Addr.ToString() == address.Text)
                    {
                        MessageBox.Show("센서 번호가 중복 되었습니다", "센서 : " + sensor.Addr.ToString());
                        return;
                    }
                }
            }

            if (name.Text == "")
            {
                MessageBox.Show("이릅을 입력해 주세요", "센서 : " + sensor.Addr.ToString());
                return;
            }

            try
            {
                if (StartHour.Text == "" || StartMinute.Text == ""
                   || EndHour.Text == "" || EndMinute.Text == "")
                {
                    MessageBox.Show("시간을 입력해 주세요", "센서 : " + sensor.Addr.ToString());
                    return;
                }

                if (Convert.ToInt32(StartHour.Text) > 23 || Convert.ToInt32(EndHour.Text) > 23
                    || Convert.ToInt32(StartMinute.Text) > 58 || Convert.ToInt32(EndMinute.Text) > 58)
                {
                    MessageBox.Show("정확한 시간을 입력해 주세요", "센서 : " + sensor.Addr.ToString());
                    return;
                }

                try
                {
                    sensor.Addr = Convert.ToInt32(address.Text);
                }
                catch
                {
                    MessageBox.Show("잘못된 번호 입니다", "센서 : " + sensor.Addr.ToString());
                    return;
                }

                DateTime start = new DateTime(1, 1, 1, Convert.ToInt32(StartHour.Text), Convert.ToInt32(StartMinute.Text), 0);
                DateTime end = new DateTime(1, 1, 1, Convert.ToInt32(EndHour.Text), Convert.ToInt32(EndMinute.Text), 0);

                if (start != sensor.StartTime || end != sensor.EndTime)
                {
                    if (MessageBoxResult.Yes == MessageBox.Show("해당 맵에 있는 모든 비상버튼에 감시시간을 동일하게 설정 하시겠습니까?", "감시 시간 설정", MessageBoxButton.YesNo))
                    {
                        bool isfind = false;
                        foreach (Map map in maps)
                        {
                            foreach (SensorControl sc in map.SensorList)
                            {
                                if (sensor.Addr == sc.Addr)
                                {
                                    isfind = true;
                                    break;
                                }
                            }

                            if (isfind)
                            {
                                foreach (SensorControl sc in map.SensorList)
                                {
                                    sc.StartTime = start;
                                    sc.EndTime = end;
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        sensor.StartTime = start;
                        sensor.EndTime = end;
                    }
                }

                sensor.SensorName = name.Text;
                sensor.SetViewText(Properties.Settings.Default.nViewButtonText);
                sensor.SetTooltip();
            }
            catch (Exception ex)
            {
                string except = ex.Message;
                MessageBox.Show("잘못된 시간 입니다", "비상버튼 : " + sensor.Addr.ToString());
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public void SetRight()
        {
            left.Visibility = Visibility.Hidden;
            right.Visibility = Visibility.Visible;
        }
    }
}
