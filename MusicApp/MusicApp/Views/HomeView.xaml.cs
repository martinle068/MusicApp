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
	/// <summary>
	/// Interaction logic for HomeView.xaml. This class handles the main view logic
	/// for displaying and interacting with playlists and songs in the MusicApp.
	/// </summary>
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

		/// <summary>
		/// Handles the KeyDown event for the search TextBox, triggering the search command when Enter is pressed.
		/// </summary>
		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				var viewModel = DataContext as HomeViewModel;
				viewModel?.SearchCommand.Execute(null);
			}
		}

		/// <summary>
		/// Handles the MouseWheel event for the ListView ScrollViewer to enable smooth scrolling.
		/// </summary>
		private void ListViewScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			ScrollViewer scv = (ScrollViewer)sender;
			scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta / 4);
			e.Handled = true;
		}

		/// <summary>
		/// Handles the MouseLeftButtonUp event for selecting a playlist.
		/// </summary>
		private void PlaylistListBoxItemCommand(object sender, MouseButtonEventArgs e)
		{
			var viewModel = DataContext as HomeViewModel;
			viewModel?.SelectPlaylistCommand.Execute(null);
		}

		/// <summary>
		/// Handles the MouseLeftButtonUp event for playing a song from the radio list.
		/// </summary>
		private async void RadioSongListBoxCommand(object sender, MouseButtonEventArgs e, ObservableCollection<MySong> songs, int index)
		{
			var viewModel = DataContext as HomeViewModel;
			if (viewModel != null)
			{
				await viewModel.HandleRadioSongSelection(songs, index);
			}
		}

		/// <summary>
		/// Handles the MouseLeftButtonUp event for playing a popular song.
		/// </summary>
		private void PopularSongsListBoxCommand(object sender, MouseButtonEventArgs e)
		{
			var viewModel = DataContext as HomeViewModel;
			viewModel?.SelectPopularSongCommand.Execute(null);
		}

		/// <summary>
		/// Executes the delete command for a playlist.
		/// </summary>
		private void ExecuteDeletePlaylistCommand(object sender)
		{
			var playlist = GetItemFromMenuItem<Playlist>(sender);
			if (playlist != null)
			{
				var viewModel = DataContext as HomeViewModel;
				viewModel?.DeletePlaylistCommand.Execute(playlist);
			}
		}

		/// <summary>
		/// Handles the Click event for removing a playlist using a context menu.
		/// </summary>
		private void RemovePlaylist_ComboBox(object sender, RoutedEventArgs e)
		{
			ExecuteDeletePlaylistCommand(sender);
		}

		/// <summary>
		/// Handles the ScrollChanged event for dynamically loading additional content as the user scrolls.
		/// </summary>
		private async void DynamicScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			// Check if the user is nearing the bottom of the scrollable area
			if (sender == DynamicScrollViewer && e.VerticalOffset + e.ViewportHeight >= e.ExtentHeight - 200)
			{
				if (!_isLoadingPanel)
				{
					_isLoadingPanel = true;
					await AddDynamicListBoxSectionAsync();
					_isLoadingPanel = false;
				}
			}
		}

		/// <summary>
		/// Prevents the ListBox from triggering the main ScrollViewer's event when scrolling.
		/// </summary>
		private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			e.Handled = true;
		}

		/// <summary>
		/// Adds a dynamically generated ListBox section to the main panel based on a random selection of ListBoxType.
		/// </summary>
		private async Task AddDynamicListBoxSectionAsync()
		{
			var selectedType = (ListBoxType)_random.Next(Enum.GetValues(typeof(ListBoxType)).Length);

			switch (selectedType)
			{
				case ListBoxType.RandomSongsFromAllPlaylists:
					await AddRandomSongsListBoxAsync();
					break;

				case ListBoxType.SongsFromSpecificAuthor:
					await AddSongsFromAuthorListBoxAsync();
					break;
			}
		}

		/// <summary>
		/// Adds a ListBox with a random collection of songs from all playlists to the main panel.
		/// </summary>
		private async Task AddRandomSongsListBoxAsync()
		{
			var viewModel = DataContext as HomeViewModel;
			if (viewModel == null || viewModel.AllPlaylistSongs == null || !viewModel.AllPlaylistSongs.Any())
				return;

			var randomSongs = await GetRandomSongs();
			if (randomSongs == null || randomSongs.Count == 0) return;

			var textBlock = new TextBlock
			{
				Text = "Your Favourite Songs",
				FontWeight = FontWeights.Bold,
				FontSize = 24,
				Foreground = new SolidColorBrush(Colors.White),
				Margin = new Thickness(10, 0, 0, 0)
			};
			MainPanel.Children.Add(textBlock);

			var listBox = CreateListBox(randomSongs);
			MainPanel.Children.Add(listBox);
		}

		/// <summary>
		/// Adds a ListBox with songs from a specific author to the main panel.
		/// </summary>
		private async Task AddSongsFromAuthorListBoxAsync()
		{
			var viewModel = DataContext as HomeViewModel;
			if (viewModel == null || viewModel.AllPlaylistSongs == null || !viewModel.AllPlaylistSongs.Any())
				return;

			var randomAuthor = GetRandomAuthor(viewModel.AllPlaylistSongs);
			if (randomAuthor == null) return;

			var authorSongs = await MyYouTubeService.FetchSongsAsync(randomAuthor, 10);
			if (authorSongs == null || authorSongs.Count == 0) return;

			var textBlock = new TextBlock
			{
				Text = $"Songs by {randomAuthor}",
				FontWeight = FontWeights.Bold,
				FontSize = 24,
				Foreground = new SolidColorBrush(Colors.White),
				Margin = new Thickness(10, 0, 0, 0)
			};
			MainPanel.Children.Add(textBlock);

			var listBox = CreateListBox(authorSongs);
			MainPanel.Children.Add(listBox);
		}

		/// <summary>
		/// Creates a ListBox with the specified songs and sets up the necessary event handlers and templates.
		/// </summary>
		private ListBox CreateListBox(ObservableCollection<MySong> songs)
		{
			var listBox = new ListBox
			{
				BorderThickness = new Thickness(0),
				Background = new SolidColorBrush(Colors.Transparent),
				SelectedIndex = -1,
				ItemsSource = songs
			};

			listBox.PreviewMouseLeftButtonUp += (s, e) => RadioSongListBoxCommand(s, e, songs, listBox.SelectedIndex);
			listBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ListBox_ScrollChanged), true);

			var itemsPanelTemplate = new ItemsPanelTemplate
			{
				VisualTree = new FrameworkElementFactory(typeof(StackPanel))
			};
			itemsPanelTemplate.VisualTree.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
			listBox.ItemsPanel = itemsPanelTemplate;

			var dataTemplate = CreateDataTemplate();
			listBox.ItemTemplate = dataTemplate;

			return listBox;
		}

		/// <summary>
		/// Creates a DataTemplate for the items in the dynamically generated ListBoxes.
		/// </summary>
		private DataTemplate CreateDataTemplate()
		{
			var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
			stackPanelFactory.SetValue(StackPanel.MarginProperty, new Thickness(10));
			stackPanelFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);

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

			var rectangleGeometry = new RectangleGeometry(new Rect(0, 0, 120, 120), 10, 10);
			imageFactory.SetValue(Image.ClipProperty, rectangleGeometry);

			borderFactory.AppendChild(imageFactory);
			viewboxFactory.AppendChild(borderFactory);
			stackPanelFactory.AppendChild(viewboxFactory);

			var textBlockNameFactory = new FrameworkElementFactory(typeof(TextBlock));
			textBlockNameFactory.SetBinding(TextBlock.TextProperty, new Binding("Name"));
			textBlockNameFactory.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.White));
			textBlockNameFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
			textBlockNameFactory.SetValue(TextBlock.WidthProperty, 130.0);
			textBlockNameFactory.SetValue(TextBlock.MarginProperty, new Thickness(5, 10, 5, 0));
			stackPanelFactory.AppendChild(textBlockNameFactory);

			var textBlockArtistsFactory = new FrameworkElementFactory(typeof(TextBlock));
			textBlockArtistsFactory.SetBinding(TextBlock.TextProperty, new Binding("ArtistsString"));
			textBlockArtistsFactory.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.White));
			textBlockArtistsFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
			textBlockArtistsFactory.SetValue(TextBlock.WidthProperty, 130.0);
			textBlockArtistsFactory.SetValue(TextBlock.MarginProperty, new Thickness(5, 10, 5, 0));
			stackPanelFactory.AppendChild(textBlockArtistsFactory);

			var contextMenuSetter = new Setter(ContextMenuService.ContextMenuProperty, CreateContextMenu());
			stackPanelFactory.SetValue(FrameworkElement.ContextMenuProperty, CreateContextMenu());

			var dataTemplate = new DataTemplate { VisualTree = stackPanelFactory };
			return dataTemplate;
		}

		/// <summary>
		/// Creates a context menu with an "Add to Playlist" option for each song item.
		/// </summary>
		private ContextMenu CreateContextMenu()
		{
			var contextMenu = new ContextMenu();
			var addMenuItem = new MenuItem { Header = "Add to Playlist" };
			addMenuItem.Click += AddSongToPlaylist_ComboBox;
			contextMenu.Items.Add(addMenuItem);
			return contextMenu;
		}

		/// <summary>
		/// Gets a collection of random songs from the database for recommendation.
		/// </summary>
		private async Task<ObservableCollection<MySong>?> GetRandomSongs()
		{
			var viewModel = DataContext as HomeViewModel;
			var randomSongIds = viewModel?._mainViewModel.SongDatabase?.GetSongsForRecommendation();
			if (randomSongIds == null || randomSongIds.Count == 0) return null;

			var fetchTasks = randomSongIds.Select(async songId =>
			{
				return MySong.Create(await MyYouTubeService.FetchSongVideoInfoAsync(songId));
			});

			var songs = await Task.WhenAll(fetchTasks);

			if (songs == null || songs.Length == 0) return null;

			var randomSongs = new ObservableCollection<MySong>(songs.Where(song => song != null));

			return randomSongs;
		}

		/// <summary>
		/// Gets a random author from the list of available songs.
		/// </summary>
		private string? GetRandomAuthor(IEnumerable<MySong> songs)
		{
			var authors = songs.SelectMany(song => song.Artists.Select(artist => artist.Name)).Distinct().ToList();
			if (authors.Count == 0) return null;
			return authors[_random.Next(authors.Count)];
		}

		/// <summary>
		/// Executes the command to add a song to a playlist from the context menu.
		/// </summary>
		private void ExecuteAddSongToPlaylist(object sender)
		{
			var song = Utils.Utils.GetItemFromMenuItem<MySong>(sender);

			if (song != null)
			{
				var viewModel = DataContext as HomeViewModel;
				viewModel?._mainViewModel.PlayerViewModel.AddSongToPlaylistCommand.Execute(song);
			}
		}

		/// <summary>
		/// Handles the Click event for adding a song to a playlist via the context menu.
		/// </summary>
		private void AddSongToPlaylist_ComboBox(object sender, RoutedEventArgs e)
		{
			ExecuteAddSongToPlaylist(sender);
		}
	}
}
