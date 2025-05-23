using System;
using System.Globalization;
using System.Windows.Data;

namespace krushtype.Core.Utilities.Converters
{
    public class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
                
            return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;
                
            if ((bool)value)
                return parameter;
                
            return Binding.DoNothing;
        }
    }
} 