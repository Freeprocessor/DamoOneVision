using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DamoOneVision.Converters
{
	public class StringToVisibilityConverter : IValueConverter
	{
		/// <summary>
		/// SelectedOption이 ConverterParameter와 일치하면 Visible, 그렇지 않으면 Collapsed를 반환합니다.
		/// </summary>
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			string selectedOption = value as string;
			string targetOption = parameter as string;

			if (string.Equals( selectedOption, targetOption, StringComparison.OrdinalIgnoreCase ))
				return Visibility.Visible;
			else
				return Visibility.Collapsed;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
	}
}
