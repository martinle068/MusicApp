using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

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
