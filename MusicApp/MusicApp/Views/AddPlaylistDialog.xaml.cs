using System.Windows;
using System.Windows.Controls;

namespace MusicApp.Views
{
	public partial class AddPlaylistDialog : Window
	{
		public string PlaylistName { get; private set; }
		public string PlaylistDescription { get; private set; }
		public string PlaylistVisibility { get; private set; }

		public AddPlaylistDialog()
		{
			InitializeComponent();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void Create_Click(object sender, RoutedEventArgs e)
		{
			PlaylistName = PlaylistNameTextBox.Text;
			PlaylistDescription = PlaylistDescriptionTextBox.Text;
			PlaylistVisibility = ((ComboBoxItem)PlaylistVisibilityComboBox.SelectedItem).Content.ToString();
			DialogResult = true;
			Close();
		}
	}
}
