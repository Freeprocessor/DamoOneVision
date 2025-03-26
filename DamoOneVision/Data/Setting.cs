using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

using System.Text.Json;
using DamoOneVision.Models;
using DamoOneVision.Services;

namespace DamoOneVision.Data
{
	public class AppSettings
	{
		public string InfraredCameraSerial { get; set; }
		public string SideCamera1Serial { get; set; }
		public string SideCamera2Serial { get; set; }
		public string SideCamera3Serial { get; set; }	
		public string LastOpenedModel { get; set; }

		public static AppSettings Load( string filePath )
		{
			if (File.Exists( filePath ))
			{
				string json = File.ReadAllText(filePath);
				return JsonSerializer.Deserialize<AppSettings>( json );
			}
			return new AppSettings();
		}
	}





	internal class SettingManager
	{
		string localAppData;
		string appFolder;
		string modelPath;
		string settingPath;
		private DeviceControlService _deviceControlService;
		public AppSettings Settings { get; private set; }

		public SettingManager( DeviceControlService deviceControlService)
		{
			_deviceControlService = deviceControlService; 
			localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			appFolder = Path.Combine( localAppData, "DamoOneVision" );
			modelPath = Path.Combine( appFolder, "Model" );
			settingPath = Path.Combine( appFolder, "settings.json" );

			InitializeSetting();
			LoadSettings();
		}

		private void InitializeSetting(  )
		{
			string ModelPath = Path.Combine(modelPath, "Models.model");

			if (!File.Exists( appFolder ))
			{
				Directory.CreateDirectory( appFolder );
			}

			if (!File.Exists( modelPath ))
			{
				Directory.CreateDirectory( modelPath );
			}

			if (!File.Exists( settingPath ))
			{
				AppSettings settings = new AppSettings
				{
					LastOpenedModel = "Default",
					InfraredCameraSerial = "",
					SideCamera1Serial = "",
					SideCamera2Serial = "",
					SideCamera3Serial = ""
				};

				var options = new JsonSerializerOptions { WriteIndented = true };
				string json = JsonSerializer.Serialize(settings, options);
				File.WriteAllText( settingPath, json );
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
					XAxisWaitingPostion = 1000.0,
					XAxisEndPostion = 200000.0,
					XAxisTrackingSpeed = 10000.0,
					XAxisReturnSpeed = 50000.0,
					XAxisAcceleration = 0.1,
					XAxisDeceleration = 0.1,
					XAxisJogSpeed = 10000.0,
					XAxisJogAcceleration = 0.1,
					XAxisJogDeceleration = 0.1,

					XAxisOriginDirection = 0,
					XAxisOriginSensor = 1,
					XAxisOriginUseZPhase = 0,
					XAxisOriginDelay = 1000.0,
					XAxisOriginOffset = 2000.0,

					XAxisOriginSpeed1 = 10000.0,
					XAxisOriginSpeed2 = 5000.0,
					XAxisOriginCreepSpeed = 1000.0,
					XAxisOriginZPhaseSpeed = 500.0,
					XAxisOriginAcceleration = 0.1,
					XAxisOriginDeceleration = 0.1,


					ZAxisWorkPostion = 43000.0,
					ZAxisEndPostion = 130000.0,
					ZAxisSpeed = 20000.0,
					ZAxisAcceleration = 0.1,
					ZAxisDeceleration = 0.1,
					ZAxisJogSpeed = 10000.0,
					ZAxisJogAcceleration = 0.1,
					ZAxisJogDeceleration = 0.1,

					ZAxisOriginDirection = 0,
					ZAxisOriginSensor = 1,
					ZAxisOriginUseZPhase = 1,
					ZAxisOriginDelay = 1000.0,
					ZAxisOriginOffset = 0.0,

					ZAxisOriginSpeed1 = 10000.0,
					ZAxisOriginSpeed2 = 5000.0,
					ZAxisOriginCreepSpeed = 1000.0,
					ZAxisOriginZPhaseSpeed = 500.0,
					ZAxisOriginAcceleration = 0.1,
					ZAxisOriginDeceleration = 0.1,
				};


				ModelData ModelData = new ModelData();
				ModelData.InfraredCameraModels.Add( infraredCameraModels );
				ModelData.MotionModels.Add( motionModel );

				var options = new JsonSerializerOptions { WriteIndented = true };
				string json = JsonSerializer.Serialize(ModelData, options);
				File.WriteAllText( ModelPath, json );
			}

		}
		private void LoadSettings( )
		{
			Settings = AppSettings.Load( settingPath );
			LoadModel( Settings.LastOpenedModel );
		}

		private void LoadModel( string modelName )
		{
			//Logger.WriteLine( $"Loading model {modelName}" );
			string modelFilePath = Path.Combine(modelPath, "Models.model");
			if (File.Exists( modelFilePath ))
			{
				string json = File.ReadAllText(modelFilePath);
				ModelData modelData = JsonSerializer.Deserialize<ModelData>(json);

				// 원하는 이름의 모델을 검색
				var targetMotionModel = modelData.MotionModels.FirstOrDefault(m => m.Name == modelName);
				var targetCameraModel = modelData.InfraredCameraModels.FirstOrDefault(m => m.Name == modelName);

				if (targetMotionModel != null)
				{
					_deviceControlService.SetModel( targetMotionModel );
					Logger.WriteLine( $"Model : '{modelName}' loaded" );
				}

				if (targetCameraModel != null)
				{
					// TODO: 이 모델을 어디에 적용할지에 따라 값을 설정
				}
			}
		}
	}

}
