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

		public async Task<string?> GetAudioStreamUrlAsync(string songId)
		{
			var videoId = YoutubeExplode.Videos.VideoId.Parse(songId);
			var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);
			var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
			return audioStreamInfo?.Url;
		}

		public string GetThumbnailUrl(string videoId)
		{
			return $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
		}

		public async Task<List<Playlist>> FetchAllPlaylistsAsync()
		{
			var playlists = new List<Playlist>();
			string nextPageToken = null;

			try
			{
				do
				{
					var request = _googleYouTubeService.Playlists.List("snippet");
					request.Mine = true;
					request.MaxResults = 10;
					request.PageToken = nextPageToken;

					var response = await request.ExecuteAsync();
					playlists.AddRange(response.Items);
					nextPageToken = response.NextPageToken;

				} while (nextPageToken != null);
			}
			catch (GoogleApiException ex)
			{
				MessageBox.Show($"An API error occurred: {ex.Message}");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}");
			}

			return playlists;
		}

		public async Task<List<MySong>?> FetchPlaylistContentAsync(string playlistId)
		{
			try
			{
				var request = _googleYouTubeService.PlaylistItems.List("snippet,contentDetails");
				request.PlaylistId = playlistId;
				request.MaxResults = 200; // Maximum number of items to retrieve per request

				var response = await request.ExecuteAsync();

				var tasks = response.Items.Select(async item =>
				{
					var itemInfo = await _youtubeMusicClient.GetSongVideoInfoAsync(item.ContentDetails.VideoId);
					var song = await MySong.CreateAsync(itemInfo);
					return song;
				});

				var playlistSongs = await Task.WhenAll(tasks);
				return playlistSongs.ToList();
			}
			catch (GoogleApiException ex)
			{
				MessageBox.Show($"An API error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;

			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}
		}

		public async Task AddNewPlaylist(string title, string? description = null, string privacyStatus = "private")
		{
			try
			{
				var newPlaylist = new Playlist
				{
					Snippet = new PlaylistSnippet
					{
						Title = title,
						Description = description ?? ""
					},
					Status = new PlaylistStatus
					{
						PrivacyStatus = privacyStatus == "public" ? "public" : "private"
					}
				};

				var request = _googleYouTubeService.Playlists.Insert(newPlaylist, "snippet,status");
				var response = await request.ExecuteAsync();
				MessageBox.Show($"New playlist created. Title: {response.Snippet.Title}, PlaylistId: {response.Id}", "Playlist Created", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (GoogleApiException ex)
			{
				MessageBox.Show($"An API error occurred: {ex.Message}\n{ex.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}


		public async Task DeletePlaylistAsync(string playlistId)
		{
			try
			{
				var request = _googleYouTubeService.Playlists.Delete(playlistId);
				await request.ExecuteAsync();
				MessageBox.Show($"Playlist with ID {playlistId} deleted.", "Playlist Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (GoogleApiException ex)
			{
				MessageBox.Show($"An API error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public async Task AddSongToPlaylist(Playlist playlist, MySong song)
		{
			try
			{
				var newPlaylistItem = new PlaylistItem
				{
					Snippet = new PlaylistItemSnippet
					{
						PlaylistId = playlist.Id,
						ResourceId = new ResourceId
						{
							Kind = "youtube#video",
							VideoId = song.Id
						}
					}
				};

				var request = _googleYouTubeService.PlaylistItems.Insert(newPlaylistItem, "snippet");
				await request.ExecuteAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Failed to add song to playlist: {ex.Message}");
			}
		}
	}
}
