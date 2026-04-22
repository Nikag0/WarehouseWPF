using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace WMS.Desktop.Converters
{
    public class RackTypeToVisibilityConverter : IValueConverter
    {
        // ConverterParameter — имя enum value или список через запятую, например: "RackType1" или "RackType1,RackType2"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;

            var actual = value.ToString();
            var param = parameter.ToString();

            var expected = param.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim());

            return expected.Any(e => string.Equals(e, actual, StringComparison.OrdinalIgnoreCase))
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}