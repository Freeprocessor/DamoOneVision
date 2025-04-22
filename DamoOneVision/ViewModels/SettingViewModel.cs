using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
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

		public ObservableCollection<MotionModel> MotionModels { get; set; } = new ObservableCollection<MotionModel>();

		public ObservableCollection<string> AvailableModelNames { get; } = new();



		private string _selectedModelName = "";
		public string SelectedModelName
		{
			get => _selectedModelName;
			set
			{
				if (_selectedModelName != value)
				{
					_selectedModelName = value;
					OnPropertyChanged( nameof( SelectedModelName ) );
					LoadModel( _selectedModelName ); // 선택 즉시 모델 불러오기
				}
			}
		}

		private string _newModelName = "";
		public string NewModelName
		{
			get => _newModelName;
			set
			{
				_newModelName = value;
				OnPropertyChanged( nameof( NewModelName ) );
			}
		}



		private InfraredCameraModel? _selectedInfraredCameraModel;
		public InfraredCameraModel? SelectedInfraredCameraModel
		{
			get => _selectedInfraredCameraModel;
			set
			{
				if (_selectedInfraredCameraModel != value)
				{
					_selectedInfraredCameraModel = value;
					OnPropertyChanged( nameof( SelectedInfraredCameraModel ) );
				}
			}
		}

		private MotionModel? _selectedMotionModel;
		public MotionModel? SelectedMotionModel
		{
			get => _selectedMotionModel;
			set
			{
				if (_selectedMotionModel != value)
				{
					_selectedMotionModel = value;
					OnPropertyChanged( nameof( SelectedMotionModel ) );
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
		public ICommand SaveWithNameCommand { get; }
		public ICommand DeleteModelCommand { get; }
		public ICommand ActiveValuePlus1 { get; }
		public ICommand ActiveValuePlus10 { get; }
		public ICommand ActiveValuePlus100 { get; }
		public ICommand ActiveValueMinus1 { get; }
		public ICommand ActiveValueMinus10 { get; }
		public ICommand ActiveValueMinus100 { get; }

		private readonly SettingManager _settingManager;
		private readonly CameraService _cameraService;
		private readonly MainViewModel _mainViewModel;

		private bool _isImageDisplay = false;
		private bool _isBinarized = false;

		public SettingViewModel( SettingManager settingManager, CameraService cameraService, MainViewModel mainViewModel )
		{
			_settingManager = settingManager;
			_cameraService = cameraService;
			_mainViewModel = mainViewModel;

			SaveWithNameCommand = new RelayCommand( SaveAsNewModel );
			SelectPropertyCommand = new RelayCommand<string>( OnSelectProperty );
			DeleteModelCommand = new RelayCommand( DeleteSelectedModel );

			ActiveValuePlus1 = new RelayCommand( ( ) => { ActiveValue += ActiveValueTick; } );
			ActiveValuePlus10 = new RelayCommand( ( ) => { ActiveValue += ActiveValueTick * 10; } );
			ActiveValuePlus100 = new RelayCommand( ( ) => { ActiveValue += ActiveValueTick * 100; } );
			ActiveValueMinus1 = new RelayCommand( ( ) => { ActiveValue -= ActiveValueTick; } );
			ActiveValueMinus10 = new RelayCommand( ( ) => { ActiveValue -= ActiveValueTick * 10; } );
			ActiveValueMinus100 = new RelayCommand( ( ) => { ActiveValue -= ActiveValueTick * 100; } );

			LoadAvailableModelNames();

			// 🔥 마지막 열었던 모델 이름으로 자동 로딩
			string lastModel = _settingManager.LastOpenedModel();

			if (AvailableModelNames.Contains( lastModel ))
			{
				LoadModel( lastModel );
				SelectedModelName = lastModel;
			}
			else if (AvailableModelNames.Any())
			{
				LoadModel( AvailableModelNames.First() );
				SelectedModelName = AvailableModelNames.First();
			}

		}

		private void LoadModel( string modelName )
		{
			var data = _settingManager.LoadModelData(modelName);
			if (data == null || data.InfraredCameraModels == null || data.MotionModels == null ) return;

			InfraredCameraModels.Clear();
			MotionModels.Clear();

			foreach (var model in data.InfraredCameraModels)
				InfraredCameraModels.Add( model );
			foreach (var model in data.MotionModels)
				MotionModels.Add( model );

			SelectedInfraredCameraModel = InfraredCameraModels.FirstOrDefault();
			SelectedMotionModel = MotionModels.FirstOrDefault();

			_mainViewModel.ModelName = modelName;
		}

		private void SaveAsNewModel( )
		{
			if (string.IsNullOrWhiteSpace( NewModelName ))
			{
				Logger.WriteLine( "모델 이름이 비어 있습니다." );
				return;
			}

			var data = new ModelData
			{
				InfraredCameraModels = InfraredCameraModels.ToList(),
				MotionModels = MotionModels.ToList()
			};

			_settingManager.SaveModelData( NewModelName, data );
			LoadAvailableModelNames();
			SelectedModelName = NewModelName;
			NewModelName = "";
		}

		private void LoadAvailableModelNames( )
		{
			AvailableModelNames.Clear();
			foreach (var name in _settingManager.GetAvailableModelNames())
				AvailableModelNames.Add( name );
		}

		private void DeleteSelectedModel( )
		{
			if (string.IsNullOrWhiteSpace( SelectedModelName ))
				return;

			var result = MessageBox.Show(
		$"'{SelectedModelName}' 모델을 삭제하시겠습니까?",
		"모델 삭제 확인",
		MessageBoxButton.YesNo,
		MessageBoxImage.Warning);

			if (result != MessageBoxResult.Yes)
				return;

			_settingManager.DeleteModel( SelectedModelName );

			LoadAvailableModelNames();

			if (AvailableModelNames.Any())
			{
				SelectedModelName = AvailableModelNames.First();
				LoadModel( SelectedModelName );
			}
			else
			{
				InfraredCameraModels.Clear();
				SelectedInfraredCameraModel = null;
				SelectedModelName = "";
				Logger.WriteLine( "모델이 삭제되었습니다." );
			}
		}


		private void OnSelectProperty( string propertyName )
		{
			ActivePropertyName = propertyName;
		}

		private void UpdateActiveValueFromModel( )
		{
			if (SelectedInfraredCameraModel == null)
				return;

			switch (ActivePropertyName)
			{
				case "BinarizedThreshold":
					ActiveValue = SelectedInfraredCameraModel.BinarizedThreshold;
					ActiveValueMin = 0;
					ActiveValueMax = 65535; // 예시: 16비트 이미지의 최대값
					ActiveValueTick = 1; // 예시: 1단위로 조정
					_isBinarized = true;
					//Logger.WriteLine( $"BinarizedThreshold: {SelectedModel.BinarizedThreshold}" );
					break;
				case "CircleCenterX":
					ActiveValue = SelectedInfraredCameraModel.CircleCenterX;
					ActiveValueMin = 0;
					ActiveValueMax = 640; // 예시: 화면 너비
					ActiveValueTick = 1; // 예시: 1픽셀 단위
					_isBinarized = false;
					break;
				case "CircleCenterY":
					ActiveValue = SelectedInfraredCameraModel.CircleCenterY;
					ActiveValueMin = 0;
					ActiveValueMax = 480; // 예시: 화면 높이
					ActiveValueTick = 1; // 예시: 1픽셀 단위
					_isBinarized = false;
					break;
				case "CircleMinRadius":
					ActiveValue = SelectedInfraredCameraModel.CircleMinRadius;
					ActiveValueMin = 0;
					ActiveValueMax = 240; // 예시: 최소 반지름
					ActiveValueTick = 0.1;
					_isBinarized = false;
					break;
				case "CircleMaxRadius":
					ActiveValue = SelectedInfraredCameraModel.CircleMaxRadius;
					ActiveValueMin = 0;
					ActiveValueMax = 240; // 예시: 최대 반지름
					ActiveValueTick = 0.1;
					_isBinarized = false;
					break;
				case "CircleInnerRadius":
					ActiveValue = SelectedInfraredCameraModel.CircleInnerRadius;
					ActiveValueMin = 0;
					ActiveValueMax = 240; // 예시: 최대 반지름
					ActiveValueTick = 1;
					_isBinarized = false;
					break;
				case "CircleMinAreaRatio":
					ActiveValue = SelectedInfraredCameraModel.CircleMinAreaRatio;
					ActiveValueMin = 0;
					ActiveValueMax = 1.5; // 예시: 비율
					ActiveValueTick = 0.01; // 예시: 0.01 단위로 조정
					_isBinarized = false;
					break;
				case "CircleMaxAreaRatio":
					ActiveValue = SelectedInfraredCameraModel.CircleMaxAreaRatio;
					ActiveValueMin = 0;
					ActiveValueMax = 1.5; // 예시: 비율
					ActiveValueTick = 0.01; // 예시: 0.01 단위로 조정
					_isBinarized = false;
					break;
				case "AvgTemperatureMin":
					ActiveValue = SelectedInfraredCameraModel.AvgTemperatureMin;
					ActiveValueMin = 0; // 예시: 최소 온도
					ActiveValueMax = 65535; // 예시: 최대 온도
					ActiveValueTick = 1; // 예시: 0.1 단위로 조정
					_isBinarized = false;
					break;
				case "AvgTemperatureMax":
					ActiveValue = SelectedInfraredCameraModel.AvgTemperatureMax;
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
			ConversionImage();
		}

		private void UpdateModelFromActiveValue( )
		{
			if (SelectedInfraredCameraModel == null)
				return;

			switch (ActivePropertyName)
			{
				case "BinarizedThreshold":
					SelectedInfraredCameraModel.BinarizedThreshold = ActiveValue;
					break;
				case "CircleCenterX":
					SelectedInfraredCameraModel.CircleCenterX = ActiveValue;
					break;
				case "CircleCenterY":
					SelectedInfraredCameraModel.CircleCenterY = ActiveValue;
					break;
				case "CircleMinRadius":
					SelectedInfraredCameraModel.CircleMinRadius = ActiveValue;
					break;
				case "CircleMaxRadius":
					SelectedInfraredCameraModel.CircleMaxRadius = ActiveValue;
					break;
				case "CircleInnerRadius":
					SelectedInfraredCameraModel.CircleInnerRadius = ActiveValue;
					break;
				case "CircleMinAreaRatio":
					SelectedInfraredCameraModel.CircleMinAreaRatio = ActiveValue;
					break;
				case "CircleMaxAreaRatio":
					SelectedInfraredCameraModel.CircleMaxAreaRatio = ActiveValue;
					break;
				case "AvgTemperatureMin":
					SelectedInfraredCameraModel.AvgTemperatureMin = ActiveValue;
					break;
				case "AvgTemperatureMax":
					SelectedInfraredCameraModel.AvgTemperatureMax = ActiveValue;
					break;
					// CircleMinRadius, CircleMaxRadius, etc...
			}
			//Logger.WriteLine( $"Updated {ActivePropertyName}: {ActiveValue}" );
			ConversionImage();
		}


		public async void ConversionImage( )
		{
			await Conversion.InfraredCameraModel( true, _isBinarized, _cameraService.GetBinarizedImage(), _cameraService.GetScaleImage(), _cameraService.GetImage(), _cameraService._infraredCameraDisplay, SelectedInfraredCameraModel );
		}


		public async void UpdateCameraSettings( )
		{
			_isImageDisplay = true;
			if (SelectedInfraredCameraModel == null)
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
					await Conversion.InfraredCameraModel( true, _isBinarized, _cameraService.GetBinarizedImage(), _cameraService.GetScaleImage(), _cameraService.GetImage(), _cameraService._infraredCameraDisplay, SelectedInfraredCameraModel );
					await Task.Delay( 100 );
				}
			} );
		}

		public void UpdateCameraSettingsStop( )
		{
			_isImageDisplay = false;
		}

		//private void LoadModels( )
		//{
		//	// Load the model data using SettingManager.
		//	ModelData data = _settingManager.LoadModelData();
		//	InfraredCameraModels.Clear();
		//	foreach (var model in data.InfraredCameraModels)
		//	{
		//		InfraredCameraModels.Add( model );
		//	}
		//	if (InfraredCameraModels.Any())
		//	{
		//		SelectedModel = InfraredCameraModels.First();
		//	}
		//}
		//private void SaveModels( )
		//{
		//	// 1. 기존 파일 읽어오기
		//	string modelFilePath = _settingManager.GetModelFilePath();
		//	ModelData data = new ModelData();

		//	if (File.Exists( modelFilePath ))
		//	{
		//		string existingJson = File.ReadAllText(modelFilePath);
		//		data = JsonSerializer.Deserialize<ModelData>( existingJson );
		//		if (data == null)
		//			data = new ModelData();
		//	}

		//	// 2. 기존 데이터 중 MotionModels는 그대로 두고,
		//	//    InfraredCameraModels만 현재 ViewModel 값으로 업데이트
		//	data.InfraredCameraModels = InfraredCameraModels.ToList();

		//	// 3. (옵션) LastOpenedModel을 설정하거나 Settings 업데이트 등
		//	// _settingManager.Settings.LastOpenedModel = SelectedModel?.Name ?? "Default";

		//	// 4. 저장
		//	var options = new JsonSerializerOptions { WriteIndented = true };
		//	string json = JsonSerializer.Serialize(data, options);
		//	File.WriteAllText( modelFilePath, json );

		//	_settingManager.LoadSettings();
		//}
	}
}
