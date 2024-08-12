﻿using System;
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
		private string? _continuationToken;

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
					if (Songs.Items.ElementAtOrDefault(value) is MySong)
					{
						_mainViewModel.ResetIndices();
						_mainViewModel.CurrentMusicSource = MainViewModel.MusicSource.Search;
						_mainViewModel.PlayerViewModel.ProvidePlayerInfo(new(Songs.Items), value, GetInfoString("Queue"), _continuationToken);
						_mainViewModel.SwitchToPlayerView();
					}
				}
			}
		}

		public string SearchQuery
		{
			get => _searchQuery;
			set
			{
				SetProperty(ref _searchQuery, value);
				_continuationToken = null;
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
				var shelf = await _youTubeService.FetchSongsAsync(query, _continuationToken);
				if (shelf == null) 
				{
					MessageBox.Show("No songs found.");
					return;
				}

				_continuationToken = shelf.ContinuationToken;

				if (!shelf.Items.Any())
				{
					MessageBox.Show("No songs found.");
					return;
				}

				Songs = shelf;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}");
			}
		}

		private void ExecuteBack(object parameter)
		{
			_continuationToken = null;
			_mainViewModel.NavigateBack();
		}
	}
}
