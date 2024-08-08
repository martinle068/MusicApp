﻿using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Google.Apis.YouTube.v3.Data;
using MusicApp.Commands;
using MusicApp.Models;
using YoutubeExplode.Playlists;
using Playlist = Google.Apis.YouTube.v3.Data.Playlist;

namespace MusicApp.ViewModels
{
	public class HomeViewModel : BaseViewModel
	{
		private readonly MainViewModel _mainViewModel;
		private string _searchQuery = string.Empty;
		private ObservableCollection<Playlist> _playlists;
		private int _selectedPlaylistIndex = -1;

		public int SelectedPlaylistIndex
		{
			get => _selectedPlaylistIndex;
			set 
			{
				SetProperty(ref _selectedPlaylistIndex, value);
				HandlePlaylistSelectionAsync(value);
			}
		}

		private async void HandlePlaylistSelectionAsync(int index)
		{
			try
			{
				if (Playlists.ElementAtOrDefault(index) is Playlist playlist)
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

		public HomeViewModel(MainViewModel mainViewModel)
		{
			_mainViewModel = mainViewModel;
			Playlists = new ObservableCollection<Playlist>();
			SearchCommand = new RelayCommand(async _ => await ExecuteSearch());
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
	}
}
