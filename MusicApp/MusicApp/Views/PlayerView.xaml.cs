using MusicApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicApp.Views
{
	public partial class PlayerView : UserControl
	{
		public PlayerView()
		{
			InitializeComponent();
		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				var viewModel = DataContext as PlayerViewModel;
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
	}
}
