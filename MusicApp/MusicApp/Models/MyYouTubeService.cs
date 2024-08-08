﻿using System.Collections.Generic;
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
				var playlistSongs = new List<MySong>();
				var request = _googleYouTubeService.PlaylistItems.List("snippet,contentDetails");
				request.PlaylistId = playlistId;
				request.MaxResults = 200; // Maximum number of items to retrieve per request

				var response = await request.ExecuteAsync();

				foreach (var item in response.Items)
				{
					var itemInfo = await _youtubeMusicClient.GetSongVideoInfoAsync(item.ContentDetails.VideoId);

					var song = await MySong.CreateAsync(itemInfo);
					playlistSongs.Add(song);
				}

				return playlistSongs; 
			}
			catch (GoogleApiException ex)
			{
				Console.WriteLine($"An API error occurred: {ex.Message}");
				return null;

			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				return null;
			}
		}
	}
}
