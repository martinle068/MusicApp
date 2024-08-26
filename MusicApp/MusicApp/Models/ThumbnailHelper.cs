using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

public class ThumbnailHelper
{
	/// <summary>
	/// Asynchronously retrieves the high-quality thumbnail for a given song ID.
	/// If the high-quality thumbnail is not available, it attempts to retrieve the medium-quality thumbnail.
	/// If neither are available, it falls back to a low-quality thumbnail.
	/// </summary>
	/// <param name="songId">The ID of the song to retrieve the thumbnail for.</param>
	/// <returns>A <see cref="BitmapImage"/> containing the thumbnail image.</returns>
	public static async Task<BitmapImage> GetHighQualityThumbnailAsync(string songId)
	{
		try
		{
			var imageUrl = $"https://img.youtube.com/vi/{songId}/maxresdefault.jpg";

			return await GetSquareThumbnailAsync(imageUrl);
		}
		catch (Exception)
		{
			try
			{
				return await GetMediumQualityThumbnailAsync(songId);
			}
			catch (Exception)
			{
				var imageUrl = $"https://img.youtube.com/vi/{songId}/0.jpg";

				return await GetSquareThumbnailAsync(imageUrl);
			}
		}
	}

	/// <summary>
	/// Asynchronously retrieves the medium-quality thumbnail for a given song ID.
	/// </summary>
	/// <param name="songId">The ID of the song to retrieve the thumbnail for.</param>
	/// <returns>A <see cref="BitmapImage"/> containing the thumbnail image.</returns>
	public static async Task<BitmapImage> GetMediumQualityThumbnailAsync(string songId)
	{
		var imageUrl = $"https://img.youtube.com/vi/{songId}/sd1.jpg";

		return await GetSquareThumbnailAsync(imageUrl);
	}

	/// <summary>
	/// Asynchronously retrieves the low-quality thumbnail for a given song ID.
	/// </summary>
	/// <param name="songId">The ID of the song to retrieve the thumbnail for.</param>
	/// <returns>A <see cref="BitmapImage"/> containing the thumbnail image.</returns>
	public static async Task<BitmapImage> GetLowQualityThumbnailAsync(string songId)
	{
		var imageUrl = $"https://img.youtube.com/vi/{songId}/1.jpg";

		return await GetSquareThumbnailAsync(imageUrl);
	}

	/// <summary>
	/// Asynchronously retrieves and crops a thumbnail to a square aspect ratio.
	/// </summary>
	/// <param name="videoUrl">The URL of the video thumbnail to retrieve.</param>
	/// <returns>A <see cref="BitmapImage"/> containing the square-cropped thumbnail image.</returns>
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

	/// <summary>
	/// Converts a <see cref="Bitmap"/> to a <see cref="BitmapImage"/>.
	/// </summary>
	/// <param name="bitmap">The bitmap to convert.</param>
	/// <returns>A <see cref="BitmapImage"/> created from the bitmap.</returns>
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
