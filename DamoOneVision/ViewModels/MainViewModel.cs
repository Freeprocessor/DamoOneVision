using Advantech.Adam;
using DamoOneVision.Camera;
using DamoOneVision.Data;
using DamoOneVision.Models;
using DamoOneVision.Services;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Forms;  // WinForms 네임스페이스 추가 (참조 필요)
using DamoOneVision.ImageProcessing;
using Newtonsoft.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DamoOneVision.ViewModels
{
	public class MainViewModel: INotifyPropertyChanged
	{
		/// <summary>
		/// PropertyChanged 이벤트 핸들러, WPF 바인딩을 위해 필요
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;

		// View 쪽에서 구독할 이벤트
		public event Action? RequestJetDisplayCapture;

		/// <summary>
		/// INotifyPropertyChanged 기본 구현
		/// </summary>
		/// <param name="propertyName"></param>
		private void OnPropertyChanged( string propertyName )
		=> PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );


		/// <summary>
		/// 로컬 앱 폴더 경로 app/local/DamoOneVision
		/// </summary>
		private string appFolder = "";

		/// <summary>
		/// 모델 폴더 경로 app/local/DamoOneVision/Model
		/// </summary>
		private string modelfolder = "";

		/// <summary>
		/// 모델 파일 경로 app/local/DamoOneVision/Model/Models.model
		/// </summary>
		private string modelfile = "";

		/// <summary>
		/// 이미지 경로 리스트
		/// UI 바인딩을 위해 ObservableCollection 사용
		/// </summary>
		public ObservableCollection<string> ImagePaths { get; set; }

		/// <summary>
		/// JSON 파일 핸들러
		/// </summary>
		private readonly JsonHandler _jsonHandler;

		/// <summary>
		/// 적외선 열화상 카메라 모델 리스트
		/// </summary>
		public ObservableCollection<InfraredCameraModel> InfraredCameraModels { get; set; }

		/// <summary>
		/// 현재 선택된/사용중인 적외선 열화상 카메라 모델
		/// </summary>
		private InfraredCameraModel currentInfraredCameraModel;


		/// <summary>
		/// StartClockAsync를 중지하기 위한 플래그
		/// </summary>
		private bool _stopClock;

		private DateTime? _lastProductTime = null;

		/// <summary>
		/// 현재 시간 내부 변수
		/// </summary>
		private string _currentTime;

		/// <summary>
		/// 현재 시간 UI 바인딩을 위한 프로퍼티
		/// </summary>
		public string CurrentTime
		{
			get => _currentTime;
			set
			{
				if (_currentTime != value)
				{
					_currentTime = value;
					OnPropertyChanged( nameof( CurrentTime ) );
				}
			}
		}

		private InfraredInspectionResult _inspectionResult = new();
		public InfraredInspectionResult InspectionResult
		{
			get => _inspectionResult;
			set
			{
				_inspectionResult = value;
				OnPropertyChanged( nameof( InspectionResult ) );
			}
		}

		private readonly DeviceControlService _deviceControlService;


		private readonly CameraService _cameraService;




		private string _isGoodStatus;
		public string IsGoodStatus
		{
			get => _isGoodStatus;
			set
			{
				if (_isGoodStatus != value)
				{
					_isGoodStatus = value;
					OnPropertyChanged( nameof( IsGoodStatus ) );
				}
			}
		}

		private string _isGoodColor;
		public string IsGoodColor
		{
			get => _isGoodColor;
			set
			{
				if (_isGoodColor != value)
				{
					_isGoodColor = value;
					OnPropertyChanged( nameof( IsGoodColor ) );
				}
			}
		}


		private string _selectedImage;
		public string SelectedImage
		{
			get => _selectedImage;
			set
			{
				if (_selectedImage != value)
				{
					_selectedImage = value;
					OnPropertyChanged( nameof( SelectedImage ) );
					// 이미지가 선택되었을 때 자동으로 실행할 로직을 여기에 넣거나 커맨드를 실행할 수 있음.
				}
			}
		}

		private string _currentTemperature;
		public string CurrentTemperature
		{
			get => _currentTemperature;
			set
			{
				if (_currentTemperature != value)
				{
					_currentTemperature = value;
					OnPropertyChanged( nameof( CurrentTemperature ) );
				}
			}
		}

		private int _goodCount;
		public int GoodCount
		{
			get => _goodCount;
			set
			{
				if (_goodCount != value)
				{
					_goodCount = value;
					_settingManager.Settings.GoodCount = value;
					_settingManager.SaveSettings(); // ✅ 저장!
					OnPropertyChanged( nameof( GoodCount ) );
					OnPropertyChanged( nameof( GoodDisplay ) );
				}
			}
		}

		private int _rejectCount;
		public int RejectCount
		{
			get => _rejectCount;
			set
			{
				if (_rejectCount != value)
				{
					_rejectCount = value;
					_settingManager.Settings.RejectCount = value;
					_settingManager.SaveSettings(); // ✅ 저장!
					OnPropertyChanged( nameof( RejectCount ) );
					OnPropertyChanged( nameof( RejectDisplay ) );
				}
			}
		}

		private double _rate;
		public string RateDisplay => $"{_rate:F0} bpm";

		public string GoodDisplay => $"{GoodCount:N0} pcs ({GoodPercent}%)";
		public string RejectDisplay => $"{RejectCount:N0} pcs ({RejectPercent}%)";

		private string _modelName = "True Seal";
		public string ModelName
		{
			get => _modelName;
			set { _modelName = value; OnPropertyChanged( nameof( ModelName ) ); }
		}

		private int GoodPercent => (GoodCount + RejectCount) > 0 ? GoodCount * 100 / (GoodCount + RejectCount) : 0;
		private int RejectPercent => (GoodCount + RejectCount) > 0 ? RejectCount * 100 / (GoodCount + RejectCount) : 0;




		


		// -----------------------------------------------------------------------
		// 2. 실제로 UI에서 바인딩할 Command들
		// -----------------------------------------------------------------------

		public ICommand MachineStartCommand { get; }
		public ICommand MachineStopCommand { get; }



		public ICommand TestGoodCommand { get; }
		public ICommand TestRejectCommand { get; }


		private readonly SettingManager _settingManager;
		//public MainViewModel()
		//{

		//}

		public MainViewModel( DeviceControlService deviceControlService, CameraService cameraService, SettingManager settingManager)
		{
			_deviceControlService = deviceControlService;
			_cameraService = cameraService;
			_settingManager = settingManager;

			InitLocalAppFolder();
			_jsonHandler = new JsonHandler( modelfile );

			InfraredCameraModels = new ObservableCollection<InfraredCameraModel>();
			LoadModelsAsync();

			//이벤트 구독
			_deviceControlService.TriggerDetected += async ( ) =>
			{
				// Service가 "Trigger 발생"을 알리면 이 콜백이 실행됨
				bool result = await _cameraService.VisionTrigger();
				return result;
			};



			MachineStartCommand = new AsyncRelayCommand(
				async _ => await Task.Run(async()=>{
					/// 카메라 연결 실패, 성공여부 확인해서 함수 중단 or 계속진행
					await _cameraService.ConnectAction();
					_deviceControlService.ConveyorReadStart();
					//await _deviceControlService.ZAxisMoveEndPos();
					//Logger.WriteLine( "Camera AutoFocus Start" );
					//_cameraService.InfraredCameraAutoFocus();
					//await Task.Delay( 5000 );
					//Logger.WriteLine( "Camera AutoFocus End" );
					//_cameraService.InfraredCameraManualFocus();
					await _deviceControlService.ZAxisMoveWorkPos();
					await _deviceControlService.MachineStartAction();

				} )
			);

			MachineStopCommand = new AsyncRelayCommand(
				async _ => await Task.Run( async ( ) => {
					await _deviceControlService.MachineStopAction();
					_deviceControlService.ConveyorReadStop();
				} )
			);


			//ListBoxSelectionChangedCommand = new RelayCommand(
			//	_ => ListBox_SelectionChanged()



			///
			TestGoodCommand = new RelayCommand( ( ) => OnProductDetected( true ) );
			TestRejectCommand = new RelayCommand( ( ) => OnProductDetected( false ) );


			///
			ModelName = _settingManager.CurrentModel;

			GoodCount = _settingManager.Settings.GoodCount;
			RejectCount = _settingManager.Settings.RejectCount;

			IsGoodColor = "Transparent";

		}




		/// <summary>
		/// 로컬 앱 폴더 및 이미지 폴더, 모델 폴더 생성
		/// </summary>
		private void InitLocalAppFolder( )
		{
			ImagePaths = new ObservableCollection<string>();
			string localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			appFolder = System.IO.Path.Combine( localAppData, "DamoOneVision" );
			/// 임시 추가. 추후에 날짜별로 정리해서 불러올 수 있도록 수정
			//imageFolder = System.IO.Path.Combine( imageFolder, "2025-04-2" );
			modelfolder = System.IO.Path.Combine( appFolder, "Model" );
			modelfile = System.IO.Path.Combine( modelfolder, "Models.json" );
			if (!Directory.Exists( appFolder ))
			{
				Directory.CreateDirectory( appFolder );
			}
		}

		/// <summary>
		/// 모델 데이터를 JSON 파일로 저장하는 메서드
		/// </summary>
		private async void SaveModelsAsync( )
		{
			var data = new ModelData { InfraredCameraModels = new List<InfraredCameraModel>(InfraredCameraModels) };
			await _jsonHandler.SaveModelsAsync( data );
		}

		/// <summary>
		/// 적외선 열화상 카메라 모델 추가 버튼 클릭 이벤트 핸들러
		/// </summary>
		private async void LoadModelsAsync( )
		{
			var data = await _jsonHandler.LoadModelsAsync();
			InfraredCameraModels.Clear();
			foreach (var model in data.InfraredCameraModels)
			{
				InfraredCameraModels.Add( model );
			}
			currentInfraredCameraModel = InfraredCameraModels[ 0 ];
			await Task.CompletedTask;
		}





		//private void ListBox_SelectionChanged( )
		//{
		//	if (e.AddedItems.Count > 0)
		//	{
		//		string? selectedImagePath = e.AddedItems[0] as string;
		//		if (!string.IsNullOrEmpty( selectedImagePath ) && File.Exists( selectedImagePath ))
		//		{
		//			// 선택된 이미지를 VisionImage에 표시
		//			try
		//			{
		//				_infraredCameraImage = _infraredCamera.LoadImage( MilSystem, selectedImagePath );
		//				MIL.MdispSelect( _mainInfraredCameraDisplay, _infraredCameraImage );
		//			}
		//			catch (Exception ex)
		//			{
		//				MessageBox.Show( $"이미지를 불러오는 중 오류 발생: {ex.Message}" );
		//				Logger.WriteLine( $"이미지를 불러오는 중 오류 발생: {ex.Message}" );
		//			}
		//		}
		//	}
		//}

		//private void LoadModel( string modelData )
		//{
		//	// 여기에 모델 로딩 로직을 구현하세요
		//	DeserializeModelData( modelData );
		//	MessageBox.Show( "모델이 로드되었습니다 : {modelData}" );
		//	Logger.WriteLine( "모델이 로드되었습니다 : {modelData}" );


		//	// 예를 들어, modelData를 역직렬화하여 애플리케이션의 상태나 설정에 적용할 수 있습니다.
		//}


		//private void DeserializeModelData( string serializedData )
		//{
		//	var items = JsonConvert.DeserializeObject<List<ComboBoxItemViewModel>>(serializedData);
		//}

		public void OnProductDetected( bool isGood )
		{
			var now = DateTime.Now;

			if (_lastProductTime != null)
			{
				var diff = now - _lastProductTime.Value;
				if (diff.TotalSeconds > 0)
				{
					_rate = 60.0 / diff.TotalSeconds;
					OnPropertyChanged( nameof( RateDisplay ) );
				}
			}
			_lastProductTime = now;

			if (isGood) GoodCount++;
			else RejectCount++;

			OnPropertyChanged( nameof( GoodDisplay ) );
			OnPropertyChanged( nameof( RejectDisplay ) );
		}






		/// <summary>
		/// 현재 시간을 1초마다 업데이트
		/// </summary>
		public async void StartClockAsync( )
		{
			_stopClock = false;

			await Task.Run( ( ) =>
			{
				while (!_stopClock)
				{
					Application.Current.Dispatcher.Invoke( ( ) =>
					{
						CurrentTime = DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" );
					} );

					Thread.Sleep( 1000 );
				}
			} );
		}

		public void StopClock( )
		{
			_stopClock = true;
		}



	}

}
