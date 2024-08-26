using Google.Apis.YouTube.v3;
using MusicApp.Models;
using MusicApp.Views;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicApp.ViewModels
{
	/// <summary>
	/// ViewModel for the main application window, responsible for managing the navigation between different views,
	/// handling YouTube service initialization, and managing the application's state.
	/// </summary>
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

		/// <summary>
		/// Represents the different music sources the application can play from.
		/// </summary>
		public enum MusicSource
		{
			None,
			Playlist,
			Search,
			Popular
		}

		/// <summary>
		/// Gets or sets the current view model, updating the mini player visibility and storing the view in history.
		/// </summary>
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

		/// <summary>
		/// Gets or sets a value indicating whether the mini player is visible.
		/// </summary>
		public bool IsMiniPlayerVisible
		{
			get => _isMiniPlayerVisible;
			set => SetProperty(ref _isMiniPlayerVisible, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MainViewModel"/> class.
		/// </summary>
		/// <param name="mv">The main window instance.</param>
		public MainViewModel(MainWindow mv)
		{
			InitializeAsync();
			MainWindow = mv;
		}

		/// <summary>
		/// Resets the selected indices for all view models.
		/// </summary>
		public void ResetIndices()
		{
			HomeViewModel.ResetIndices();
			SearchViewModel.ResetIndices();
			PlayerViewModel.ResetIndices();
		}

		/// <summary>
		/// Initializes the local database for storing song information.
		/// </summary>
		private void InitializeDB()
		{
			string databasePath = "songs.db";

			SongDatabase = new SongDatabase(databasePath);
		}

		/// <summary>
		/// Initializes the YouTube service and view models asynchronously.
		/// </summary>
		private async void InitializeAsync()
		{
			InitializeDB();
			MyYouTubeService = await MyYouTubeService.CreateYoutubeServiceAsync();
			HomeViewModel = new HomeViewModel(this);
			SearchViewModel = new SearchViewModel(this, MyYouTubeService);
			PlayerViewModel = new PlayerViewModel(this, MyYouTubeService);

			CurrentViewModel = HomeViewModel;
		}

		/// <summary>
		/// Switches the current view to the HomeView.
		/// </summary>
		public void SwitchToHomeView()
		{
			CurrentViewModel = HomeViewModel;
		}

		/// <summary>
		/// Switches the current view to the SearchView.
		/// </summary>
		public void SwitchToSearchView()
		{
			CurrentViewModel = SearchViewModel;
		}

		/// <summary>
		/// Switches the current view to the PlayerView.
		/// </summary>
		public void SwitchToPlayerView()
		{
			CurrentViewModel = PlayerViewModel;
		}

		/// <summary>
		/// Executes a search and navigates to the SearchView asynchronously.
		/// </summary>
		/// <param name="query">The search query string.</param>
		public async Task SearchAndNavigateAsync(string query)
		{
			await SearchViewModel.ExecuteSearch(query);
			SwitchToSearchView();
		}

		/// <summary>
		/// Navigates back to the previous view in the history stack.
		/// </summary>
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
