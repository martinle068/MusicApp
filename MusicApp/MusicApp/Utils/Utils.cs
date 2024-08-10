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
	public static class ShuffleExtensionClass
	{
		private static Random rng = new Random();

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
	
	internal class Utils
	{
		public static string SongToString(MySong song)
		{
			return $" {song.Artists.First().Name} - {song.Name}";
		}

		public static Playlist? GetPlaylistFromMenuItem(object sender)
		{
			if (sender is MenuItem menuItem)
			{
				var contextMenu = menuItem.Parent as ContextMenu;

				if (contextMenu != null)
				{
					if (contextMenu.PlacementTarget is FrameworkElement placementTarget)
					{
						return placementTarget.DataContext as Playlist;
					}
				}
			}
			return null;
		}
	}
}
