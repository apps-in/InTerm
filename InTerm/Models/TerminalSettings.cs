using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InTerm.Models
{
    public class TerminalSettings
    {
        public TerminalConnectionSettings ConnectionSettings { get; set; }

        public DataForwardingSettings DataForwardingSettings { get; set; }

        public bool AutoScroll { get; set; }

        public bool ShowHex { get; set; }

        public bool ShowTimestamps { get; set; }

        public bool LocalEcho { get; set; }

        public bool SystemEvents { get; set; }

        public TailData AppendData { get; set; }

        public TerminalSettings()
        {
            ConnectionSettings = TerminalConnectionSettings.DefaultSettings;
            DataForwardingSettings = new DataForwardingSettings();
            AutoScroll = true;
            ShowHex = false;
            ShowTimestamps = true;
            LocalEcho = true;
            SystemEvents = true;
            AppendData = TailData.AppendCrLr;
        }
    }
}
