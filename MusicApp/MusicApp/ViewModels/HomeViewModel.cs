using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Google.Apis.YouTube.v3.Data;
using MusicApp.Commands;
using MusicApp.Models;
using MusicApp.Views;
using static MusicApp.Utils.Utils;
using System.Linq;

namespace MusicApp.ViewModels
{
	/// <summary>
	/// ViewModel for the HomeView, responsible for managing playlists, popular songs, and handling user interactions.
	/// </summary>
	public class HomeViewModel : BaseViewModel
	{
		public readonly MainViewModel _mainViewModel;
		private string _searchQuery = string.Empty;
		private ObservableCollection<Playlist> _playlists = new();
		private Playlist _selectedPlaylist = new();
		private int _selectedPlaylistIndex = -1;
		private MyShelf<MySong> _popularSongs = new MyShelf<MySong>(new(), null);
		private int _selectedPopularSongIndex = -1;
		private string? _popularSongsContinuationToken;

		private ObservableCollection<MySong> _allPlaylistSongs = new();

		/// <summary>
		/// Gets or sets the collection of all songs from all playlists.
		/// </summary>
		public ObservableCollection<MySong> AllPlaylistSongs
		{
			get => _allPlaylistSongs;
			set => SetProperty(ref _allPlaylistSongs, value);
		}

		/// <summary>
		/// Resets the selected indices for playlists and popular songs.
		/// </summary>
		public void ResetIndices()
		{
			SelectedPlaylistIndex = -1;
			SelectedPopularSongIndex = -1;
		}

		/// <summary>
		/// Gets or sets the index of the selected popular song.
		/// </summary>
		public int SelectedPopularSongIndex
		{
			get => _selectedPopularSongIndex;
			set
			{
				SetProperty(ref _selectedPopularSongIndex, value);
			}
		}

		/// <summary>
		/// Gets or sets the collection of popular songs.
		/// </summary>
		public MyShelf<MySong> PopularSongs
		{
			get => _popularSongs;
			set => SetProperty(ref _popularSongs, value);
		}

		/// <summary>
		/// Gets or sets the selected playlist.
		/// </summary>
		public Playlist SelectedPlaylist
		{
			get => _selectedPlaylist;
			set => SetProperty(ref _selectedPlaylist, value);
		}

		/// <summary>
		/// Gets or sets the index of the selected playlist.
		/// </summary>
		public int SelectedPlaylistIndex
		{
			get => _selectedPlaylistIndex;
			set
			{
				SetProperty(ref _selectedPlaylistIndex, value);
				if (value >= 0 && value < Playlists.Count)
				{
					SelectedPlaylist = Playlists[value];
				}
			}
		}

		/// <summary>
		/// Gets or sets the search query string.
		/// </summary>
		public string SearchQuery
		{
			get => _searchQuery;
			set => SetProperty(ref _searchQuery, value);
		}

		/// <summary>
		/// Gets or sets the collection of playlists.
		/// </summary>
		public ObservableCollection<Playlist> Playlists
		{
			get => _playlists;
			set => SetProperty(ref _playlists, value);
		}

		public ICommand SearchCommand { get; }
		public ICommand AddPlaylistCommand { get; }
		public ICommand SelectPlaylistCommand { get; }
		public ICommand SelectPopularSongCommand { get; }
		public ICommand DeletePlaylistCommand { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="HomeViewModel"/> class.
		/// </summary>
		/// <param name="mainViewModel">The main ViewModel instance.</param>
		public HomeViewModel(MainViewModel mainViewModel)
		{
			_mainViewModel = mainViewModel;
			SearchCommand = new RelayCommand(async _ => await ExecuteSearch());
			AddPlaylistCommand = new RelayCommand(OpenAddPlaylistDialog);
			SelectPlaylistCommand = new RelayCommand(async _ => await HandlePlaylistSelectionAsync());
			SelectPopularSongCommand = new RelayCommand(_ => HandlePopularSongSelection());
			DeletePlaylistCommand = new RelayCommand(ExecuteDeletePlaylist);
			LoadData();
		}

		/// <summary>
		/// Loads playlists and popular songs asynchronously.
		/// </summary>
		private async void LoadData()
		{
			var loadPlaylistsTask = LoadPlaylists();
			var loadPopularSongsTask = LoadPopularSongs();

			// Start loading both playlists and popular songs concurrently
			await Task.WhenAll(loadPlaylistsTask, loadPopularSongsTask);
			await LoadAllPlaylistSongs();
		}

		/// <summary>
		/// Loads popular songs asynchronously.
		/// </summary>
		private async Task LoadPopularSongs()
		{
			var popularSongs = await _mainViewModel.MyYouTubeService.FetchPopularSongsAsync(null);
			PopularSongs = popularSongs;
		}

		/// <summary>
		/// Loads playlists asynchronously.
		/// </summary>
		private async Task LoadPlaylists()
		{
			var playlists = await _mainViewModel.MyYouTubeService.FetchAllPlaylistsAsync();
			Playlists = playlists;
		}

		/// <summary>
		/// Loads all songs from all playlists asynchronously.
		/// </summary>
		private async Task LoadAllPlaylistSongs()
		{
			var allSongs = new ObservableCollection<MySong>();

			var tasks = Playlists.Select(async playlist =>
			{
				var playlistItems = await _mainViewModel.MyYouTubeService.FetchPlaylistContentAsync(playlist.Id);
				if (playlistItems != null)
				{
					foreach (var song in playlistItems)
					{
						Application.Current.Dispatcher.Invoke(() => allSongs.Add(song));
						_mainViewModel.SongDatabase?.InsertSong(song.Id);
					}
				}
			});

			await Task.WhenAll(tasks);

			AllPlaylistSongs = allSongs;
		}

		/// <summary>
		/// Handles the selection of a playlist asynchronously.
		/// </summary>
		private async Task HandlePlaylistSelectionAsync()
		{
			try
			{
				if (Playlists.ElementAtOrDefault(SelectedPlaylistIndex) is Playlist playlist)
				{
					var playlistItems = await _mainViewModel.MyYouTubeService.FetchPlaylistContentAsync(playlist.Id);
					if (playlistItems == null)
					{
						MessageBox.Show("Failed to fetch playlist content.");
						return;
					}
					else if (playlistItems.Count == 0)
					{
						MessageBox.Show("This playlist is empty.");
						return;
					}

					_mainViewModel.ResetIndices();
					_mainViewModel.CurrentMusicSource = MainViewModel.MusicSource.Playlist;
					_mainViewModel.PlayerViewModel.ProvidePlayerInfo(playlistItems, 0, GetInfoString(playlist.Snippet.Title));
					_mainViewModel.SwitchToPlayerView();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		/// <summary>
		/// Handles the selection of a radio song asynchronously.
		/// </summary>
		/// <param name="Songs">The collection of songs to play.</param>
		/// <param name="index">The index of the selected song.</param>
		public async Task HandleRadioSongSelection(ObservableCollection<MySong> Songs, int index)
		{
			if (Songs.ElementAtOrDefault(index) is MySong song and not null)
			{
				var newSongs = new ObservableCollection<MySong>() { song };
				var radioPlaylistId = await song.GetPlaylistIdAsync();
				var radioSongs = await _mainViewModel.MyYouTubeService.FetchRadioSongsAsync(radioPlaylistId);
				_mainViewModel.ResetIndices();
				_mainViewModel.CurrentMusicSource = MainViewModel.MusicSource.Search;

				if (radioSongs == null)
				{
					MessageBox.Show("No songs found.");
					return;
				}

				foreach (var item in radioSongs)
				{
					newSongs.Add(item);
				}

				_mainViewModel.PlayerViewModel.ProvidePlayerInfo(newSongs, 0, GetInfoString($"Radio {song.Name}"));
				_mainViewModel.SwitchToPlayerView();
			}
		}

		/// <summary>
		/// Handles the selection of a popular song.
		/// </summary>
		private void HandlePopularSongSelection()
		{
			if (PopularSongs.Items.ElementAtOrDefault(SelectedPopularSongIndex) is MySong song)
			{
				_mainViewModel.CurrentMusicSource = MainViewModel.MusicSource.Popular;
				_mainViewModel.PlayerViewModel.ProvidePlayerInfo(PopularSongs.Items, SelectedPopularSongIndex, GetInfoString("Popular Songs"), _popularSongsContinuationToken);
				_mainViewModel.SwitchToPlayerView();
			}
		}

		/// <summary>
		/// Executes the search command asynchronously.
		/// </summary>
		private async Task ExecuteSearch()
		{
			if (!string.IsNullOrWhiteSpace(SearchQuery))
			{
				_mainViewModel.SearchViewModel.SearchQuery = SearchQuery;
				await _mainViewModel.SearchAndNavigateAsync(SearchQuery);
			}
		}

		/// <summary>
		/// Opens the Add Playlist dialog.
		/// </summary>
		/// <param name="parameter">The command parameter.</param>
		private void OpenAddPlaylistDialog(object parameter)
		{
			var dialog = new AddPlaylistDialog();
			if (dialog.ShowDialog() == true)
			{
				AddNewPlaylist(dialog.PlaylistName, dialog.PlaylistDescription, dialog.PlaylistVisibility == "Public");
			}
		}

		/// <summary>
		/// Adds a new playlist asynchronously.
		/// </summary>
		/// <param name="title">The title of the new playlist.</param>
		/// <param name="description">The description of the new playlist.</param>
		/// <param name="isPublic">Indicates whether the playlist is public.</param>
		private async void AddNewPlaylist(string title, string description, bool isPublic)
		{
			try
			{
				var privacyStatus = isPublic ? "public" : "private";

				await _mainViewModel.MyYouTubeService.AddNewPlaylist(title, description, privacyStatus);

				await Task.Delay(5000); // Wait for the playlist to be created
				await LoadPlaylists();
			}
			catch (Google.GoogleApiException ex)
			{
				MessageBox.Show($"An API error occurred: {ex.Message}\n{ex.Error.Message}");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}");
			}
		}

		/// <summary>
		/// Executes the delete playlist command.
		/// </summary>
		/// <param name="parameter">The command parameter.</param>
		private void ExecuteDeletePlaylist(object parameter)
		{
			if (parameter is Playlist playlist)
			{
				var result = MessageBox.Show($"Are you sure you want to delete the playlist \"{playlist.Snippet.Title}\"?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
				if (result == MessageBoxResult.Yes)
				{
					DeletePlaylist(playlist);
				}
			}
		}

		/// <summary>
		/// Deletes a playlist asynchronously.
		/// </summary>
		/// <param name="playlist">The playlist to delete.</param>
		private async void DeletePlaylist(Playlist playlist)
		{
			if (playlist == null)
			{
				return;
			}

			var index = Playlists.IndexOf(playlist);
			try
			{
				await _mainViewModel.MyYouTubeService.DeletePlaylistAsync(playlist.Id);
				Playlists.RemoveAt(index);
			}
			catch (Google.GoogleApiException ex)
			{
				MessageBox.Show($"An API error occurred: {ex.Message}\n{ex.Error.Message}");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}");
			}
		}
	}
}
