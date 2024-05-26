using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InTerm.Models
{
    public class DataForwardingSettings
    {
        public string FilePath { get; set; }

        public bool AppendData { get; set; }

        public bool HexData { get; set; }

        public bool Timestamps { get; set; }

        public bool LocalEcho { get; set; }

        public bool SystemEvents { get; set; }

    }
}
