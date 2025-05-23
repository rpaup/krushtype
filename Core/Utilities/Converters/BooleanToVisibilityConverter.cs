using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace krushtype.Core.Utilities.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                bool reverseLogic = parameter != null && parameter.ToString() == "Reverse";
                
                if (reverseLogic)
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                else
                    return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool reverseLogic = parameter != null && parameter.ToString() == "Reverse";
                
                if (reverseLogic)
                    return visibility != Visibility.Visible;
                else
                    return visibility == Visibility.Visible;
            }
            
            return false;
        }
    }
}
