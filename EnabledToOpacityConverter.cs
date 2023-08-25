using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PendleCodeMonkey.VoronoiDiagram
{
	public class EnabledToOpacityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> ((bool?)value ?? true) ? 1.0 : 0.2;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> DependencyProperty.UnsetValue;
	}
}
