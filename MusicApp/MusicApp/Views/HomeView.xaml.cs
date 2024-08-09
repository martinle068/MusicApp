using MusicApp.ViewModels;
using System.Windows;
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
		private void ListViewScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
		{
			ScrollViewer scv = (ScrollViewer)sender;
			scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta / 4);
			e.Handled = true;
		}
		private void ListBoxItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var viewModel = DataContext as HomeViewModel;
			viewModel?.SelectPlaylistCommand.Execute(null);
		}
	}
}
