using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Models;

namespace MusicApp.Models
{
	public class MySong
	{
		public string Name { get; }
		public string Id { get; }
		public string ArtistAndSongName { get; }
		public ShelfItem[] Artists { get; }
		public ShelfItem Album { get; }
		public TimeSpan Duration { get; }
		public bool IsExplicit { get; }
		public string PlaysInfo { get; }
		public Radio Radio { get; }
		public BitmapImage? Thumbnail { get; private set; }

		private MySong(Song song)
		{
			Name = song.Name;
			Id = song.Id;
			Artists = song.Artists;
			Album = song.Album;
			Duration = song.Duration;
			IsExplicit = song.IsExplicit;
			PlaysInfo = song.PlaysInfo;
			Radio = song.Radio;
			ArtistAndSongName = $"{Artists.First().Name} - {Name}";
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
