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
using System.Collections.ObjectModel;
using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;
using Newtonsoft.Json.Linq;
using Google.Apis.Requests;
using YouTubeMusicAPI.Models.Info;

namespace MusicApp.Models
{
	public class MyYouTubeService
	{
		private static readonly string[] Scopes = {
			YouTubeService.Scope.Youtube,
			"https://www.googleapis.com/auth/youtube.force-ssl"
		};
		private static readonly string ApplicationName = "MusicApp";
		private static YouTubeService _googleYouTubeService = new();

		private static YouTubeMusicClient _youtubeMusicClient = new();
		private static YoutubeClient _youtubeClient = new();

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

		public async Task<MyShelf<MySong>?> FetchSongShelvesAsync(string query, string? continuationToken)
		{
			IEnumerable<Shelf> shelves = await _youtubeMusicClient.SearchAsync(query, continuationToken, ShelfKind.Songs);

			List<Song> songs = shelves.SelectMany(shelf => shelf.Items).OfType<Song>().ToList();
			continuationToken = shelves.FirstOrDefault()?.NextContinuationToken;
			var mySongs = await Task.WhenAll(songs.Select(async song => MySong.Create(song)));

			return new MyShelf<MySong>(new ObservableCollection<MySong>(mySongs), continuationToken);
		}

		public static async Task<ObservableCollection<MySong>> FetchSongsAsync(string query, int count)
		{
			IEnumerable<Song> songs = await _youtubeMusicClient.SearchAsync<Song>(query, count);
			var mySongs = await Task.WhenAll(songs.Select(async song => MySong.Create(song)));
			return new ObservableCollection<MySong>(mySongs);
		}

		public static async Task<MySong?> FetchSongAsync(string query)
		{
			IEnumerable<Song> songs = await _youtubeMusicClient.SearchAsync<Song>(query, 1);
			if (!songs.Any()) return null;

			return MySong.Create(songs.FirstOrDefault());
		}

		public async Task<string?> GetAudioStreamUrlAsync(string songId)
		{
			var videoId = YoutubeExplode.Videos.VideoId.Parse(songId);
			var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);
			var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
			return audioStreamInfo?.Url;
		}

		public async Task<ObservableCollection<Playlist>> FetchAllPlaylistsAsync()
		{
			var playlists = new List<Playlist>();
			string? nextPageToken = null;

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
				MessageBox.Show($"1An error occurred: {ex.Message}");
			}

			return new ObservableCollection<Playlist>(playlists);
		}

		public static async Task<SongVideoInfo> FetchSongVideoInfoAsync(string songId)
		{
			return await _youtubeMusicClient.GetSongVideoInfoAsync(songId);
		}

		public async Task<ObservableCollection<MySong>?> FetchPlaylistContentAsync(string playlistId)
		{
			try
			{
				var request = _googleYouTubeService.PlaylistItems.List("snippet,contentDetails");
				request.PlaylistId = playlistId;
				request.MaxResults = 200; // Maximum number of items to retrieve per request

				var response = await request.ExecuteAsync();

				var tasks = response.Items.Select(async item =>
				{
					var itemInfo = await FetchSongVideoInfoAsync(item.ContentDetails.VideoId);
					var song = MySong.Create(itemInfo);
					return song;
				});

				var playlistSongs = await Task.WhenAll(tasks);
				return new ObservableCollection<MySong>(playlistSongs);
			}
			catch (GoogleApiException ex)
			{
				MessageBox.Show($"An API error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;

			}
			catch (Exception ex)
			{
				MessageBox.Show($"2An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

		public async Task RemoveSongFromPlaylist(Playlist playlist, MySong song)
		{
			try
			{
				string? nextPageToken = null;
				PlaylistItem? playlistItem = null;

				// Loop through pages of playlist items until we find the song
				do
				{
					var playlistItemsRequest = _googleYouTubeService.PlaylistItems.List("id,snippet");
					playlistItemsRequest.PlaylistId = playlist.Id;
					playlistItemsRequest.MaxResults = 20;  
					playlistItemsRequest.PageToken = nextPageToken;

					var playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();

					playlistItem = playlistItemsResponse.Items
						.FirstOrDefault(item => item.Snippet.ResourceId.VideoId == song.Id);

					nextPageToken = playlistItemsResponse.NextPageToken;

				} while (playlistItem == null && !string.IsNullOrEmpty(nextPageToken));

				// If the song is found in the playlist, delete it
				if (playlistItem != null)
				{
					var deleteRequest = _googleYouTubeService.PlaylistItems.Delete(playlistItem.Id);
					await deleteRequest.ExecuteAsync();
				}
				else
				{
					MessageBox.Show("Song not found in the playlist.");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Failed to remove song from playlist: {ex.Message}");
			}
		}

		public async Task<MyShelf<MySong>> FetchPopularSongsAsync(string? nextPageToken)
		{
			var trendingVideosRequest = _googleYouTubeService.Videos.List("snippet");
			trendingVideosRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
			trendingVideosRequest.RegionCode = "US";
			trendingVideosRequest.MaxResults = 20;
			trendingVideosRequest.VideoCategoryId = "10";

			if (nextPageToken != null)
			{
				trendingVideosRequest.PageToken = nextPageToken;
			}

			var trendingVideosResponse = await trendingVideosRequest.ExecuteAsync();

			var songs = await ConvertToSongs(trendingVideosResponse);
			

			return await ConvertSongsToMySongsShelf(songs, trendingVideosResponse.NextPageToken);
		}

		public async Task<ObservableCollection<MySong>?> FetchRadioSongsAsync(string radioPlaylistId)
		{
			try
			{
				string playlistBrowseId = _youtubeMusicClient.GetCommunityPlaylistBrowseId(radioPlaylistId);
				CommunityPlaylistInfo playlist = await _youtubeMusicClient.GetCommunityPlaylistInfoAsync(playlistBrowseId);

				var result = new ObservableCollection<MySong>();
				
				var tasks = playlist.Songs.Select(async playlistSong =>
				{
					var song = MySong.Create(playlistSong);
					result.Add(song);
				});

				await Task.WhenAll(tasks);

				return result;
			}
			catch (Google.GoogleApiException ex)
			{
				MessageBox.Show($"An API error occurred: {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"3An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			return null;
		}


		private async Task<MyShelf<MySong>> ConvertSongsToMySongsShelf(IEnumerable<Song> songs, string? nextPageToken)
		{
			var mySongs = await Task.WhenAll(songs.Select(async song => MySong.Create(song)));
			return new MyShelf<MySong>(new ObservableCollection<MySong>(mySongs), nextPageToken);
		}

		private async Task<List<Song>> ConvertToSongs<T>(IEnumerable<T> items, Func<T, string> getTitle)
		{
			var songs = new List<Song>();
			var tasks = items.Select(async item =>
			{
				var title = getTitle(item);

				IEnumerable<Song> searchResults = await _youtubeMusicClient.SearchAsync<Song>(title, 1);
				var song = searchResults.FirstOrDefault();
				if (song != null)
				{
					songs.Add(song);
				}

			}).ToList();

			await Task.WhenAll(tasks);
			return songs;
		}


		// Specific method for VideoListResponse
		private async Task<List<Song>> ConvertToSongs(VideoListResponse response)
		{
			return await ConvertToSongs(response.Items, video => video.Snippet.Title);
		}

		// Specific method for PlaylistItemListResponse
		private async Task<List<Song>> ConvertToSongs(PlaylistItemListResponse response)
		{
			return await ConvertToSongs(response.Items, item => item.Snippet.Title);
		}
	}
}
