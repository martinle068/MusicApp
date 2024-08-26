using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicApp.ViewModels
{
	/// <summary>
	/// A base class for ViewModels that provides property change notification and navigation to previous ViewModel functionality.
	/// </summary>
	public class BaseViewModel : INotifyPropertyChanged
	{
		private BaseViewModel _previousViewModel;

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the PropertyChanged event to notify the UI of a property value change.
		/// </summary>
		/// <param name="propertyName">The name of the property that changed.</param>
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Sets the specified property if the value is different, and raises the PropertyChanged event.
		/// </summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="field">A reference to the field that stores the property's value.</param>
		/// <param name="value">The new value to set.</param>
		/// <param name="propertyName">The name of the property being set.</param>
		/// <returns>True if the property was changed, otherwise false.</returns>
		protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (Equals(field, value)) return false;
			field = value;
			OnPropertyChanged(propertyName);
			return true;
		}

		/// <summary>
		/// Sets the previous ViewModel for navigation purposes.
		/// </summary>
		/// <param name="previousViewModel">The previous ViewModel to set.</param>
		public void SetPreviousViewModel(BaseViewModel previousViewModel)
		{
			_previousViewModel = previousViewModel;
		}

		/// <summary>
		/// Gets the previous ViewModel for navigation purposes.
		/// </summary>
		/// <returns>The previous ViewModel.</returns>
		public BaseViewModel GetPreviousViewModel()
		{
			return _previousViewModel;
		}
	}
}
