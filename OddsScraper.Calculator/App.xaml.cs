﻿using System.Windows;

namespace OddsScraper.Calculator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            FSharp.CommonScraping.CanopyExtensions.initialize();
        }
    }
}
