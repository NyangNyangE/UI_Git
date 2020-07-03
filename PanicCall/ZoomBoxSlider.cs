// Copyright (c) 2009 Tom Wright. All rights reserved.
// http://rightondevelopment.blogspot.com/
// This code is distributed under the Microsoft Public License (Ms-PL).
/*
Microsoft Public License (Ms-PL)
[OSI Approved License]

This license governs use of the accompanying software. If you use the software, you
accept this license. If you do not accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
same meaning here as under U.S. copyright law.
A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
*/
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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace PanicCall
{

    public class ZoomBoxSliderDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double? d = value as double?;
            if (d != null)
                return String.Format("{0:0.}%", d);
            return "0%";
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("unexpected Convertback");
        }
    }


    public class ZoomBoxSlider : TWWPFUtilityLib.TWFadeAwayControl
    {
        private static DependencyProperty ZoomTickProperty;
        private static DependencyProperty MinZoomTickProperty;
        private static DependencyProperty MaxZoomTickProperty;
        private static DependencyProperty ZoomProperty;
        private static DependencyProperty ZoomBoxProperty;
        private static DependencyProperty TargetElementProperty;

        public double ZoomTick
        {
            set { SetValue(ZoomTickProperty, value); }
            get { return (double)GetValue(ZoomTickProperty); }
        }
        public double MinZoomTick
        {
            set { SetValue(MinZoomTickProperty, value); }
            get { return (double)GetValue(MinZoomTickProperty); }
        }
        public double MaxZoomTick
        {
            set { SetValue(MaxZoomTickProperty, value); }
            get { return (double)GetValue(MaxZoomTickProperty); }
        }

        public double Zoom
        {
            set { SetValue(ZoomProperty, value); }
            get { return (double)GetValue(ZoomProperty); }
        }

        public ZoomBoxLibrary.ZoomBoxPanel ZoomBox
        {
            set { SetValue(ZoomBoxProperty, value); }
            get { return (ZoomBoxLibrary.ZoomBoxPanel)GetValue(ZoomBoxProperty); }
        }

        public UIElement TargetElement
        {
            set { SetValue(TargetElementProperty, value); }
            get { return (UIElement)GetValue(TargetElementProperty); }
        }

        static ZoomBoxSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomBoxSlider), new FrameworkPropertyMetadata(typeof(ZoomBoxSlider)));

            ZoomTickProperty = DependencyProperty.Register(
                "ZoomTick", typeof(double), typeof(ZoomBoxSlider),
                new FrameworkPropertyMetadata(50.0,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Journal, null, null),
                null);
            MinZoomTickProperty = DependencyProperty.Register(
                "MinZoomTick", typeof(double), typeof(ZoomBoxSlider),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Journal, null, null),
                null);
            MaxZoomTickProperty = DependencyProperty.Register(
                "MaxZoomTick", typeof(double), typeof(ZoomBoxSlider),
                new FrameworkPropertyMetadata(100.0,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Journal, null, null),
                null);

            ZoomProperty = DependencyProperty.Register(
                "Zoom", typeof(double), typeof(ZoomBoxSlider),
                new FrameworkPropertyMetadata(100.0,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Journal, PropertyChanged_Zoom, null),
                null);

            ZoomBoxProperty = DependencyProperty.Register(
                "ZoomBox", typeof(ZoomBoxLibrary.ZoomBoxPanel), typeof(ZoomBoxSlider),
                new FrameworkPropertyMetadata(null, PropertyChanged_ZoomBox),
                new ValidateValueCallback(ZoomBoxSlider.ValidateIsZoomBox));

            TargetElementProperty = DependencyProperty.Register(
                "TargetElement", typeof(UIElement), typeof(ZoomBoxSlider),
                new FrameworkPropertyMetadata(null),
                new ValidateValueCallback(ZoomBoxSlider.ValidateIsUIElement));
        }

        private static bool ValidateIsZoomBox(object value)
        {
            return (value == null) || (value is ZoomBoxLibrary.ZoomBoxPanel);
        }
        private static bool ValidateIsUIElement(object value)
        {
            return (value == null) || (value is UIElement);
        }
        private static void PropertyChanged_ZoomBox(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomBoxSlider z = d as ZoomBoxSlider;
            if (z != null)
                z.ZoomBoxChangeEvent();
        }
        void ZoomBoxChangeEvent()
        {
            if (ZoomBox != null)
            {
                Binding binding;

                binding = new Binding();
                binding.Source = ZoomBox;
                binding.Path = new PropertyPath("Zoom");
                binding.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(this, ZoomProperty, binding);

                binding = new Binding();
                binding.Source = ZoomBox;
                binding.Path = new PropertyPath("ZoomTick");
                binding.Mode = BindingMode.TwoWay;
                BindingOperations.SetBinding(this, ZoomTickProperty, binding);

                binding = new Binding();
                binding.Source = ZoomBox;
                binding.Path = new PropertyPath("MinZoomTick");
                binding.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(this, MinZoomTickProperty, binding);

                binding = new Binding();
                binding.Source = ZoomBox;
                binding.Path = new PropertyPath("MaxZoomTick");
                binding.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(this, MaxZoomTickProperty, binding);
            }
        }


        public ZoomBoxSlider() :
            base()
        {
            SetUpCommands();

        }

        private void SetUpCommands()
        {
            // Set up command bindings.
            CommandBinding binding = new CommandBinding(NavigationCommands.Zoom,
                    ZoomCommand_Executed, ZoomCommand_CanExecute);
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(NavigationCommands.IncreaseZoom,
                    IncreaseZoomCommand_Executed, IncreaseZoomCommand_CanExecute);
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(NavigationCommands.DecreaseZoom,
                    DecreaseZoomCommand_Executed, DecreaseZoomCommand_CanExecute);
            this.CommandBindings.Add(binding);       
        }


        #region  Commands
        private void ZoomCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void IncreaseZoomCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (ZoomTick < MaxZoomTick) ? true : false;
        }
        private void DecreaseZoomCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (ZoomTick > MinZoomTick) ? true : false;
        }



        private void ZoomCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ZoomBox != null)
            {
                Maps maps = MainWindow.maps;

                if (maps.Count > 0)
                {
                    BitmapImage tmp = new BitmapImage(new Uri(maps[maps.SelectIndex].ImagePath));

                    maps[maps.SelectIndex].PosY = (ZoomBox.RenderSize.Height / 2) - (tmp.Height / 2);
                    maps[maps.SelectIndex].PosX = (ZoomBox.RenderSize.Width / 2) - (tmp.Width / 2);

                    ZoomBox.PanX = maps[maps.SelectIndex].PosX;
                    ZoomBox.PanY = maps[maps.SelectIndex].PosY;
                    ZoomBox.Zoom = 100;

                    maps[maps.SelectIndex].Zoom = ZoomBox.Zoom;
                    ZoomBox.ApplyZoom(true);
                }
            }
            
            FocusOnTarget();
        }
        private void IncreaseZoomCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ZoomBox != null)
            {
                NavigationCommands.IncreaseZoom.Execute(null, ZoomBox);
                Maps maps = MainWindow.maps;
                maps[maps.SelectIndex].Zoom = ZoomBox.Zoom;
            }
            FocusOnTarget();

        }
        private void DecreaseZoomCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ZoomBox != null)
            {
                NavigationCommands.DecreaseZoom.Execute(null, ZoomBox);
                Maps maps = MainWindow.maps;
                maps[maps.SelectIndex].Zoom = ZoomBox.Zoom;
            }
            FocusOnTarget();
        }

        private static void PropertyChanged_Zoom(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomBoxSlider z = d as ZoomBoxSlider;
            if (z != null)
            {
                z.FadeAway = false;
                z.SetIconScale();
            }
        }

        private void FocusOnTarget()
        {
            if (TargetElement != null)
                TargetElement.Focus();
            else if (ZoomBox != null)
                ZoomBox.Focus();
        }

        public void SetIconScale()
        {
            Maps maps = MainWindow.maps;

            if (maps.Count < 1)
                return;

            double Scale = 100.0 / ZoomBox.Zoom;
            double _top, _left, cx, cy, newTop, newLeft;
            ScaleTransform scale = new ScaleTransform(Scale, Scale);

            foreach (PanicControl panic in maps[maps.SelectIndex].PanicList.Values)
            {
                _top = (double)panic.GetValue(Canvas.TopProperty);
                _left = (double)panic.GetValue(Canvas.LeftProperty);

                cx = _left + ((panic.ActualWidth * panic.RenderTransform.Value.M11) / 2);
                cy = _top + ((panic.ActualHeight * panic.RenderTransform.Value.M22) / 2);

                panic.RenderTransform = scale;

                newTop = cy - ((panic.ActualHeight * Scale) / 2);
                newLeft = cx - ((panic.ActualWidth * Scale) / 2);

                panic.SetValue(Canvas.TopProperty, newTop);
                panic.SetValue(Canvas.LeftProperty, newLeft);
            }
            foreach (PisControl pis in maps[maps.SelectIndex].PisList) 
            {
                pis.RenderTransform = scale;
            }

            maps[maps.SelectIndex].Zoom = ZoomBox.Zoom;
        }
        #endregion

    }

}
