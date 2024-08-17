using Google.Apis.YouTube.v3;
using MusicApp.Models;
using MusicApp.Views;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicApp.ViewModels
{
	public class MainViewModel : BaseViewModel
	{
		public MainWindow MainWindow { get; private set; }
		public SongDatabase? SongDatabase { get; private set; }
		public MyYouTubeService MyYouTubeService { get; private set; }
		public Stack<BaseViewModel> HistoryStack { get; set; } = new Stack<BaseViewModel>();
		private BaseViewModel _currentViewModel;
		private bool _isMiniPlayerVisible;
		public HomeViewModel HomeViewModel { get; private set; }
		public SearchViewModel SearchViewModel { get; private set; }
		public PlayerViewModel PlayerViewModel { get; private set; }
		public MusicSource CurrentMusicSource { get; set; } = MusicSource.None;

		public enum MusicSource
		{
			None,
			Playlist,
			Search,
			Popular
		}

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

		public MainViewModel(MainWindow mv)
		{
			InitializeAsync();
			MainWindow = mv;
		}

		public void ResetIndices()
		{
			HomeViewModel.ResetIndices();
			SearchViewModel.ResetIndices();
			PlayerViewModel.ResetIndices();
		}

		private void InitializeDB()
		{
			string databasePath = "songs.db";

			SongDatabase = new SongDatabase(databasePath);
		}

		private async void InitializeAsync()
		{
			InitializeDB();
			MyYouTubeService = await MyYouTubeService.CreateYoutubeServiceAsync();
			HomeViewModel = new HomeViewModel(this);
			SearchViewModel = new SearchViewModel(this, MyYouTubeService);
			PlayerViewModel = new PlayerViewModel(this, MyYouTubeService);

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
