using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;

namespace MusicApp
{
	public partial class MainWindow : Window
	{
		private YouTubeMusicClient _youtubeMusicClient;
		private YoutubeClient _youtubeClient;
		private List<Song> _songs;
		private MediaPlayer _mediaPlayer;
		private DispatcherTimer _timer;
		private bool _isDragging;
		private bool _isPlaying;
		const string playSymbol = "play"; // ▶
		const string pauseSymbol = "pause"; // ⏸
		public MainWindow()
		{
			InitializeComponent();
			_youtubeMusicClient = new YouTubeMusicClient();
			_youtubeClient = new YoutubeClient();
			_songs = new List<Song>();
			_mediaPlayer = new MediaPlayer();
			_mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
			_timer = new DispatcherTimer();
			_timer.Interval = TimeSpan.FromSeconds(1);
			_timer.Tick += Timer_Tick;
		}

		private async void buttonSearch_Click(object sender, RoutedEventArgs e)
		{
			await SearchSongs();
		}

		private async void textBoxQuery_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				await SearchSongs();
				e.Handled = true;
			}
		}

		private async void listBoxResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (listBoxResults.SelectedItem != null)
			{
				await PlaySelectedSong();
			}
		}

		private async Task SearchSongs()
		{
			string query = textBoxQuery.Text;
			if (string.IsNullOrWhiteSpace(query))
			{
				MessageBox.Show("Please enter a search query.");
				return;
			}

			try
			{
				_songs = await FetchSongsAsync(query);
				if (_songs.Count == 0)
				{
					MessageBox.Show("No songs found.");
					return;
				}

				listBoxResults.Items.Clear();
				foreach (var song in _songs)
				{
					listBoxResults.Items.Add($"{song.Artists.First().Name} - {song.Name}");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}");
			}
		}

		private static async Task<List<Song>> FetchSongsAsync(string query)
		{
			YouTubeMusicClient client = new YouTubeMusicClient();
			IEnumerable<Song> searchResults = await client.SearchAsync<Song>(query);
			return searchResults.ToList();
		}

		private async void buttonPlayPause_Click(object sender, RoutedEventArgs e)
		{
			if (_mediaPlayer.Source == null)
			{
				if (listBoxResults.SelectedIndex != -1)
				{
					await PlaySelectedSong();
				}
				else
				{
					MessageBox.Show("Please select a song to play.");
				}
			}
			else
			{
				if (_isPlaying)
				{
					_mediaPlayer.Pause();
					buttonPlayPause.Content = playSymbol; // ▶
					_timer.Stop();
				}
				else
				{
					_mediaPlayer.Play();
					buttonPlayPause.Content = pauseSymbol; // ⏸
					_timer.Start();
				}
				_isPlaying = !_isPlaying;
			}
		}

		private async Task PlaySelectedSong()
		{
			if (listBoxResults.SelectedIndex == -1)
			{
				return;
			}

			var selectedSong = _songs[listBoxResults.SelectedIndex];
			string youtubeVideoUrl = selectedSong.Id;

			try
			{
				var videoId = YoutubeExplode.Videos.VideoId.Parse(youtubeVideoUrl);
				var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);
				var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

				if (audioStreamInfo == null)
				{
					MessageBox.Show("No audio stream found.");
					return;
				}

				_mediaPlayer.Open(new Uri(audioStreamInfo.Url));
				_mediaPlayer.Play();
				_isPlaying = true;

				labelCurrentSong.Content = selectedSong.Name;
				labelArtist.Content = selectedSong.Artists.First().Name;

				DisplayPicture();

				_timer.Start();
				buttonPlayPause.Content = pauseSymbol; // ⏸
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}");
			}
		}

		private void MediaPlayer_MediaOpened(object sender, EventArgs e)
		{
			if (_mediaPlayer.NaturalDuration.HasTimeSpan)
			{
				trackBarSeek.Maximum = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
				labelTotalTime.Content = _mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
			}
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			if (!_isDragging && _mediaPlayer.Source != null)
			{
				trackBarSeek.Value = _mediaPlayer.Position.TotalSeconds;
				labelCurrentTime.Content = _mediaPlayer.Position.ToString(@"mm\:ss");
			}
		}

		private void trackBarSeek_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_mediaPlayer.Source != null)
			{
				_isDragging = true;
			}
		}

		private void trackBarSeek_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_mediaPlayer.Source != null)
			{
				_isDragging = false;
				_mediaPlayer.Position = TimeSpan.FromSeconds(trackBarSeek.Value);
			}
		}

		private void trackBarSeek_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_isDragging && _mediaPlayer.Source != null)
			{
				labelCurrentTime.Content = TimeSpan.FromSeconds(trackBarSeek.Value).ToString(@"mm\:ss");
			}
		}

		private void buttonPrevious_Click(object sender, RoutedEventArgs e)
		{
			if (listBoxResults.SelectedIndex > 0)
			{
				listBoxResults.SelectedIndex--;
			}
			else
			{
				_mediaPlayer.Position = TimeSpan.Zero;
			}
		}

		private async void buttonNext_Click(object sender, RoutedEventArgs e)
		{
			if (listBoxResults.SelectedIndex < listBoxResults.Items.Count - 1)
			{
				listBoxResults.SelectedIndex++;
				await PlaySelectedSong();
			}
		}

		private void buttonStop_Click(object sender, RoutedEventArgs e)
		{
			_mediaPlayer.Stop();
			_timer.Stop();
			labelCurrentTime.Content = "00:00";
			labelTotalTime.Content = "00:00";
			trackBarSeek.Value = 0;
			buttonPlayPause.Content = "U+25B6"; // ▶
			_isPlaying = false;
		}

		private async void DisplayPicture()
		{
			if (listBoxResults.SelectedIndex != -1)
			{
				var selectedSong = _songs[listBoxResults.SelectedIndex];
				if (selectedSong.Thumbnails != null && selectedSong.Thumbnails.Any())
				{
					var imageUrl = $"https://img.youtube.com/vi/{selectedSong.Id}/maxresdefault.jpg";
					Trace.WriteLine(imageUrl);
					if (imageUrl != null)
					{
						pictureBoxSong.Source = await ThumbnailHelper.GetSquareThumbnailAsync(imageUrl);
					}
				}
				else
				{
					pictureBoxSong.Source = null;
				}
			}
			else
			{
				pictureBoxSong.Source = null;
			}
		}
	}
}
