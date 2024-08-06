using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YouTubeMusicAPI.Models.Shelf;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Media;

namespace MusicApp.Models
{
	public class YouTubeService
	{
		private YouTubeMusicClient _youtubeMusicClient;
		private YoutubeClient _youtubeClient;

		public YouTubeService()
		{
			_youtubeMusicClient = new YouTubeMusicClient();
			_youtubeClient = new YoutubeClient();
		}

		public async Task<List<MySong>> FetchSongsAsync(string query)
		{
			IEnumerable<Song> searchResults = await _youtubeMusicClient.SearchAsync<Song>(query);
			var mySongs = await Task.WhenAll(searchResults.Select(async song => await MySong.CreateAsync(song)));
			return mySongs.ToList();
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
