using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.IO;
using System.Windows;
using System.Security.RightsManagement;

namespace MusicApp.Models
{
	public class MyYouTubeService
	{
		private static readonly string[] Scopes = {
			YouTubeService.Scope.Youtube,
			"https://www.googleapis.com/auth/youtube.force-ssl"
		};
		private static readonly string ApplicationName = "MusicApp";
		private static YouTubeService _googleYouTubeService;

		private static YouTubeMusicClient _youtubeMusicClient;
		private static YoutubeClient _youtubeClient;

		public static async Task<MyYouTubeService> CreateYoutubeServiceAsync()
		{
			var service = new MyYouTubeService();
			service.InitializeYoutubeClients();
			await service.InitializeGoogleYTService();
			return service;
		}

		private void InitializeYoutubeClients()
		{
			_youtubeMusicClient = new YouTubeMusicClient();
			_youtubeClient = new YoutubeClient();
		}

		private async Task InitializeGoogleYTService()
		{
			try
			{
				string credPath = "token.json";

				UserCredential credential;

				using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
				{
					credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
						GoogleClientSecrets.FromStream(stream).Secrets,
						Scopes,
						"user",
						CancellationToken.None,
						new FileDataStore(credPath, true));
				}

				// Create YouTube Service
				_googleYouTubeService = new YouTubeService(new BaseClientService.Initializer()
				{
					HttpClientInitializer = credential,
					ApplicationName = ApplicationName
				});
			}
			catch
			{
				MessageBox.Show("Failed to connect to Google YouTube API");
			}
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
