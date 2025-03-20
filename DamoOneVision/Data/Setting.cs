using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

using System.Text.Json;
using DamoOneVision.Models;

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
		string modelPath;


		public SettingManager(  )
		{
			localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			appFolder = Path.Combine( localAppData, "DamoOneVision" );
			modelPath = Path.Combine( appFolder, "Model" );

			InitializeSetting();
		}

		private void InitializeSetting(  )
		{
			string SettingPath = Path.Combine(appFolder, "settings.json");
			string ModelPath = Path.Combine(modelPath, "Models.model");

			if (!File.Exists( appFolder ))
			{
				Directory.CreateDirectory( appFolder );
			}

			if (!File.Exists( modelPath ))
			{
				Directory.CreateDirectory( modelPath );
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

			if (!File.Exists( ModelPath ))
			{
				InfraredCameraModel infraredCameraModels = new InfraredCameraModel
				{
					Name = "Default",
					CircleCenterX = 219.0,
					CircleCenterY = 184.0,
					CircleMinRadius = 115.0,
					CircleMaxRadius = 183.5,
					BinarizedThreshold = 12500
				};

				MotionModel motionModel = new MotionModel
				{
					Name = "Default",
					XAxisSpeed = 100,
					XAxisReturnSpeed = 100,
					XAxisAcceleration = 100,
					XAxisDeceleration = 100,
					XAxisJogSpeed = 100,
					XAxisWaitingPostion = 100,
					XAxisEndPostion = 100,
					ZAxisSpeed = 100,
					ZAxisReturnSpeed = 100,
					ZAxisAcceleration = 100,
					ZAxisDeceleration = 100,
					ZAxisJogSpeed = 100,
					ZAxisWaitingPostion = 100,
					ZAxisEndPostion = 100
				};


				ModelData ModelData = new ModelData();
				ModelData.InfraredCameraModels.Add( infraredCameraModels );
				ModelData.MotionModels.Add( motionModel );

				var options = new JsonSerializerOptions { WriteIndented = true };
				string json = JsonSerializer.Serialize(ModelData, options);
				File.WriteAllText( ModelPath, json );
			}
		}

	}

}
