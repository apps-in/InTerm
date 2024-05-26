using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InTerm.Models
{
    public class SerialDataChunk
    {
        public byte[] Data { get; }
        public DateTime Timestamp { get; }
        public SerialDataType Type { get; }

        public SerialDataChunk(byte[] data, SerialDataType type = SerialDataType.INCOMING)
        {
            Data = data;
            Type = type;
            Timestamp = DateTime.Now;
        }
    }
}
