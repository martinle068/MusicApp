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
		private readonly MyYouTubeService _youTubeService;
		private ObservableCollection<MySong> _songs;
		private ObservableCollection<string> _songList;
		private string _searchQuery = string.Empty;
		private int _selectedSongIndex = -1;

		public ObservableCollection<MySong> Songs
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
			set
			{
				if (SetProperty(ref _selectedSongIndex, value))
				{
					if (Songs.ElementAtOrDefault(value) is MySong)
					{
						_mainViewModel.PlayerViewModel.Songs = new(Songs); // Add all search results to the player
						_mainViewModel.PlayerViewModel.SelectedSongIndex = -1;
						_mainViewModel.PlayerViewModel.SelectedSongIndex = value;
						//_mainViewModel.PlayerViewModel.PlaySelectedSong();
						_mainViewModel.SwitchToPlayerView();
						SelectedSongIndex = -1;
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

		public SearchViewModel(MainViewModel mainViewModel, MyYouTubeService ys)
		{
			_mainViewModel = mainViewModel;
			_youTubeService = ys;
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
					//await song.LoadThumbnailAsync();
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
			_mainViewModel.NavigateBack();
		}
	}
}
