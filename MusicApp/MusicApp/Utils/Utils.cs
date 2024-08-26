using MusicApp.Models;
using MusicApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using YouTubeMusicAPI.Models;
using Google.Apis.YouTube.v3.Data;

namespace MusicApp.Utils
{
	/// <summary>
	/// Provides an extension method to shuffle an IList.
	/// </summary>
	public static class ShuffleExtensionClass
	{
		private static Random rng = new Random();

		/// <summary>
		/// Shuffles the elements of the list in place using the Fisher-Yates algorithm.
		/// </summary>
		/// <typeparam name="T">The type of elements in the list.</typeparam>
		/// <param name="list">The list to shuffle.</param>
		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}

	/// <summary>
	/// A utility class that provides various helper methods for the MusicApp application.
	/// </summary>
	internal class Utils
	{
		/// <summary>
		/// Converts a <see cref="MySong"/> object to a string representation of the song's artist(s) and name.
		/// </summary>
		/// <param name="song">The <see cref="MySong"/> object to convert.</param>
		/// <returns>A string in the format "Artist(s) - Song Name".</returns>
		public static string SongToString(MySong song)
		{
			return $" {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Name}";
		}

		/// <summary>
		/// Retrieves the data context of the placement target of a <see cref="MenuItem"/>, cast to a specified type.
		/// </summary>
		/// <typeparam name="T">The type to cast the data context to.</typeparam>
		/// <param name="sender">The <see cref="MenuItem"/> that was clicked.</param>
		/// <returns>The data context cast to the specified type, or null if the cast fails.</returns>
		public static T? GetItemFromMenuItem<T>(object sender) where T : class
		{
			if (sender is MenuItem menuItem)
			{
				var contextMenu = menuItem.Parent as ContextMenu;

				if (contextMenu != null)
				{
					if (contextMenu.PlacementTarget is FrameworkElement placementTarget)
					{
						return placementTarget.DataContext as T;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Generates a string to display information about the current playback source.
		/// </summary>
		/// <param name="name">The name of the playback source (e.g., playlist or album name).</param>
		/// <returns>A string in the format "Playing from {name}".</returns>
		public static string GetInfoString(string name)
		{
			return $"Playing from {name}";
		}
	}
}
