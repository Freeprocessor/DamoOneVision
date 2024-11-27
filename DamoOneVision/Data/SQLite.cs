using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

using DamoOneVision.Models;

namespace DamoOneVision.Data
{



	internal class SQLiteHelper
	{
		private readonly string connectionString;

		public SQLiteHelper( string dbPath )
		{
			connectionString = $"Data Source={dbPath};Version=3;";
			InitializeDatabase();
		}

		private void InitializeDatabase( )
		{
			using(var connection = new SQLiteConnection( connectionString ))
			{
				connection.Open();

				string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Models (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Data TEXT NOT NULL
                );";

				using (var command = new SQLiteCommand( createTableQuery, connection ))
				{
					command.ExecuteNonQuery();
				}
			}
		}

		public void SaveModel( string name, string data )
		{
			using (var connection = new SQLiteConnection( connectionString ))
			{
				connection.Open();
				string insertQuery = @"
				INSERT INTO Models (Name, Data)
				VALUES (@Name, @Data);";
				using (var command = new SQLiteCommand( insertQuery, connection ))
				{
					command.Parameters.AddWithValue( "@Name", name );
					command.Parameters.AddWithValue( "@Data", data );
					command.ExecuteNonQuery();
				}
			}
		}

		public List<ModelItem> GetModelList( )
		{
			var models = new List<ModelItem>();

			using (var connection = new SQLiteConnection( connectionString ))
			{
				connection.Open();

				string selectQuery = "SELECT Id, Name FROM Models;";
				using (var command = new SQLiteCommand( selectQuery, connection ))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						models.Add( new ModelItem
						{
							Id = reader.GetInt32( 0 ),
							Name = reader.GetString( 1 )
						} );
					}
				}
			}

			return models;
		}

		public string LoadModelData( int id )
		{
			using (var connection = new SQLiteConnection( connectionString ))
			{
				connection.Open();

				string selectQuery = "SELECT Data FROM Models WHERE Id = @Id;";
				using (var command = new SQLiteCommand( selectQuery, connection ))
				{
					command.Parameters.AddWithValue( "@Id", id );
					return command.ExecuteScalar()?.ToString();
				}
			}
		}

		


	}


}
