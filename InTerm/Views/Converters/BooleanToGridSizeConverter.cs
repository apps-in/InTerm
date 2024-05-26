using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace InTerm.Views.Converters
{
    public class BooleanToGridSizeConverter : IValueConverter
    {
        public GridLength True { get; set; } = new GridLength(1, GridUnitType.Star);
        public GridLength False { get; set; } = GridLength.Auto;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? True : False;
            }
            throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
