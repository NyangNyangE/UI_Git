﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace PanicCall
{
    /// <summary>
    /// ctlPanic.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ctlPanic : UserControl
    {
        public int h_size;
        public int w_size;
 
           
        public ctlPanic()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public int h_Size
        {
            get { return h_size; }
            set { h_size = value; }
        }

        public int w_Size
        {
            get { return w_size; }
            set { w_size = value; }
        }

    }
}
