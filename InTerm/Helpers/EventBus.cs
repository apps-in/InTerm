using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InTerm.Helpers
{
    public class EventBus
    {
        public static EventBus Instance { get; private set; } = new EventBus();

        public delegate void SettingsSaveRequest();

        public event SettingsSaveRequest SettingsSaveRequested;

        public void RequestSettingsSave()
        {
            SettingsSaveRequested?.Invoke();
        }
    }
}
