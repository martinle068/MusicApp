using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using YouTubeMusicAPI.Models;

namespace MusicApp.Converters
{
	public class SongToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Song song)
			{
				return $"{song.Artists.First().Name} - {song.Name}";
			}
			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
