using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MusicApp.Views
{
	public class ThumbnailConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string songId)
			{
				return GetThumbnailAsync(songId);
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		private async Task<BitmapImage> GetThumbnailAsync(string songId)
		{
			return await ThumbnailHelper.GetLowQualityThumbnailAsync(songId);
		}
	}
}
