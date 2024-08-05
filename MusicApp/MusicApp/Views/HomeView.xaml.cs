using MusicApp.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicApp.Views
{
	public partial class HomeView : UserControl
	{
		public HomeView()
		{
			InitializeComponent();
		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				var viewModel = DataContext as HomeViewModel;
				viewModel?.SearchCommand.Execute(null);
			}
		}
	}
}
