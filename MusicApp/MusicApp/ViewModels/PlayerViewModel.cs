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
using YouTubeMusicAPI.Models;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.Windows;

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
		private ObservableCollection<Song> _songs;
		private Song _selectedSong;
		private string _currentSongName;
		private string _currentArtistName;
		private string _currentTime;
		private string _totalTime;
		private double _sliderValue;
		private ImageSource _currentSongThumbnail;
		private string _playPauseText;
		private string? _searchQuery = null;
		private ObservableCollection<string> _songList;
		private int _selectedSongIndex;

		public ObservableCollection<string> SongList
		{
			get => _songList;
			set => SetProperty(ref _songList, value);
		}

		public ObservableCollection<Song> Songs
		{
			get => _songs;
			set => SetProperty(ref _songs, value);
		}

		public Song SelectedSong
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
					SelectedSong = Songs.ElementAtOrDefault(value);
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

		public string TotalTime
		{
			get => _totalTime;
			set => SetProperty(ref _totalTime, value);
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

		public string SearchQuery
		{
			get => _searchQuery;
			set => SetProperty(ref _searchQuery, value);
		}

		public ICommand BackCommand { get; }
		public ICommand PlayPauseCommand { get; }
		public ICommand NextCommand { get; }
		public ICommand PreviousCommand { get; }
		public ICommand SearchCommand { get; }

		public PlayerViewModel(MainViewModel mainViewModel)
		{
			_mainViewModel = mainViewModel;
			_youTubeService = new YouTubeService();
			_mediaPlayer = new MediaPlayer();
			_mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
			_timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(1)
			};
			_timer.Tick += Timer_Tick;
			Songs = new ObservableCollection<Song>();
			_songList = new ObservableCollection<string>();

			PlayPauseText = "Play";

			BackCommand = new RelayCommand(ExecuteBack);
			PlayPauseCommand = new RelayCommand(ExecutePlayPause);
			NextCommand = new RelayCommand(ExecuteNext);
			PreviousCommand = new RelayCommand(ExecutePrevious);
			SearchCommand = new RelayCommand(async _ => await ExecuteSearch());

			if (!string.IsNullOrWhiteSpace(_mainViewModel.SearchQuery))
			{
				SearchQuery = _mainViewModel.SearchQuery;
				ExecuteSearchImmediate();
			}
		}

		private async void ExecuteSearchImmediate()
		{
			await ExecuteSearch();
		}

		private void ExecuteBack(object parameter)
		{
			_mainViewModel.SwitchToHomeView();
		}

		private void ExecutePlayPause(object parameter)
		{
			if (_isPlaying)
			{
				_mediaPlayer.Pause();
				_isPlaying = false;
				PlayPauseText = "Play";
			}
			else
			{
				_mediaPlayer.Play();
				_isPlaying = true;
				PlayPauseText = "Pause";
			}
		}

		private void ExecuteNext(object parameter)
		{
			if (SelectedSongIndex < Songs.Count - 1)
			{
				SelectedSongIndex++;
			}
		}

		private void ExecutePrevious(object parameter)
		{
			if (SelectedSongIndex > 0)
			{
				SelectedSongIndex--;
			}
			else
			{
				_mediaPlayer.Position = TimeSpan.Zero;
			}
		}

		private async Task ExecuteSearch()
		{
			string query = SearchQuery;
			if (string.IsNullOrWhiteSpace(query))
			{
				MessageBox.Show("Please enter a search query.");
				return;
			}

			try
			{
				var songs = await _youTubeService.FetchSongsAsync(query);
				if (!songs.Any())
				{
					MessageBox.Show("No songs found.");
					return;
				}

				Songs.Clear();
				SongList.Clear();

				foreach (var song in songs)
				{
					Songs.Add(song);
					SongList.Add($"{song.Artists.First().Name} - {song.Name}");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}");
			}
		}

		private async void PlaySelectedSong()
		{
			if (SelectedSong == null)
			{
				return;
			}

			string youtubeVideoUrl = SelectedSong.Id;

			try
			{
				var videoId = YoutubeExplode.Videos.VideoId.Parse(youtubeVideoUrl);
				var streamManifest = await new YoutubeClient().Videos.Streams.GetManifestAsync(videoId);
				var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

				if (audioStreamInfo == null)
				{
					MessageBox.Show("No audio stream found.");
					return;
				}

				_mediaPlayer.Open(new Uri(audioStreamInfo.Url));
				_mediaPlayer.Play();
				_isPlaying = true;

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

		private void MediaPlayer_MediaOpened(object sender, EventArgs e)
		{
			if (_mediaPlayer.NaturalDuration.HasTimeSpan)
			{
				SliderValue = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
				TotalTime = _mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
			}
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			if (!_isDragging && _mediaPlayer.Source != null)
			{
				SliderValue = _mediaPlayer.Position.TotalSeconds;
				CurrentTime = _mediaPlayer.Position.ToString(@"mm\:ss");
			}
		}

		private async Task DisplayPicture()
		{
			if (SelectedSong != null && SelectedSong.Thumbnails.Any())
			{
				var imageUrl = $"https://img.youtube.com/vi/{SelectedSong.Id}/maxresdefault.jpg";
				if (imageUrl != null)
				{
					CurrentSongThumbnail = await ThumbnailHelper.GetSquareThumbnailAsync(imageUrl);
				}
			}
			else
			{
				CurrentSongThumbnail = null;
			}
		}
	}
}
