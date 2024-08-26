using MusicApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicApp.Views
{
	/// <summary>
	/// Interaction logic for MiniPlayerView.xaml.
	/// This class handles the interactions for the mini player view.
	/// </summary>
	public partial class MiniPlayerView : UserControl
	{
		/// <summary>
		/// Initializes a new instance of the MiniPlayerView class.
		/// </summary>
		public MiniPlayerView()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handles the MouseDown event on the MiniPlayerView.
		/// Switches to the full player view if the click is not on a Slider, Button, or Label.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The MouseButtonEventArgs containing event data.</param>
		private void MiniPlayerView_MouseDown(object sender, MouseButtonEventArgs e)
		{
			// Ensure the click is not on a Slider, Button, or Label
			if (e.OriginalSource is not (Slider or Button or Label))
			{
				// Retrieve the main view model from the application context
				var mainViewModel = Application.Current.MainWindow.DataContext as MainViewModel;
				if (mainViewModel != null)
				{
					// Switch to the full player view and hide the mini player
					mainViewModel.CurrentViewModel = mainViewModel.PlayerViewModel;
					mainViewModel.IsMiniPlayerVisible = false;
				}
			}
		}

		/// <summary>
		/// Handles the PreviewMouseLeftButtonDown event for the track bar (seek slider).
		/// Begins the seek operation.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The MouseButtonEventArgs containing event data.</param>
		private void TrackBarSeek_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (DataContext is PlayerViewModel viewModel)
			{
				viewModel.TrackBarSeek_PreviewMouseLeftButtonDown(sender, e);
			}
		}

		/// <summary>
		/// Handles the PreviewMouseLeftButtonUp event for the track bar (seek slider).
		/// Ends the seek operation.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The MouseButtonEventArgs containing event data.</param>
		private void TrackBarSeek_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (DataContext is PlayerViewModel viewModel)
			{
				viewModel.TrackBarSeek_PreviewMouseLeftButtonUp(sender, e);
			}
		}

		/// <summary>
		/// Handles the ValueChanged event for the track bar (seek slider).
		/// Updates the current position in the media.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The RoutedPropertyChangedEventArgs containing event data.</param>
		private void TrackBarSeek_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (DataContext is PlayerViewModel viewModel)
			{
				viewModel.TrackBarSeek_ValueChanged(sender, e);
			}
		}

		/// <summary>
		/// Handles the PreviewKeyDown event for buttons.
		/// Suppresses the Enter and Spacebar key presses to prevent unintended command execution.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The KeyEventArgs containing event data.</param>
		private void Button_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Space)
			{
				e.Handled = true; // Suppress the Enter and Spacebar key presses
			}
		}
	}
}
