using System.Collections.ObjectModel;
using System.Windows;
using Google.Apis.YouTube.v3.Data;

namespace MusicApp.Views
{
	public partial class PlaylistSelectionDialog : Window
	{
		public Playlist SelectedPlaylist { get; private set; }

		public PlaylistSelectionDialog(ObservableCollection<Playlist> playlists)
		{
			InitializeComponent();
			PlaylistsListBox.ItemsSource = playlists;
		}

		public Playlist? SelectPlaylist()
		{
			ShowDialog();
			return SelectedPlaylist ?? null;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			SelectedPlaylist = PlaylistsListBox.SelectedItem as Playlist;
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
