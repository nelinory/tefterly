using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tefterly.Core.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool returnValue = false;

            if (value is bool)
                returnValue = (bool)value;
            else if (value is Nullable<bool>)
                returnValue = ((Nullable<bool>)value ?? false);

            if (IsInverted == true)
                returnValue = !returnValue;

            return returnValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}