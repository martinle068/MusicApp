using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Models
{
	/// <summary>
	/// Represents a collection of items of type <typeparamref name="T"/> with an optional continuation token for pagination.
	/// </summary>
	/// <typeparam name="T">The type of items stored in the shelf. Must be a reference type.</typeparam>
	public class MyShelf<T> where T : class
	{
		/// <summary>
		/// Gets or sets the collection of items.
		/// </summary>
		public ObservableCollection<T> Items { get; set; }

		/// <summary>
		/// Gets or sets the continuation token, which can be used for paginated data fetching.
		/// </summary>
		public string? ContinuationToken { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MyShelf{T}"/> class with the specified items and continuation token.
		/// </summary>
		/// <param name="items">The collection of items.</param>
		/// <param name="continuationToken">The continuation token, or null if not applicable.</param>
		public MyShelf(ObservableCollection<T> items, string? continuationToken)
		{
			Items = items;
			ContinuationToken = continuationToken;
		}
	}
}
