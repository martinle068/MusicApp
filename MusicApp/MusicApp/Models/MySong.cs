using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Info;

namespace MusicApp.Models
{
	public class MySong
	{
		public string Name { get; }
		public string Id { get; }
		public string ArtistAndSongName { get; }
		public ShelfItem[] Artists { get; }
		public TimeSpan Duration { get; }
		public BitmapImage? Thumbnail { get; private set; }

		private MySong(Song song)
		{
			Name = song.Name;
			Id = song.Id;
			Artists = song.Artists;
			Duration = song.Duration;
			ArtistAndSongName = $"{Artists.First().Name} - {Name}";
		}

		private MySong(string name, string id, ShelfItem[] artists, TimeSpan duration)
		{
			Name = name;
			Id = id;
			Artists = artists;
			Duration = duration;
			ArtistAndSongName = $"{Artists.First().Name} - {Name}";
		}

		public static async Task<MySong> CreateAsync(SongVideoInfo songVideoInfo)
		{
			MySong mySong = new MySong(songVideoInfo.Name, songVideoInfo.Id, songVideoInfo.Artists, songVideoInfo.Duration)
			{
				Thumbnail = await ThumbnailHelper.GetLowQualityThumbnailAsync(songVideoInfo.Id)
			};
			return mySong;
		}

		public static async Task<MySong> CreateAsync(Song song)
		{
			var mySong = new MySong(song)
			{
				Thumbnail = await ThumbnailHelper.GetLowQualityThumbnailAsync(song.Id)
			};
			return mySong;
		}
	}
}
