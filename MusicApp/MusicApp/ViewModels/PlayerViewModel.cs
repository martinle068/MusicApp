using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LibVLCSharp.Shared;
using MusicApp.Commands;
using MusicApp.Models;
using MusicApp.Utils;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
using YoutubeExplode;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using MusicApp.Views;
using System.Collections.ObjectModel;
using static MusicApp.Utils.Utils;
using Newtonsoft.Json.Linq;
using System.Runtime;


namespace MusicApp.ViewModels
{
	/// <summary>
	/// ViewModel for managing the music player, handling song playback, control commands, and UI bindings.
	/// </summary>
	public class PlayerViewModel : BaseViewModel
	{
		private readonly MainViewModel _mainViewModel;
		private MyYouTubeService _youTubeService;
		private LibVLC _libVLC;
		private MediaPlayer _mediaPlayer;
		private DispatcherTimer _timer;
		private bool _isDragging;
		private bool _isPlaying;
		private ObservableCollection<MySong> _songs = new();
		private MySong? _selectedSong;
		private string? _currentSongName;
		private string? _currentArtistName;
		private string? _currentTime;
		private double _totalTime;
		private string? _formattedTotalTime;
		private double _sliderValue;
		private ImageSource? _currentSongThumbnail;
		private string? _playPauseText;
		private int _selectedSongIndex = -1;
		private bool _isSongSelected;
		private string? _infoText;
		private string? _continuationToken;
		private string? _mixId;

		/// <summary>
		/// Gets or sets the continuation token for fetching more songs in a playlist or search result.
		/// </summary>
		public string? ContinuationToken
		{
			get => _continuationToken;
			set => SetProperty(ref _continuationToken, value);
		}

		/// <summary>
		/// Gets or sets the informational text displayed in the player (e.g., current playlist or radio station name).
		/// </summary>
		public string? InfoText
		{
			get => _infoText;
			set => SetProperty(ref _infoText, value);
		}

		/// <summary>
		/// Gets or sets the collection of songs currently loaded in the player.
		/// </summary>
		public ObservableCollection<MySong> Songs
		{
			get => _songs;
			set
			{
				SetProperty(ref _songs, value);
				GC.Collect();
			}
		}

		/// <summary>
		/// Gets or sets the currently selected song in the player.
		/// </summary>
		public MySong? SelectedSong
		{
			get => _selectedSong;
			set => SetProperty(ref _selectedSong, value);
		}

		/// <summary>
		/// Gets or sets the index of the currently selected song in the song collection.
		/// </summary>
		public int SelectedSongIndex
		{
			get => _selectedSongIndex;
			set
			{
				if (SetProperty(ref _selectedSongIndex, value))
				{
					if (value != -1)
					{
						var currentSong = SelectedSong;
						SelectedSong = Songs?.ElementAtOrDefault(value);

						if (SelectedSong != null && currentSong != SelectedSong)
						{
							PlaySelectedSong();
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the name of the current song being played.
		/// </summary>
		public string? CurrentSongName
		{
			get => _currentSongName;
			set => SetProperty(ref _currentSongName, value);
		}

		/// <summary>
		/// Gets or sets the name of the current artist being played.
		/// </summary>
		public string? CurrentArtistName
		{
			get => _currentArtistName;
			set => SetProperty(ref _currentArtistName, value);
		}

		/// <summary>
		/// Gets or sets the current playback time of the song in "mm:ss" format.
		/// </summary>
		public string? CurrentTime
		{
			get => _currentTime;
			set => SetProperty(ref _currentTime, value);
		}

		/// <summary>
		/// Gets or sets the total duration of the current song in seconds.
		/// </summary>
		public double TotalTime
		{
			get => _totalTime;
			set
			{
				if (SetProperty(ref _totalTime, value))
				{
					FormattedTotalTime = TimeSpan.FromSeconds(_totalTime).ToString(@"mm\:ss");
				}
			}
		}

		/// <summary>
		/// Gets or sets the total time of the current song formatted as "mm:ss".
		/// </summary>
		public string? FormattedTotalTime
		{
			get => _formattedTotalTime;
			set => SetProperty(ref _formattedTotalTime, value);
		}

		/// <summary>
		/// Gets or sets the value of the slider that tracks song playback.
		/// </summary>
		public double SliderValue
		{
			get => _sliderValue;
			set => SetProperty(ref _sliderValue, value);
		}

		/// <summary>
		/// Gets or sets the thumbnail image of the current song.
		/// </summary>
		public ImageSource? CurrentSongThumbnail
		{
			get => _currentSongThumbnail;
			set => SetProperty(ref _currentSongThumbnail, value);
		}

		/// <summary>
		/// Gets or sets the text displayed on the Play/Pause button.
		/// </summary>
		public string? PlayPauseText
		{
			get => _playPauseText;
			set => SetProperty(ref _playPauseText, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether a song is currently playing.
		/// </summary>
		public bool IsPlaying
		{
			get => _isPlaying;
			set => SetProperty(ref _isPlaying, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether a song is currently selected.
		/// </summary>
		public bool IsSongSelected
		{
			get => _isSongSelected;
			set
			{
				if (SetProperty(ref _isSongSelected, value))
				{
					CommandManager.InvalidateRequerySuggested();
				}
			}
		}

		public ICommand BackCommand { get; }
		public ICommand PlayPauseCommand { get; }
		public ICommand NextCommand { get; }
		public ICommand PreviousCommand { get; }
		public ICommand ShuffleCommand { get; }
		public ICommand AddSongToPlaylistCommand { get; }
		public ICommand RemoveSongFromPlaylistCommand { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PlayerViewModel"/> class, setting up the VLC player, commands, and event handlers.
		/// </summary>
		public PlayerViewModel(MainViewModel mainViewModel, MyYouTubeService ys)
		{
			_mainViewModel = mainViewModel;
			_youTubeService = ys;

			Core.Initialize();
			_libVLC = new LibVLC();
			_mediaPlayer = new MediaPlayer(_libVLC);

			_mediaPlayer.Playing += MediaPlayer_Playing;
			_mediaPlayer.EndReached += MediaPlayer_EndReached;
			_mediaPlayer.EncounteredError += MediaPlayer_EncounteredError;

			_timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(1)
			};
			_timer.Tick += Timer_Tick;

			Songs = new ObservableCollection<MySong>();

			PlayPauseText = "Play";
			IsSongSelected = false;

			BackCommand = new RelayCommand(ExecuteBack);
			PlayPauseCommand = new RelayCommand(ExecutePlayPause);
			NextCommand = new RelayCommand(ExecuteNext);
			PreviousCommand = new RelayCommand(ExecutePrevious);
			ShuffleCommand = new RelayCommand(ExecuteShuffleSongs);
			AddSongToPlaylistCommand = new RelayCommand(ExecuteAddSongToPlaylist);
			RemoveSongFromPlaylistCommand = new RelayCommand(ExecuteRemoveSongFromPlaylist);
		}

		/// <summary>
		/// Resets the selected song index to its default value (-1).
		/// </summary>
		public void ResetIndices()
		{
			SelectedSongIndex = -1;
		}

		/// <summary>
		/// Provides the player with a new set of songs, selects the specified song, and updates the UI with the provided information text.
		/// </summary>
		/// <param name="songs">The collection of songs to load into the player.</param>
		/// <param name="index">The index of the song to start playing.</param>
		/// <param name="text">The informational text to display.</param>
		/// <param name="continuationToken">Optional. A token for fetching more songs if available.</param>
		public void ProvidePlayerInfo(ObservableCollection<MySong> songs, int index, string text, string? continuationToken = null)
		{
			Songs = songs;
			SelectedSongIndex = -1;
			SelectedSongIndex = index;
			SelectedSong = Songs?.ElementAtOrDefault(index);
			InfoText = text;
			ContinuationToken = continuationToken;
		}

		/// <summary>
		/// Removes a song from the currently selected playlist, with a confirmation prompt.
		/// </summary>
		private void ExecuteRemoveSongFromPlaylist(object parameter)
		{
			RemoveSongFromPlaylist(parameter);
		}

		/// <summary>
		/// Asynchronously removes the specified song from the currently selected playlist.
		/// </summary>
		private async void RemoveSongFromPlaylist(object parameter)
		{
			if (_mainViewModel.CurrentMusicSource != MainViewModel.MusicSource.Playlist)
				return;

			if (parameter is MySong song && _mainViewModel.HomeViewModel.SelectedPlaylist != null)
			{
				var selectedPlaylist = _mainViewModel.HomeViewModel.SelectedPlaylist;

				var confirmation = MessageBox.Show($"Are you sure you want to remove '{song.ArtistAndSongName}' from '{selectedPlaylist.Snippet.Title}'?",
												   "Confirm Removal", MessageBoxButton.YesNo);

				if (confirmation == MessageBoxResult.Yes)
				{
					await _mainViewModel.MyYouTubeService.RemoveSongFromPlaylist(selectedPlaylist, song);

					Songs.Remove(song);
					Songs = new(Songs);
				}
			}
			else
			{
				MessageBox.Show("Song or playlist is null");
			}
		}

		/// <summary>
		/// Adds a song to a selected playlist.
		/// </summary>
		private void ExecuteAddSongToPlaylist(object parameter)
		{
			AddSongToPlaylist(parameter);
		}

		/// <summary>
		/// Asynchronously adds the specified song to a selected playlist, allowing the user to choose the target playlist.
		/// </summary>
		private async void AddSongToPlaylist(object parameter)
		{
			if (parameter is MySong song)
			{
				var dialog = new PlaylistSelectionDialog(_mainViewModel.HomeViewModel.Playlists);
				var selectedPlaylist = dialog.SelectPlaylist();

				if (selectedPlaylist != null)
				{
					await _mainViewModel.MyYouTubeService.AddSongToPlaylist(selectedPlaylist, song);

					if (_mainViewModel.HomeViewModel.SelectedPlaylist == selectedPlaylist)
					{
						var result = await _mainViewModel.MyYouTubeService.FetchPlaylistContentAsync(selectedPlaylist.Id);
						if (result != null)
						{
							Songs = result;
							SelectedSongIndex = -1;
							SelectedSongIndex = Songs.IndexOf(song);
						}
					}
				}
				else
				{
					MessageBox.Show("No playlist selected");
				}
			}
			else
			{
				MessageBox.Show("Song is null");
			}
		}

		/// <summary>
		/// Shuffles the current list of songs, while keeping the selected song in its position.
		/// </summary>
		private void ExecuteShuffleSongs(object parameter)
		{
			if (SelectedSong != null)
			{
				var selectedSong = SelectedSong;
				Songs.Shuffle();
				SelectedSongIndex = Songs.IndexOf(selectedSong);
			}
		}

		/// <summary>
		/// Toggles between playing and pausing the current song.
		/// </summary>
		private void ExecutePlayPause(object parameter)
		{
			if (!IsSongSelected)
				return;

			if (_isPlaying)
			{
				_mediaPlayer.Pause();
				PlayPauseText = "Play";
				_timer.Stop();
			}
			else
			{
				_mediaPlayer.Play();
				PlayPauseText = "Pause";
				_timer.Start();
			}
			IsPlaying = !IsPlaying;
		}

		/// <summary>
		/// Plays the next song in the list, or loads more songs if at the end of the current list.
		/// </summary>
		private async void ExecuteNext(object parameter)
		{
			if (!IsSongSelected)
				return;

			if (SelectedSongIndex >= 0 && SelectedSongIndex < Songs.Count - 1)
			{
				SelectedSongIndex++;
			}
			else if (_mainViewModel.CurrentMusicSource is MainViewModel.MusicSource.Search)
			{
				//var newShelf = await _youTubeService.FetchMixSongsAsync(_mixId, ContinuationToken);
				//ProcessNewShelf(newShelf);
				MessageBox.Show("Not implemented yet.");
			}
			else if (_mainViewModel.CurrentMusicSource is MainViewModel.MusicSource.Popular)
			{
				var newShelf = await _youTubeService.FetchPopularSongsAsync(ContinuationToken);
				ProcessNewShelf(newShelf);
			}
		}

		/// <summary>
		/// Processes a newly fetched shelf of songs, adding them to the current playlist.
		/// </summary>
		/// <param name="newShelf">The new shelf of songs to add to the playlist.</param>
		private void ProcessNewShelf(MyShelf<MySong>? newShelf)
		{
			if (newShelf != null)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					foreach (var song in newShelf.Items)
					{
						Songs.Add(song);
					}
				});
			}
			ContinuationToken = newShelf?.ContinuationToken;
			SelectedSongIndex++;
		}

		/// <summary>
		/// Plays the previous song in the list, or restarts the current song if at the beginning.
		/// </summary>
		private void ExecutePrevious(object parameter)
		{
			if (!IsSongSelected)
				return;

			if (SelectedSongIndex > 0)
			{
				SelectedSongIndex--;
			}
			else
			{
				_mediaPlayer.Time = 0;
			}
		}

		/// <summary>
		/// Navigates back to the previous view, making the mini player visible.
		/// </summary>
		private void ExecuteBack(object parameter)
		{
			_mainViewModel.IsMiniPlayerVisible = true;
			_mainViewModel.NavigateBack();
		}

		/// <summary>
		/// Plays the currently selected song, fetching its audio stream and metadata, and updating the UI.
		/// </summary>
		public async void PlaySelectedSong()
		{
			if (SelectedSong == null)
			{
				IsSongSelected = false;
				return;
			}

			IsSongSelected = true;
			string songId = SelectedSong.Id;

			_mainViewModel.SongDatabase?.UpdateSongCount(songId);

			try
			{
				var audioUrl = await _youTubeService.GetAudioStreamUrlAsync(songId);

				if (audioUrl == null)
				{
					MessageBox.Show("No audio stream found.");
					return;
				}

				_mediaPlayer.Media = new Media(_libVLC, new Uri(audioUrl));
				_mediaPlayer.Play();
				IsPlaying = true;

				CurrentSongName = SelectedSong.Name;
				CurrentArtistName = string.Join(", ", SelectedSong.Artists.Select(artist => artist.Name));
				await DisplayPicture();

				_timer.Start();
				PlayPauseText = "Pause";
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}");
			}
		}

		/// <summary>
		/// Handles the MediaPlayer Playing event, updating the total song duration and resetting the slider value.
		/// </summary>
		private void MediaPlayer_Playing(object sender, EventArgs e)
		{
			if (_mediaPlayer.Media != null && _mediaPlayer.Media.Duration > 0)
			{
				TotalTime = _mediaPlayer.Media.Duration / 1000.0;
				SliderValue = 0;
			}
		}

		/// <summary>
		/// Handles the MediaPlayer EndReached event, automatically playing the next song in the list.
		/// </summary>
		private void MediaPlayer_EndReached(object sender, EventArgs e)
		{
			NextCommand.Execute(null);
		}

		/// <summary>
		/// Handles the MediaPlayer EncounteredError event, displaying an error message if media playback fails.
		/// </summary>
		private void MediaPlayer_EncounteredError(object sender, EventArgs e)
		{
			MessageBox.Show("Media playback encountered an error.");
		}

		/// <summary>
		/// Handles the Timer Tick event, updating the slider value and current time display during playback.
		/// </summary>
		private void Timer_Tick(object sender, EventArgs e)
		{
			if (!_isDragging && _mediaPlayer.Media != null)
			{
				SliderValue = _mediaPlayer.Time / 1000.0;
				CurrentTime = TimeSpan.FromMilliseconds(_mediaPlayer.Time).ToString(@"mm\:ss");
			}
		}

		/// <summary>
		/// Handles the event when the user begins dragging the playback slider, pausing time updates.
		/// </summary>
		public void TrackBarSeek_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (_mediaPlayer.Media != null)
			{
				_isDragging = true;
			}
		}

		/// <summary>
		/// Handles the event when the user releases the playback slider, seeking to the new time in the song.
		/// </summary>
		public void TrackBarSeek_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (_mediaPlayer.Media != null)
			{
				_isDragging = false;
				_mediaPlayer.Time = (long)SliderValue * 1000;
			}
		}

		/// <summary>
		/// Handles the event when the playback slider value changes, updating the current time display.
		/// </summary>
		public void TrackBarSeek_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
		{
			if (_isDragging && _mediaPlayer.Media != null)
			{
				CurrentTime = TimeSpan.FromSeconds(SliderValue).ToString(@"mm\:ss");
			}
		}

		/// <summary>
		/// Asynchronously fetches and displays the thumbnail image for the currently selected song.
		/// </summary>
		private async Task DisplayPicture()
		{
			if (SelectedSong != null)
			{
				CurrentSongThumbnail = await ThumbnailHelper.GetHighQualityThumbnailAsync(SelectedSong.Id);
			}
			else
			{
				CurrentSongThumbnail = null;
			}
		}
	}
}
