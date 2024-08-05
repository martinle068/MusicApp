using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicApp.ViewModels
{
	public class BaseViewModel : INotifyPropertyChanged
	{
		private BaseViewModel _previousViewModel;

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (Equals(field, value)) return false;
			field = value;
			OnPropertyChanged(propertyName);
			return true;
		}

		public void SetPreviousViewModel(BaseViewModel previousViewModel)
		{
			_previousViewModel = previousViewModel;
		}

		public BaseViewModel GetPreviousViewModel()
		{
			return _previousViewModel;
		}
	}
}
