using InTerm.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InTerm.Helpers
{
    public class SettingsHelper
    {
        private const string SETTINGS_FILE_NAME = "interm.cfg";

        public static SettingsHelper Instance { get; private set; } = new SettingsHelper();

        private readonly ILog log = LogManager.GetLogger(typeof(SettingsHelper));

        public AppSettings AppSettings { get; private set; }

        private SettingsHelper()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                using (StreamReader reader = new StreamReader(SETTINGS_FILE_NAME))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                    AppSettings = (AppSettings)serializer.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                log.Error($"Error loading app settings. {e}");
                AppSettings = new AppSettings();
            }
        }

        public void SaveSettings(AppSettings appSettings)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(SETTINGS_FILE_NAME))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                    serializer.Serialize(writer, appSettings);
                }
                AppSettings = appSettings;
            }
            catch (Exception e)
            {
                log.Error($"Error saving app settings. {e}");
            }
        }

    }
}
