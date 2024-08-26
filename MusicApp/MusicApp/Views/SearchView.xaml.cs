using System.Windows.Controls;
using System.Windows.Input;
using MusicApp.ViewModels;

namespace MusicApp.Views
{
	/// <summary>
	/// Interaction logic for SearchView.xaml.
	/// This class handles the user interactions in the search view of the application.
	/// </summary>
	public partial class SearchView : UserControl
	{
		/// <summary>
		/// Initializes a new instance of the SearchView class.
		/// </summary>
		public SearchView()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handles the KeyDown event for the TextBox.
		/// Executes the search command when the Enter key is pressed.
		/// </summary>
		/// <param name="sender">The source of the event, expected to be a TextBox.</param>
		/// <param name="e">The KeyEventArgs containing event data.</param>
		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				var viewModel = DataContext as SearchViewModel;
				if (viewModel != null)
				{
					var textBox = sender as TextBox;
					if (textBox != null)
					{
						// Force update the binding source
						textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
					}
					viewModel.SearchCommand.Execute(null);
				}
			}
		}
	}
}
