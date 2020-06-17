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
    /// InputElevator.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InputElevator : Window
    {
        Elevator EV = new Elevator();
        List<ElevatorCamera> CameraList = new List<ElevatorCamera>();
        int OldHo;

        public InputElevator()
        {
            InitializeComponent();
        }

        public InputElevator(Elevator _Elevator)
        {
            InitializeComponent();

            EV = _Elevator;

            if (EV == null)
            {
                MessageBox.Show("등록된 엘리베이터가 없습니다.");
                this.Close();
            }

            SetElevatorInfo();
            SetCameraList();

            int SelectIndex = 0;
            foreach (ElevatorCamera Cam in CameraList)
            {
                if (CheckEqualCamera(Cam, EV.GetCamera()))
                {
                    SelectCamera.SelectedIndex = SelectIndex;
                    break;
                }
                SelectIndex++;
            }
        }

        public bool CheckEqualCamera(ElevatorCamera _Src, ElevatorCamera _Des)
        {
            if (_Src.Channel == _Des.Channel)
            {
                if (_Src.DVR.Ip == _Des.DVR.Ip && _Src.DVR.Port == _Des.DVR.Port)
                {
                    if (_Src.DVR.Id == _Des.DVR.Id && _Src.DVR.Password == _Des.DVR.Password)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void SetElevatorInfo()
        {
            ElevatorName.Text = EV.GetElevatorName();
            if (ElevatorName.Text == "")
            {
                ElevatorName.Text = "승강기";
            }
            ElevatorNumberHo.Text = EV.GetElevatorNumberHo().ToString();
            OldHo = EV.GetElevatorNumberHo();
        }

        private void SetCameraList()
        {
            foreach (Dvr Temp in Application.Current.Properties["DvrList"] as List<Dvr>)
            {
                for (int i = 0; i < 16; i++)
                {
                    ElevatorCamera Cam = new ElevatorCamera();
                    Cam.Channel = i + 1;
                    Cam.DVR = Temp;

                    CameraList.Add(Cam);
                }
            }
        }

        private void SelectCamera_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (ElevatorCamera Cam in CameraList)
            {
                string Item = "[" + Cam.DVR.Ip + "]" + " CH" + (Cam.Channel).ToString();
                SelectCamera.Items.Add(Item);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (ElevatorName.Text == "" || ElevatorNumberHo.Text == "")
            {
                MessageBox.Show("엘리베이터의 이름 및 번호를 지정하시기 바랍니다.");
                return;
            }

            try
            {
                if (Convert.ToInt32(ElevatorNumberHo.Text) < 1 || Convert.ToInt32(ElevatorNumberHo.Text) > 999)
                {
                    MessageBox.Show("엘리베이터의 호 번호는 1 ~ 999 까지 설정 가능합니다.");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("엘리베이터의 번호는 숫자만 입력 가능합니다.");
                return;
            }

            List<Elevator> ElevatorList = (List<Elevator>)Application.Current.Properties["ElevatorList"];
            foreach (Elevator EV in ElevatorList)
            {
                if (Convert.ToInt32(ElevatorNumberHo.Text) != OldHo)
                {
                    if (EV.GetElevatorNumberHo() == Convert.ToInt32(ElevatorNumberHo.Text))
                    {
                        MessageBox.Show("이미 등록된 번호입니다.");
                        return;
                    }
                }
            }

            try
            {
                EV.SetElevatorName(ElevatorName.Text);
                EV.SetElevatorNumberHo(Convert.ToInt32(ElevatorNumberHo.Text));

                try
                {
                    EV.SetCamera(CameraList[SelectCamera.SelectedIndex]);
                }
                catch
                {
                    EV.SetCamera(null);
                }

                this.DialogResult = true;
                this.Close();
            }
            catch
            {
                MessageBox.Show("데이터 등록에 실패하였습니다.");
                this.DialogResult = false;
                this.Close();
            }
        }
    }
}
