using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace PanicCall
{
    /// <summary>
    /// InputPisIP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InputPisIP : Window
    {
        PisControl pis;
        bool _isChecked1;
        bool _isChecked2;
        List<Group> groupList;
        public InputPisIP(PisControl pis)
        {
            InitializeComponent();
            this.pis = pis;
        }


        private void WinLoaded(object sender, RoutedEventArgs e)
        {
            groupList = (List<Group>)Application.Current.Properties["GroupList"];
            if(string.IsNullOrWhiteSpace(pis.Ip) == false)
                PisIPText.Text = pis.Ip;

            PisAddr.Text = pis.addr.ToString();

            if (string.IsNullOrWhiteSpace(pis.serverIp) == false)
                MainIPTextBox.Text = pis.serverIp;

            if (string.IsNullOrWhiteSpace(pis.pisName) == false)
                PisName.Text = pis.pisName;


            //일반형 or 입구형에 따라 보여주는 폼을 달리함
            if (pis.isGatePis)
                form2();
            else
                form1();

            PisRangeCom1.Items.Clear();
            PisRangeCom1.Items.Add("전체 맵");
            for (int i = 0; i < MainWindow.maps.Count; i++)
                PisRangeCom1.Items.Add(MainWindow.maps[i].MapName);
            for (int i = 0; i < groupList.Count; i++)
                PisRangeCom1.Items.Add(groupList[i].groupName);
            PisRangeCom1.Items.Add("없음");

            PisRangeCom1.SelectedIndex = pis.group1.selectedIndex;



            PisRangeCom2.Items.Clear();
            PisRangeCom2.Items.Add("전체 맵");
            for (int i = 0; i < MainWindow.maps.Count; i++)
                PisRangeCom2.Items.Add(MainWindow.maps[i].MapName);
            for (int i = 0; i < groupList.Count; i++)
                PisRangeCom2.Items.Add(groupList[i].groupName);           
            PisRangeCom2.Items.Add("없음");

            PisRangeCom2.SelectedIndex = pis.group2.selectedIndex;



            PisRangeCom3.Items.Clear();
            PisRangeCom3.Items.Add("전체 맵");
            for (int i = 0; i < MainWindow.maps.Count; i++)
                PisRangeCom3.Items.Add(MainWindow.maps[i].MapName);
            for (int i = 0; i < groupList.Count; i++)
                PisRangeCom3.Items.Add(groupList[i].groupName);
            PisRangeCom3.Items.Add("없음");

            PisRangeCom3.SelectedIndex = pis.group3.selectedIndex;




        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            confirm();
        }

        private void confirm()
        {
            try
            {
                pis.group1 = new Group();
                pis.group2 = new Group();
                pis.group3 = new Group();

                //입구 PIS 선택 되어있을때
                if (RadioButton2.IsChecked.Value)
                {
                    if (string.IsNullOrWhiteSpace(PisAddr.Text) || string.IsNullOrWhiteSpace(PisIPText.Text))
                    {
                        MessageBox.Show("입력이 없습니다.");
                        close();
                        return;
                    }
                    else
                    {
                        Regex reg = new Regex(@"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");

                        if (reg.IsMatch(PisIPText.Text))
                        {
                            if (string.IsNullOrWhiteSpace(MainIPTextBox.Text) == false)
                            {
                                if (reg.IsMatch(MainIPTextBox.Text))
                                {
                                    pis.Ip = PisIPText.Text;
                                    pis.addr = Int32.Parse(PisAddr.Text);
                                    pis.serverIp = MainIPTextBox.Text;
                                    pis.pisName = PisName.Text;
                                    pis.isGatePis = true;
                                    pis.group1.selectedIndex = PisRangeCom1.SelectedIndex;
                                    pis.group2.selectedIndex = PisRangeCom2.SelectedIndex;
                                    pis.group3.selectedIndex = PisRangeCom3.SelectedIndex;
                                    pis.group1.selectedGroupName = PisRangeCom1.SelectedItem.ToString();
                                    pis.group2.selectedGroupName = PisRangeCom2.SelectedItem.ToString();
                                    pis.group3.selectedGroupName = PisRangeCom3.SelectedItem.ToString();


                                    close();
                                }
                            }
                            else
                            {
                                pis.Ip = PisIPText.Text;
                                pis.addr = Int32.Parse(PisAddr.Text);
                                pis.serverIp = null;
                                pis.pisName = PisName.Text;
                                pis.isGatePis = true;
                                pis.group1.selectedIndex = PisRangeCom1.SelectedIndex;
                                pis.group2.selectedIndex = PisRangeCom2.SelectedIndex;
                                pis.group3.selectedIndex = PisRangeCom3.SelectedIndex;
                                pis.group1.selectedGroupName = PisRangeCom1.SelectedItem.ToString();
                                pis.group2.selectedGroupName = PisRangeCom2.SelectedItem.ToString();
                                pis.group3.selectedGroupName = PisRangeCom3.SelectedItem.ToString();


                                close();
                            }
                        }
                        else
                        {
                            MessageBox.Show("잘못된 IP 입력입니다.");
                            close();
                        }

                    }
                }
                //일반 PIS 선택 되었을때
                else
                {
                    pis.isGatePis = false;

                    if (string.IsNullOrWhiteSpace(PisAddr.Text))
                    {
                        MessageBox.Show("입력이 없습니다.");
                        close();
                        return;
                    }
                    else
                    {
                        //  현재 RS485 방식이므로 ip 필요없음
                        if (string.IsNullOrWhiteSpace((string)PisRangeCom1.SelectedItem))
                        {
                            MessageBox.Show("PIS 에 표시되는 주차 공간 범위 설정이 없습니다.");
                            close();
                            return;
                        }

                        //맵이 아닌 그룹을 택한경우 그리고 없음이 아닌경우
                        if (PisRangeCom1.SelectedIndex > MainWindow.maps.Count && PisRangeCom1.SelectedIndex != PisRangeCom1.Items.Count - 1)  
                        {
                            //그룹 인덱스 = 현재 선택된 인덱스 - 맵개수 - 전체맵1개
                            int index = PisRangeCom1.SelectedIndex - MainWindow.maps.Count - 1;
                            pis.group1 = groupList[index];
                        }

                        //맵이 아닌 그룹을 택한경우 그리고 없음이 아닌경우
                        if (PisRangeCom2.SelectedIndex > MainWindow.maps.Count && PisRangeCom2.SelectedIndex != PisRangeCom2.Items.Count - 1)  
                        {
                            //그룹 인덱스 = 현재 선택된 인덱스 - 맵개수 - 전체맵1개
                            int index = PisRangeCom2.SelectedIndex - MainWindow.maps.Count - 1;
                            pis.group2 = groupList[index];
                        }

                        pis.pisName = PisName.Text;
                        pis.group1.selectedIndex = PisRangeCom1.SelectedIndex;
                        pis.group2.selectedIndex = PisRangeCom2.SelectedIndex;
                        pis.group1.selectedGroupName = PisRangeCom1.SelectedItem.ToString();
                        pis.group2.selectedGroupName = PisRangeCom2.SelectedItem.ToString();      //일반pis 일경우 group1 밖에 사용안하므로 group1 == group2
                        pis.addr = Int32.Parse(PisAddr.Text);
                        pis.serverIp = null;
                        close();
                    }
                }
            }
            catch(Exception ee)
            {
            }
        }
        private void close()
        {
            pis.changeTitle();
            this.Close();
        }

        private void Checked1(object sender, RoutedEventArgs e)
        {
            form1();
        }

        //일반
        private void form1()
        {
            
            this.Height = 155;
            MainIPTextBlock.Visibility = Visibility.Collapsed;
            MainIPTextBox.Visibility = Visibility.Collapsed;
            PisIPTextBlock.Visibility = Visibility.Collapsed;
            PisIPText.Visibility = Visibility.Collapsed;
            PisRange1.Visibility = Visibility.Visible;
            PisRangeCom1.Visibility = Visibility.Visible;
            PisRange2.Visibility = Visibility.Visible;
            PisRangeCom2.Visibility = Visibility.Visible;
            PisRange3.Visibility = Visibility.Collapsed;
            PisRangeCom3.Visibility = Visibility.Collapsed;

            IsChecked1 = true;
            RadioButton1.IsChecked = true;
        }

        private void Checked2(object sender, RoutedEventArgs e)
        {
            form2();
        }

        //입구
        private void form2()
        {
            this.Height = 225;
            MainIPTextBlock.Visibility = Visibility.Visible;
            MainIPTextBox.Visibility = Visibility.Visible;
            PisIPTextBlock.Visibility = Visibility.Visible;
            PisIPText.Visibility = Visibility.Visible;
            PisRange2.Visibility = Visibility.Visible;
            PisRangeCom2.Visibility = Visibility.Visible;
            PisRange3.Visibility = Visibility.Visible;
            PisRangeCom3.Visibility = Visibility.Visible;

            IsChecked2 = true;
            RadioButton2.IsChecked = true;
        }
        private bool IsChecked1
        {
            get { return _isChecked1; }
            set
            {
                _isChecked1 = value;
                _isChecked2 = !_isChecked1;
            }
        }
        private bool IsChecked2
        {
            get { return _isChecked2; }
            set
            {
                _isChecked2 = value;
                _isChecked1 = !_isChecked2;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                confirm();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        

        

    }
}
