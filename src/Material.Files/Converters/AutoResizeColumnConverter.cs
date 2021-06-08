using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Material.Files.Converters
{
    public class AutoResizeColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sizeCell = 100.0;
            if (parameter is double size)
            {
                sizeCell = size;
            }

            if (!(value is double panelSize)) 
                return 1;
            
            var probablyColumns = panelSize / sizeCell;
            var columns = Math.Floor(probablyColumns);
            var rounded = Math.Round(probablyColumns, 2);
            return rounded - columns > 0.5 ? columns : Math.Ceiling(rounded);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NullReferenceException();
        }
    }
}