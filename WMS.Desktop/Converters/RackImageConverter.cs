using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WMS.Desktop.Converters
{
    public class RackImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                RackType.RackType1 => "/Images/RackType1.jpg",
                RackType.RackType2 => "/Images/RackType2.jpg",
                RackType.RackType3=> "/Images/RackType3.jpg",
                _ => null
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
