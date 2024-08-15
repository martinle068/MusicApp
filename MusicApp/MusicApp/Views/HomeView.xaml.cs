using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Google.Apis.YouTube.v3.Data;
using static MusicApp.Utils.Utils;
using System.Windows.Media;
using System.Windows.Data;
using System.Threading.Tasks;
using MusicApp.ViewModels;
using System.Collections.ObjectModel;
using MusicApp.Models;
using YouTubeMusicAPI.Models;

namespace MusicApp.Views
{
	public partial class HomeView : UserControl
	{
		public enum ListBoxType
		{
			RandomSongsFromAllPlaylists,
			SongsFromSpecificAuthor
		}

		private bool _isLoadingPanel = false;
		private readonly Random _random = new();

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

		private void ListViewScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			ScrollViewer scv = (ScrollViewer)sender;
			scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta / 4);
			e.Handled = true;
		}

		private void PlaylistListBoxItemCommand(object sender, MouseButtonEventArgs e)
		{
			var viewModel = DataContext as HomeViewModel;
			viewModel?.SelectPlaylistCommand.Execute(null);
		}

		private async void RadioSongListBoxCommand(object sender, MouseButtonEventArgs e, ObservableCollection<MySong> songs, int index)
		{
			var viewModel = DataContext as HomeViewModel;
			if (viewModel != null)
			{
				await viewModel.HandleRadioSongSelection(songs, index);
			}
		}

		private void PopularSongsListBoxCommand(object sender, MouseButtonEventArgs e)
		{
			var viewModel = DataContext as HomeViewModel;
			viewModel?.SelectPopularSongCommand.Execute(null);
		}

		private void ExecuteDeletePlaylistCommand(object sender)
		{
			var playlist = GetItemFromMenuItem<Playlist>(sender);
			if (playlist != null)
			{
				var viewModel = DataContext as HomeViewModel;
				viewModel?.DeletePlaylistCommand.Execute(playlist);
			}
		}

		private void RemovePlaylist_ComboBox(object sender, RoutedEventArgs e)
		{
			ExecuteDeletePlaylistCommand(sender);
		}

		private async void DynamicScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			// Check if the user is nearing the bottom of the scrollable area
			if (sender == DynamicScrollViewer && e.VerticalOffset + e.ViewportHeight >= e.ExtentHeight - 100)
			{
				if (!_isLoadingPanel)
				{
					_isLoadingPanel = true;
					await AddDynamicListBoxSectionAsync();
					_isLoadingPanel = false;
				}
			}
		}

		// Ensure ListBox scrolling does not trigger the main ScrollViewer's event
		private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			e.Handled = true;
		}


		private async Task AddDynamicListBoxSectionAsync()
		{

			// Randomly select a ListBoxType
			var selectedType = (ListBoxType)_random.Next(Enum.GetValues(typeof(ListBoxType)).Length);

			// Generate the appropriate ListBox based on the selected type
			switch (selectedType)
			{
				case ListBoxType.RandomSongsFromAllPlaylists:
					AddRandomSongsListBox();
					break;

				case ListBoxType.SongsFromSpecificAuthor:
					await AddSongsFromAuthorListBoxAsync();
					break;
			}
		}

		private void AddRandomSongsListBox()
		{
			var viewModel = DataContext as HomeViewModel;
			if (viewModel == null || viewModel.AllPlaylistSongs == null || !viewModel.AllPlaylistSongs.Any())
				return;

			// Get a random collection of 10 songs
			var randomSongs = GetRandomSongs(viewModel.AllPlaylistSongs, 10);

			// Add a new TextBlock for the new "Random Songs" section
			var textBlock = new TextBlock
			{
				Text = "Your Favourite Songs",
				FontWeight = FontWeights.Bold,
				FontSize = 24,
				Foreground = new SolidColorBrush(Colors.White),
				Margin = new Thickness(10, 0, 0, 0)
			};
			MainPanel.Children.Add(textBlock);

			// Add a new ListBox for the random songs
			var listBox = CreateListBox(randomSongs);
			MainPanel.Children.Add(listBox);
		}

		private async Task AddSongsFromAuthorListBoxAsync()
		{
			var viewModel = DataContext as HomeViewModel;
			if (viewModel == null || viewModel.AllPlaylistSongs == null || !viewModel.AllPlaylistSongs.Any())
				return;

			// Get a random author from the available songs
			var randomAuthor = GetRandomAuthor(viewModel.AllPlaylistSongs);
			if (randomAuthor == null) return;

			// Get songs from this author
			var authorSongs = await MyYouTubeService.FetchSongsAsync(randomAuthor, 10);
			if (authorSongs == null || authorSongs.Count == 0) return;

			// Add a new TextBlock for the new "Songs from Author" section
			var textBlock = new TextBlock
			{
				Text = $"Songs by {randomAuthor}",
				FontWeight = FontWeights.Bold,
				FontSize = 24,
				Foreground = new SolidColorBrush(Colors.White),
				Margin = new Thickness(10, 0, 0, 0)
			};
			MainPanel.Children.Add(textBlock);

			// Add a new ListBox for the author's songs
			var listBox = CreateListBox(authorSongs);
			MainPanel.Children.Add(listBox);
		}

		private ListBox CreateListBox(ObservableCollection<MySong> songs)
		{
			var listBox = new ListBox
			{
				BorderThickness = new Thickness(0),
				Background = new SolidColorBrush(Colors.Transparent),
				SelectedIndex = -1,
				ItemsSource = songs
			};

			// Bind the SelectedIndex to SelectedPlaylistIndex (or similar)
			listBox.SetBinding(ListBox.SelectedIndexProperty, new Binding("SelectedPlaylistIndex") { Source = this.DataContext });

			// Attach the MouseLeftButtonUp event handler for playing songs
			listBox.PreviewMouseLeftButtonUp += (s, e) => RadioSongListBoxCommand(s, e, songs, listBox.SelectedIndex);

			// Prevent ListBox scrolling from triggering DynamicScrollViewer's event
			listBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ListBox_ScrollChanged), true);

			// Create and set the ItemsPanelTemplate
			var itemsPanelTemplate = new ItemsPanelTemplate
			{
				VisualTree = new FrameworkElementFactory(typeof(StackPanel))
			};
			itemsPanelTemplate.VisualTree.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
			listBox.ItemsPanel = itemsPanelTemplate;

			// Create the DataTemplate for the ListBox items
			var dataTemplate = CreateDataTemplate();
			listBox.ItemTemplate = dataTemplate;

			return listBox;
		}

		private DataTemplate CreateDataTemplate()
		{
			var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
			stackPanelFactory.SetValue(StackPanel.MarginProperty, new Thickness(10));
			stackPanelFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);

			// Image inside Viewbox
			var viewboxFactory = new FrameworkElementFactory(typeof(Viewbox));
			viewboxFactory.SetValue(Viewbox.HorizontalAlignmentProperty, HorizontalAlignment.Center);
			viewboxFactory.SetValue(Viewbox.VerticalAlignmentProperty, VerticalAlignment.Center);

			var borderFactory = new FrameworkElementFactory(typeof(Border));
			borderFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(51, 51, 51)));
			borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
			borderFactory.SetValue(Border.WidthProperty, 120.0);
			borderFactory.SetValue(Border.HeightProperty, 120.0);
			borderFactory.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Center);

			var imageFactory = new FrameworkElementFactory(typeof(Image));
			imageFactory.SetBinding(Image.SourceProperty, new Binding("Thumbnail"));
			imageFactory.SetValue(Image.WidthProperty, 120.0);
			imageFactory.SetValue(Image.HeightProperty, 120.0);
			imageFactory.SetValue(Image.StretchProperty, Stretch.UniformToFill);

			// Set the Clip property directly
			var rectangleGeometry = new RectangleGeometry(new Rect(0, 0, 120, 120), 10, 10);
			imageFactory.SetValue(Image.ClipProperty, rectangleGeometry);

			borderFactory.AppendChild(imageFactory);
			viewboxFactory.AppendChild(borderFactory);
			stackPanelFactory.AppendChild(viewboxFactory);

			// Song Name
			var textBlockNameFactory = new FrameworkElementFactory(typeof(TextBlock));
			textBlockNameFactory.SetBinding(TextBlock.TextProperty, new Binding("Name"));
			textBlockNameFactory.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.White));
			textBlockNameFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
			textBlockNameFactory.SetValue(TextBlock.WidthProperty, 130.0);
			textBlockNameFactory.SetValue(TextBlock.MarginProperty, new Thickness(5, 10, 5, 0));
			stackPanelFactory.AppendChild(textBlockNameFactory);

			// Artists String
			var textBlockArtistsFactory = new FrameworkElementFactory(typeof(TextBlock));
			textBlockArtistsFactory.SetBinding(TextBlock.TextProperty, new Binding("ArtistsString"));
			textBlockArtistsFactory.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.White));
			textBlockArtistsFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
			textBlockArtistsFactory.SetValue(TextBlock.WidthProperty, 130.0);
			textBlockArtistsFactory.SetValue(TextBlock.MarginProperty, new Thickness(5, 10, 5, 0));
			stackPanelFactory.AppendChild(textBlockArtistsFactory);

			var dataTemplate = new DataTemplate { VisualTree = stackPanelFactory };
			return dataTemplate;
		}

		private ObservableCollection<MySong> GetRandomSongs(IEnumerable<MySong> songs, int count)
		{
			var randomSongs = songs.OrderBy(x => _random.Next()).Take(count);
			return new ObservableCollection<MySong>(randomSongs);
		}

		private string? GetRandomAuthor(IEnumerable<MySong> songs)
		{
			var authors = songs.SelectMany(song => song.Artists.Select(artist => artist.Name)).Distinct().ToList();
			if (authors.Count == 0) return null;
			return authors[_random.Next(authors.Count)];
		}
	}
}
