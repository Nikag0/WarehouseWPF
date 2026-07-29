using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WMS.Domain;

namespace WMS.Desktop.Converters
{
    public class RackImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                RackType.R1 => "/VisualizationResources/RackType1.jpg",
                RackType.R2 => "/VisualizationResources/RackType2.jpg",
                RackType.R3=> "/VisualizationResources/RackType3.jpg",
                _ => null
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
