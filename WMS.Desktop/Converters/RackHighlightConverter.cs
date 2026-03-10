using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace WMS.Desktop.Converters
{
    public class RackHighlightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var items = value as IEnumerable<IssueStockDto>;
            var rackCode = parameter as string;

            if (items == null || rackCode == null)
                return Brushes.Transparent;

            foreach (var item in items)
            {
                var rack = ExtractRackCode(item.CellCode); 
                if (rack == rackCode)
                    return Brushes.OrangeRed;
            }

            return Brushes.Transparent;
        }

        private string ExtractRackCode(string cellCode)
        {
            if (string.IsNullOrWhiteSpace(cellCode))
                return null;

            var parts = cellCode.Split('-');
            return parts.Length >= 2 ? $"{parts[0]}-{parts[1]}" : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
