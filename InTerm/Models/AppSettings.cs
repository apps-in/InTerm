using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InTerm.Models
{
    public class AppSettings
    {
        public TerminalSettings TerminalSettings { get; set; }

        public AppSettings()
        {
            TerminalSettings = new TerminalSettings();
        }
    }
}
