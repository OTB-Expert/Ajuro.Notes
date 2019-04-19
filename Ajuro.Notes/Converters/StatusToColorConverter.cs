using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MemoDrops.Converters
{
		public class StatusToColorConverter : IValueConverter
		{
			public List<Color> Colors = new List<Color>() {
            /* Saved */ System.Windows.Media.Colors.Blue,
            /* Changed */ System.Windows.Media.Colors.Red,
            /* Uploaded */ System.Windows.Media.Colors.Orange
		};

			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				// KnownStatus status = (KnownStatus)value;
				return Colors[(int)value];
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}
	}