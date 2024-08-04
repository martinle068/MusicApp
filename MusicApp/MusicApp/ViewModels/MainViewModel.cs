namespace MusicApp.ViewModels
{
	public class MainViewModel : BaseViewModel
	{
		private BaseViewModel _currentViewModel;
		private string _searchQuery = "k";

		public BaseViewModel CurrentViewModel
		{
			get => _currentViewModel;
			set => SetProperty(ref _currentViewModel, value);
		}

		public string SearchQuery
		{
			get => _searchQuery;
			set => SetProperty(ref _searchQuery, value);
		}

		public MainViewModel()
		{
			CurrentViewModel = new HomeViewModel(this);
		}

		public void SwitchToPlayerView()
		{
			CurrentViewModel = new PlayerViewModel(this);
		}

		public void SwitchToHomeView()
		{
			CurrentViewModel = new HomeViewModel(this);
		}
	}
}
