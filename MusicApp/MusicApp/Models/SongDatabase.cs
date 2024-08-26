using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

public class SongDatabase
{
	private readonly string _connectionString;

	/// <summary>
	/// Initializes a new instance of the <see cref="SongDatabase"/> class.
	/// </summary>
	/// <param name="databasePath">The path to the SQLite database file.</param>
	public SongDatabase(string databasePath)
	{
		_connectionString = $"Data Source={databasePath};";
		CreateTable();
	}

	/// <summary>
	/// Creates the Songs table in the database if it doesn't exist.
	/// </summary>
	private void CreateTable()
	{
		using (var connection = new SQLiteConnection(_connectionString))
		{
			connection.Open();

			string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Songs (
                    Id TEXT PRIMARY KEY,
                    Count INTEGER DEFAULT 0
                )";

			using (var command = new SQLiteCommand(createTableQuery, connection))
			{
				command.ExecuteNonQuery();
			}
		}
	}

	/// <summary>
	/// Inserts a new song into the Songs table with an initial count of 0.
	/// If the song already exists, the operation is ignored.
	/// </summary>
	/// <param name="id">The unique identifier of the song.</param>
	public void InsertSong(string id)
	{
		using (var connection = new SQLiteConnection(_connectionString))
		{
			connection.Open();

			string insertQuery = @"
                INSERT INTO Songs (Id, Count)
                VALUES (@id, 0)
                ON CONFLICT(Id) DO NOTHING";

			using (var command = new SQLiteCommand(insertQuery, connection))
			{
				command.Parameters.AddWithValue("@id", id);
				command.ExecuteNonQuery();
			}
		}
	}

	/// <summary>
	/// Increments the count of a song in the Songs table by 1.
	/// </summary>
	/// <param name="id">The unique identifier of the song.</param>
	public void UpdateSongCount(string id)
	{
		using (var connection = new SQLiteConnection(_connectionString))
		{
			connection.Open();

			string updateQuery = @"
                UPDATE Songs 
                SET Count = Count + 1 
                WHERE Id = @id";

			using (var command = new SQLiteCommand(updateQuery, connection))
			{
				command.Parameters.AddWithValue("@id", id);
				command.ExecuteNonQuery();
			}
		}
	}

	/// <summary>
	/// Retrieves the current count of a song from the Songs table.
	/// </summary>
	/// <param name="id">The unique identifier of the song.</param>
	/// <returns>The count of the song, or 0 if the song does not exist.</returns>
	public int GetSongCount(string id)
	{
		using (var connection = new SQLiteConnection(_connectionString))
		{
			connection.Open();

			string selectQuery = "SELECT Count FROM Songs WHERE Id = @id";

			using (var command = new SQLiteCommand(selectQuery, connection))
			{
				command.Parameters.AddWithValue("@id", id);
				var result = command.ExecuteScalar();
				return result != null ? Convert.ToInt32(result) : 0;
			}
		}
	}

	/// <summary>
	/// Retrieves 5 songs with the lowest count and 5 random songs from the Songs table.
	/// Also increments the count of each selected song by 1.
	/// </summary>
	/// <returns>A list of song IDs for recommendation.</returns>
	public List<string> GetSongsForRecommendation()
	{
		using (var connection = new SQLiteConnection(_connectionString))
		{
			connection.Open();

			// Get 5 songs with the lowest count
			string queryLowestCount = @"
			SELECT Id 
			FROM Songs 
			ORDER BY Count ASC 
			LIMIT 5";

			var songIds = new List<string>();

			using (var command = new SQLiteCommand(queryLowestCount, connection))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						songIds.Add(reader.GetString(0));
					}
				}
			}

			// Get 5 random songs, excluding those with the lowest count
			string queryRandomSongs = @"
			SELECT Id 
			FROM Songs 
			WHERE Id NOT IN (" + string.Join(",", songIds.Select(id => $"'{id}'")) + @")
			ORDER BY RANDOM() 
			LIMIT 5";

			using (var command = new SQLiteCommand(queryRandomSongs, connection))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						songIds.Add(reader.GetString(0));
					}
				}
			}

			// Increment the count for each selected song
			string queryIncrementCount = @"
			UPDATE Songs 
			SET Count = Count + 1 
			WHERE Id = @Id";

			foreach (var songId in songIds)
			{
				using (var updateCommand = new SQLiteCommand(queryIncrementCount, connection))
				{
					updateCommand.Parameters.AddWithValue("@Id", songId);
					updateCommand.ExecuteNonQuery();
				}
			}

			return songIds;
		}
	}

	/// <summary>
	/// Increases the count of a song by 1.
	/// </summary>
	/// <param name="id">The unique identifier of the song.</param>
	public void IncreaseSongCount(string id)
	{
		UpdateSongCount(id);
	}
}
