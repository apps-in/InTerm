using CommunityToolkit.Mvvm.ComponentModel;
using InTerm.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InTerm.ViewModels
{
    public partial class DataForwardingViewModel : ObservableObject
    {
        private const string FORWARDING_STOPPED_MESSAGE = "Forwarding stopped";
        private const string FORWARDING_SETUP_ERROR_MESSAGE = "Forwarding setup error";
        private const string DATA_FORWARDING_ERROR_MESSAGE = "Data forwarding error";

        private StreamWriter streamWriter;

        private DataForwardingSettings settings;

        [ObservableProperty]
        private bool isStarted;

        [ObservableProperty]
        private bool isValid;

        [ObservableProperty]
        private string message;

        private SerialDataType? lastDataType;

        public DataForwardingViewModel()
        {
            streamWriter = null;
            isStarted = false;
            isValid = false;
            message = FORWARDING_STOPPED_MESSAGE;
            lastDataType = null;
        }

        public void Setup(DataForwardingSettings dataForwardingSettings)
        {
            lock (this)
            {
                settings = dataForwardingSettings;
                IsStarted = true;
                try
                {
                    streamWriter = new StreamWriter(dataForwardingSettings.FilePath, dataForwardingSettings.AppendData);
                    Message = dataForwardingSettings.FilePath;
                    IsValid = true;
                }
                catch
                {
                    Message = FORWARDING_SETUP_ERROR_MESSAGE;
                    IsValid = false;
                }
            }
        }

        public void AddData(SerialDataChunk data)
        {
            lock (this)
            {
                if (IsValid && streamWriter != null)
                {
                    if (data.Type == SerialDataType.ECHO && settings.LocalEcho)
                    {
                        WriteBinaryData(data);
                    }
                    if (data.Type == SerialDataType.SYSTEM && settings.SystemEvents)
                    {
                        WriteTextData(data);
                    }
                    if (data.Type == SerialDataType.INCOMING)
                    {
                        if (settings.HexData)
                        {
                            WriteBinaryData(data);
                        }
                        else
                        {
                            WriteTextData(data);
                        }
                    }
                }
            }
        }

        private void WriteBinaryData(SerialDataChunk data)
        {
            string text = string.Join(" ", data.Data.Select(b => $"0x{b.ToString("X2")}")); 
            WriteData(data, text);
        }

        private void WriteTextData(SerialDataChunk data)
        {
            WriteData(data, Encoding.UTF8.GetString(data.Data));
        }

        private void WriteData(SerialDataChunk metadata, string data)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (settings.Timestamps)
                {
                    stringBuilder.Append(metadata.Timestamp.ToString("yyyy.MM.dd HH:mm:ss.fff: "));
                }
                stringBuilder.Append(data);
                if (settings.Timestamps || metadata.Type == SerialDataType.SYSTEM || metadata.Type != lastDataType)
                {
                    streamWriter.WriteLine(stringBuilder.ToString());
                }
                else
                {
                    streamWriter.Write(stringBuilder.ToString());
                }
            } catch
            {
                IsValid = false;
                Message = DATA_FORWARDING_ERROR_MESSAGE;
            }
        }


        public void Stop()
        {
            lock (this)
            {
                IsStarted = false;
                Message = FORWARDING_STOPPED_MESSAGE;
                try
                {
                    if (streamWriter != null)
                    {
                        streamWriter.Flush();
                        streamWriter.Close();
                        streamWriter.Dispose();
                    }
                }
                catch
                {
                    //do nothing
                }
                finally
                {
                    streamWriter = null;
                }
            }
        }

        public void Dispose()
        {
            if (IsStarted)
            {
                Stop();
            }   
        }
    }
}
