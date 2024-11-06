using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace DamoOneVision.Data
{



	internal class Database
	{
		private readonly string _dbPath;
		private readonly string _connectionString;
		private static readonly object _dbLock = new object();

		public Database( )
		{
			_dbPath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "MyDatabase.db" );
			_connectionString = $"Data Source={_dbPath}";
			InitializeDatabase();
		}
		private void InitializeDatabase( )
		{
			lock (_dbLock)
			{
				if (!File.Exists( _dbPath ))
				{
					SQLiteConnection.CreateFile( _dbPath );
				}

				using (var connection = new SQLiteConnection( _connectionString ))
				{
					connection.Open();

					string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS ImageData (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ImagePath TEXT NOT NULL,
                            CaptureTime DATETIME NOT NULL
                        );
                    ";

					using (var command = new SQLiteCommand( createTableQuery, connection ))
					{
						command.ExecuteNonQuery();
					}
				}
			}
		}

		public void SaveImageMetadata( string imagePath, DateTime captureTime )
		{
			lock (_dbLock)
			{
				using (var connection = new SQLiteConnection( _connectionString ))
				{
					connection.Open();

					string insertQuery = @"
                        INSERT INTO ImageData (ImagePath, CaptureTime)
                        VALUES (@ImagePath, @CaptureTime);
                    ";

					using (var command = new SQLiteCommand( insertQuery, connection ))
					{
						command.Parameters.AddWithValue( "@ImagePath", imagePath );
						command.Parameters.AddWithValue( "@CaptureTime", captureTime );
						command.ExecuteNonQuery();
					}
				}
			}
		}

		public List<ImageData> GetAllImages( )
		{
			var images = new List<ImageData>();

			lock (_dbLock)
			{
				using (var connection = new SQLiteConnection( _connectionString ))
				{
					connection.Open();

					string selectQuery = "SELECT * FROM ImageData ORDER BY CaptureTime DESC";

					using (var command = new SQLiteCommand( selectQuery, connection ))
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							images.Add( new ImageData
							{
								Id = Convert.ToInt32( reader[ "Id" ] ),
								ImagePath = reader[ "ImagePath" ].ToString(),
								CaptureTime = Convert.ToDateTime( reader[ "CaptureTime" ] )
							} );
						}
					}
				}
			}

			return images;
		}
	}

	public class ImageData
	{
		public int Id { get; set; }
		public string ImagePath { get; set; }
		public DateTime CaptureTime { get; set; }
	}
}
