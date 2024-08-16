using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

public class SongDatabase
{
	private readonly string _connectionString;

	public SongDatabase(string databasePath)
	{
		_connectionString = $"Data Source={databasePath};";
		CreateTable();
	}

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

	public void IncreaseSongCount(string id)
	{
		UpdateSongCount(id);
	}
}
