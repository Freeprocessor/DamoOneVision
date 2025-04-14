using System;
using System.IO;
using System.Linq;
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

	public class SettingManager
	{
		string localAppData;
		string appFolder;
		string modelPath;
		string settingPath;
		private DeviceControlService _deviceControlService;
		private CameraService _cameraService;
		public AppSettings Settings { get; private set; }

		public string AppFolder => appFolder;
		public string ModelFolder => modelPath;
		public string SettingPath => settingPath;

		public SettingManager( DeviceControlService deviceControlService, CameraService cameraService )
		{
			_deviceControlService = deviceControlService;
			_cameraService = cameraService;
			localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			appFolder = Path.Combine( localAppData, "DamoOneVision" );
			modelPath = Path.Combine( appFolder, "Model" );
			settingPath = Path.Combine( appFolder, "settings.json" );

			InitializeSetting();
			LoadSettings();
		}

		private void InitializeSetting( )
		{
			// 폴더 생성
			if (!Directory.Exists( appFolder ))
				Directory.CreateDirectory( appFolder );
			if (!Directory.Exists( modelPath ))
				Directory.CreateDirectory( modelPath );

			// 설정 파일 생성
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

			// 모델 파일 생성
			string defaultModelFile = Path.Combine(modelPath, "Models.json");
			if (!File.Exists( defaultModelFile ))
			{


				InfraredCameraModel defaultCameraModel = new InfraredCameraModel
				{
					Name = "Default",
					BinarizedThreshold = 32000,
					CircleCenterX = 320.0,
					CircleCenterY = 240.0,
					CircleMinRadius = 137.0,
					CircleMaxRadius = 240.0,
					CircleMinAreaRatio = 0.97,
					CircleMaxAreaRatio = 1.5,
					AvgTemperatureMin = 33000,
					AvgTemperatureMax = 35000
				};

				MotionModel defaultMotionModel = new MotionModel
				{
					Name = "Default",
					XAxisWaitingPostion = 1000.0,
					XAxisEndPostion = 200000.0,
					XAxisTrackingSpeed = 10000.0,
					XAxisReturnSpeed = 850000,
					XAxisMoveAcceleration = 0.015,
					XAxisMoveDeceleration = 0.015,
					XAxisReturnAcceleration = 0.05,
					XAxisReturnDeceleration = 0.05,
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
					ZAxisWorkPostion = 47000.0,
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

				ModelData modelData = new ModelData();
				modelData.InfraredCameraModels.Add( defaultCameraModel );
				modelData.MotionModels.Add( defaultMotionModel );

				var options = new JsonSerializerOptions { WriteIndented = true };
				string json = JsonSerializer.Serialize(modelData, options);
				File.WriteAllText( defaultModelFile, json );
			}
		}

		public void LoadSettings( )
		{
			Settings = AppSettings.Load( settingPath );
			LoadModel( Settings.LastOpenedModel );
		}

		// Load ModelData from the Models.json file.
		public ModelData LoadModelData( )
		{
			string defaultModelFile = Path.Combine(modelPath, "Models.json");
			if (File.Exists( defaultModelFile ))
			{
				string json = File.ReadAllText(defaultModelFile);
				return JsonSerializer.Deserialize<ModelData>( json );
			}
			return new ModelData();
		}

		// Load a specific model by name.
		public void LoadModel( string modelName )
		{
			string defaultModelFile = Path.Combine(modelPath, "Models.json");

			if (File.Exists( defaultModelFile ))
			{
				string json = File.ReadAllText(defaultModelFile);
				ModelData modelData = JsonSerializer.Deserialize<ModelData>(json);

				var targetMotionModel = modelData.MotionModels.FirstOrDefault(m => m.Name == modelName);
				var targetCameraModel = modelData.InfraredCameraModels.FirstOrDefault(m => m.Name == modelName);

				if (targetMotionModel != null)
				{
					_deviceControlService.SetModel( targetMotionModel );
					Logger.WriteLine( $"Motion Model : '{modelName}' loaded" );
				}

				if (targetCameraModel != null)
				{
					_cameraService.SetModel( targetCameraModel );
					Logger.WriteLine( $"Camera Model : '{modelName}' loaded" );
				}
			}
			else
			{
				Logger.WriteLine( "모델 파일이 존재하지 않습니다." );
			}
		}

		// Expose the model file path for saving.
		public string GetModelFilePath( )
		{
			string defaultModelFile = Path.Combine(modelPath, "Models.json");
			return defaultModelFile;
		}
	}
}
