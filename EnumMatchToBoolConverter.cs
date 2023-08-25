﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace PendleCodeMonkey.VoronoiDiagram
{
	public class EnumMatchToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
							  object parameter, CultureInfo culture)
		{
			if (value != null && parameter != null)
			{
				string? checkValue = value.ToString();
				string? targetValue = parameter.ToString();
				return checkValue!.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
			}

			return false;
		}

		public object? ConvertBack(object value, Type targetType,
								  object parameter, CultureInfo culture)
		{
			if (value != null && parameter != null)
			{
				bool useValue = (bool)value;
				string? targetValue = parameter.ToString();
				if (useValue)
				{
					return Enum.Parse(targetType, targetValue!);
				}
			}

			return null;
		}
	}
}
