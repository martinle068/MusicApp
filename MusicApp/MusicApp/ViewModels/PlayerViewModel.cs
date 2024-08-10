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


namespace MusicApp.ViewModels
{
	public class PlayerViewModel : BaseViewModel
	{
		private readonly MainViewModel _mainViewModel;
		private MyYouTubeService _youTubeService;
		private LibVLC _libVLC;
		private MediaPlayer _mediaPlayer;
		private DispatcherTimer _timer;
		private bool _isDragging;
		private bool _isPlaying;
		private List<MySong> _songs;
		private MySong? _selectedSong;
		private string _currentSongName;
		private string _currentArtistName;
		private string _currentTime;
		private double _totalTime;
		private string _formattedTotalTime;
		private double _sliderValue;
		private ImageSource _currentSongThumbnail;
		private string _playPauseText;
		private int _selectedSongIndex = -1;
		private bool _isSongSelected;

		public List<MySong> Songs
		{
			get => _songs;
			set => SetProperty(ref _songs, value);
		}

		public MySong? SelectedSong
		{
			get => _selectedSong;
			set => SetProperty(ref _selectedSong, value);
		}

		public int SelectedSongIndex
		{
			get => _selectedSongIndex;
			set
			{
				if (SetProperty(ref _selectedSongIndex, value))
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

		public string CurrentSongName
		{
			get => _currentSongName;
			set => SetProperty(ref _currentSongName, value);
		}

		public string CurrentArtistName
		{
			get => _currentArtistName;
			set => SetProperty(ref _currentArtistName, value);
		}

		public string CurrentTime
		{
			get => _currentTime;
			set => SetProperty(ref _currentTime, value);
		}

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

		public string FormattedTotalTime
		{
			get => _formattedTotalTime;
			set => SetProperty(ref _formattedTotalTime, value);
		}

		public double SliderValue
		{
			get => _sliderValue;
			set => SetProperty(ref _sliderValue, value);
		}

		public ImageSource CurrentSongThumbnail
		{
			get => _currentSongThumbnail;
			set => SetProperty(ref _currentSongThumbnail, value);
		}

		public string PlayPauseText
		{
			get => _playPauseText;
			set => SetProperty(ref _playPauseText, value);
		}

		public bool IsPlaying
		{
			get => _isPlaying;
			set => SetProperty(ref _isPlaying, value);
		}

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

			Songs = new List<MySong>();

			PlayPauseText = "Play";
			IsSongSelected = false;

			BackCommand = new RelayCommand(ExecuteBack);
			PlayPauseCommand = new RelayCommand(ExecutePlayPause);
			NextCommand = new RelayCommand(ExecuteNext);
			PreviousCommand = new RelayCommand(ExecutePrevious);
			ShuffleCommand = new RelayCommand(ExecuteShuffleSongs);
			AddSongToPlaylistCommand = new RelayCommand(ExecuteAddSongToPlaylist);
		}


		private void ExecuteAddSongToPlaylist(object parameter)
		{
			AddSongToPlaylist(parameter);
		}

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


		private void ExecuteShuffleSongs(object parameter)
		{
			if (SelectedSong != null)
			{
				Songs.Shuffle();
				Songs = new(Songs);
				SelectedSongIndex = Songs.IndexOf(SelectedSong);
			}
		}

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

		private void ExecuteNext(object parameter)
		{
			if (!IsSongSelected)
				return;

			if (SelectedSongIndex < Songs.Count - 1)
			{
				SelectedSongIndex++;
			}
		}

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

		private void ExecuteBack(object parameter)
		{
			_mainViewModel.IsMiniPlayerVisible = true;
			_mainViewModel.NavigateBack();
		}

		public async void PlaySelectedSong()
		{
			if (SelectedSong == null)
			{
				IsSongSelected = false;
				return;
			}

			IsSongSelected = true;
			string songId = SelectedSong.Id;

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
				CurrentArtistName = SelectedSong.Artists.First().Name;
				await DisplayPicture();

				_timer.Start();
				PlayPauseText = "Pause";
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}");
			}
		}

		private void MediaPlayer_Playing(object sender, EventArgs e)
		{
			if (_mediaPlayer.Media != null && _mediaPlayer.Media.Duration > 0)
			{
				TotalTime = _mediaPlayer.Media.Duration / 1000.0;
				SliderValue = 0;
			}
		}

		private void MediaPlayer_EndReached(object sender, EventArgs e)
		{
			ExecuteNext(null);
		}

		private void MediaPlayer_EncounteredError(object sender, EventArgs e)
		{
			MessageBox.Show("Media playback encountered an error.");
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			if (!_isDragging && _mediaPlayer.Media != null)
			{
				SliderValue = _mediaPlayer.Time / 1000.0;
				CurrentTime = TimeSpan.FromMilliseconds(_mediaPlayer.Time).ToString(@"mm\:ss");
			}
		}

		public void TrackBarSeek_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (_mediaPlayer.Media != null)
			{
				_isDragging = true;
			}
		}

		public void TrackBarSeek_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (_mediaPlayer.Media != null)
			{
				_isDragging = false;
				_mediaPlayer.Time = (long)SliderValue * 1000;
			}
		}

		public void TrackBarSeek_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
		{
			if (_isDragging && _mediaPlayer.Media != null)
			{
				CurrentTime = TimeSpan.FromSeconds(SliderValue).ToString(@"mm\:ss");
			}
		}

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
