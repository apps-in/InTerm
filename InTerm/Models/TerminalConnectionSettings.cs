using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.WebControls;

namespace InTerm.Models
{
    public class TerminalConnectionSettings
    {

        public string Port { get; set; }

        public int BaudRate { get; set; }

        public int DataBits { get; set; }

        public StopBits StopBits { get; set; }

        public Parity Parity { get; set; }        

        private TerminalConnectionSettings()
        {
            Port = null;
            BaudRate = 115200;
            DataBits = 8;
            StopBits = StopBits.One;
            Parity = Parity.None;            
        }

        public TerminalConnectionSettings(string port, int baudRate, int dataBits, StopBits stopBits, Parity parity) {
            Port = port;
            BaudRate = baudRate;
            DataBits = dataBits;
            StopBits = stopBits;
            Parity = parity;
        }

        public bool ConnectionEqual(TerminalConnectionSettings other)
        {
            if (other != null)
            {
                return other.Port == Port && other.BaudRate == BaudRate && other.DataBits == DataBits && other.StopBits == StopBits && other.Parity == Parity;
            }
            return false;
        }

        public bool Equals(TerminalConnectionSettings other)
        {
            if (other != null)
            {
                return ConnectionEqual(other);
            }
            return false;
        }

        public static TerminalConnectionSettings DefaultSettings { get => new TerminalConnectionSettings(); }
    }
}
