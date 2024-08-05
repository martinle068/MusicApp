using System.Windows.Controls;
using System.Windows.Input;
using MusicApp.ViewModels;

namespace MusicApp.Views
{
	public partial class SearchView : UserControl
	{
		public SearchView()
		{
			InitializeComponent();
		}

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
