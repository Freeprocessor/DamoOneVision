using DamoOneVision.Camera;
using DamoOneVision.Services;
using DamoOneVision.ViewModels;
using DamoOneVision.Views;
using Matrox.MatroxImagingLibrary;
using System.Configuration;
using System.Data;
using System.Windows;

namespace DamoOneVision
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		/// <summary>
		/// 열화상 Camera
		/// </summary>
		private CameraManager _infraredCamera;
		/// <summary>
		/// 측면 1 Camera
		/// </summary>
		private CameraManager _sideCamera1;
		/// <summary>
		/// 측면 2 Camera
		/// </summary>
		private CameraManager _sideCamera2;
		/// <summary>
		/// 측면	3 Camera	
		/// </summary>
		private CameraManager _sideCamera3;

		/// <summary>
		/// Modbus 서비스
		/// </summary>
		private ModbusService _modbus;

		/// <summary>
		/// Motion 서비스
		/// </summary>
		private MotionService _motionService;

		/// <summary>
		/// Advantech 카드 서비스
		/// </summary>
		private AdvantechCardService _advantechCard;

		/// <summary>
		/// Device Control Service
		/// </summary>
		private DeviceControlService _deviceControlService;

		private CameraService _cameraService;


		private MilSystemService _milSystemService;

		private MainViewModel _mainViewModel;

		private ManualViewModel _manualViewModel;

		private MainWindow _mainWindow;

		private MainUserControl _mainUserControl;

		private ManualUserControl _manualUserControl;



		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );

			_infraredCamera = new CameraManager( "Matrox", "InfraredCamera" );
			_sideCamera1 = new CameraManager( "Matrox", "SideCamera1" );
			_sideCamera2 = new CameraManager( "Matrox", "SideCamera2" );
			_sideCamera3 = new CameraManager( "Matrox", "SideCamera3" );

			_modbus = new ModbusService( "192.168.2.100", 502 );

			_motionService = new MotionService();

			_advantechCard = new AdvantechCardService( "192.168.2.20", 502 );

			

			_milSystemService = new MilSystemService();

			_cameraService = new CameraService( _infraredCamera, _sideCamera1, _sideCamera2, _sideCamera3,
				_milSystemService.InfraredDisplay, _milSystemService.SideCam1Display, _milSystemService.SideCam2Display, _milSystemService.SideCam3Display );
			// MainViewModel 초기화

			_deviceControlService = new DeviceControlService( _modbus, _advantechCard, _motionService);


			/// ViewModel 생성
			_mainViewModel = new MainViewModel( _deviceControlService, _cameraService);
			_manualViewModel = new ManualViewModel( _deviceControlService, _motionService, _cameraService );
			//// MainUserControl 초기화
			//_mainUserControl = new MainUserControl( _mainViewModel );
			//// ManualUserControl 초기화
			//_manualUserControl = new ManualUserControl( _manualViewModel );

			// MainUserControl을 MainWindow에 등록
			_mainWindow = new MainWindow( _mainViewModel, _manualViewModel, _milSystemService );
			//MainWindow.Content = _mainUserControl;
			MainWindow.Show();
		}

		protected override async void OnExit( ExitEventArgs e )
		{
			base.OnExit( e );
			_cameraService?.Dispose();

			_motionService.ReleaseLibrary();

			var tasks = new[]
				{
					_infraredCamera.DisconnectAsync(),
					_sideCamera1.DisconnectAsync(),
					_sideCamera2.DisconnectAsync(),
					_sideCamera3.DisconnectAsync()
				};

			await Task.WhenAll( tasks );

			await _milSystemService.DisposeAsync();
			MILContext.Instance.Dispose();
			await Task.Delay( 100 );
			Logger.Shutdown();

			Application.Current.Shutdown();

		}
	}
}