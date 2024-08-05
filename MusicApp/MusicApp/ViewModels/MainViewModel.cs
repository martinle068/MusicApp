using System.Windows;
using System.Windows.Input;

namespace MusicApp.ViewModels
{
	public class MainViewModel : BaseViewModel
	{
		private BaseViewModel _currentViewModel;
		private PlayerViewModel _playerViewModel;
		private HomeViewModel _homeViewModel;
		private bool _isMiniPlayerVisible;

		public BaseViewModel CurrentViewModel
		{
			get => _currentViewModel;
			set => SetProperty(ref _currentViewModel, value);
		}

		public bool IsMiniPlayerVisible
		{
			get => _isMiniPlayerVisible;
			set => SetProperty(ref _isMiniPlayerVisible, value);
		}

		public PlayerViewModel PlayerViewModel => _playerViewModel;

		public MainViewModel()
		{
			_homeViewModel = new HomeViewModel(this);
			_playerViewModel = new PlayerViewModel(this);
			CurrentViewModel = _homeViewModel;
		}

		public void SwitchToPlayerView(string? searchQuery)
		{
			if (string.IsNullOrWhiteSpace(searchQuery))
			{
				return;
			}

			_playerViewModel.SearchQuery = searchQuery;
			_playerViewModel.SearchCommand.Execute(null);
			CurrentViewModel = _playerViewModel;
			IsMiniPlayerVisible = false; // Hide the mini player when the player view is open
		}

		public void SwitchToHomeView()
		{
			CurrentViewModel = _homeViewModel;
			IsMiniPlayerVisible = _playerViewModel.IsSongLoaded;
		}
	}
}
