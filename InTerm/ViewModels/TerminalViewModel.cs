using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InTerm.Helpers;
using InTerm.Models;
using InTerm.Views;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace InTerm.ViewModels
{
    public partial class TerminalViewModel : ObservableObject
    {
        private readonly System.Windows.Media.Brush TIMESTAMP_COLOR = System.Windows.Media.Brushes.Gray;

        private readonly System.Windows.Media.Brush SYSTEM_MESSAGE_COLOR = System.Windows.Media.Brushes.Green;

        private readonly System.Windows.Media.Brush OUTGOING_MESSAGE_COLOR = System.Windows.Media.Brushes.Blue;

        private readonly ILog log = LogManager.GetLogger(typeof(App));

        private readonly HistoryHelper historyHelper = HistoryHelper.Instance;

        private readonly DataForwardingViewModel dataForwarding = new DataForwardingViewModel();

        private TerminalConnectionSettings connectionSettings;

        private DataForwardingSettings dataForwardingSettings;

        private SerialPort serialPort;

        private FlowDocument textData;

        private FlowDocument hexData;

        [ObservableProperty]
        private bool connected;

        [ObservableProperty]
        private string connectDisconnectButtonText;

        [ObservableProperty]
        private string connectionStateText;

        [ObservableProperty]
        private string dataAppendText;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendDataCommand))]
        private string dataInput;

        [ObservableProperty]
        private bool autoScroll;

        [ObservableProperty]
        private bool showHexData;

        [ObservableProperty]
        private bool showTimestamps;

        [ObservableProperty]
        private bool localEcho;

        [ObservableProperty]
        private bool systemEvents;

        [ObservableProperty]
        private TailData appendData;

        public List<TailData> AppendDataOptions { get; } = Enum.GetValues(typeof(TailData)).Cast<TailData>().ToList();

        private bool hasConnectionError;

        public FlowDocument TextData { get => textData; }

        public FlowDocument HexData { get => hexData; }

        private List<SerialDataChunk> data = new List<SerialDataChunk>();

        public List<SerialDataChunk> Data { get => data; }

        public DataForwardingViewModel DataForwarding { get => dataForwarding; }

        public delegate void ApplyHistoryRequest();

        public event ApplyHistoryRequest OnHistoryApplied;

        public delegate void AutoScrollRequest();

        public event AutoScrollRequest OnAutoScrollRequested;

        public TerminalViewModel() : this(new TerminalSettings())
        {

        }

        public TerminalViewModel(TerminalSettings terminalSettings)
        {
            hasConnectionError = false;
            connectionSettings = terminalSettings.ConnectionSettings;
            dataForwardingSettings = terminalSettings.DataForwardingSettings;
            autoScroll = terminalSettings.AutoScroll;
            showHexData = terminalSettings.ShowHex;
            showTimestamps = terminalSettings.ShowTimestamps;
            localEcho = terminalSettings.LocalEcho;
            systemEvents = terminalSettings.SystemEvents;
            AppendData = terminalSettings.AppendData;
            textData = new FlowDocument();
            hexData = new FlowDocument();
            UpdateState();
        }

        [RelayCommand]
        public void ShowSettings()
        {
            TerminalSettingsWindow settingsWindow = new TerminalSettingsWindow(connectionSettings);
            var result = settingsWindow.ShowDialog();
            if (result == true)
            {
                UpdateSettings(settingsWindow.Settings);
                ConnectDisconnectCommand.NotifyCanExecuteChanged();
            }
        }

        partial void OnAppendDataChanged(TailData value)
        {
            switch (value)
            {
                case TailData.AppendNothing:
                    DataAppendText = string.Empty;
                    break;
                case TailData.AppendCr:
                    DataAppendText = "\\r";
                    break;
                case TailData.AppendLf:
                    DataAppendText = "\\n";
                    break;
                case TailData.AppendCrLr:
                    DataAppendText = "\\r\\n";
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        [RelayCommand(CanExecute = nameof(CanConnectDisconnect))]
        public void ConnectDisconnect()
        {
            if (connectionSettings != null && connectionSettings.Port != null)
            {
                if (serialPort != null)
                {
                    try
                    {
                        serialPort.Close();
                        AppendSystemEventChunk($"Connection closed [{FormatConnectionData()}]");
                    }
                    catch (Exception e)
                    {
                        log.Error($"Error closing COM port: {e}");
                    }
                    finally
                    {
                        serialPort.DataReceived -= OnSerialDataReceived;
                        serialPort.ErrorReceived -= OnErrorReceived;
                        serialPort = null;
                        UpdateState();
                    }
                }
                else
                {
                    try
                    {
                        hasConnectionError = false;
                        serialPort = new SerialPort(connectionSettings.Port, connectionSettings.BaudRate, connectionSettings.Parity, connectionSettings.DataBits, connectionSettings.StopBits);
                        serialPort.Open();
                        serialPort.DataReceived += OnSerialDataReceived;
                        serialPort.ErrorReceived += OnErrorReceived;
                        AppendSystemEventChunk($"Connected [{FormatConnectionData()}]");
                    }
                    catch (Exception e)
                    {
                        hasConnectionError = false;
                        serialPort.DataReceived -= OnSerialDataReceived;
                        serialPort.ErrorReceived -= OnErrorReceived;
                        serialPort = null;
                        log.Error($"Error opening COM port: {e}");
                        AppendSystemEventChunk($"Connection failed [{FormatConnectionData()}]");
                    }
                    finally
                    {
                        UpdateState();
                    }
                }
            }
            else
            {
                ShowSettings();
            }

        }

        [RelayCommand]
        private void SetupForwarding()
        {
            DataForwardingSettingsWindow settingsWindow = new DataForwardingSettingsWindow(dataForwardingSettings);
            var result = settingsWindow.ShowDialog();
            if (result == true)
            {
                dataForwardingSettings = settingsWindow.Settings;
                dataForwarding.Setup(dataForwardingSettings);
            }
        }

        [RelayCommand]
        private void StopForwarding()
        {
            dataForwarding.Stop();
        }

        private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            AppendSystemEventChunk($"Connection error [{FormatConnectionData()}]");
        }

        private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort != null)
            {
                try
                {
                    int count = serialPort.BytesToRead;
                    if (count > 0)
                    {
                        byte[] buffer = new byte[count];
                        serialPort.Read(buffer, 0, count);
                        AppendDataChunk(buffer, false);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error reading COM port data");
                    log.Error(ex);
                }
            }
        }

        partial void OnShowTimestampsChanged(bool value)
        {
            RebuildData();
        }

        partial void OnLocalEchoChanged(bool value)
        {
            RebuildData();
        }

        partial void OnSystemEventsChanged(bool value)
        {
            RebuildData();
        }

        private bool CanConnectDisconnect()
        {
            return connectionSettings != null;
        }

        [RelayCommand(CanExecute = nameof(CanSendData))]
        private void SendData()
        {
            if (serialPort != null)
            {
                try
                {
                    string append = null;
                    switch (AppendData)
                    {
                        case TailData.AppendNothing:
                            append = string.Empty;
                            break;
                        case TailData.AppendCr:
                            append = "\r";
                            break;
                        case TailData.AppendLf:
                            append = "\n";
                            break;
                        case TailData.AppendCrLr:
                            append = "\r\n";
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    string text = $"{DataInput}{append}";
                    historyHelper.AddToHistory(DataInput);
                    DataInput = string.Empty;
                    serialPort.Write(text);
                    if (LocalEcho)
                    {
                        var bytes = Encoding.UTF8.GetBytes(text);
                        AppendDataChunk(bytes, true);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error sending data");
                    log.Error(ex);
                }
            }
        }

        private bool CanSendData()
        {
            return serialPort != null && DataInput.Length > 0;
        }

        public void GetNextHistoryItem()
        {
            string text = historyHelper.Next();
            if (text != null)
            {
                DataInput = text;
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    OnHistoryApplied?.Invoke();
                });
            }
        }

        public void GetPreviousHistoryItem()
        {
            string text = historyHelper.Previous();
            if (text != null)
            {
                DataInput = text;
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    OnHistoryApplied?.Invoke();
                });
            }
        }

        private void AppendSystemEventChunk(string eventData)
        {
            var chunk = new SerialDataChunk(Encoding.UTF8.GetBytes(eventData), SerialDataType.SYSTEM);
            data.Add(chunk);
            DataForwarding.AddData(chunk);
            AddTextDataLine(chunk);
            AddHexDataLine(chunk);
            if (AutoScroll)
            {
                OnAutoScrollRequested?.Invoke();
            }
        }

        private void AppendDataChunk(byte[] bytes, bool echo)
        {
            var chunk = new SerialDataChunk(bytes, echo ? SerialDataType.ECHO : SerialDataType.INCOMING);
            data.Add(chunk);
            DataForwarding.AddData(chunk);
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                AddTextDataLine(chunk);
                AddHexDataLine(chunk);
                if (AutoScroll)
                {
                    OnAutoScrollRequested?.Invoke();
                }
            });
        }

        private void AddTextDataLine(SerialDataChunk chunk)
        {
            if (chunk.Type == SerialDataType.ECHO && !LocalEcho)
            {
                return;
            }
            if (chunk.Type == SerialDataType.SYSTEM && !SystemEvents)
            {
                return;
            }
            Paragraph paragraph = new Paragraph();
            if (ShowTimestamps)
            {
                paragraph.Inlines.Add(new Run { Text = chunk.Timestamp.ToString("yyyy.MM.dd HH:mm:ss.fff: "), Foreground = TIMESTAMP_COLOR });
            }
            string text = Encoding.UTF8.GetString(chunk.Data);
            if (text.EndsWith("\r\n"))
            {
                text = text.Substring(0, text.Length - 2);
            }
            else if (text.EndsWith("\r") || text.EndsWith("\n"))
            {
                text = text.Substring(0, text.Length - 1);
            }
            var run = new Run();
            if (chunk.Type == SerialDataType.ECHO)
            {
                run.Foreground = OUTGOING_MESSAGE_COLOR;
            }
            if (chunk.Type == SerialDataType.SYSTEM)
            {
                run.Foreground = SYSTEM_MESSAGE_COLOR;
            }
            run.Text = text;
            paragraph.Inlines.Add(run);
            textData.Blocks.Add(paragraph);
        }

        private void AddHexDataLine(SerialDataChunk chunk)
        {
            if (chunk.Type == SerialDataType.ECHO && !LocalEcho)
            {
                return;
            }
            if (chunk.Type == SerialDataType.SYSTEM && !SystemEvents)
            {
                return;
            }
            Paragraph paragraph = new Paragraph();
            if (ShowTimestamps)
            {
                paragraph.Inlines.Add(new Run { Text = chunk.Timestamp.ToString("yyyy.MM.dd HH:mm:ss.fff: "), Foreground = TIMESTAMP_COLOR });
            }
            var run = new Run();
            if (chunk.Type == SerialDataType.ECHO)
            {
                run.Foreground = OUTGOING_MESSAGE_COLOR;
            }
            if (chunk.Type == SerialDataType.SYSTEM)
            {
                run.Foreground = SYSTEM_MESSAGE_COLOR;
                run.Text = Encoding.UTF8.GetString(chunk.Data);
            }
            else
            {
                run.Text = string.Join(" ", chunk.Data.Select(b => $"0x{b.ToString("X2")}"));
            }
            paragraph.Inlines.Add(run);
            hexData.Blocks.Add(paragraph);
        }

        private void RebuildTextData()
        {
            textData.Blocks.Clear();
            foreach (var chunk in data)
            {
                AddTextDataLine(chunk);
            }
        }

        private void RebuildHexData()
        {
            hexData.Blocks.Clear();
            foreach (var chunk in data)
            {
                AddHexDataLine(chunk);
            }
        }

        private void RebuildData()
        {
            RebuildTextData();
            RebuildHexData();
            if (AutoScroll)
            {
                OnAutoScrollRequested?.Invoke();
            }
        }

        [RelayCommand]
        private void Clear()
        {
            data.Clear();
            RebuildTextData();
            RebuildHexData();
        }

        [RelayCommand]
        private void CopyText()
        {
            CopyDataToClipboard(textData.ToString());
        }

        [RelayCommand]
        private void SaveText()
        {
            SaveDataToFile(textData.ToString());
        }

        [RelayCommand]
        private void CopyHex()
        {
            CopyDataToClipboard(hexData.ToString());
        }

        [RelayCommand]
        private void SaveHex()
        {
            SaveDataToFile(hexData.ToString());
        }

        private void CopyDataToClipboard(string data)
        {
            try
            {
                Clipboard.SetText(data);
            }
            catch (Exception e)
            {
                log.Error($"Error copying data to clipboard: {e}");
            }
        }

        private void SaveDataToFile(string data)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                FileName = "terminal.txt",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };
            var result = dialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(dialog.FileName))
                    {
                        writer.Write(data);
                        MessageBox.Show("Data saved to file", "Save to file");
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Data save error", "Save to file");
                    log.Error($"Error saving data to file: {e}");
                }
            }
        }

        private void UpdateSettings(TerminalConnectionSettings settings)
        {
            if (!settings.Equals(connectionSettings))
            {
                bool reconnect = !settings.ConnectionEqual(connectionSettings);
                connectionSettings = settings;
                EventBus.Instance.RequestSettingsSave();
                if (reconnect)
                {
                    if (serialPort != null)
                    {
                        ConnectDisconnect();
                    }
                    ConnectDisconnect();
                }
                else
                {
                    UpdateState();
                }
            }
        }

        private string FormatConnectionData()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(connectionSettings.Port);
            stringBuilder.Append(" ");
            stringBuilder.Append(connectionSettings.DataBits);
            switch (connectionSettings.Parity)
            {
                case Parity.None:
                    stringBuilder.Append("N");
                    break;
                case Parity.Odd:
                    stringBuilder.Append("O");
                    break;
                case Parity.Even:
                    stringBuilder.Append("E");
                    break;
                case Parity.Mark:
                    stringBuilder.Append("M");
                    break;
                case Parity.Space:
                    stringBuilder.Append("S");
                    break;
            }
            switch (connectionSettings.StopBits)
            {
                case StopBits.None:
                    stringBuilder.Append("0");
                    break;
                case StopBits.One:
                    stringBuilder.Append("1");
                    break;
                case StopBits.Two:
                    stringBuilder.Append("2");
                    break;
                case StopBits.OnePointFive:
                    stringBuilder.Append("1.5");
                    break;
            }
            stringBuilder.Append($"@{connectionSettings.BaudRate} bps");
            return stringBuilder.ToString();
        }

        private void UpdateState()
        {
            ConnectDisconnectButtonText = (serialPort?.IsOpen ?? false) ? "Disconnect" : "Connect";
            if (connectionSettings?.Port == null)
            {
                ConnectionStateText = "Not initialized";
                DataAppendText = string.Empty;
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                string connectionData = FormatConnectionData();
                stringBuilder.Append(connectionData);
                stringBuilder.Append(" ");
                if (hasConnectionError)
                {
                    stringBuilder.Append("[Connection error]");
                }
                else
                {
                    stringBuilder.Append($"[{(serialPort != null ? "Connected" : "Not connected")}]");
                }
                ConnectionStateText = stringBuilder.ToString();               
            }
        }

        public TerminalSettings GetTerminalSettings()
        {
            return new TerminalSettings
            {
                ConnectionSettings = connectionSettings,
                DataForwardingSettings = dataForwardingSettings,
                AutoScroll = AutoScroll,
                ShowHex = ShowHexData,
                ShowTimestamps = ShowTimestamps,
                LocalEcho = LocalEcho,
                SystemEvents = SystemEvents,
                AppendData = AppendData
            };
        }

        public void Dispose()
        {
            DataForwarding.Dispose();
        }
    }
}
