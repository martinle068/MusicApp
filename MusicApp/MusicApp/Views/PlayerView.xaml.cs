using System.Windows.Controls;
using MusicApp.ViewModels;

namespace MusicApp.Views
{
	public partial class PlayerView : UserControl
	{
		public PlayerView()
		{
			InitializeComponent();

			//this.DataContext = new PlayerViewModel(new MainViewModel());
		}
	}
}
