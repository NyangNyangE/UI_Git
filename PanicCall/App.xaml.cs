using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace PanicCall
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            KeyboardManager.DisableSystemKeys();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            KeyboardManager.EnableSystemKeys();
            base.OnExit(e);
        }
    }
}
