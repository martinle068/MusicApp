using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Info;
using static MusicApp.Utils.Utils;

namespace MusicApp.Models
{
	/// <summary>
	/// Represents a song with various details such as name, artist, duration, and thumbnail.
	/// </summary>
	public class MySong
	{
		/// <summary>
		/// Gets the name of the song.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the unique identifier of the song.
		/// </summary>
		public string? Id { get; }

		/// <summary>
		/// Gets a formatted string containing the artist(s) and the song name.
		/// </summary>
		public string ArtistAndSongName { get; }

		/// <summary>
		/// Gets an array of <see cref="ShelfItem"/> representing the artists of the song.
		/// </summary>
		public ShelfItem[] Artists { get; }

		/// <summary>
		/// Gets a string representing the names of the artists, joined by commas.
		/// </summary>
		public string ArtistsString => string.Join(", ", Artists.Select(artist => artist.Name));

		/// <summary>
		/// Gets the duration of the song.
		/// </summary>
		public TimeSpan Duration { get; }

		/// <summary>
		/// Gets the URL of the song's thumbnail.
		/// </summary>
		public string? Thumbnail { get; private set; }

		private string? _playlistId { get; } = null;

		/// <summary>
		/// Asynchronously retrieves the playlist ID associated with the song, if available.
		/// </summary>
		/// <returns>The playlist ID as a string, or null if not available.</returns>
		public async Task<string?> GetPlaylistIdAsync()
		{
			if (_playlistId != null)
			{
				return _playlistId;
			}

			var song = await MyYouTubeService.FetchSongAsync(ArtistAndSongName);

			return song?._playlistId;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySong"/> class based on a <see cref="Song"/> object.
		/// </summary>
		/// <param name="song">The <see cref="Song"/> object to initialize from.</param>
		private MySong(Song song)
		{
			Name = song.Name;
			Id = song.Id;
			Artists = song.Artists;
			_playlistId = song.Radio.PlaylistId;
			Duration = song.Duration;
			Thumbnail = song.Thumbnails.LastOrDefault()?.Url;
			ArtistAndSongName = GetArtistAndSongNameString();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySong"/> class with specified parameters.
		/// </summary>
		/// <param name="name">The name of the song.</param>
		/// <param name="id">The unique identifier of the song.</param>
		/// <param name="artists">An array of <see cref="ShelfItem"/> representing the artists of the song.</param>
		/// <param name="duration">The duration of the song.</param>
		/// <param name="thumbnail">The URL of the song's thumbnail.</param>
		/// <param name="playlistId">The playlist ID associated with the song, if available.</param>
		private MySong(string name, string? id, ShelfItem[] artists, TimeSpan duration, string? thumbnail, string? playlistId = null)
		{
			Name = name;
			Id = id;
			Artists = artists;
			_playlistId = playlistId;
			Duration = duration;
			Thumbnail = thumbnail;
			ArtistAndSongName = GetArtistAndSongNameString();
		}

		/// <summary>
		/// Creates a new <see cref="MySong"/> instance from a <see cref="SongVideoInfo"/> object.
		/// </summary>
		/// <param name="songVideoInfo">The <see cref="SongVideoInfo"/> object to create from.</param>
		/// <returns>A new instance of <see cref="MySong"/>.</returns>
		public static MySong Create(SongVideoInfo songVideoInfo)
		{
			MySong mySong = new MySong(songVideoInfo.Name, songVideoInfo.Id, songVideoInfo.Artists, songVideoInfo.Duration, songVideoInfo.Thumbnails.LastOrDefault()?.Url);

			return mySong;
		}

		/// <summary>
		/// Creates a new <see cref="MySong"/> instance from a <see cref="CommunityPlaylistSongInfo"/> object.
		/// </summary>
		/// <param name="communityPlaylistSongInfo">The <see cref="CommunityPlaylistSongInfo"/> object to create from.</param>
		/// <returns>A new instance of <see cref="MySong"/>, or null if the input is null.</returns>
		public static MySong? Create(CommunityPlaylistSongInfo communityPlaylistSongInfo)
		{
			if (communityPlaylistSongInfo == null)
			{
				return null;
			}
			MySong mySong = new MySong(communityPlaylistSongInfo.Name, communityPlaylistSongInfo.Id, communityPlaylistSongInfo.Artists, communityPlaylistSongInfo.Duration, communityPlaylistSongInfo.Thumbnails.LastOrDefault()?.Url);

			return mySong;
		}

		/// <summary>
		/// Creates a new <see cref="MySong"/> instance from a <see cref="Song"/> object.
		/// </summary>
		/// <param name="song">The <see cref="Song"/> object to create from.</param>
		/// <returns>A new instance of <see cref="MySong"/>.</returns>
		public static MySong Create(Song song)
		{
			var mySong = new MySong(song);

			return mySong;
		}

		/// <summary>
		/// Generates a formatted string representing the artist(s) and song name.
		/// </summary>
		/// <returns>A string in the format "Artist(s) - Song Name".</returns>
		private string GetArtistAndSongNameString()
		{
			return $" {string.Join(", ", Artists.Select(artist => artist.Name))} - {Name}";
		}
	}
}
