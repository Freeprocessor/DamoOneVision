using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace DamoOneVision.Converters
{
	public class FileNameConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if (value is string path)
			{
				string fileName = Path.GetFileNameWithoutExtension(path);

				// 예: RAWInfraredCamera_20250514_103524_3
				var match = Regex.Match(fileName, @"_(\d{8})_(\d{6})_(\d+)$");
				if (match.Success)
				{
					string time = match.Groups[2].Value;   // 103524
					string number = match.Groups[3].Value; // 3

					if (TimeSpan.TryParseExact( time, "hhmmss", null, out TimeSpan t ))
					{
						return $"{t:hh\\:mm\\:ss} ({number})";
					}

					return $"{time} ({number})";
				}

				// 매칭 실패 시 파일명 마지막 일부만 반환
				return fileName.Length > 12 ? fileName[ ^12.. ] : fileName;
			}

			return value;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
			=> throw new NotImplementedException();
	}
}
