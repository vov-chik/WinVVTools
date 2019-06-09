// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System.Windows;
using WinVVTools.Helpers;

namespace WinVVTools
{
    /// <summary>
    /// Logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var bootstapper = new Bootstrapper();
            bootstapper.Run();
        }
    }
}
