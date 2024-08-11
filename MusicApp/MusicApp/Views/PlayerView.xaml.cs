using MusicApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MusicApp.Models;

namespace MusicApp.Views
{
	public partial class PlayerView : UserControl
	{
		public PlayerView()
		{
			InitializeComponent();
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

		private void ExecuteAddSongToPlaylist(object sender)
		{
			var song = Utils.Utils.GetItemFromMenuItem<MySong>(sender);

			if (song != null)
			{
				var viewModel = DataContext as PlayerViewModel;
				viewModel?.AddSongToPlaylistCommand.Execute(song);
			}
		}

		private void AddSongToPlaylist_ComboBox(object sender, RoutedEventArgs e)
		{
			ExecuteAddSongToPlaylist(sender);
		}

		private void ExecuteRemoveSongFromPlaylist(object sender)
		{
			var song = Utils.Utils.GetItemFromMenuItem<MySong>(sender);

			if (song != null)
			{
				var viewModel = DataContext as PlayerViewModel;
				viewModel?.RemoveSongFromPlaylistCommand.Execute(song);
			}
		}

		private void RemoveSongFromPlaylist(object sender, RoutedEventArgs e)
		{
			ExecuteRemoveSongFromPlaylist(sender);
		}

		private void PreviewMouseRightButtonDown_IgnoreIndexSelection(object sender, MouseButtonEventArgs e)
		{
			// Mark the event as handled so the SelectedIndex doesn't change
			e.Handled = true;
		}

	}
}
