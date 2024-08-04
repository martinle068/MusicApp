using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

public class ThumbnailHelper
{
	public static async Task<BitmapImage> GetSquareThumbnailAsync(string videoUrl)
	{
		using (HttpClient client = new HttpClient())
		{
			var response = await client.GetAsync(videoUrl);
			response.EnsureSuccessStatusCode();
			var stream = await response.Content.ReadAsStreamAsync();

			using (var originalImage = new Bitmap(stream))
			{
				int size = Math.Min(originalImage.Width, originalImage.Height);
				int x = (originalImage.Width - size) / 2;
				int y = (originalImage.Height - size) / 2;

				using (var croppedImage = new Bitmap(size, size))
				{
					using (Graphics g = Graphics.FromImage(croppedImage))
					{
						g.DrawImage(originalImage, new Rectangle(0, 0, size, size), x, y, size, size, GraphicsUnit.Pixel);
					}

					return BitmapToBitmapImage(croppedImage);
				}
			}
		}
	}

	private static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
	{
		using (MemoryStream memory = new MemoryStream())
		{
			bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
			memory.Position = 0;

			BitmapImage bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.StreamSource = memory;
			bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			bitmapImage.EndInit();
			bitmapImage.Freeze();

			return bitmapImage;
		}
	}
}
