using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Tefterly.Core.Converters
{
    // Credit: https://stackoverflow.com/questions/48350454/wpf-change-icon-depending-on-binding-in-control-template
    public class BooleanToIconStateConverter : MarkupExtension, IValueConverter
    {
        public object TrueValue { get; set; } = Binding.DoNothing;
        public object FalseValue { get; set; } = Binding.DoNothing;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return Binding.DoNothing;

            return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == TrueValue)
                return true;

            if (value == FalseValue)
                return false;

            return Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
