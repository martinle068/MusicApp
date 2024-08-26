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
using System.Collections.ObjectModel;
using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;
using YouTubeMusicAPI.Models.Info;

namespace MusicApp.Models
{
	/// <summary>
	/// A service class to interact with YouTube Music and YouTube API for various music-related functionalities.
	/// </summary>
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

		/// <summary>
		/// Initializes and creates an instance of <see cref="MyYouTubeService"/> with all necessary YouTube clients.
		/// </summary>
		/// <returns>An initialized <see cref="MyYouTubeService"/> instance.</returns>
		public static async Task<MyYouTubeService> CreateYoutubeServiceAsync()
		{
			var service = new MyYouTubeService();
			service.InitializeYoutubeClients();
			await service.InitializeGoogleYTService();
			return service;
		}

		/// <summary>
		/// Initializes the YouTubeMusicClient and YoutubeClient.
		/// </summary>
		private void InitializeYoutubeClients()
		{
			_youtubeMusicClient = new YouTubeMusicClient();
			_youtubeClient = new YoutubeClient();
		}

		/// <summary>
		/// Initializes the Google YouTube API service with OAuth2 authentication.
		/// </summary>
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

		/// <summary>
		/// Fetches song shelves based on a query with an optional continuation token.
		/// </summary>
		/// <param name="query">The search query.</param>
		/// <param name="continuationToken">The continuation token for pagination.</param>
		/// <returns>A <see cref="MyShelf{T}"/> containing a collection of songs and a continuation token.</returns>
		public async Task<MyShelf<MySong>?> FetchSongShelvesAsync(string query, string? continuationToken)
		{
			IEnumerable<Shelf> shelves = await _youtubeMusicClient.SearchAsync(query, continuationToken, ShelfKind.Songs);

			List<Song> songs = shelves.SelectMany(shelf => shelf.Items).OfType<Song>().ToList();
			continuationToken = shelves.FirstOrDefault()?.NextContinuationToken;
			var mySongs = await Task.WhenAll(songs.Select(async song => MySong.Create(song)));

			return new MyShelf<MySong>(new ObservableCollection<MySong>(mySongs), continuationToken);
		}

		/// <summary>
		/// Fetches songs based on a query with a specified limit on the number of results.
		/// </summary>
		/// <param name="query">The search query.</param>
		/// <param name="count">The number of songs to fetch.</param>
		/// <returns>An <see cref="ObservableCollection{T}"/> containing the fetched songs.</returns>
		public static async Task<ObservableCollection<MySong>> FetchSongsAsync(string query, int count)
		{
			IEnumerable<Song> songs = await _youtubeMusicClient.SearchAsync<Song>(query, count);
			var mySongs = await Task.WhenAll(songs.Select(async song => MySong.Create(song)));
			return new ObservableCollection<MySong>(mySongs);
		}

		/// <summary>
		/// Fetches a single song based on a query.
		/// </summary>
		/// <param name="query">The search query.</param>
		/// <returns>A <see cref="MySong"/> object if found; otherwise, null.</returns>
		public static async Task<MySong?> FetchSongAsync(string query)
		{
			IEnumerable<Song> songs = await _youtubeMusicClient.SearchAsync<Song>(query, 1);
			if (!songs.Any()) return null;

			return MySong.Create(songs.FirstOrDefault());
		}

		/// <summary>
		/// Retrieves the audio stream URL for a given song ID.
		/// </summary>
		/// <param name="songId">The ID of the song.</param>
		/// <returns>The URL of the audio stream.</returns>
		public async Task<string?> GetAudioStreamUrlAsync(string songId)
		{
			var videoId = YoutubeExplode.Videos.VideoId.Parse(songId);
			var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);
			var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
			return audioStreamInfo?.Url;
		}

		/// <summary>
		/// Fetches all playlists associated with the authenticated user.
		/// </summary>
		/// <returns>An <see cref="ObservableCollection{T}"/> containing the playlists.</returns>
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
				MessageBox.Show($"An error occurred: {ex.Message}");
			}

			return new ObservableCollection<Playlist>(playlists);
		}

		/// <summary>
		/// Fetches video information for a given song ID.
		/// </summary>
		/// <param name="songId">The ID of the song.</param>
		/// <returns>A <see cref="SongVideoInfo"/> object containing video information.</returns>
		public static async Task<SongVideoInfo> FetchSongVideoInfoAsync(string songId)
		{
			return await _youtubeMusicClient.GetSongVideoInfoAsync(songId);
		}

		/// <summary>
		/// Fetches the content of a playlist given its ID.
		/// </summary>
		/// <param name="playlistId">The ID of the playlist.</param>
		/// <returns>An <see cref="ObservableCollection{T}"/> containing the songs in the playlist, or null if an error occurs.</returns>
		public async Task<ObservableCollection<MySong>?> FetchPlaylistContentAsync(string playlistId)
		{
			try
			{
				var request = _googleYouTubeService.PlaylistItems.List("snippet,contentDetails");
				request.PlaylistId = playlistId;
				request.MaxResults = 200;

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
				MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}
		}

		/// <summary>
		/// Creates a new playlist on YouTube with the specified title, description, and privacy status.
		/// </summary>
		/// <param name="title">The title of the new playlist.</param>
		/// <param name="description">The description of the new playlist. Defaults to an empty string if not provided.</param>
		/// <param name="privacyStatus">The privacy status of the playlist ("public" or "private").</param>
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

		/// <summary>
		/// Deletes a playlist from YouTube based on the given playlist ID.
		/// </summary>
		/// <param name="playlistId">The ID of the playlist to be deleted.</param>
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

		/// <summary>
		/// Adds a song to a playlist on YouTube.
		/// </summary>
		/// <param name="playlist">The playlist to which the song will be added.</param>
		/// <param name="song">The song to add to the playlist.</param>
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

		/// <summary>
		/// Removes a song from a playlist on YouTube.
		/// </summary>
		/// <param name="playlist">The playlist from which the song will be removed.</param>
		/// <param name="song">The song to remove from the playlist.</param>
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

		/// <summary>
		/// Fetches popular songs from YouTube, with an optional next page token for pagination.
		/// </summary>
		/// <param name="nextPageToken">The continuation token for pagination.</param>
		/// <returns>A <see cref="MyShelf{T}"/> containing popular songs and a continuation token.</returns>
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

		/// <summary>
		/// Fetches songs from a radio playlist on YouTube Music based on its ID.
		/// </summary>
		/// <param name="radioPlaylistId">The ID of the radio playlist.</param>
		/// <returns>An <see cref="ObservableCollection{T}"/> containing the songs in the radio playlist, or null if an error occurs.</returns>
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
				MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			return null;
		}

		/// <summary>
		/// Converts a list of <see cref="Song"/> objects to a <see cref="MyShelf{T}"/> containing <see cref="MySong"/> objects.
		/// </summary>
		/// <param name="songs">The list of songs to convert.</param>
		/// <param name="nextPageToken">The continuation token for pagination.</param>
		/// <returns>A <see cref="MyShelf{T}"/> containing the converted songs and a continuation token.</returns>
		private async Task<MyShelf<MySong>> ConvertSongsToMySongsShelf(IEnumerable<Song> songs, string? nextPageToken)
		{
			var mySongs = await Task.WhenAll(songs.Select(async song => MySong.Create(song)));
			return new MyShelf<MySong>(new ObservableCollection<MySong>(mySongs), nextPageToken);
		}

		/// <summary>
		/// Converts a collection of items to a list of <see cref="Song"/> objects based on a title extraction function.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="items">The collection of items to convert.</param>
		/// <param name="getTitle">A function to extract the title from each item.</param>
		/// <returns>A list of <see cref="Song"/> objects.</returns>
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

		/// <summary>
		/// Converts a <see cref="VideoListResponse"/> to a list of <see cref="Song"/> objects.
		/// </summary>
		/// <param name="response">The response containing the video list.</param>
		/// <returns>A list of <see cref="Song"/> objects.</returns>
		private async Task<List<Song>> ConvertToSongs(VideoListResponse response)
		{
			return await ConvertToSongs(response.Items, video => video.Snippet.Title);
		}

		/// <summary>
		/// Converts a <see cref="PlaylistItemListResponse"/> to a list of <see cref="Song"/> objects.
		/// </summary>
		/// <param name="response">The response containing the playlist item list.</param>
		/// <returns>A list of <see cref="Song"/> objects.</returns>
		private async Task<List<Song>> ConvertToSongs(PlaylistItemListResponse response)
		{
			return await ConvertToSongs(response.Items, item => item.Snippet.Title);
		}
	}
}
