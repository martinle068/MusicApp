using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicApp.ViewModels
{
	public class BaseViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingField, value))
				return false;

			backingField = value;
			OnPropertyChanged(propertyName);
			return true;
		}
	}
}
