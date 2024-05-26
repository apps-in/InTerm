using CommunityToolkit.Mvvm.ComponentModel;
using InTerm.Helpers;
using InTerm.Models;
using InTerm.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InTerm.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private AppSettings appSettings = SettingsHelper.Instance.AppSettings;

        [ObservableProperty]
        private string appVersion;

        [ObservableProperty]
        private TerminalView terminal;
        public MainViewModel()
        {
            System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
#if DEBUG
            AppVersion = $"{assemblyName.Version.Major}.{assemblyName.Version.Minor}.{assemblyName.Version.Build}.{assemblyName.Version.Revision}";
#else
            AppVersion = $"{assemblyName.Version.Major}.{assemblyName.Version.Minor}";
#endif
            terminal = new TerminalView(appSettings.TerminalSettings);
            EventBus.Instance.SettingsSaveRequested += SaveSettings;
        }

        public void OnClose()
        {
            SaveSettings();
            terminal.Dispose();
            HistoryHelper.Instance.SaveHistory();
        }

        private void SaveSettings()
        {
            var terminalSettings = Terminal.GetTerminalSettings();
            AppSettings newSettings = new AppSettings
            {
                TerminalSettings = terminalSettings
            };
            SettingsHelper.Instance.SaveSettings(newSettings);
        }

    }
   
}
