using System.Windows;
using System.Windows.Controls;

namespace MusicApp.Views
{
	/// <summary>
	/// Interaction logic for AddPlaylistDialog.xaml.
	/// This dialog allows the user to input details for creating a new playlist.
	/// </summary>
	public partial class AddPlaylistDialog : Window
	{
		/// <summary>
		/// Gets the name of the playlist entered by the user.
		/// </summary>
		public string PlaylistName { get; private set; }

		/// <summary>
		/// Gets the description of the playlist entered by the user.
		/// </summary>
		public string PlaylistDescription { get; private set; }

		/// <summary>
		/// Gets the visibility status of the playlist (e.g., Public, Private) selected by the user.
		/// </summary>
		public string PlaylistVisibility { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AddPlaylistDialog"/> class.
		/// </summary>
		public AddPlaylistDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handles the Click event of the Cancel button. 
		/// Closes the dialog without saving any changes.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The event data.</param>
		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		/// <summary>
		/// Handles the Click event of the Create button. 
		/// Saves the entered playlist details and closes the dialog.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The event data.</param>
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
