using System;
using System.Globalization;
using System.Windows.Data;

namespace krushtype.Core.Utilities.Converters
{
    public class ActiveViewToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            Type currentViewType = value.GetType();
            Type targetViewType = parameter as Type;

            return currentViewType == targetViewType;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
} 