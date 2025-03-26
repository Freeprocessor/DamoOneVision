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
using DamoOneVision.ImageProcessing;
using Newtonsoft.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace DamoOneVision.ViewModels
{
	internal class MainViewModel: INotifyPropertyChanged
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
		public ICommand ListBoxSelectionChangedCommand { get; }




		SettingManager settingManager;
		//public MainViewModel()
		//{

		//}

		public MainViewModel( DeviceControlService deviceControlService, CameraService cameraService)
		{
			_deviceControlService = deviceControlService;
			_cameraService = cameraService;


			InitLocalAppFolder();
			_jsonHandler = new JsonHandler( modelfile );

			InfraredCameraModels = new ObservableCollection<InfraredCameraModel>();
			LoadModelsAsync();


			//이벤트 구독
			_deviceControlService.TriggerDetected += async ( ) =>
			{
				// Service가 "Trigger 발생"을 알리면 이 콜백이 실행됨
				await _cameraService.VisionTrigger();
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


			settingManager = new SettingManager( deviceControlService );
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
			modelfolder = System.IO.Path.Combine( appFolder, "Model" );
			modelfile = System.IO.Path.Combine( modelfolder, "Models.model" );
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
		private void LoadAllImages( )
		{
			// Images 폴더 내의 모든 BMP 파일 로드
			ImagePaths.Clear(); // 기존 리스트 비우기(원하는 경우 생략)
								//이미지가 있는지 확인, 없으면 만들기

			string[] files = Directory.GetFiles(imageFolder, "*.bmp");

			foreach (var file in files)
			{
				ImagePaths.Add( file );
			}

			MessageBox.Show( $"{files.Length}개의 이미지가 로드되었습니다." );
			Logger.WriteLine( $"{files.Length}개의 이미지가 로드되었습니다." );
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


		/// <summary>
		/// 모델 수정 버튼 클릭 이벤트 핸들러
		/// </summary>
		private void EditModel( )
		{
			if (true)
			{
				// 선택된 모델의 복사본 생성 (원본 변경을 방지)
				var modelCopy = new InfraredCameraModel
				{
					Name = currentInfraredCameraModel.Name,
					CircleCenterX = currentInfraredCameraModel.CircleCenterX,
					CircleCenterY = currentInfraredCameraModel.CircleCenterY,
					CircleMinRadius = currentInfraredCameraModel.CircleMinRadius,
					CircleMaxRadius = currentInfraredCameraModel.CircleMaxRadius,
					BinarizedThreshold = currentInfraredCameraModel.BinarizedThreshold
				};

				/// TODO : 수정할 모델을 설정하는 창 생성
				//var saveWindow = new SettingWindow(modelCopy);
				//if (saveWindow.ShowDialog() == true)
				//{
				//	// 원본 모델 업데이트
				//	currentInfraredCameraModel.Name = saveWindow.Model.Name;
				//	currentInfraredCameraModel.CircleCenterX = saveWindow.Model.CircleCenterX;
				//	currentInfraredCameraModel.CircleCenterY = saveWindow.Model.CircleCenterY;
				//	currentInfraredCameraModel.CircleMinRadius = saveWindow.Model.CircleMinRadius;
				//	currentInfraredCameraModel.CircleMaxRadius = saveWindow.Model.CircleMaxRadius;
				//	currentInfraredCameraModel.BinarizedThreshold = saveWindow.Model.BinarizedThreshold;

				//	SaveModelsAsync(); // 자동 저장
				//}
			}
			//else
			//{
			//	MessageBox.Show( "수정할 모델을 선택하세요.", "선택 오류", MessageBoxButton.OK, MessageBoxImage.Warning );
			//}
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

		private void LoadModel( string modelData )
		{
			// 여기에 모델 로딩 로직을 구현하세요
			DeserializeModelData( modelData );
			MessageBox.Show( "모델이 로드되었습니다 : {modelData}" );
			Logger.WriteLine( "모델이 로드되었습니다 : {modelData}" );


			// 예를 들어, modelData를 역직렬화하여 애플리케이션의 상태나 설정에 적용할 수 있습니다.
		}


		private void DeserializeModelData( string serializedData )
		{
			var items = JsonConvert.DeserializeObject<List<ComboBoxItemViewModel>>(serializedData);
		}






		/// <summary>
		/// 현재 시간을 1초마다 업데이트
		/// </summary>
		public async void StartClockAsync( )
		{
			//시계 중지 플래그 초기화
			_stopClock = false;

			await Task.Run( ( ) =>
			{
				while (!_stopClock)
				{
					CurrentTime = DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" );

					System.Threading.Thread.Sleep( 1000 );
				}
			} );

		}

		public void StopClock( )
		{
			_stopClock = true;
		}









	}

}
