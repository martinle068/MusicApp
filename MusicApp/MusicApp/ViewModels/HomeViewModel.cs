using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Google.Apis.YouTube.v3.Data;
using MusicApp.Commands;
using MusicApp.Models;
using MusicApp.Views;
using YoutubeExplode.Playlists;
using Playlist = Google.Apis.YouTube.v3.Data.Playlist;
using static MusicApp.Utils.Utils;

namespace MusicApp.ViewModels
{
	public class HomeViewModel : BaseViewModel
	{
		private readonly MainViewModel _mainViewModel;
		private string _searchQuery = string.Empty;
		private ObservableCollection<Playlist> _playlists = new();
		private Playlist _selectedPlaylist = new();
		private int _selectedPlaylistIndex = -1;
		private ObservableCollection<MySong> _popularSongsList = new();
		private int _selectedPopularSongIndex = -1;

		public void ResetIndices()
		{
			SelectedPlaylistIndex = -1;
			SelectedPopularSongIndex = -1;
		}

		public int SelectedPopularSongIndex
		{
			get => _selectedPopularSongIndex;
			set
			{
				SetProperty(ref _selectedPopularSongIndex, value);
				_mainViewModel.CurrentMusicSource = MainViewModel.MusicSource.Popular;
			}
		}

		public ObservableCollection<MySong> PopularSongs
		{
			get => _popularSongsList;
			set => SetProperty(ref _popularSongsList, value);
		}

		public Playlist SelectedPlaylist
		{
			get => _selectedPlaylist;
			set => SetProperty(ref _selectedPlaylist, value);
		}

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

		public string SearchQuery
		{
			get => _searchQuery;
			set => SetProperty(ref _searchQuery, value);
		}

		public ObservableCollection<Playlist> Playlists
		{
			get => _playlists;
			set => SetProperty(ref _playlists, value);
		}

		public ICommand SearchCommand { get; }
		public ICommand AddPlaylistCommand { get; }
		public ICommand SelectPlaylistCommand { get;}
		public ICommand DeletePlaylistCommand { get; }

		public HomeViewModel(MainViewModel mainViewModel)
		{
			_mainViewModel = mainViewModel;
			Playlists = new ObservableCollection<Playlist>();
			PopularSongs = new ObservableCollection<MySong>();
			SearchCommand = new RelayCommand(async _ => await ExecuteSearch());
			AddPlaylistCommand = new RelayCommand(OpenAddPlaylistDialog);
			SelectPlaylistCommand = new RelayCommand(async _ => await HandlePlaylistSelectionAsync());
			DeletePlaylistCommand = new RelayCommand(ExecuteDeletePlaylist);
			LoadPlaylists();
			LoadPopularSongs();
		}

		private async void LoadPopularSongs()
		{
			var popularSongs = await _mainViewModel.MyYouTubeService.FetchPopularSongsAsync();
			PopularSongs = popularSongs;
		}

		private async Task ExecuteSearch()
		{
			if (!string.IsNullOrWhiteSpace(SearchQuery))
			{
				_mainViewModel.SearchViewModel.SearchQuery = SearchQuery;
				await _mainViewModel.SearchAndNavigateAsync(SearchQuery);
			}
		}

		private async void LoadPlaylists()
		{
			var playlists = await _mainViewModel.MyYouTubeService.FetchAllPlaylistsAsync();
			Playlists = playlists;
		}

		private void OpenAddPlaylistDialog(object parameter)
		{
			var dialog = new AddPlaylistDialog();
			if (dialog.ShowDialog() == true)
			{
				AddNewPlaylist(dialog.PlaylistName, dialog.PlaylistDescription, dialog.PlaylistVisibility == "Public");
			}
		}

		private async void AddNewPlaylist(string title, string description, bool isPublic)
		{
			try
			{
				var privacyStatus = isPublic ? "public" : "private";

				await _mainViewModel.MyYouTubeService.AddNewPlaylist(title, description, privacyStatus);

				await Task.Delay(5000); // Wait for the playlist to be created
				LoadPlaylists();
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
