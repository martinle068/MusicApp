using NAudio.Wave;
using System;

namespace MusicApp
{
	//public class MusicPlayer
	//{
	//	private WaveOutEvent _outputDevice;
	//	private AudioFileReader _audioFile;

	//	public TimeSpan TotalTime => _audioFile?.TotalTime ?? TimeSpan.Zero;
	//	public TimeSpan CurrentTime
	//	{
	//		get => _audioFile?.CurrentTime ?? TimeSpan.Zero;
	//		set
	//		{
	//			if (_audioFile != null)
	//			{
	//				_audioFile.CurrentTime = value;
	//			}
	//		}
	//	}

	//	public bool IsPaused => _outputDevice?.PlaybackState == PlaybackState.Paused;

	//	public event EventHandler<StoppedEventArgs> PlaybackStopped;

	//	public void Initialize(string filePath)
	//	{
	//		_audioFile = new AudioFileReader(filePath);
	//		_outputDevice = new WaveOutEvent();
	//		_outputDevice.Init(_audioFile);
	//		_outputDevice.PlaybackStopped += (s, e) => PlaybackStopped?.Invoke(this, e);
	//	}

	//	public void Play()
	//	{
	//		_outputDevice?.Play();
	//	}

	//	public void PauseOrResume()
	//	{
	//		if (_outputDevice != null)
	//		{
	//			if (_outputDevice.PlaybackState == PlaybackState.Playing)
	//			{
	//				_outputDevice.Pause();
	//			}
	//			else if (_outputDevice.PlaybackState == PlaybackState.Paused)
	//			{
	//				_outputDevice.Play();
	//			}
	//		}
	//	}

	//	public void Stop()
	//	{
	//		if (_outputDevice != null)
	//		{
	//			_outputDevice.Stop();
	//			_outputDevice.Dispose();
	//			_outputDevice = null;
	//		}

	//		if (_audioFile != null)
	//		{
	//			_audioFile.Dispose();
	//			_audioFile = null;
	//		}
	//	}

	//	public void SeekTo(TimeSpan time)
	//	{
	//		if (_audioFile != null && time.TotalSeconds >= 0 && time <= _audioFile.TotalTime)
	//		{
	//			_audioFile.CurrentTime = time;
	//		}
	//	}
	//}
}
