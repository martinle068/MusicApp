using MusicApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MusicApp.Models;

namespace MusicApp.Views
{
	/// <summary>
	/// Interaction logic for PlayerView.xaml.
	/// This class handles the interactions for the player view.
	/// </summary>
	public partial class PlayerView : UserControl
	{
		/// <summary>
		/// Initializes a new instance of the PlayerView class.
		/// </summary>
		public PlayerView()
		{
			InitializeComponent();
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
		/// Executes the AddSongToPlaylist command when the context menu item is clicked.
		/// </summary>
		/// <param name="sender">The source of the event, typically the MenuItem.</param>
		private void ExecuteAddSongToPlaylist(object sender)
		{
			var song = Utils.Utils.GetItemFromMenuItem<MySong>(sender);

			if (song != null)
			{
				var viewModel = DataContext as PlayerViewModel;
				viewModel?.AddSongToPlaylistCommand.Execute(song);
			}
		}

		/// <summary>
		/// Handles the RoutedEvent for adding a song to a playlist from the context menu.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The RoutedEventArgs containing event data.</param>
		private void AddSongToPlaylist_ComboBox(object sender, RoutedEventArgs e)
		{
			ExecuteAddSongToPlaylist(sender);
		}

		/// <summary>
		/// Executes the RemoveSongFromPlaylist command when the context menu item is clicked.
		/// </summary>
		/// <param name="sender">The source of the event, typically the MenuItem.</param>
		private void ExecuteRemoveSongFromPlaylist(object sender)
		{
			var song = Utils.Utils.GetItemFromMenuItem<MySong>(sender);

			if (song != null)
			{
				var viewModel = DataContext as PlayerViewModel;
				viewModel?.RemoveSongFromPlaylistCommand.Execute(song);
			}
		}

		/// <summary>
		/// Handles the RoutedEvent for removing a song from a playlist from the context menu.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The RoutedEventArgs containing event data.</param>
		private void RemoveSongFromPlaylist(object sender, RoutedEventArgs e)
		{
			ExecuteRemoveSongFromPlaylist(sender);
		}

		/// <summary>
		/// Handles the PreviewMouseRightButtonDown event to prevent the selection index from changing.
		/// This is useful for context menu operations where the selection should remain unchanged.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The MouseButtonEventArgs containing event data.</param>
		private void PreviewMouseRightButtonDown_IgnoreIndexSelection(object sender, MouseButtonEventArgs e)
		{
			// Mark the event as handled so the SelectedIndex doesn't change
			e.Handled = true;
		}

		/// <summary>
		/// Handles the PreviewKeyDown event for buttons to suppress the Enter and Spacebar key presses.
		/// This prevents unintended command execution from these keys.
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
