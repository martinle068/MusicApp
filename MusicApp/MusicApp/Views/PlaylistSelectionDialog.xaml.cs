using System.Collections.ObjectModel;
using System.Windows;
using Google.Apis.YouTube.v3.Data;

namespace MusicApp.Views
{
	/// <summary>
	/// Interaction logic for PlaylistSelectionDialog.xaml.
	/// This class handles the logic for selecting a playlist from a list of available playlists.
	/// </summary>
	public partial class PlaylistSelectionDialog : Window
	{
		/// <summary>
		/// Gets the playlist selected by the user.
		/// </summary>
		public Playlist SelectedPlaylist { get; private set; }

		/// <summary>
		/// Initializes a new instance of the PlaylistSelectionDialog class.
		/// </summary>
		/// <param name="playlists">A collection of playlists to be displayed for selection.</param>
		public PlaylistSelectionDialog(ObservableCollection<Playlist> playlists)
		{
			InitializeComponent();
			PlaylistsListBox.ItemsSource = playlists;
		}

		/// <summary>
		/// Displays the dialog and returns the selected playlist.
		/// </summary>
		/// <returns>The selected playlist, or null if no selection was made.</returns>
		public Playlist? SelectPlaylist()
		{
			ShowDialog();
			return SelectedPlaylist ?? null;
		}

		/// <summary>
		/// Handles the Click event for the OK button.
		/// Sets the selected playlist and closes the dialog.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The RoutedEventArgs containing event data.</param>
		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			SelectedPlaylist = PlaylistsListBox.SelectedItem as Playlist;
			Close();
		}

		/// <summary>
		/// Handles the Click event for the Cancel button.
		/// Closes the dialog without setting a selected playlist.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The RoutedEventArgs containing event data.</param>
		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
