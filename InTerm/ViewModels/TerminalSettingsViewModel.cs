using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InTerm.Models;
using InTerm.Views;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InTerm.ViewModels
{
    public partial class TerminalSettingsViewModel : ObservableObject
    {
        private readonly Window parent;

        [ObservableProperty]
        private string[] ports;

        public int[] BaudRateOptions { get; } = new int[] { 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };

        public int[] DataBitsOptions { get; } = new int[] { 5, 6, 7, 8 };

        public StopBits[] StopBitsOptions { get; } = new StopBits[] { StopBits.None, StopBits.One, StopBits.OnePointFive, StopBits.Two };

        public Parity[] ParityOptions { get; } = new Parity[] { Parity.None, Parity.Odd, Parity.Even, Parity.Mark, Parity.Space };

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
        private string port;

        [ObservableProperty]
        private int baudRate;

        [ObservableProperty]
        private int dataBits;

        [ObservableProperty]
        private StopBits stopBits;

        [ObservableProperty]
        private Parity parity;

        public TerminalSettingsViewModel() : this(null, null)
        {

        }
        public TerminalSettingsViewModel(Window window, TerminalConnectionSettings settings)
        {
            parent = window;
            ports = SerialPort.GetPortNames();
            if (settings != null)
            {
                Port = settings.Port;
                BaudRate = settings.BaudRate;
                DataBits = settings.DataBits;
                StopBits = settings.StopBits;
                Parity = settings.Parity;
            }
            else
            {
                BaudRate = 115200;
                DataBits = 8;
                StopBits = StopBits.One;
                Parity = Parity.None;
            }
        }

        [RelayCommand]
        public void Cancel()
        {
            parent.DialogResult = false;
            parent.Close();
        }

        private bool IsApplyAllowed()
        {
            return Port != null && Ports.Contains(Port);
        }

        [RelayCommand(CanExecute = nameof(IsApplyAllowed))]
        public void Apply()
        {
            parent.DialogResult = true;
            parent.Close();
        }

        public TerminalConnectionSettings GetSettings()
        {
            return new TerminalConnectionSettings(Port, BaudRate, DataBits, StopBits, Parity);
        }
    }
}
