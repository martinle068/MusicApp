using System.Windows.Input;
using MusicApp.Commands;

namespace MusicApp.ViewModels
{
	public class HomeViewModel : BaseViewModel
	{
		private readonly MainViewModel _mainViewModel;
		private string? _searchQuery;

		public string? SearchQuery
		{
			get => _searchQuery;
			set => SetProperty(ref _searchQuery, value);
		}

		public ICommand SearchCommand { get; }

		public HomeViewModel(MainViewModel mainViewModel)
		{
			_mainViewModel = mainViewModel;
			SearchCommand = new RelayCommand(ExecuteSearch);
		}

		private void ExecuteSearch(object parameter)
		{
			_mainViewModel.SwitchToPlayerView();
		}
	}
}
