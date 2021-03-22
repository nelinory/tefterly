using System;
using System.Globalization;
using System.Windows.Data;

namespace Tefterly.Core.Converters
{
    public class DateTimeToFormattedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dateTime = (DateTime)value;
            DateTime dateTimeNow = DateTime.Now;

            if (dateTime.Date == dateTimeNow.Date)
                return dateTime.ToString("\"Today\", h:mm tt");
            else if (dateTime.Date == dateTimeNow.Date.AddDays(-1))
                return dateTime.ToString("\"Yesterday\", h:mm tt");
            else if (dateTime.Year == dateTimeNow.Year)
                return dateTime.ToString("MMMM dd, h:mm tt");
            else
                return dateTime.ToString("MMMM dd yyyy, h:mm tt");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
