using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;  // CommunityToolkit.Mvvm 또는 다른 RelayCommand 구현 사용
using DamoOneVision.Data;
using DamoOneVision.ImageProcessing;
using DamoOneVision.Models;
using DamoOneVision.Services;
using DamoOneVision.Views;
using Matrox.MatroxImagingLibrary;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace DamoOneVision.ViewModels
{
	public partial class SettingViewModel : ObservableObject, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged( string propertyName ) =>
			 PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );

		public ObservableCollection<InfraredCameraModel> InfraredCameraModels { get; set; } = new ObservableCollection<InfraredCameraModel>();

		public ObservableCollection<MotionModel> MotionModels { get; set; } = new ObservableCollection<MotionModel>();

		public ObservableCollection<string> AvailableModelNames { get; } = new();


		[ObservableProperty]
		private InfraredInspectionResult? lastInspectionResult;

		private ConversionResultWindow? _resultWindow;


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
					// 이전 모델 이벤트 제거
					if (_selectedInfraredCameraModel != null)
						_selectedInfraredCameraModel.PropertyChanged -= OnInfraredModelPropertyChanged;

					_selectedInfraredCameraModel = value;
					OnPropertyChanged( nameof( SelectedInfraredCameraModel ) );

					// 새 모델 이벤트 등록
					if (_selectedInfraredCameraModel != null)
						_selectedInfraredCameraModel.PropertyChanged += OnInfraredModelPropertyChanged;

					// 초기 설정도 반영
					UpdateZAxisWorkPosition();
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

		private double _activeValueFactor;
		public double ActiveValueFactor
		{
			get => _activeValueFactor;
			set
			{
				if (_activeValueFactor != value)
				{
					_activeValueFactor = value;
					OnPropertyChanged( nameof( ActiveValueFactor ) );
					// ActiveValue 바뀌면, 해당 모델 속성에 반영
					UpdateModelFromActiveValue();
				}
			}
		}

		private bool _isMul1Selected;
		public bool IsMul1Selected
		{
			get => _isMul1Selected;
			set
			{
				if (_isMul1Selected != value)
				{
					_isMul1Selected = value;
					OnPropertyChanged( nameof( IsMul1Selected ) );
				}
			}
		}

		private bool _isMul10Selected;
		public bool IsMul10Selected
		{
			get => _isMul10Selected;
			set
			{
				if (_isMul10Selected != value)
				{
					_isMul10Selected = value;
					OnPropertyChanged( nameof( IsMul10Selected ) );
				}
			}
		}

		private bool _isMul100Selected;
		public bool IsMul100Selected
		{
			get => _isMul100Selected;
			set
			{
				if (_isMul100Selected != value)
				{
					_isMul100Selected = value;
					OnPropertyChanged( nameof( IsMul100Selected ) );
				}
			}
		}

		// 커맨드: 어떤 속성을 선택할지
		public ICommand SelectPropertyCommand { get; }
		public ICommand SaveWithNameCommand { get; }
		public ICommand DeleteModelCommand { get; }
		public ICommand ActiveValuePlus { get; }
		public ICommand ActiveValueMinus { get; }
		public ICommand ActiveValueMul1 { get; }
		public ICommand ActiveValueMul10 { get; }
		public ICommand ActiveValueMul100 { get; }


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

			ActiveValuePlus = new RelayCommand( ( ) => { ActiveValue = Math.Round( ActiveValue + ActiveValueTick * ActiveValueFactor, 4 ); } );
			ActiveValueMinus = new RelayCommand( ( ) => { ActiveValue = Math.Round( ActiveValue - ActiveValueTick * ActiveValueFactor, 4 ); } );

			ActiveValueMul1 = new RelayCommand( ( ) =>
			{
				ActiveValueFactor = 1;
				IsMul1Selected = true;
				IsMul10Selected = false;
				IsMul100Selected = false;
			} );

			ActiveValueMul10 = new RelayCommand( ( ) =>
			{
				ActiveValueFactor = 10;
				IsMul1Selected = false;
				IsMul10Selected = true;
				IsMul100Selected = false;
			} );

			ActiveValueMul100 = new RelayCommand( ( ) =>
			{
				ActiveValueFactor = 100;
				IsMul1Selected = false;
				IsMul10Selected = false;
				IsMul100Selected = true;
			} );


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
		private void OnInfraredModelPropertyChanged( object? sender, PropertyChangedEventArgs e )
		{
			if (e.PropertyName == nameof( InfraredCameraModel.ProductHeight ))
			{
				UpdateZAxisWorkPosition();
			}
		}
		private void UpdateZAxisWorkPosition( )
		{
			if (SelectedInfraredCameraModel != null && SelectedMotionModel != null)
			{
				double calculatedValue = (84 - SelectedInfraredCameraModel.ProductHeight) * 1000 + 48000;
				SelectedMotionModel.ZAxisWorkPosition = Math.Max( 0, calculatedValue );
				OnPropertyChanged( nameof( SelectedMotionModel ) ); // 선택적 알림
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
			UpdateZAxisWorkPosition();
		}

		private void SaveAsNewModel( )
		{
			// 1) 실제로 저장할 이름 결정
			string targetName = string.IsNullOrWhiteSpace(NewModelName)
				? SelectedModelName
				: NewModelName.Trim();

			// 2) 두 경우 모두 이름이 비어 있으면 취소
			if (string.IsNullOrWhiteSpace( targetName ))
			{
				Logger.WriteLine( "저장할 모델 이름이 지정되지 않았습니다." );
				return;
			}

			// 3) 같은 이름이 이미 존재하면 사용자에게 덮어쓰기 여부 확인
			bool nameExists = _settingManager.GetAvailableModelNames()
							 .Contains(targetName);
			if (nameExists)
			{
				var result = MessageBox.Show(
			$"'{targetName}' 모델을 덮어쓰시겠습니까?",
			"모델 덮어쓰기 확인",
			MessageBoxButton.YesNo,
			MessageBoxImage.Question);

				if (result != MessageBoxResult.Yes)
					return;
			}


			// 3.5) 기준 온도 계산 및 모델에 저장
			var result1 = MessageBox.Show(
			$"'{targetName}' 기준온도를 재설정하시겠습니까?",
			"기준온도 재설정 확인",
			MessageBoxButton.YesNo,
			MessageBoxImage.Question);

			if (result1 == MessageBoxResult.Yes)
			{
				MIL_ID image = _cameraService.GetImage();
				if (image == MIL.M_NULL)
				{
					Logger.WriteLine( "기준 온도 계산용 이미지가 없습니다." );
					MessageBox.Show( "기준 온도 계산용 이미지가 없습니다.\n기존 설정값으로 대체합니다." );
				}
				else
				{
					foreach (var model in InfraredCameraModels)
					{
						model.ReferenceBaseTemperature = CalculateReferenceRoiAverage( image );
						Logger.WriteLine( $"모델 기준 온도 저장됨: {model.ReferenceBaseTemperature}" );
					}
				}
			}


			// 4) 데이터 구성
			var data = new ModelData
			{
				InfraredCameraModels = InfraredCameraModels.ToList(),
				MotionModels = MotionModels.ToList()
			};

			// 5) 저장
			_settingManager.SaveModelData( targetName, data );

			// 6) 모델 목록 갱신 및 상태 업데이트
			LoadAvailableModelNames();
			SelectedModelName = targetName;
			NewModelName = ""; // 입력창 초기화
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
					ActiveValueMin = 10;
					ActiveValueMax = 380; // 예시: 16비트 이미지의 최대값
					ActiveValueTick = 0.01; // 예시: 1단위로 조정
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
				case "CircleAreaMinLength":
					ActiveValue = SelectedInfraredCameraModel.CircleAreaMinLength;
					ActiveValueMin = 0;
					ActiveValueMax = 100; // 예시: 비율
					ActiveValueTick = 1; // 예시: 0.01 단위로 조정
					_isBinarized = false;
					break;
				case "AvgTemperatureMin":
					ActiveValue = SelectedInfraredCameraModel.AvgTemperatureMin;
					ActiveValueMin = 10; // 예시: 최소 온도
					ActiveValueMax = 100; // 예시: 최대 온도
					ActiveValueTick = 0.01; // 예시: 0.1 단위로 조정
					_isBinarized = false;
					break;
				case "AvgTemperatureMax":
					ActiveValue = SelectedInfraredCameraModel.AvgTemperatureMax;
					ActiveValueMin = 10; // 예시: 최소 온도
					ActiveValueMax = 100; // 예시: 최대 온도
					ActiveValueTick = 0.01; // 예시: 0.1 단위로 조정
					_isBinarized = false;
					break;
				case "ProductHeight":
					ActiveValue = SelectedInfraredCameraModel.ProductHeight;
					ActiveValueMin = 40; // 예시: 최소 온도
					ActiveValueMax = 150; // 예시: 최대 온도
					ActiveValueTick = 0.1; // 예시: 0.1 단위로 조정
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
				case "CircleAreaMinLength":
					SelectedInfraredCameraModel.CircleAreaMinLength = ActiveValue;
					break;
				case "AvgTemperatureMin":
					SelectedInfraredCameraModel.AvgTemperatureMin = ActiveValue;
					break;
				case "AvgTemperatureMax":
					SelectedInfraredCameraModel.AvgTemperatureMax = ActiveValue;
					break;
				case "ProductHeight":
					SelectedInfraredCameraModel.ProductHeight = ActiveValue;
					break;
					// CircleMinRadius, CircleMaxRadius, etc...
			}
			//Logger.WriteLine( $"Updated {ActivePropertyName}: {ActiveValue}" );
			ConversionImage();
		}


		public async void ConversionImage( )
		{
			var result = await Conversion.InfraredCameraModel( true, _isBinarized, _cameraService.GetBinarizedImage(), _cameraService.GetScaleImage(), _cameraService.GetImage(), _cameraService._infraredCameraDisplay, SelectedInfraredCameraModel );
			if (result == null)
			{
				Logger.WriteLine( "ConversionImage 결과가 null입니다." );
				return;
			}

			// 👉 여기에 결과 저장 및 창 띄우기
			LastInspectionResult = result;
			ShowResultWindow();

			if (result.IsGood)

			{
				_mainViewModel.IsGoodColor = "Green";
				_mainViewModel.IsGoodStatus = "Good";

			}
			else
			{
				_mainViewModel.IsGoodColor = "Red";
				_mainViewModel.IsGoodStatus = "Reject";
			}
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
					var result = await Conversion.InfraredCameraModel( true, _isBinarized, _cameraService.GetBinarizedImage(), _cameraService.GetScaleImage(), _cameraService.GetImage(), _cameraService._infraredCameraDisplay, SelectedInfraredCameraModel );
					if (result == null)
					{
						Logger.WriteLine( "ConversionImage 결과가 null입니다." );
						return;
					}
					if (result.IsGood)

					{
						_mainViewModel.IsGoodColor = "Green";
						_mainViewModel.IsGoodStatus = "Good";

					}
					else
					{
						_mainViewModel.IsGoodColor = "Red";
						_mainViewModel.IsGoodStatus = "Reject";
					}
					await Task.Delay( 100 );
				}
			} );
		}

		public void UpdateCameraSettingsStop( )
		{
			_isImageDisplay = false;
		}

		private double CalculateReferenceRoiAverage( MIL_ID image )
		{
			int refX = 610, refY = 10, refW = 20, refH = 20;

			MIL_ID roi = MIL.M_NULL;
			MIL.MbufChild2d( image, refX, refY, refW, refH, ref roi );

			ushort[] roiData = new ushort[refW * refH];
			MIL.MbufGet( roi, roiData );
			MIL.MbufFree( roi );

			// ushort[] → double로 변환 후 평균
			return roiData.Select( v => (v / 100.0) - 273.15 ).Average();  // 단위 변환

		}
		public void ShowResultWindow( )
		{
			if (_resultWindow == null || !_resultWindow.IsVisible)
			{
				Application.Current.Dispatcher.Invoke( ( ) =>
				{
					_resultWindow = new ConversionResultWindow
					{
						DataContext = this,
						WindowStartupLocation = WindowStartupLocation.Manual,
						Width = 280,
						Height = 320,
						Topmost = true,
						ResizeMode = ResizeMode.NoResize,
						ShowInTaskbar = false
					};

					var mainWindow = Application.Current.MainWindow;
					if (mainWindow != null)
					{
						var screenTopLeft = mainWindow.PointToScreen(new Point(0, 0));

						// === 위치 조정 ===
						double settingPanelWidth = 400;     // 오른쪽 설정창 너비
						double resultWindowWidth = 280;     // 결과 요약창 너비
						double padding = 20;                // 설정창과 결과창 사이 여백
						double topMargin = 140;             // 상단 여백 (온도 텍스트 피하기)

						_resultWindow.Left = screenTopLeft.X + mainWindow.ActualWidth - settingPanelWidth - resultWindowWidth - padding - 20;
						_resultWindow.Top = screenTopLeft.Y + topMargin;
					}

					_resultWindow.Closed += ( s, e ) => _resultWindow = null;
					_resultWindow.Show();
				} );
			}
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
