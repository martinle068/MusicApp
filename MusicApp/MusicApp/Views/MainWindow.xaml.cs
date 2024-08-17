using System.Windows;
using System.Windows.Input;
using MusicApp.ViewModels;

namespace MusicApp.Views
{
	public partial class MainWindow : Window
	{

		public MainWindow()
		{
			InitializeComponent();

			DataContext = new MainViewModel(this);
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space)
			{
				var viewModel = DataContext as MainViewModel; 
				var playerViewModel = viewModel?.PlayerViewModel;

				if (playerViewModel?.SelectedSongIndex != -1)
				{
					playerViewModel?.PlayPauseCommand.Execute(null);
					e.Handled = true; 
				}
			}
		}
	}
}
