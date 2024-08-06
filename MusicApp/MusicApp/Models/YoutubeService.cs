using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YouTubeMusicAPI.Models.Shelf;
using System.Windows.Media.Imaging;

namespace MusicApp.Models
{
	//string name,

	//string id,
	//ShelfItem[] artists,
 //   ShelfItem album,
	//TimeSpan duration,
 //   bool isExplicit,

	//string playsInfo,
	//Radio radio,
	public class MySong
	{
		public string Name { get; }
		public string Id { get; }
		public ShelfItem[] Artists { get; }
		public ShelfItem Album { get; }
		public TimeSpan Duration { get; }
		public bool IsExplicit { get; }
		public string PlaysInfo { get; }
		public Radio Radio { get; }
		public BitmapImage Thumbnail { get; set; }

		public MySong(Song song)
		{
			Name = song.Name;
			Id = song.Id;
			Artists = song.Artists;
			Album = song.Album;
			Duration = song.Duration;
			IsExplicit = song.IsExplicit;
			PlaysInfo = song.PlaysInfo;
			Radio = song.Radio;
			Thumbnail = ThumbnailHelper.GetLowQualityThumbnailAsync(song.Id).Result;
		}
	}

	public class YouTubeService
	{
		private YouTubeMusicClient _youtubeMusicClient;
		private YoutubeClient _youtubeClient;

		public YouTubeService()
		{
			_youtubeMusicClient = new YouTubeMusicClient();
			_youtubeClient = new YoutubeClient();
		}

		public async Task<List<Song>> FetchSongsAsync(string query)
		{
			IEnumerable<Song> searchResults = await _youtubeMusicClient.SearchAsync<Song>(query);
			return searchResults.ToList();
		}

		public async Task<string?> GetAudioStreamUrlAsync(string videoId)
		{
			var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);
			var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
			return audioStreamInfo?.Url;
		}

		public string GetThumbnailUrl(string videoId)
		{
			return $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
		}
	}
}
