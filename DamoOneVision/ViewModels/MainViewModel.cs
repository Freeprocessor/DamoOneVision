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

		/// <summary>
		/// INotifyPropertyChanged 기본 구현
		/// </summary>
		/// <param name="propertyName"></param>
		private void OnPropertyChanged( string propertyName )
		=> PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );


		
		private StringBuilder _logBuilder = new StringBuilder();
		public string LogContents
		{
			get => _logBuilder.ToString();
			private set
			{
				_logBuilder = new StringBuilder( value );
				OnPropertyChanged( nameof( LogContents ) );
			}
		}

		/// <summary>
		/// 로컬 앱 폴더 경로 app/local/DamoOneVision
		/// </summary>
		private string appFolder = "";

		/// <summary>
		/// 이미지 폴더 경로 app/local/DamoOneVision/Images
		/// </summary>
		private string imageFolder = "";

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

		private readonly DeviceControlService _deviceControlService;


		private readonly CameraService _cameraService;


		private bool _isVisionConnected;
		public bool IsVisionConnected
		{
			get => _isVisionConnected;
			set
			{
				if (_isVisionConnected != value)
				{
					_isVisionConnected = value;
					OnPropertyChanged( nameof( IsVisionConnected ) );
					(ConnectCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
					(DisconnectCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
				}
			}	
		}

		private bool _isBusy;
		public bool IsBusy
		{
			get => _isBusy;
			set
			{
				if (_isBusy != value)
				{
					_isBusy = value;
					OnPropertyChanged( nameof( IsBusy ) );
					(ConnectCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
					(DisconnectCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
				}
			}
		}

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


		// Connect 버튼이 활성화되는 조건 (예: 아직 연결 안 됐고, 작업 중이 아님)
		public bool CanConnect => !IsVisionConnected && !IsBusy;

		// Disconnect 버튼이 활성화되는 조건 (예: 이미 연결되어 있고, 작업 중이 아님)
		public bool CanDisconnect => IsVisionConnected && !IsBusy;

		


		// -----------------------------------------------------------------------
		// 2. 실제로 UI에서 바인딩할 Command들
		// -----------------------------------------------------------------------
		public ICommand ConnectCommand { get; }
		public ICommand DisconnectCommand { get; }
		public ICommand MachineStartCommand { get; }
		public ICommand MachineStopCommand { get; }
		public ICommand VisionTriggerCommand { get; }
		public ICommand LoadImagesCommand { get; }
		public ICommand ImageSelectedCommand { get; }



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


			// Logger 이벤트를 받아, ViewModel에서 로그를 누적
			Logger.OnLogReceived += ( s, msg ) =>
			{
				Application.Current?.Dispatcher?.Invoke( ( ) =>
				{
					_logBuilder.AppendLine( msg );
					// TextBox와 바인딩된 프로퍼티를 갱신
					OnPropertyChanged( nameof( LogContents ) );
				} );
			};

			//이벤트 구독
			_deviceControlService.TriggerDetected += async ( ) =>
			{
				// Service가 "Trigger 발생"을 알리면 이 콜백이 실행됨
				bool result = await _cameraService.VisionTrigger();
				return result;
			};

			ConnectCommand = new AsyncRelayCommand(
				_cameraService.ConnectAction,
				() => CanConnect
			);

			DisconnectCommand = new AsyncRelayCommand(
				_cameraService.DisconnectAction,
				() => CanDisconnect
			);

			MachineStartCommand = new AsyncRelayCommand(
				async _ => await Task.Run(async()=>{
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

			VisionTriggerCommand = new AsyncRelayCommand(
				_ => _cameraService.VisionTrigger()
			);
			_cameraService.CameraConnectedChanged += OnCameraConnectedChanged;
			_cameraService.BusyStateChanged += OnBusyStateChanged;
			//ListBoxSelectionChangedCommand = new RelayCommand(
			//	_ => ListBox_SelectionChanged()
			//);
			LoadImagesCommand = new AsyncRelayCommand( _ => LoadAllImages());

			ImageSelectedCommand = new RelayCommand<string>( OnImageSelected );


			///
			TestGoodCommand = new RelayCommand( ( ) => OnProductDetected( true ) );
			TestRejectCommand = new RelayCommand( ( ) => OnProductDetected( false ) );


			///
			ModelName = _settingManager.CurrentModel;

			GoodCount = _settingManager.Settings.GoodCount;
			RejectCount = _settingManager.Settings.RejectCount;

		}

		private void OnCameraConnectedChanged( bool connected )
		{
			// UI 스레드에서 프로퍼티를 업데이트해야 할 수 있으므로 Dispatcher 사용 고려
			Application.Current.Dispatcher.Invoke( ( ) =>
			{
				IsVisionConnected = connected;
			} );
		}

		private void OnBusyStateChanged( bool busy )
		{
			Application.Current.Dispatcher.Invoke( ( ) =>
			{
				IsBusy = busy;
			} );
		}


		/// <summary>
		/// 로컬 앱 폴더 및 이미지 폴더, 모델 폴더 생성
		/// </summary>
		private void InitLocalAppFolder( )
		{
			ImagePaths = new ObservableCollection<string>();
			string localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			appFolder = System.IO.Path.Combine( localAppData, "DamoOneVision" );
			imageFolder = System.IO.Path.Combine( appFolder, "Images" );
			/// 임시 추가. 추후에 날짜별로 정리해서 불러올 수 있도록 수정
			imageFolder = System.IO.Path.Combine( imageFolder, "InfraredCamera" );
			imageFolder = System.IO.Path.Combine( imageFolder, "RAWInfraredCamera" );
			//imageFolder = System.IO.Path.Combine( imageFolder, "2025-04-2" );
			modelfolder = System.IO.Path.Combine( appFolder, "Model" );
			modelfile = System.IO.Path.Combine( modelfolder, "Models.json" );
			if (!Directory.Exists( appFolder ))
			{
				Directory.CreateDirectory( appFolder );
			}
			if (!Directory.Exists( imageFolder ))
			{
				Directory.CreateDirectory( imageFolder );
			}
		}

		/// <summary>
		/// 이미지 로드버튼 클릭 이벤트 핸들러 
		/// 이미지 폴더 내의 모든 BMP 파일 로드
		/// </summary>
		public async Task LoadAllImages( )
		{
			var dialog = new CommonOpenFileDialog
			{
				IsFolderPicker = true,
				Title = "이미지가 있는 폴더를 선택하세요.",
				InitialDirectory = imageFolder // 기본 경로 설정
			};

			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				string selectedFolder = dialog.FileName;
				ImagePaths.Clear();
				string[] files = Directory.GetFiles(selectedFolder, "*.bmp");

				foreach (var file in files)
				{
					ImagePaths.Add( file );
				}

				MessageBox.Show( $"{files.Length}개의 이미지가 로드되었습니다." );
				Logger.WriteLine( $"{files.Length}개의 이미지가 로드되었습니다." );
			}
		}


		private void OnImageSelected( string imagePath )
		{
			if (!string.IsNullOrEmpty( imagePath ) && File.Exists( imagePath ))
			{
				_cameraService.InfraredCameraLoadImage( imagePath );
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
