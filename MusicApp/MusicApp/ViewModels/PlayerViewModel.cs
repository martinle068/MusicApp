using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MusicApp.Commands;
using MusicApp.Models;
using YouTubeMusicAPI.Client;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.Windows;
using YouTubeMusicAPI.Models;

namespace MusicApp.ViewModels
{
	public class PlayerViewModel : BaseViewModel
	{
		private readonly MainViewModel _mainViewModel;
		private YouTubeService _youTubeService;
		private MediaPlayer _mediaPlayer;
		private DispatcherTimer _timer;
		private bool _isDragging;
		private bool _isPlaying;
		private ObservableCollection<MySong> _songs;
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

		public ObservableCollection<MySong> Songs
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
					SelectedSong = Songs?.ElementAtOrDefault(value);
					if (SelectedSong != null)
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

		public bool IsSongLoaded
		{
			get => _mediaPlayer.Source != null;
		}

		public ICommand BackCommand { get; }
		public ICommand PlayPauseCommand { get; }
		public ICommand NextCommand { get; }
		public ICommand PreviousCommand { get; }

		public PlayerViewModel(MainViewModel mainViewModel, YouTubeService ys)
		{
			_mainViewModel = mainViewModel;
			_youTubeService = ys;
			_mediaPlayer = new MediaPlayer();
			_mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
			_mediaPlayer.MediaEnded += MediaPlayer_MediaEnded; // Subscribe to MediaEnded event
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
				_mediaPlayer.Position = TimeSpan.Zero;
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
			string youtubeVideoUrl = SelectedSong.Id;

			try
			{
				var videoId = YoutubeExplode.Videos.VideoId.Parse(youtubeVideoUrl);
				var audioUrl = await _youTubeService.GetAudioStreamUrlAsync(videoId);

				if (audioUrl == null)
				{
					MessageBox.Show("No audio stream found.");
					return;
				}

				_mediaPlayer.Open(new Uri(audioUrl));
				_mediaPlayer.Play();
				IsPlaying = true;

				CurrentSongName = SelectedSong.Name;
				CurrentArtistName = SelectedSong.Artists.First().Name;
				await DisplayPicture();

				_timer.Start();
				PlayPauseText = "Pause";

				// Show the mini player when a song starts playing
				//_mainViewModel.IsMiniPlayerVisible = true;
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
				TotalTime = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds; // Set total time in seconds
				SliderValue = 0; // Reset the slider value
				_mediaPlayer.Position = TimeSpan.Zero;
				_timer.Start(); // Start the timer
			}
		}

		private void MediaPlayer_MediaEnded(object sender, EventArgs e)
		{
			ExecuteNext(null);
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			if (!_isDragging && _mediaPlayer.Source != null)
			{
				SliderValue = _mediaPlayer.Position.TotalSeconds; // Update the slider value
				CurrentTime = _mediaPlayer.Position.ToString(@"mm\:ss");
			}
		}

		public void TrackBarSeek_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_mediaPlayer.Source != null)
			{
				_isDragging = true;
			}
		}

		public void TrackBarSeek_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_mediaPlayer.Source != null)
			{
				_isDragging = false;
				_mediaPlayer.Position = TimeSpan.FromSeconds(SliderValue);
			}
		}

		public void TrackBarSeek_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_isDragging && _mediaPlayer.Source != null)
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
