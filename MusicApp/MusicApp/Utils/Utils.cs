using MusicApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTubeMusicAPI.Models;

namespace MusicApp.Utils
{
	internal class Utils
	{
		public static string SongToString(MySong song)
		{
			return $" {song.Artists.First().Name} - {song.Name}";
		}
	}
}
