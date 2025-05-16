using System;
using System.IO;
using System.Windows;

namespace RealsonnetApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Handle unhandled exceptions
            DispatcherUnhandledException += (sender, ex) => 
            {
                File.WriteAllText("error_log.txt", $"Dispatcher Exception: {ex.Exception}");
                ex.Handled = true; // Prevent app from closing immediately
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, ex) => 
            {
                File.WriteAllText("error_log.txt", $"Unhandled Exception: {ex.ExceptionObject}");
            };

            base.OnStartup(e);
        }
    }
}