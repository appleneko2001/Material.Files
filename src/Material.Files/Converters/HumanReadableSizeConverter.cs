using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Material.Files.Converters {
    public class HumanReadableSizeConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch(value)
            {
                case ulong v:
                    return v.ToHumanReadableSizeString();
                default:
                    return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NullReferenceException();
        }
    }
}