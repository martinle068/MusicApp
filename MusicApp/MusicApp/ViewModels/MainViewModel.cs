using System.Threading.Tasks;

namespace MusicApp.ViewModels
{
	public class MainViewModel : BaseViewModel
	{
		private BaseViewModel _currentViewModel;
		private bool _isMiniPlayerVisible;
		public HomeViewModel HomeViewModel { get; }
		public SearchViewModel SearchViewModel { get; }
		public PlayerViewModel PlayerViewModel { get; }

		public BaseViewModel CurrentViewModel
		{
			get => _currentViewModel;
			set
			{
				SetProperty(ref _currentViewModel, value);
				IsMiniPlayerVisible = _currentViewModel != PlayerViewModel && PlayerViewModel.SelectedSongIndex != -1;
			}
		}

		public bool IsMiniPlayerVisible
		{
			get => _isMiniPlayerVisible;
			set => SetProperty(ref _isMiniPlayerVisible, value);
		}

		public MainViewModel()
		{
			HomeViewModel = new HomeViewModel(this);
			SearchViewModel = new SearchViewModel(this);
			PlayerViewModel = new PlayerViewModel(this);

			CurrentViewModel = HomeViewModel;
		}

		public void SwitchToHomeView()
		{
			CurrentViewModel = HomeViewModel;
		}

		public void SwitchToSearchView()
		{
			CurrentViewModel = SearchViewModel;
		}

		public void SwitchToPlayerView()
		{
			CurrentViewModel = PlayerViewModel;
		}

		public async Task SearchAndNavigateAsync(string query)
		{
			await SearchViewModel.ExecuteSearch(query);
			SwitchToSearchView();
		}
	}
}
