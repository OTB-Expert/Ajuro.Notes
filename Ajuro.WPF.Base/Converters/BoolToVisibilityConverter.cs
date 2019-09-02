using System;
using System.Windows;
using System.Windows.Data;
namespace Ajuro.WPF.Base.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool))
            {
                return null;
            }

            bool visible = (bool)value;
            bool reverse = false;

            if (parameter != null)
            {
                Boolean.TryParse(parameter.ToString(), out reverse);
            }
            if (reverse)
            {
                return visible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return visible ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
			return null;
           // throw new NotImplementedException();
        }
    }
}
