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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PanicCall
{
    /// <summary>
    /// AddGroup.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AddGroup : UserControl
    {
        double beginX = 0;
        double beginY = 0;
        bool isHidden = false;
        bool isMouseDown = false;

        private List<Group> groupList;
        private int count;
        private struct labelInfo
        {
            public int index;
            public bool isFocused;
            public labelInfo(int index, bool isFocused)
            {
                this.index = index; this.isFocused = isFocused;
            }
        }

        public AddGroup()
        {
            InitializeComponent();
        }

        //그룹 UI 에 그룹목록에 그룹들 버튼 생성
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            groupList = (List<Group>)Application.Current.Properties["GroupList"];
            count = 0;
            foreach (Group group in groupList)
            {
                addLabel();
                groupList.Distinct();
                count++;
            }

        }

        //그룹 UI 에 그룹 버튼 추가
        private void ClickedAddButton(object sender, RoutedEventArgs e)
        {
            Group group = new Group();
            group.groupName = "Group" + groupList.Count;
            groupList.Add(group);
            addLabel();
            count++;
        }

        private void addLabel()
        {
            Label label = new Label();
            label.Content = "Group" + count.ToString();
            labelInfo info = new labelInfo(count, false);
            label.Tag = info;
            label.Background = new SolidColorBrush(Color.FromArgb(255, 37, 37, 38));
            label.Foreground = Brushes.White;
            label.FontSize = 12;
            label.Margin = new Thickness(5, 5, 5, 5);
            label.MouseLeftButtonDown += new MouseButtonEventHandler(label_MouseDown);
            Panel.Children.Add(label);
        }

        //그룹 UI 에 그룹 버튼 삭제
        private void ClickedDelButton(object sender, RoutedEventArgs e)
        {
            try
            {
                groupList.RemoveAt(groupList.Count - 1);
                Panel.Children.RemoveAt(Panel.Children.Count - 1); //마지막
                count--;
            }
            catch (Exception ex)
            { 
            }
        }

        //그룹 버튼 클릭시 클릭된 버튼 표시 및 해당 버튼에 속해있는 Panic 버튼 표시
        private void label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label)sender;
            labelInfo info = (labelInfo)label.Tag;
            if (info.isFocused)
            {
                MainWindow.isSelectChecked = false;
                label.Foreground = Brushes.White;
                info.isFocused = false;
                hideSelectedButton();

            } 
            else 
            {
                if(MainWindow.selectedGroup != null)
                    hideSelectedButton();

                offFocus();
                MainWindow.isSelectChecked = true;
                label.Foreground = Brushes.Red;
                info.isFocused = true;
                MainWindow.selectedGroup = groupList[info.index];

                showSelectedButton();
            }
            label.Tag = info;
        }

        //선택된 그룹만 선택될수 있도록 나머지 그룹 포커스 삭제
        private void offFocus()
        {
            foreach (Label label in Panel.Children)
            {
                label.Foreground = Brushes.White;
                labelInfo info = (labelInfo)label.Tag;
                info.isFocused = false;
                label.Tag = info;
            }
        }

        //그룹에 포함된 Panic 버튼 강조표시 삭제
        private void hideSelectedButton()
        {
            try
            {
                foreach (PanicControl panic in MainWindow.selectedGroup.panics)
                {
                    panic.unSelectedPanic();
                }
            }
            catch(Exception e){}
        }

        //그룹에 포함된 Panic 버튼 강조표시 
        private void showSelectedButton()
        {
            try
            {
                foreach (PanicControl panic in MainWindow.selectedGroup.panics)
                {
                    panic.selectedPanic();
                }
            }
            catch (Exception e) { }
        }


        //종료
        private void ClickedCloseButton(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;   

            hideSelectedButton();
            Application.Current.Properties["GroupList"] = groupList;

            MainWindow.isOpenAddGroup = false;
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

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.isMouseDown = false;

            UserControl.ReleaseMouseCapture();
        }
  


    }
}
