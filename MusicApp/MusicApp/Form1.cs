using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
using Timer = System.Windows.Forms.Timer;

namespace MusicApp
{
	public partial class Form1 : Form
	{
		private YouTubeMusicClient _client;
		private List<Song> _songs;
		private TextBox textBoxQuery;
		private Button buttonSearch;
		private ListBox listBoxResults;
		private Button buttonPlay;
		private Button buttonPause;
		private Button buttonStop;
		private PictureBox pictureBoxSong;
		private Label labelCurrentSong;
		private Label labelCurrentTime;
		private Label labelTotalTime;
		private TrackBar trackBarSeek;
		private ToolTip toolTip;
		private WaveOutEvent outputDevice;
		private AudioFileReader audioFile;
		private string currentFilePath;
		private Timer timer;
		private bool isSeeking = false;

		public Form1()
		{
			InitializeComponent();
			InitializeMyComponents();
			_client = new YouTubeMusicClient();
			_songs = new List<Song>();
		}

		private void InitializeMyComponents()
		{
			// TextBox for search query
			textBoxQuery = new TextBox();
			textBoxQuery.KeyDown += new KeyEventHandler(textBoxQuery_KeyDown);

			// Button for search
			buttonSearch = new Button();
			buttonSearch.Text = "Search";
			buttonSearch.Click += new EventHandler(buttonSearch_Click);

			// ListBox for displaying search results
			listBoxResults = new ListBox();
			listBoxResults.MouseMove += new MouseEventHandler(listBoxResults_MouseMove);
			listBoxResults.Click += new EventHandler(listBoxResults_Click);
			listBoxResults.DoubleClick += new EventHandler(listBoxResults_DoubleClick);
			listBoxResults.KeyDown += new KeyEventHandler(listBoxResults_KeyDown);

			// Button for playing selected song
			buttonPlay = new Button();
			buttonPlay.Text = "Play";
			buttonPlay.Click += new EventHandler(buttonPlay_Click);

			// Button for pausing the song
			buttonPause = new Button();
			buttonPause.Text = "Pause";
			buttonPause.Click += new EventHandler(buttonPause_Click);

			// Button for stopping the song
			buttonStop = new Button();
			buttonStop.Text = "Stop";
			buttonStop.Click += new EventHandler(buttonStop_Click);

			// PictureBox for displaying song image
			pictureBoxSong = new PictureBox();
			pictureBoxSong.SizeMode = PictureBoxSizeMode.StretchImage;

			// Label for displaying currently playing song
			labelCurrentSong = new Label();
			labelCurrentSong.Text = "Currently Playing: None";

			// Label for displaying current playback time
			labelCurrentTime = new Label();
			labelCurrentTime.Text = "00:00";

			// Label for displaying total playback time
			labelTotalTime = new Label();
			labelTotalTime.Text = "00:00";

			// TrackBar for seeking within the song
			trackBarSeek = new TrackBar();
			trackBarSeek.TickStyle = TickStyle.None;
			trackBarSeek.MouseDown += new MouseEventHandler(trackBarSeek_MouseDown);
			trackBarSeek.MouseUp += new MouseEventHandler(trackBarSeek_MouseUp);
			trackBarSeek.Scroll += new EventHandler(trackBarSeek_Scroll);

			// ToolTip for displaying song details
			toolTip = new ToolTip();

			// Timer to update TrackBar
			timer = new Timer();
			timer.Interval = 500; // Update every 500 milliseconds
			timer.Tick += new EventHandler(timer_Tick);

			// Add controls to the form
			Controls.Add(textBoxQuery);
			Controls.Add(buttonSearch);
			Controls.Add(listBoxResults);
			Controls.Add(buttonPlay);
			Controls.Add(buttonPause);
			Controls.Add(buttonStop);
			Controls.Add(pictureBoxSong);
			Controls.Add(labelCurrentSong);
			Controls.Add(labelCurrentTime);
			Controls.Add(labelTotalTime);
			Controls.Add(trackBarSeek);

			// Set the layout of the form
			this.Load += new EventHandler(Form1_Load);
			this.Resize += new EventHandler(Form1_Resize);
			ArrangeControls();
		}

		private void ArrangeControls()
		{
			int margin = 10;

			// Arrange the search box and button
			textBoxQuery.SetBounds(margin, margin, ClientSize.Width - 2 * margin - 80, 30);
			buttonSearch.SetBounds(ClientSize.Width - margin - 80, margin, 80, 30);

			// Arrange the list box
			listBoxResults.SetBounds(margin, textBoxQuery.Bottom + margin, ClientSize.Width - 2 * margin - 220, ClientSize.Height - textBoxQuery.Bottom - 4 * margin - 140);

			// Arrange the picture box
			pictureBoxSong.SetBounds(listBoxResults.Right + margin, textBoxQuery.Bottom + margin, 210, 210);

			// Arrange the current song label
			labelCurrentSong.SetBounds(margin, listBoxResults.Bottom + margin, ClientSize.Width - 2 * margin, 20);

			// Arrange the track bar and time labels
			labelCurrentTime.SetBounds(margin, labelCurrentSong.Bottom + margin, 60, 20);
			trackBarSeek.SetBounds(labelCurrentTime.Right + margin, labelCurrentSong.Bottom + margin, ClientSize.Width - 4 * margin - 120, 20);
			labelTotalTime.SetBounds(trackBarSeek.Right + margin, labelCurrentSong.Bottom + margin, 60, 20);

			// Arrange the play, pause, and stop buttons
			buttonPlay.SetBounds(margin, trackBarSeek.Bottom + margin, 75, 30);
			buttonPause.SetBounds(buttonPlay.Right + margin, trackBarSeek.Bottom + margin, 75, 30);
			buttonStop.SetBounds(buttonPause.Right + margin, trackBarSeek.Bottom + margin, 75, 30);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			ArrangeControls();
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			ArrangeControls();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Enter && textBoxQuery.Focused)
			{
				buttonSearch.PerformClick();
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private async void buttonSearch_Click(object sender, EventArgs e)
		{
			await SearchSongs();
		}

		private async void textBoxQuery_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				await SearchSongs();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private async void listBoxResults_DoubleClick(object sender, EventArgs e)
		{
			await PlaySelectedSong();
		}

		private async void listBoxResults_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				await PlaySelectedSong();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private void listBoxResults_Click(object sender, EventArgs e)
		{
			DisplaySongImage();
		}

		private async Task SearchSongs()
		{
			string query = textBoxQuery.Text;
			if (string.IsNullOrWhiteSpace(query))
			{
				MessageBox.Show("Please enter a search query.");
				return;
			}

			try
			{
				_songs = await FetchSongsAsync(query);
				if (_songs.Count == 0)
				{
					MessageBox.Show("No songs found.");
					return;
				}

				listBoxResults.Items.Clear();
				foreach (var song in _songs)
				{
					listBoxResults.Items.Add(song.Name);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}");
			}
		}

		private async void buttonPlay_Click(object sender, EventArgs e)
		{
			await PlaySelectedSong();
		}

		private async Task PlaySelectedSong()
		{
			if (listBoxResults.SelectedIndex == -1)
			{
				MessageBox.Show("Please select a song to play.");
				return;
			}

			try
			{
				string youtubeVideoUrl = _songs[listBoxResults.SelectedIndex].Id;
				await PlaySongAsync(youtubeVideoUrl);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred: {ex.Message}");
			}
		}

		private void buttonPause_Click(object sender, EventArgs e)
		{
			if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
			{
				outputDevice.Pause();
				((Button)sender).Text = "Resume";
			}
			else if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Paused)
			{
				outputDevice.Play();
				((Button)sender).Text = "Pause";
			}
		}

		private void buttonStop_Click(object sender, EventArgs e)
		{
			StopPlayback();
		}

		private void StopPlayback()
		{
			if (outputDevice != null)
			{
				outputDevice.Stop();
				outputDevice.Dispose();
				outputDevice = null;

				if (audioFile != null)
				{
					audioFile.Dispose();
					audioFile = null;
				}

				if (currentFilePath != null && File.Exists(currentFilePath))
				{
					File.Delete(currentFilePath);
					currentFilePath = null;
				}

				// Reset the pause button text
				buttonPause.Text = "Pause";

				// Reset the label and TrackBar
				labelCurrentSong.Text = "Currently Playing: None";
				labelCurrentTime.Text = "00:00";
				labelTotalTime.Text = "00:00";
				trackBarSeek.Value = 0;
				trackBarSeek.Maximum = 100;
				timer.Stop();
			}
		}

		private async Task<List<Song>> FetchSongsAsync(string query)
		{
			IEnumerable<Song> searchResults = await _client.SearchAsync<Song>(query);
			return searchResults.ToList();
		}

		private async Task PlaySongAsync(string youtubeVideoUrl)
		{
			// Stop any current playback
			StopPlayback();

			var youtube = new YoutubeClient();
			var videoId = YoutubeExplode.Videos.VideoId.Parse(youtubeVideoUrl);

			var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
			var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

			if (audioStreamInfo == null)
			{
				MessageBox.Show("No audio stream found.");
				return;
			}

			currentFilePath = Path.GetTempFileName() + ".mp3";
			await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, currentFilePath);

			audioFile = new AudioFileReader(currentFilePath);
			outputDevice = new WaveOutEvent();
			outputDevice.Init(audioFile);
			outputDevice.Play();

			// Update label and TrackBar
			labelCurrentSong.Text = $"Currently Playing: {_songs[listBoxResults.SelectedIndex].Name}";
			trackBarSeek.Maximum = (int)audioFile.TotalTime.TotalSeconds;
			trackBarSeek.Value = 0;
			labelTotalTime.Text = audioFile.TotalTime.ToString(@"mm\:ss");
			timer.Start();

			outputDevice.PlaybackStopped += (s, a) =>
			{
				StopPlayback();
			};
		}

		private void listBoxResults_MouseMove(object sender, MouseEventArgs e)
		{
			int index = listBoxResults.IndexFromPoint(e.Location);
			if (index != ListBox.NoMatches)
			{
				string toolTipText = $"{_songs[index].Name}, {string.Join(", ", _songs[index].Artists.Select(artist => artist.Name))} - {_songs[index].Album.Name}";
				if (toolTip.GetToolTip(listBoxResults) != toolTipText)
				{
					toolTip.SetToolTip(listBoxResults, toolTipText);
				}
			}
		}

		private void DisplaySongImage()
		{
			if (listBoxResults.SelectedIndex != -1)
			{
				var selectedSong = _songs[listBoxResults.SelectedIndex];
				if (selectedSong.Thumbnails != null && selectedSong.Thumbnails.Any())
				{
					pictureBoxSong.ImageLocation = selectedSong.Thumbnails.FirstOrDefault()?.Url;
				}
				else
				{
					pictureBoxSong.Image = null;
				}
			}
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (audioFile != null && outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing && !isSeeking)
			{
				int currentTime = (int)audioFile.CurrentTime.TotalSeconds;
				if (currentTime <= trackBarSeek.Maximum)
				{
					trackBarSeek.Value = currentTime;
					labelCurrentTime.Text = audioFile.CurrentTime.ToString(@"mm\:ss");
				}
			}
		}

		private void trackBarSeek_MouseDown(object sender, MouseEventArgs e)
		{
			isSeeking = true;
			UpdateTrackBarValueFromMouse(e);
		}

		private void trackBarSeek_MouseUp(object sender, MouseEventArgs e)
		{
			UpdateTrackBarValueFromMouse(e);
			isSeeking = false;
			if (audioFile != null && outputDevice != null)
			{
				audioFile.CurrentTime = TimeSpan.FromSeconds(trackBarSeek.Value);
			}
		}

		private void trackBarSeek_Scroll(object sender, EventArgs e)
		{
			if (isSeeking && audioFile != null && outputDevice != null)
			{
				audioFile.CurrentTime = TimeSpan.FromSeconds(trackBarSeek.Value);
				labelCurrentTime.Text = audioFile.CurrentTime.ToString(@"mm\:ss");
			}
		}

		private void UpdateTrackBarValueFromMouse(MouseEventArgs e)
		{
			int mouseX = e.X;
			double ratio = Math.Max(0, Math.Min(1, (double)mouseX / trackBarSeek.Width));
			int newPosition = (int)(trackBarSeek.Maximum * ratio);
			trackBarSeek.Value = newPosition;
			labelCurrentTime.Text = TimeSpan.FromSeconds(trackBarSeek.Value).ToString(@"mm\:ss");
		}

	}
}
