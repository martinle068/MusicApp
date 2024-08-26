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
	/// <summary>
	/// ViewModel for handling song search functionality, interacting with the YouTube Music API, and managing search results.
	/// </summary>
	public class SearchViewModel : BaseViewModel
	{
		private readonly MainViewModel _mainViewModel;
		private readonly MyYouTubeService _youTubeService;
		private MyShelf<MySong> _songs;
		private string _searchQuery = string.Empty;
		private int _selectedSongIndex = -1;
		private string? _searchContinuationToken;

		/// <summary>
		/// Resets the selected song index to its default value (-1).
		/// </summary>
		public void ResetIndices()
		{
			SelectedSongIndex = -1;
		}

		/// <summary>
		/// Gets or sets the collection of songs returned by the search query.
		/// </summary>
		public MyShelf<MySong> Songs
		{
			get => _songs;
			set => SetProperty(ref _songs, value);
		}

		/// <summary>
		/// Gets or sets the index of the currently selected song in the search results.
		/// </summary>
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

		/// <summary>
		/// Handles the selection of a song, fetching related songs and updating the player view.
		/// </summary>
		/// <param name="index">The index of the selected song.</param>
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

		/// <summary>
		/// Gets or sets the search query entered by the user.
		/// </summary>
		public string SearchQuery
		{
			get => _searchQuery;
			set
			{
				SetProperty(ref _searchQuery, value);
				_searchContinuationToken = null;
			}
		}

		/// <summary>
		/// Command for executing the search based on the search query.
		/// </summary>
		public ICommand SearchCommand { get; }

		/// <summary>
		/// Command for navigating back to the previous view.
		/// </summary>
		public ICommand BackCommand { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchViewModel"/> class.
		/// </summary>
		/// <param name="mainViewModel">The main view model controlling the application's state.</param>
		/// <param name="ys">The YouTube service for interacting with the YouTube Music API.</param>
		public SearchViewModel(MainViewModel mainViewModel, MyYouTubeService ys)
		{
			_mainViewModel = mainViewModel;
			_youTubeService = ys;
			_songs = new MyShelf<MySong>(new(), null);

			SearchCommand = new RelayCommand(async _ => await ExecuteSearch(_searchQuery));
			BackCommand = new RelayCommand(ExecuteBack);
		}

		/// <summary>
		/// Executes the search using the YouTube Music API and updates the song collection.
		/// </summary>
		/// <param name="query">The search query entered by the user.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
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

		/// <summary>
		/// Navigates back to the previous view, clearing the continuation token.
		/// </summary>
		/// <param name="parameter">Optional parameter for the command.</param>
		private void ExecuteBack(object parameter)
		{
			_searchContinuationToken = null;
			_mainViewModel.NavigateBack();
		}
	}
}
