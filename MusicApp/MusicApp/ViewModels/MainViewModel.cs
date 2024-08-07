using MusicApp.Models;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicApp.ViewModels
{
	public class MainViewModel : BaseViewModel
	{
		private YouTubeService _youTubeService;
		public Stack<BaseViewModel> HistoryStack { get; set; } = new Stack<BaseViewModel>();
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
				HistoryStack.Push(value);
			}
		}

		public bool IsMiniPlayerVisible
		{
			get => _isMiniPlayerVisible;
			set => SetProperty(ref _isMiniPlayerVisible, value);
		}

		public MainViewModel()
		{
			_youTubeService = new YouTubeService();
			HomeViewModel = new HomeViewModel(this);
			SearchViewModel = new SearchViewModel(this, _youTubeService);
			PlayerViewModel = new PlayerViewModel(this, _youTubeService);

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

		public void NavigateBack()
		{
			if (HistoryStack.Count > 0)
			{
				HistoryStack.Pop();
				BaseViewModel destination = HistoryStack.Pop();

				if (destination is SearchViewModel)
				{
					SwitchToSearchView();
				}
				else if (destination is PlayerViewModel)
				{
					SwitchToPlayerView();
				}
				else
				{
					SwitchToHomeView();
				}
			}
		}
	}
}
