using MusicApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicApp.Views
{
	public partial class MiniPlayerView : UserControl
	{
		public MiniPlayerView()
		{
			InitializeComponent();
		}

		private void MiniPlayerView_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.OriginalSource is not (Slider or Button or Label))
			{
				var mainViewModel = Application.Current.MainWindow.DataContext as MainViewModel;
				if (mainViewModel != null)
				{
					mainViewModel.CurrentViewModel = mainViewModel.PlayerViewModel;
					mainViewModel.IsMiniPlayerVisible = false;
				}
			}
		}

		private void TrackBarSeek_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (DataContext is PlayerViewModel viewModel)
			{
				viewModel.TrackBarSeek_PreviewMouseLeftButtonDown(sender, e);
			}
		}

		private void TrackBarSeek_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (DataContext is PlayerViewModel viewModel)
			{
				viewModel.TrackBarSeek_PreviewMouseLeftButtonUp(sender, e);
			}
		}

		private void TrackBarSeek_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (DataContext is PlayerViewModel viewModel)
			{
				viewModel.TrackBarSeek_ValueChanged(sender, e);
			}
		}

		private void Button_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Space)
			{
				e.Handled = true; // Suppress the Enter and Spacebar key presses
			}
		}
	}
}
