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
		private MyShelf<MySong> _songs;
		private string _searchQuery = string.Empty;
		private int _selectedSongIndex = -1;
		private string? _searchContinuationToken;

		public void ResetIndices()
		{
			SelectedSongIndex = -1;
		}

		public MyShelf<MySong> Songs
		{
			get => _songs;
			set => SetProperty(ref _songs, value);
		}

		public int SelectedSongIndex
		{
			set
			{
				if (SetProperty(ref _selectedSongIndex, value))
				{
					HandleSelectedSongIndex(value);
				}
			}
		}

		private async void HandleSelectedSongIndex(int index)
		{
			if (Songs.Items.ElementAtOrDefault(index) is MySong song and not null)
			{
				var newSongs = new MyShelf<MySong>(new ObservableCollection<MySong>() { song }, null);
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
					newSongs.Items.Add(item);
				}

				_mainViewModel.PlayerViewModel.ProvidePlayerInfo(newSongs.Items, 0, GetInfoString("Queue"));
				_mainViewModel.SwitchToPlayerView();
			}
		}

		public string SearchQuery
		{
			get => _searchQuery;
			set
			{
				SetProperty(ref _searchQuery, value);
				_searchContinuationToken = null;
			}
		}

		public ICommand SearchCommand { get; }
		public ICommand BackCommand { get; }

		public SearchViewModel(MainViewModel mainViewModel, MyYouTubeService ys)
		{
			_mainViewModel = mainViewModel;
			_youTubeService = ys;
			_songs = new MyShelf<MySong>(new(), null);

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
				var shelf = await _youTubeService.FetchSongShelvesAsync(query, _searchContinuationToken);
				if (shelf == null) 
				{
					MessageBox.Show("No songs found.");
					return;
				}

				Songs = shelf;
				_searchContinuationToken = shelf.ContinuationToken;

				if (!shelf.Items.Any())
				{
					MessageBox.Show("No songs found.");
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}");
			}
		}

		private void ExecuteBack(object parameter)
		{
			_searchContinuationToken = null;
			_mainViewModel.NavigateBack();
		}
	}
}
