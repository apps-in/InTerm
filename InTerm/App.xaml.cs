using InTerm.Views;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace InTerm
{
    public partial class App : Application
    {
        private readonly ILog log = LogManager.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {          
            base.OnStartup(e);
            System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            log.Info("Starting...");
            log.Info($"Version: {assemblyName.Version}");
            SetupExceptionHandling();
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            string message = $"Unhandled exception ({source})";
            try
            {
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = string.Format($"Unhandled exception in {assemblyName.Name} v.{assemblyName.Version}");
            }
            catch (Exception e)
            {
                log.Error($"Unhandled exception logging failed. {e}");
            }
            finally
            {
                log.Error($"{message}. {exception}");
            }
        }

        /// <summary>
        /// On exit event handler
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            log.Info("Closing...");
        }
    }
}
