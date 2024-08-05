using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AngleSharp.Dom;
using MusicApp.Commands;
using MusicApp.Models;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
using static MusicApp.Utils.Utils;

namespace MusicApp.ViewModels
{
	public class SearchViewModel : BaseViewModel
	{
		private readonly MainViewModel _mainViewModel;
		private readonly YouTubeService _youTubeService;
		private ObservableCollection<Song> _songs;
		private ObservableCollection<string> _songList;
		private string _searchQuery = string.Empty;
		private int _selectedSongIndex = -1;

		public ObservableCollection<Song> Songs
		{
			get => _songs;
			set => SetProperty(ref _songs, value);
		}

		public ObservableCollection<string> SongList
		{
			get => _songList;
			set => SetProperty(ref _songList, value);
		}

		public int SelectedSongIndex
		{
			get => _selectedSongIndex;
			set
			{
				if (SetProperty(ref _selectedSongIndex, value))
				{
					if (Songs.ElementAtOrDefault(value) is Song selectedSong)
					{
						_mainViewModel.PlayerViewModel.Songs = Songs; // Add all search results to the player
						_mainViewModel.PlayerViewModel.SongList = SongList;
						_mainViewModel.PlayerViewModel.SelectedSongIndex = value;
						_mainViewModel.PlayerViewModel.PlaySelectedSong();
						_mainViewModel.SwitchToPlayerView();
					}
				}
			}
		}

		public string SearchQuery
		{
			get => _searchQuery;
			set => SetProperty(ref _searchQuery, value);
		}

		public ICommand SearchCommand { get; }
		public ICommand BackCommand { get; }

		public SearchViewModel(MainViewModel mainViewModel)
		{
			_mainViewModel = mainViewModel;
			_youTubeService = new YouTubeService();
			Songs = new();
			SongList = new();

			SearchCommand = new RelayCommand(async _ => await ExecuteSearch(_searchQuery));
			BackCommand = new RelayCommand(ExecuteBack);
		}

		public async Task ExecuteSearch(string query)
		{
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
					SongList.Add(SongToString(song));
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}");
			}
		}

		private void ExecuteBack(object parameter)
		{
			_mainViewModel.SwitchToHomeView();
		}
	}
}
