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

		private readonly ModbusService _modbus;
		private readonly AdvantechCard _advantechCard;


		/// <summary>
		/// 열화상 Camera
		/// </summary>
		private readonly CameraManager _infraredCamera;
		/// <summary>
		/// 측면 1 Camera
		/// </summary>
		private readonly CameraManager _sideCamera1;
		/// <summary>
		/// 측면 2 Camera
		/// </summary>
		private readonly CameraManager _sideCamera2;
		/// <summary>
		/// 측면	3 Camera	
		/// </summary>
		private readonly CameraManager _sideCamera3;

		private MIL_ID MilSystem = MIL.M_NULL;

		private MIL_ID _infraredCameraImage;
		private MIL_ID _infraredCameraConversionImage;

		private MIL_ID _sideCamera1Image;
		private MIL_ID _sideCamera1ConversionImage;

		private MIL_ID _sideCamera2Image;
		private MIL_ID _sideCamera2ConversionImage;

		private MIL_ID _sideCamera3Image;
		private MIL_ID _sideCamera3ConversionImage;


		private MIL_ID _mainInfraredCameraDisplay;
		private MIL_ID _mainSideCamera1Display;
		private MIL_ID _mainSideCamera2Display;
		private MIL_ID _mainSideCamera3Display;


		/// <summary>
		/// 카메라 연속 촬영 모드
		/// </summary>
		private bool _isVisionContinuous = false; // Continuous 모드 상태

		/// <summary>
		/// 이미지 캡처 중인지 여부
		/// </summary>
		private bool _isVisionCapturing = false; 

		/// <summary>
		/// 카메라 연결 상태
		/// </summary>
		private bool _isVisionConnected = false;
		public bool IsVisionConnected
		{
			get => _isVisionConnected;
			set
			{
				if (_isVisionConnected != value)
				{
					_isVisionConnected = value;
					OnPropertyChanged( nameof( IsVisionConnected ) );
					// 연결 상태 바뀌면 커맨드 CanExecute 재평가 필요
					(ConnectCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
					(DisconnectCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
				}
			}
		}


		/// <summary>
		/// Camera 연결 중인지 여부
		/// </summary>
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

					// IsBusy 바뀌면 CanExecute 바뀔 수 있음
					(ConnectCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
					(DisconnectCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
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

		public MainViewModel( ModbusService modbus, AdvantechCard advantechCard, CameraManager infraredCamera, CameraManager sideCamera1, CameraManager sideCamera2, CameraManager sideCamera3,
			MIL_ID infraredCameraImage, MIL_ID infraredCameraConversionImage, MIL_ID sideCamera1Image, MIL_ID sideCamera1ConversionImage, MIL_ID sideCamera2Image, MIL_ID sideCamera2ConversionImage, MIL_ID sideCamera3Image, MIL_ID sideCamera3ConversionImage,
			MIL_ID mainInfraredCameraDisplay, MIL_ID mainSideCamera1Display, MIL_ID mainSideCamera2Display, MIL_ID mainSideCamera3Display)
		{

			MilSystem = MILContext.Instance.MilSystem;

			_infraredCameraImage = infraredCameraImage;
			_infraredCameraConversionImage = infraredCameraConversionImage;
			_sideCamera1Image = sideCamera1Image;
			_sideCamera1ConversionImage = sideCamera1ConversionImage;
			_sideCamera2Image = sideCamera2Image;
			_sideCamera2ConversionImage = sideCamera2ConversionImage;
			_sideCamera3Image = sideCamera3Image;
			_sideCamera3ConversionImage = sideCamera3ConversionImage;

			_mainInfraredCameraDisplay = mainInfraredCameraDisplay;
			_mainSideCamera1Display = mainSideCamera1Display;
			_mainSideCamera2Display = mainSideCamera2Display;
			_mainSideCamera3Display = mainSideCamera3Display;


			_modbus = modbus;
			_advantechCard = advantechCard;

			_infraredCamera = infraredCamera;
			_sideCamera1 = sideCamera1;
			_sideCamera2 = sideCamera2;
			_sideCamera3 = sideCamera3;

			InitLocalAppFolder();
			_jsonHandler = new JsonHandler( modelfile );

			InfraredCameraModels = new ObservableCollection<InfraredCameraModel>();
			LoadInfraredModelsAsync();


			//이벤트 구독
			advantechCard.TriggerDetected += async ( ) =>
			{
				// Service가 "Trigger 발생"을 알리면 이 콜백이 실행됨
				await VisionTrigger();
			};

			ConnectCommand = new AsyncRelayCommand(
				async _ => await ConnectAction(),
				_ => CanConnect
			);

			DisconnectCommand = new AsyncRelayCommand(
				async _ => await DisconnectAction(),
				_ => CanDisconnect
			);

			MachineStartCommand = new AsyncRelayCommand(
				async _ => await MachineStartAction()
			);

			MachineStopCommand = new AsyncRelayCommand(
				async _ => await MachineStopAction()
			);

			VisionTriggerCommand = new RelayCommand(
				_ => VisionTrigger()
			);

			//ListBoxSelectionChangedCommand = new RelayCommand(
			//	_ => ListBox_SelectionChanged()
			//);






			settingManager = new SettingManager();
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
		private async void SaveInfraredModelsAsync( )
		{
			var data = new InfraredCameraModelData { InfraredCameraModels = new List<InfraredCameraModel>(InfraredCameraModels) };
			await _jsonHandler.SaveInfraredModelsAsync( data );
		}

		/// <summary>
		/// 적외선 열화상 카메라 모델 추가 버튼 클릭 이벤트 핸들러
		/// </summary>
		private async void LoadInfraredModelsAsync( )
		{
			var data = await _jsonHandler.LoadInfraredModelsAsync();
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

				var saveWindow = new SettingWindow(modelCopy);
				if (saveWindow.ShowDialog() == true)
				{
					// 원본 모델 업데이트
					currentInfraredCameraModel.Name = saveWindow.Model.Name;
					currentInfraredCameraModel.CircleCenterX = saveWindow.Model.CircleCenterX;
					currentInfraredCameraModel.CircleCenterY = saveWindow.Model.CircleCenterY;
					currentInfraredCameraModel.CircleMinRadius = saveWindow.Model.CircleMinRadius;
					currentInfraredCameraModel.CircleMaxRadius = saveWindow.Model.CircleMaxRadius;
					currentInfraredCameraModel.BinarizedThreshold = saveWindow.Model.BinarizedThreshold;

					SaveInfraredModelsAsync(); // 자동 저장
				}
			}
			//else
			//{
			//	MessageBox.Show( "수정할 모델을 선택하세요.", "선택 오류", MessageBoxButton.OK, MessageBoxImage.Warning );
			//}
		}


		private async Task ConnectAction( )
		{
			if (IsVisionConnected)
			{
				Logger.WriteLine( "이미 카메라가 연결되어 있습니다." );
				return;
			}
			IsBusy = true;
			try
			{
				var tasks = new[]
				{
					_infraredCamera.ConnectAsync( ),
					_sideCamera1.ConnectAsync( ),
					_sideCamera2.ConnectAsync( ),
					_sideCamera3.ConnectAsync( )
				};

				await Task.WhenAll( tasks );

				IsVisionConnected = true;

			}
			catch (Exception ex)
			{
				MessageBox.Show( $"카메라 연결 오류\n{ex.Message}" );
				Logger.WriteLine( $"카메라 연결 오류\n{ex.Message}" );
				var tasks = new[]
				{
					_infraredCamera.DisconnectAsync(),
					_sideCamera1.DisconnectAsync(),
					_sideCamera2.DisconnectAsync(),
					_sideCamera3.DisconnectAsync()
				};

				await Task.WhenAll( tasks );

			}
			finally
			{
				IsBusy = false;
			}
		}

		private async Task DisconnectAction( )
		{
			if (!_isVisionConnected)
			{
				Logger.WriteLine( "카메라가 연결되어 있지 않습니다." );
				return;
			}
			IsBusy = true;

			_infraredCameraImage = _infraredCamera.ReciveImage();
			_sideCamera1Image = _sideCamera1.ReciveImage();
			_sideCamera2Image = _sideCamera2.ReciveImage();
			_sideCamera3Image = _sideCamera3.ReciveImage();

			var tasks = new[]
				{
					_infraredCamera.DisconnectAsync(),
					_sideCamera1.DisconnectAsync(),
					_sideCamera2.DisconnectAsync(),
					_sideCamera3.DisconnectAsync()
				};

			await Task.WhenAll( tasks );

			IsBusy = false;
			IsVisionConnected = false;
		}

		private async Task MachineStartAction( )
		{

			await ConnectAction();

			await _modbus.SelfHolding( 1, 1 );
			await _modbus.SelfHolding( 4, 4 );

			await _modbus.SelfHolding( 0x20, 0x20 );
			await _modbus.SelfHolding( 0x22, 0x22 );
			await _modbus.SelfHolding( 0x24, 0x24 );

			//modbus.WriteHoldingRegisters32( 0, 0x00, 20000 );
			int pos = 85000;
			int speed = 20000;
			_modbus.HoldingRegister32[ 0x00 ] = pos;
			_modbus.HoldingRegister32[ 0x01 ] = speed;

			await Task.Delay( 100 );

			if (_modbus.InputRegister32[ 0x00 ] != pos)
			{
				await Task.Run( ( ) =>
				{
					//modbus.WriteSingleCoil( 0, 0x0A, true );
					_modbus.OutputCoil[ 0x0A ] = true;
					Logger.WriteLine( "Servo Move Start" );
					var startTime = DateTime.Now;
					while (true)
					{
						//bool[] coil = modbus.ReadInputs( 0, 0x0A, 1 );
						if (_modbus.InputCoil[ 0x0A ])
						{
							//modbus.WriteSingleCoil( 0, 0x0A, false );
							_modbus.OutputCoil[ 0x0A ] = false;
							Logger.WriteLine( "Servo Moveing..." );
							break;
						}
						if ((DateTime.Now - startTime).TotalMilliseconds > 15000) // 10초 타임아웃
						{
							_modbus.OutputCoil[ 0x0A ] = false;
							Logger.WriteLine( "SelfHolding operation timed out." );
							//throw new TimeoutException( "SelfHolding operation timed out." );
							break;
						}
						Thread.Sleep( 10 );
					}
					startTime = DateTime.Now;
					Logger.WriteLine( "Servo Move Complete 대기" );
					//modbus.WriteSingleCoil( 0, 0x0B, true );
					_modbus.OutputCoil[ 0x0B ] = false;
					while (true)
					{
						//bool[] coil = modbus.ReadInputs( 0, 0x0B, 1 );
						if (_modbus.InputCoil[ 0x0B ])
						{
							//modbus.WriteSingleCoil( 0, 0x0B, false );
							_modbus.OutputCoil[ 0x0B ] = false;
							Logger.WriteLine( "Servo Move Complete" );
							break;
						}
						if ((DateTime.Now - startTime).TotalMilliseconds > 15000) // 10초 타임아웃
						{
							_modbus.OutputCoil[ 0x0B ] = false;
							Logger.WriteLine( "SelfHolding operation timed out." );
							//throw new TimeoutException( "SelfHolding operation timed out." );
							break;
						}
						Thread.Sleep( 10 );
					}
				} );
			}

			await _modbus.SelfHolding( 0x10, 0x10 );


			Logger.WriteLine( "Trigger Reading Start." );
			_advantechCard.TriggerReadingStartAsync();
			Logger.WriteLine( "Machine Start." );
		}

		private async Task MachineStopAction( )
		{


			Logger.WriteLine( "Trigger Reading Stop." );
			await _modbus.SelfHolding( 0x11, 0x11 );
			Logger.WriteLine( "C/V OFF" );
			await _modbus.SelfHolding( 0x21, 0x21 );
			await _modbus.SelfHolding( 0x23, 0x23 );
			await _modbus.SelfHolding( 0x25, 0x25 );

			await _modbus.SelfHolding( 5, 5 );
			await _modbus.SelfHolding( 0, 0 );
			Logger.WriteLine( "Machine Stop." );
		}

		private async Task VisionTrigger( )
		{
			Stopwatch TectTime = new Stopwatch();
			TectTime.Start();

			if ((!_infraredCamera.IsConnected && !_sideCamera1.IsConnected && !_sideCamera2.IsConnected && !_sideCamera3.IsConnected) &&
				(_infraredCameraImage == MIL.M_NULL && _sideCamera1Image == MIL.M_NULL && _sideCamera2Image == MIL.M_NULL && _sideCamera3Image == MIL.M_NULL))
			{
				Logger.WriteLine( "카메라가 연결되어 있지 않고, 로드된 이미지도 없습니다." );
				MessageBox.Show( "카메라가 연결되어 있지 않고, 로드된 이미지도 없습니다." );

				return;
			}
			Logger.WriteLine( "Vision Trigger Detected" );

			//if (isContinuous)
			//{
			//	MessageBox.Show( "Continuous 모드에서는 Trigger 기능을 사용할 수 없습니다." );
			//	return;
			//}
			await Task.Run( async ( ) =>
			{
				if (!_isVisionCapturing)
				{
					_isVisionCapturing = true;


					if (_infraredCamera.IsConnected && _sideCamera1.IsConnected && _sideCamera2.IsConnected && _sideCamera3.IsConnected)
					{
						try
						{
							// 카메라에서 이미지 캡처
							var tasks = new[]
							{
								_infraredCamera.CaptureSingleImageAsync(),
								_sideCamera1.CaptureSingleImageAsync(),
								_sideCamera2.CaptureSingleImageAsync(),
								_sideCamera3.CaptureSingleImageAsync()
							};
							await Task.WhenAll( tasks );

							Logger.WriteLine( "카메라 이미지 캡처 완료" );

							_infraredCameraImage = _infraredCamera.ReciveImage();
							_sideCamera1Image = _sideCamera1.ReciveImage();
							_sideCamera2Image = _sideCamera2.ReciveImage();
							_sideCamera3Image = _sideCamera3.ReciveImage();

							Logger.WriteLine( "카메라 이미지 수신 완료" );
						}
						catch (Exception ex)
						{
							Logger.WriteLine( $"이미지 캡쳐 중 오류 발생: {ex.Message}" );
							MessageBox.Show( $"이미지 캡쳐 중 오류 발생: {ex.Message}" );

						}


						try
						{

							/// 이미지를 Property에 저장하고 화면에 표시하는 로직을 구성해야함
							/// 

							MIL.MdispSelect( _mainInfraredCameraDisplay, _infraredCameraImage );
							MIL.MdispSelect( _mainSideCamera1Display, _sideCamera1Image );
							MIL.MdispSelect( _mainSideCamera2Display, _sideCamera2Image );
							MIL.MdispSelect( _mainSideCamera3Display, _sideCamera3Image );

							//MIL.MdispSelect( InfraredCameraDisplay, InfraredCameraImage );
							//MIL.MdispSelect( SideCamera1Display, SideCamera1Image );
							//MIL.MdispSelect( SideCamera2Display, SideCamera2Image );
							//MIL.MdispSelect( SideCamera3Display, SideCamera3Image );


							Logger.WriteLine( "카메라 이미지 디스플레이 완료" );
						}
						catch (Exception ex)
						{
							Logger.WriteLine( $"이미지 디스플레이 중 오류 발생: {ex.Message}" );
							MessageBox.Show( $"이미지 디스플레이 중 오류 발생: {ex.Message}" );

						}
					}
					else
					{
						// 로드된 이미지가 있다면 그 이미지를 사용
					}


					try
					{
						if (_infraredCameraImage != MIL.M_NULL && _sideCamera1Image != MIL.M_NULL && _sideCamera2Image != MIL.M_NULL && _sideCamera3Image != MIL.M_NULL)
						{
							// 여기서 pixelData에 대한 추가 처리(예: HSLThreshold 등) 호출 가능
							// 예: Conversion.RunHSLThreshold(hMin, hMax, sMin, sMax, lMin, lMax, pixelData);
							// 처리 후 다시 DisplayImage(pixelData)로 화면에 갱신할 수 있음
							bool isGood = true;

							if (_infraredCameraConversionImage == MIL.M_NULL) MIL.MbufFree( _infraredCameraConversionImage );
							_infraredCameraConversionImage = MIL.M_NULL;
							if (_sideCamera1ConversionImage == MIL.M_NULL) MIL.MbufFree( _sideCamera1ConversionImage );
							_sideCamera1ConversionImage = MIL.M_NULL;
							if (_sideCamera2ConversionImage == MIL.M_NULL) MIL.MbufFree( _sideCamera2ConversionImage );
							_sideCamera2ConversionImage = MIL.M_NULL;
							if (_sideCamera3ConversionImage == MIL.M_NULL) MIL.MbufFree( _sideCamera3ConversionImage );
							_sideCamera3ConversionImage = MIL.M_NULL;



							//InfraredCameraConversionImage = Conversion.InfraredCameraModel( InfraredCameraImage, ref isGood, currentInfraredCameraModel );
							//await Task.Run( ( ) => Conversion.SideCameraModel( SideCamera1Image, MainSideCamera1Display ) );

							//var tasks = new[]
							//{
							//	///Overlay Display  Image 어떻게 처리할 것인지?
							//	///디스플레이 선언과 동시에 Overlay Image의 버퍼를 받아서 저장후에 
							//	///필요할때마다 가져다가 쓰는 방식으로 해야할 것 같음
							//	///
							//	Conversion.SideCameraModel( _sideCamera1Image, _mainSideCamera1Display ),
							//	Conversion.SideCameraModel( _sideCamera2Image, _mainSideCamera2Display ),
							//	Conversion.SideCameraModel( _sideCamera3Image, _mainSideCamera3Display )
							//};
							//bool[] result = await Task.WhenAll( tasks );

							//isGood = result[ 0 ] && result[ 1 ] && result[ 2 ];
							//MIL.MdispSelect( InfraredCameraConversionDisplay, InfraredCameraConversionImage );
							Logger.WriteLine( "이미지 처리 완료" );

							///GOOD REJECT LAMP 바인딩
							//if (!Dispatcher.CheckAccess())
							//{
							//	// UI 스레드에서 실행되도록 Dispatcher를 사용하여 호출
							//	Dispatcher.Invoke( ( ) => GoodLamp( isGood ) );
							//}
							//if (!isGood) EjectAction();

							//DisplayConversionImage( ConversionpixelData );
						}
					}
					catch (Exception ex)
					{
						Logger.WriteLine( $"이미지 처리 중 오류 발생: {ex.Message}" );
						MessageBox.Show( $"이미지 처리 중 오류 발생: {ex.Message}" );

					}

					finally
					{
						_isVisionCapturing = false;
					}

				}

			} );
			TectTime.Stop();
			Logger.WriteLine( $"이미지 처리 시간: {TectTime.ElapsedMilliseconds}ms" );


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

		private async void EjectAction( )
		{
			await Task.Run( async ( ) =>
			{
				await Task.Delay( 3000 );
				_advantechCard.WriteCoil = true;
				await Task.Delay( 500 );
				_advantechCard.WriteCoil = false;
			} );
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
