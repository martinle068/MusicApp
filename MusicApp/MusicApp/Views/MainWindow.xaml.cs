using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using MusicApp.ViewModels;

namespace MusicApp.Views
{
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Initializes a new instance of the MainWindow class.
		/// Sets the DataContext to a new instance of MainViewModel.
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			DataContext = new MainViewModel(this);
		}

		/// <summary>
		/// Handles the PreviewKeyDown event for the window.
		/// If the spacebar is pressed and the focus is not on a TextBox, 
		/// it toggles the play/pause state of the media player.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The KeyEventArgs containing event data.</param>
		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			// Check if the spacebar is pressed
			if (e.Key == Key.Space)
			{
				// Check if the currently focused element is a TextBox
				if (Keyboard.FocusedElement is TextBox)
				{
					// Ignore the spacebar press if the focus is on a TextBox
					return;
				}

				// Get the main view model and the player view model
				var viewModel = DataContext as MainViewModel;
				var playerViewModel = viewModel?.PlayerViewModel;

				// If a song is selected, toggle play/pause
				if (playerViewModel?.SelectedSongIndex != -1)
				{
					playerViewModel?.PlayPauseCommand.Execute(null);
					e.Handled = true;
				}
			}
		}
	}
}
