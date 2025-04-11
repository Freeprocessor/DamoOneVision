using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;  // CommunityToolkit.Mvvm 또는 다른 RelayCommand 구현 사용
using DamoOneVision.Data;
using DamoOneVision.ImageProcessing;
using DamoOneVision.Models;
using DamoOneVision.Services;
using Matrox.MatroxImagingLibrary;

namespace DamoOneVision.ViewModels
{
	public class SettingViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged( string propertyName ) =>
			 PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );

		public ObservableCollection<InfraredCameraModel> InfraredCameraModels { get; set; } = new ObservableCollection<InfraredCameraModel>();

		private InfraredCameraModel? _selectedModel;
		public InfraredCameraModel? SelectedModel
		{
			get => _selectedModel;
			set
			{
				if (_selectedModel != value)
				{
					_selectedModel = value;
					OnPropertyChanged( nameof( SelectedModel ) );
				}
			}
		}

		private string _activePropertyName;
		public string ActivePropertyName
		{
			get => _activePropertyName;
			set
			{
				if (_activePropertyName != value)
				{
					_activePropertyName = value;
					OnPropertyChanged( nameof( ActivePropertyName ) );
					// ActivePropertyName 바뀌면, ActiveValue도 재계산
					UpdateActiveValueFromModel();
				}
			}
		}

		private double _activeValue;
		public double ActiveValue
		{
			get => _activeValue;
			set
			{
				if (_activeValue != value)
				{
					_activeValue = value;
					OnPropertyChanged( nameof( ActiveValue ) );
					// ActiveValue 바뀌면, 해당 모델 속성에 반영
					UpdateModelFromActiveValue();
				}
			}
		}

		private double _activeValueMin;
		public double ActiveValueMin
		{
			get => _activeValueMin;
			set
			{
				if (_activeValueMin != value)
				{
					_activeValueMin = value;
					OnPropertyChanged( nameof( ActiveValueMin ) );
					// ActiveValue 바뀌면, 해당 모델 속성에 반영
					UpdateModelFromActiveValue();
				}
			}
		}

		private double _activeValueMax;
		public double ActiveValueMax
		{
			get => _activeValueMax;
			set
			{
				if (_activeValueMax != value)
				{
					_activeValueMax = value;
					OnPropertyChanged( nameof( ActiveValueMax ) );
					// ActiveValue 바뀌면, 해당 모델 속성에 반영
					UpdateModelFromActiveValue();
				}
			}
		}

		private double _activeValueTick;
		public double ActiveValueTick
		{
			get => _activeValueTick;
			set
			{
				if (_activeValueTick != value)
				{
					_activeValueTick = value;
					OnPropertyChanged( nameof( ActiveValueTick ) );
					// ActiveValue 바뀌면, 해당 모델 속성에 반영
					UpdateModelFromActiveValue();
				}
			}
		}

		// 커맨드: 어떤 속성을 선택할지
		public ICommand SelectPropertyCommand { get; }
		public ICommand SaveCommand { get; }

		private readonly SettingManager _settingManager;
		private readonly CameraService _cameraService;

		private bool _isImageDisplay = false;
		private bool _isBinarized = false;

		public SettingViewModel( SettingManager settingManager, CameraService cameraService )
		{
			_settingManager = settingManager;

			_cameraService = cameraService;

			LoadModels();
			SaveCommand = new RelayCommand( SaveModels );

			SelectPropertyCommand = new RelayCommand<string>( OnSelectProperty );
		}

		private void LoadModels( )
		{
			// Load the model data using SettingManager.
			ModelData data = _settingManager.LoadModelData();
			InfraredCameraModels.Clear();
			foreach (var model in data.InfraredCameraModels)
			{
				InfraredCameraModels.Add( model );
			}
			if (InfraredCameraModels.Any())
			{
				SelectedModel = InfraredCameraModels.First();
			}
		}
		private void SaveModels( )
		{
			// 1. 기존 파일 읽어오기
			string modelFilePath = _settingManager.GetModelFilePath();
			ModelData data = new ModelData();

			if (File.Exists( modelFilePath ))
			{
				string existingJson = File.ReadAllText(modelFilePath);
				data = JsonSerializer.Deserialize<ModelData>( existingJson );
				if (data == null)
					data = new ModelData();
			}

			// 2. 기존 데이터 중 MotionModels는 그대로 두고,
			//    InfraredCameraModels만 현재 ViewModel 값으로 업데이트
			data.InfraredCameraModels = InfraredCameraModels.ToList();

			// 3. (옵션) LastOpenedModel을 설정하거나 Settings 업데이트 등
			// _settingManager.Settings.LastOpenedModel = SelectedModel?.Name ?? "Default";

			// 4. 저장
			var options = new JsonSerializerOptions { WriteIndented = true };
			string json = JsonSerializer.Serialize(data, options);
			File.WriteAllText( modelFilePath, json );

			_settingManager.LoadSettings();
		}


		private void OnSelectProperty( string propertyName )
		{
			ActivePropertyName = propertyName;
		}

		private void UpdateActiveValueFromModel( )
		{
			if (SelectedModel == null)
				return;

			switch (ActivePropertyName)
			{
				case "BinarizedThreshold":
					ActiveValue = SelectedModel.BinarizedThreshold;
					ActiveValueMin = 0;
					ActiveValueMax = 65535; // 예시: 16비트 이미지의 최대값
					ActiveValueTick = 1; // 예시: 1단위로 조정
					_isBinarized = true;
					//Logger.WriteLine( $"BinarizedThreshold: {SelectedModel.BinarizedThreshold}" );
					break;
				case "CircleCenterX":
					ActiveValue = SelectedModel.CircleCenterX;
					ActiveValueMin = 0;
					ActiveValueMax = 640; // 예시: 화면 너비
					ActiveValueTick = 1; // 예시: 1픽셀 단위
					_isBinarized = false;
					break;
				case "CircleCenterY":
					ActiveValue = SelectedModel.CircleCenterY;
					ActiveValueMin = 0;
					ActiveValueMax = 480; // 예시: 화면 높이
					ActiveValueTick = 1; // 예시: 1픽셀 단위
					_isBinarized = false;
					break;
				case "CircleMinRadius":
					ActiveValue = SelectedModel.CircleMinRadius;
					ActiveValueMin = 0;
					ActiveValueMax = 240; // 예시: 최소 반지름
					ActiveValueTick = 0.1;
					_isBinarized = false;
					break;
				case "CircleMaxRadius":
					ActiveValue = SelectedModel.CircleMaxRadius;
					ActiveValueMin = 0;
					ActiveValueMax = 240; // 예시: 최대 반지름
					ActiveValueTick = 0.1;
					_isBinarized = false;
					break;
				case "CircleMinAreaRatio":
					ActiveValue = SelectedModel.CircleMinAreaRatio;
					ActiveValueMin = 0;
					ActiveValueMax = 1.5; // 예시: 비율
					ActiveValueTick = 0.01; // 예시: 0.01 단위로 조정
					_isBinarized = false;
					break;
				case "CircleMaxAreaRatio":
					ActiveValue = SelectedModel.CircleMaxAreaRatio;
					ActiveValueMin = 0;
					ActiveValueMax = 1.5; // 예시: 비율
					ActiveValueTick = 0.01; // 예시: 0.01 단위로 조정
					_isBinarized = false;
					break;
				case "AvgTemperatureMin":
					ActiveValue = SelectedModel.AvgTemperatureMin;
					ActiveValueMin = 0; // 예시: 최소 온도
					ActiveValueMax = 65535; // 예시: 최대 온도
					ActiveValueTick = 1; // 예시: 0.1 단위로 조정
					_isBinarized = false;
					break;
				case "AvgTemperatureMax":
					ActiveValue = SelectedModel.AvgTemperatureMax;
					ActiveValueMin = 0; // 예시: 최소 온도
					ActiveValueMax = 65535; // 예시: 최대 온도
					ActiveValueTick = 1; // 예시: 0.1 단위로 조정
					_isBinarized = false;
					break;

				// CircleMinRadius, CircleMaxRadius, etc...
				default:
					ActiveValue = 0;
					break;
			}
		}

		private void UpdateModelFromActiveValue( )
		{
			if (SelectedModel == null)
				return;

			switch (ActivePropertyName)
			{
				case "BinarizedThreshold":
					SelectedModel.BinarizedThreshold = ActiveValue;
					break;
				case "CircleCenterX":
					SelectedModel.CircleCenterX = ActiveValue;
					break;
				case "CircleCenterY":
					SelectedModel.CircleCenterY = ActiveValue;
					break;
				case "CircleMinRadius":
					SelectedModel.CircleMinRadius = ActiveValue;
					break;
				case "CircleMaxRadius":
					SelectedModel.CircleMaxRadius = ActiveValue;
					break;
				case "CircleMinAreaRatio":
					SelectedModel.CircleMinAreaRatio = ActiveValue;
					break;
				case "CircleMaxAreaRatio":
					SelectedModel.CircleMaxAreaRatio = ActiveValue;
					break;
				case "AvgTemperatureMin":
					SelectedModel.AvgTemperatureMin = ActiveValue;
					break;
				case "AvgTemperatureMax":
					SelectedModel.AvgTemperatureMax = ActiveValue;
					break;
					// CircleMinRadius, CircleMaxRadius, etc...
			}
			//Logger.WriteLine( $"Updated {ActivePropertyName}: {ActiveValue}" );
		}

		public async void UpdateCameraSettings( )
		{
			_isImageDisplay = true;
			if(SelectedModel == null )
			{
				Logger.WriteLine( "모델정보가 없습니다." );
				return;
			}
			else if (_cameraService.GetImage() == MIL.M_NULL)
			{
				Logger.WriteLine( "이미지 데이터가 없습니다." );
				return;
			}
			else if (_cameraService.ImageData() == null)
			{
				Logger.WriteLine( "이미지 데이터가 없습니다.(ImageData)" );
			}


			await Task.Run( async ( ) =>
				{
					while (_isImageDisplay)
					{
						await Conversion.InfraredCameraModel( true, _isBinarized, _cameraService.GetBinarizedImage(), _cameraService.GetScaleImage(), _cameraService.GetImage(), _cameraService._infraredCameraDisplay, SelectedModel, _cameraService.ImageData() );
						await Task.Delay( 100 );
					}
				} );
		}

		public void UpdateCameraSettingsStop( )
		{
			_isImageDisplay = false;
		}
	}
}
