using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace GeoFencing.Converters
{
    class BooleanToInverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool) ? !(bool)value : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is bool) ? !(bool)value : false;
        }
    }
}
