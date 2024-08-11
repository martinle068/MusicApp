using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Models
{
    public class MyShelf
    {
        public ObservableCollection<MySong> Songs { get; set; }
        public string? ContinuationToken { get; set; }

        public MyShelf(ObservableCollection<MySong> songs, string? continuationToken)
		{
			Songs = songs;
			ContinuationToken = continuationToken;
		}
    }
}
