using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using DamoOneVision.Models;
using DamoOneVision.Services;
using DamoOneVision.ViewModels;

namespace DamoOneVision.Data
{
	public class AppSettings
	{
		public string InfraredCameraSerial { get; set; }
		public string SideCamera1Serial { get; set; }
		public string SideCamera2Serial { get; set; }
		public string SideCamera3Serial { get; set; }
		public string LastOpenedModel { get; set; }
		public int GoodCount { get; set; } = 0;
		public int RejectCount { get; set; } = 0;


		public static AppSettings Load( string filePath )
		{
			try
			{
				if (File.Exists( filePath ))
				{
					string json = File.ReadAllText(filePath);
					var settings = JsonSerializer.Deserialize<AppSettings>(json);

					return settings ?? new AppSettings();
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLine( $"[설정 불러오기 오류] {ex.Message}" );
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
		public AppSettings Settings { get; set; }

		private string _currentModel;
		public string CurrentModel
		{
			get => _currentModel;
			set
			{
				_currentModel = value;

				//Logger.WriteLine( $"Current Model: {_currentModel}" );
			}
		}
		// 현재 모델 이름
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
			settingPath = Path.Combine( appFolder, "Setting.json" );

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
			string defaultModelFile = Path.Combine(modelPath, "Default.json");
			if (!File.Exists( defaultModelFile ))
			{


				InfraredCameraModel defaultCameraModel = new InfraredCameraModel
				{
					BinarizedThreshold = 32000,
					CircleCenterX = 320.0,
					CircleCenterY = 240.0,
					CircleMinRadius = 137.0,
					CircleMaxRadius = 240.0,
					CircleMinAreaRatio = 0.97,
					CircleMaxAreaRatio = 1.5,
					AvgTemperatureMin = 33000,
					AvgTemperatureMax = 35000,
					CameraFocusValue = 0.2
				};

				MotionModel defaultMotionModel = new MotionModel
				{
					XAxisWaitingPosition = 1000.0,
					XAxisEndPosition = 200000.0,
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
					ZAxisWorkPosition = 47000.0,
					ZAxisEndPosition = 130000.0,
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

		public string LastOpenedModel()
		{
			return Settings.LastOpenedModel;
		}

		public void LoadSettings( )
		{
			Settings = AppSettings.Load( settingPath );
			LoadModelData( Settings.LastOpenedModel );
		}

		public void SaveSettings( )
		{
			Directory.CreateDirectory( appFolder ); // 폴더 없으면 자동 생성
			string path = Path.Combine( appFolder, "Setting.json");

			string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions
			{
				WriteIndented = true
			});

			File.WriteAllText( path, json );
		}

		// Load ModelData from the Models.json file.
		public ModelData LoadModelData( string modelName )
		{
			string filePath = Path.Combine(modelPath, $"{modelName}.json");

			if (!File.Exists( filePath ))
			{
				Logger.WriteLine( "모델 파일이 존재하지 않습니다." );
				return new ModelData();  // or return null;
			}

			string json = File.ReadAllText(filePath);
			var modelData = JsonSerializer.Deserialize<ModelData>(json);

			if (modelData == null)
			{
				Logger.WriteLine( "모델 파일 파싱 실패" );
				return new ModelData();
			}

			// ✅ 서비스에 모델 설정까지 통합 처리
			if (modelData.MotionModels?.Any() == true)
			{
				_deviceControlService.SetModel( modelData.MotionModels.First() );
				Logger.WriteLine( $"Motion Model : '{modelName}' 로드 완료" );
			}

			if (modelData.InfraredCameraModels?.Any() == true)
			{
				_cameraService.SetModel( modelData.InfraredCameraModels.First() );
				Logger.WriteLine( $"Camera Model : '{modelName}' 로드 완료" );
			}

			Settings.LastOpenedModel = modelName;
			SaveSettings();
			CurrentModel = modelName;
			return modelData;
		}

		public void SaveModelData( string modelName, ModelData data )
		{
			Directory.CreateDirectory( modelPath );

			string filePath = Path.Combine(modelPath, $"{modelName}.json");
			var options = new JsonSerializerOptions { WriteIndented = true };
			string json = JsonSerializer.Serialize(data, options);

			File.WriteAllText( filePath, json );
		}

		public List<string> GetAvailableModelNames( )
		{
			if (!Directory.Exists( modelPath ))
				return new List<string>();

			return Directory.GetFiles( modelPath, "*.json" )
							.Select( Path.GetFileNameWithoutExtension )
							.ToList();
		}

		public void DeleteModel( string modelName )
		{
			string filePath = Path.Combine(modelPath, $"{modelName}.json");

			if (File.Exists( filePath ))
			{
				File.Delete( filePath );
				Logger.WriteLine( $"모델 '{modelName}' 삭제됨" );
			}
			else
			{
				Logger.WriteLine( $"모델 파일이 존재하지 않음: {filePath}" );
			}

			// LastOpenedModel 초기화 필요하면 여기서도 가능
			if (Settings.LastOpenedModel == modelName)
			{
				Settings.LastOpenedModel = "";
				SaveSettings();
			}
		}

		// Load a specific model by name.
		//public void LoadModel( string modelName )
		//{
		//	string defaultModelFile = Path.Combine(modelPath, "Models.json");

		//	if (File.Exists( defaultModelFile ))
		//	{
		//		string json = File.ReadAllText(defaultModelFile);
		//		ModelData modelData = JsonSerializer.Deserialize<ModelData>(json);

		//		var targetMotionModel = modelData.MotionModels.FirstOrDefault(m => m.Name == modelName);
		//		var targetCameraModel = modelData.InfraredCameraModels.FirstOrDefault(m => m.Name == modelName);

		//		if (targetMotionModel != null)
		//		{
		//			_deviceControlService.SetModel( targetMotionModel );
		//			Logger.WriteLine( $"Motion Model : '{modelName}' loaded" );
		//		}

		//		if (targetCameraModel != null)
		//		{
		//			_cameraService.SetModel( targetCameraModel );
		//			Logger.WriteLine( $"Camera Model : '{modelName}' loaded" );
		//		}
		//	}
		//	else
		//	{
		//		Logger.WriteLine( "모델 파일이 존재하지 않습니다." );
		//	}
		//}

		// Expose the model file path for saving.
		//public string GetModelFilePath( )
		//{
		//	string defaultModelFile = Path.Combine(modelPath, "Models.json");
		//	return defaultModelFile;
		//}
	}
}
