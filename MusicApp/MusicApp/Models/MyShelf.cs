using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Models
{
    public class MyShelf<T> where T : class
    {
        public ObservableCollection<T> Items { get; set; }
        public string? ContinuationToken { get; set; }

        public MyShelf(ObservableCollection<T> items, string? continuationToken)
		{
			Items = items;
			ContinuationToken = continuationToken;
		}
    }
}
