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
    public class HistoryHelper
    {

        private const string HISTORY_FILE_NAME = "interm.his";

        private const int HISTORY_SIZE = 30;

        private readonly ILog log = LogManager.GetLogger(typeof(HistoryHelper));

        public static HistoryHelper Instance { get; private set; } = new HistoryHelper();

        private List<string> history;

        private int index;

        private HistoryHelper()
        {
            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                using (StreamReader reader = new StreamReader(HISTORY_FILE_NAME))
                {
                    var serializer = new XmlSerializer(typeof(List<string>));
                    history = (List<string>)serializer.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                history = new List<string>();
                log.Error($"Error loading input history: {e}");
            }
            finally
            {
                index = history.Count;
            }
        }

        public void SaveHistory()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(HISTORY_FILE_NAME))
                {
                    var serializer = new XmlSerializer(typeof(List<string>));
                    serializer.Serialize(writer, history);
                }
            }
            catch (Exception e)
            {
                log.Error($"Error saving input history: {e}");
            }
        }

        public void AddToHistory(string data)
        {
            history.RemoveAll(x => x.Equals(data));
            history.Add(data);
            while(history.Count > HISTORY_SIZE)
            {
                history.RemoveAt(0);
            }
            index = history.Count;
        }

        public string Previous()
        {
            if (index > 0)
            {
                index--;
                return history[index];
            }
            return null;
        }

        public string Next()
        {
            if (index < history.Count)
            {
                index++;
                if (index < history.Count)
                {
                    return history[index];
                }
                else
                {
                    return string.Empty;                    
                }
            }
            return null;
        }
    }
}
