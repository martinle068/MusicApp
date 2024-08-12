using System.Windows;
using MusicApp.ViewModels;

namespace MusicApp.Views
{
	public partial class MainWindow : Window
	{

		public MainWindow()
		{
			InitializeComponent();

			DataContext = new MainViewModel(this);
		}

	}
}
