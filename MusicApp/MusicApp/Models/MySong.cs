using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Info;
using static MusicApp.Utils.Utils;

namespace MusicApp.Models
{
	public class MySong
	{
		public string Name { get; }
		public string Id { get; }
		public string ArtistAndSongName { get; }
		public ShelfItem[] Artists { get; }
		public string ArtistsString => string.Join(", ", Artists.Select(artist => artist.Name));
		private string? _playlistId { get; } = null;

		public TimeSpan Duration { get; }
		public BitmapImage? Thumbnail { get; private set; }

		public async Task<string> GetPlaylistIdAsync()
		{
			if (_playlistId != null)
			{
				return _playlistId;
			}

			var song = await MyYouTubeService.FetchSongAsync(ArtistAndSongName);

			return song._playlistId;
		}

		private MySong(Song song)
		{
			Name = song.Name;
			Id = song.Id;
			Artists = song.Artists;
			_playlistId = song.Radio.PlaylistId;
			Duration = song.Duration;
			ArtistAndSongName = GetArtistAndSongNameString();
		}

		private MySong(string name, string id, ShelfItem[] artists, TimeSpan duration, string? playlistId = null)
		{
			Name = name;
			Id = id;
			Artists = artists;
			_playlistId = playlistId;
			Duration = duration;
			ArtistAndSongName = GetArtistAndSongNameString();
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
				Thumbnail = await ThumbnailHelper.GetMediumQualityThumbnailAsync(song.Id)
			};
			return mySong;
		}

		private string GetArtistAndSongNameString()
		{
			return $" {string.Join(", ", Artists.Select(artist => artist.Name))} - {Name}";
		}

	}
}
