using InTerm.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace InTerm.Views.Converters
{
    public class TailDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TailData tailData)
            {
                switch (tailData)
                {
                    case TailData.AppendNothing:
                        return "Nothing";
                    case TailData.AppendCr:
                        return "CR";
                    case TailData.AppendLf:
                        return "LF";
                    case TailData.AppendCrLr:
                        return "CRLF";
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
