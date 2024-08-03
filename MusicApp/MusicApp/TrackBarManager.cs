using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicApp
{
	//public class TrackBarManager
	//{
	//	private Slider _trackBarSeek;
	//	private MusicPlayer _musicPlayer;
	//	public bool IsSeeking { get; private set; }

	//	public TrackBarManager(Slider trackBarSeek, MusicPlayer musicPlayer)
	//	{
	//		_trackBarSeek = trackBarSeek;
	//		_musicPlayer = musicPlayer;
	//	}

	//	public void SetMaximum(double maximum)
	//	{
	//		_trackBarSeek.Maximum = maximum;
	//	}

	//	public void SetValue(double value)
	//	{
	//		_trackBarSeek.Value = value;
	//	}

	//	public void Reset()
	//	{
	//		_trackBarSeek.Value = 0;
	//		_trackBarSeek.Maximum = 100;
	//	}

	//	public void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	//	{
	//		IsSeeking = true;
	//	}

	//	public void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	//	{
	//		IsSeeking = false;
	//		UpdateTrackBarValueFromMouse(e);
	//	}

	//	public void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	//	{
	//		if (IsSeeking)
	//		{
	//			_musicPlayer.SeekTo(TimeSpan.FromSeconds(_trackBarSeek.Value));
	//			UpdateCurrentTimeLabel(_trackBarSeek.Value);
	//		}
	//	}

	//	private void UpdateTrackBarValueFromMouse(MouseEventArgs e)
	//	{
	//		var pos = e.GetPosition(_trackBarSeek);
	//		double ratio = pos.X / _trackBarSeek.ActualWidth;
	//		double newValue = ratio * _trackBarSeek.Maximum;
	//		_trackBarSeek.Value = newValue;
	//		_musicPlayer.SeekTo(TimeSpan.FromSeconds(newValue));
	//	}

	//	private void UpdateCurrentTimeLabel(double value)
	//	{
	//		if (_musicPlayer != null)
	//		{
	//			// Here you can update a label in the UI if you have one for the current time
	//		}
	//	}
	//}
}
