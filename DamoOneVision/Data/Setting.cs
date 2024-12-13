using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

using DamoOneVision.Models;
using System.Text.Json;

namespace DamoOneVision.Data
{
	public class AppSettings
	{
		public string InfraredCameraSerial { get; set; }
		public String SideCamera1Serial { get; set; }
		public String SideCamera2Serial { get; set; }
		public String SideCamera3Serial { get; set; }	
		public string LastOpenedModel { get; set; }
	}

	internal class SettingManager
	{
		string localAppData;
		string appFolder;


		public SettingManager(  )
		{
			localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			appFolder = Path.Combine( localAppData, "DamoOneVision" );

			InitializeSetting();
		}

		private void InitializeSetting(  )
		{
			string SettingPath = Path.Combine(appFolder, "settings.json");
			if (!File.Exists( appFolder ))
			{
				Directory.CreateDirectory( appFolder );
			}

			if (!File.Exists( SettingPath ))
			{
				AppSettings settings = new AppSettings
				{
					LastOpenedModel = "",
					InfraredCameraSerial = "",
					SideCamera1Serial = "",
					SideCamera2Serial = "",
					SideCamera3Serial = ""
				};

				var options = new JsonSerializerOptions { WriteIndented = true };
				string json = JsonSerializer.Serialize(settings, options);
				File.WriteAllText( SettingPath, json );
			}

		}

	}

}
