using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media;

namespace Ajuro.WPF.Base.Converters
{
	public class StatusToColorConverter : IValueConverter
	{
		public List<Color> Colors = new List<Color>() {
            System.Windows.Media.Colors.Blue,
            System.Windows.Media.Colors.Red,
            System.Windows.Media.Colors.Orange
		};

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return Colors[(int)value];
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}