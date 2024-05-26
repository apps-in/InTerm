using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InTerm.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;

namespace InTerm.ViewModels
{
    public partial class DataForwardingSettingsViewModel : ObservableObject
    {
        private readonly Window parent;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
        private string filePath;

        [ObservableProperty]
        private bool appendData;

        [ObservableProperty]
        private bool hexData;

        [ObservableProperty]
        private bool timestamps;

        [ObservableProperty]
        private bool localEcho;

        [ObservableProperty]
        private bool systemEvents;

        public DataForwardingSettingsViewModel() : this(null, null)
        {

        }
        public DataForwardingSettingsViewModel(Window window, DataForwardingSettings settings)
        {
            parent = window;
            filePath = settings.FilePath;
            appendData = settings.AppendData;
            hexData = settings.HexData;
            timestamps = settings.Timestamps;
            localEcho = settings.LocalEcho;
            systemEvents = settings.SystemEvents;
        }

        [RelayCommand]
        public void Cancel()
        {
            parent.DialogResult = false;
            parent.Close();
        }

        private bool IsApplyAllowed()
        {
            return !string.IsNullOrEmpty(FilePath);
        }

        [RelayCommand(CanExecute = nameof(IsApplyAllowed))]
        public void Apply()
        {
            parent.DialogResult = true;
            parent.Close();
        }

        [RelayCommand]
        public void Browse()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            try
            {
                if (!string.IsNullOrEmpty(FilePath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(FilePath);
                }
            }
            catch
            {
                //do nothing
            }
            var result = dialog.ShowDialog();
            if (result == true)
            {
                FilePath = dialog.FileName;
            }
        }

        public DataForwardingSettings GetSettings()
        {
            return new DataForwardingSettings
            {
                FilePath = FilePath,
                AppendData = AppendData,
                HexData = HexData,
                Timestamps = Timestamps,
                LocalEcho = LocalEcho,
                SystemEvents = SystemEvents
            };
        }
    }
}
