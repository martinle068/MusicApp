﻿using System.Collections.ObjectModel;
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
		private ObservableCollection<Playlist> _playlists;
		private Playlist _selectedPlaylist;
		private int _selectedPlaylistIndex = -1;

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
				if (value != -1 || value < Playlists.Count)
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

					_mainViewModel.PlayerViewModel.Songs = playlistItems;
					_mainViewModel.PlayerViewModel.SelectedSongIndex = -1;
					_mainViewModel.PlayerViewModel.SelectedSongIndex = 0;
					_mainViewModel.PlayerViewModel.InfoText = GetInfoString(playlist.Snippet.Title);
					_mainViewModel.SwitchToPlayerView();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				_selectedPlaylistIndex = -1;
				OnPropertyChanged(nameof(SelectedPlaylistIndex));
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
			SearchCommand = new RelayCommand(async _ => await ExecuteSearch());
			AddPlaylistCommand = new RelayCommand(OpenAddPlaylistDialog);
			SelectPlaylistCommand = new RelayCommand(async _ => await HandlePlaylistSelectionAsync());
			DeletePlaylistCommand = new RelayCommand(ExecuteDeletePlaylist);
			LoadPlaylists();
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
			Playlists.Clear();
			foreach (var playlist in playlists)
			{
				Playlists.Add(playlist);
			}
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
