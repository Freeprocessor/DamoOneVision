using System.Windows;
using Matrox.MatroxImagingLibrary;
using System.Windows.Threading;

using SpinnakerNET;
using SpinnakerNET.GenApi;

//using LiteDB;
using DamoOneVision.Camera;
using DamoOneVision.Data;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

using DamoOneVision.ViewModels;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Drawing;
using System;
using DamoOneVision.Services;
using static OpenCvSharp.FileStorage;
using System.Net;
using System.Windows.Media.Converters;
using Newtonsoft.Json.Linq;
using DamoOneVision.ImageProcessing;
using DamoOneVision.Models;
using DamoOneVision.Views;


namespace DamoOneVision
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	///
	public partial class MainWindow : Window
	{
		//private ICamera camera;
		//private TemplateMatcher templateMatcher;
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


		private MIL_ID MilSystem = MIL.M_NULL;

		private MIL_ID _infraredCameraDisplay;
		private MIL_ID _infraredCameraConversionDisplay;

		private MIL_ID _sideCamera1Display;
		private MIL_ID _sideCamera1ConversionDisplay;

		private MIL_ID _sideCamera2Display;
		private MIL_ID _sideCamera2ConversionDisplay;

		private MIL_ID _sideCamera3Display;
		private MIL_ID _sideCamera3ConversionDisplay;

		//private MIL_ID _infraredCameraImage;
		//private MIL_ID _infraredCameraConversionImage;

		//private MIL_ID _sideCamera1Image;
		//private MIL_ID _sideCamera1ConversionImage;

		//private MIL_ID _sideCamera2Image;
		//private MIL_ID _sideCamera2ConversionImage;

		//private MIL_ID _sideCamera3Image;
		//private MIL_ID _sideCamera3ConversionImage;



		private MainViewModel _viewModel;




		public MainWindow( )
		{
			InitializeComponent();

			_modbus = new ModbusService( "192.168.2.100", 502 );

			_motionService = new MotionService();

			_advantechCard = new AdvantechCardService( "192.168.2.20", 502 );

			_deviceControlService = new DeviceControlService( _modbus, _advantechCard );

			_infraredCamera = new CameraManager( "Matrox", "InfraredCamera" );
			_sideCamera1 = new CameraManager( "Matrox", "SideCamera1" );
			_sideCamera2 = new CameraManager( "Matrox", "SideCamera2" );
			_sideCamera3 = new CameraManager( "Matrox", "SideCamera3" );

			InitMILSystem();

			_cameraService = new CameraService( _infraredCamera, _sideCamera1, _sideCamera2, _sideCamera3, 
				_infraredCameraDisplay, _sideCamera1Display, _sideCamera2Display, _sideCamera3Display);



			_viewModel = new MainViewModel( _deviceControlService, _cameraService );
			
			this.DataContext = _viewModel;


			//윈도우 로드, 클로즈 이벤트 핸들러 등록
			Loaded += MainWindow_Loaded;
			Closed += MainWindow_Closing;

			//advantechCard.Connect();
			//advantechCard.ReadBitAsync();

			//cameraManager.ImageCaptured += OnImageCaptured;

		}



		private void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
			// 모델 데이터 로드
			//LoadModelData();
			_viewModel?.StartClockAsync();

		}

		private void MainWindow_Closing( object? sender, EventArgs e )
		{
			_viewModel?.StopClock();
			//row new NotImplementedException();
		}


		private void InitMILSystem()
		{
			MilSystem = MILContext.Instance.MilSystem;

			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref _infraredCameraDisplay );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref _infraredCameraConversionDisplay );

			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref _sideCamera1Display );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref _sideCamera1ConversionDisplay );

			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref _sideCamera2Display );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref _sideCamera2ConversionDisplay );

			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref _sideCamera3Display );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref _sideCamera3ConversionDisplay );

			MIL.MdispControl( _infraredCameraDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( _infraredCameraDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( _infraredCameraDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( _infraredCameraConversionDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( _infraredCameraConversionDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( _infraredCameraConversionDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( _sideCamera1Display, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( _sideCamera1Display, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( _sideCamera1Display, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( _sideCamera1ConversionDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( _sideCamera1ConversionDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( _sideCamera1ConversionDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( _sideCamera2Display, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( _sideCamera2Display, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( _sideCamera2Display, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( _sideCamera2ConversionDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( _sideCamera2ConversionDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( _sideCamera2ConversionDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( _sideCamera3Display, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( _sideCamera3Display, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( _sideCamera3Display, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( _sideCamera3ConversionDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( _sideCamera3ConversionDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( _sideCamera3ConversionDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );


			/// 컬러맵 설정은 필요에 따라 변경 가능
			MIL.MdispLut( _infraredCameraDisplay, MIL.M_COLORMAP_JET );

			infraredCameraDisplay.DisplayId = _infraredCameraDisplay;
			infraredCameraConversionDisplay.DisplayId = _infraredCameraConversionDisplay;

			mainInfraredCameraDisplay.DisplayId = _infraredCameraDisplay;
			//mainInfraredCameraConversionDisplay.DisplayId = MainInfraredCameraConversionDisplay;

			sideCamera1Display.DisplayId = _sideCamera1Display;
			sideCamera1ConversionDisplay.DisplayId = _sideCamera1ConversionDisplay;

			mainSideCamera1Display.DisplayId = _sideCamera1Display;
			//mainSideCamera1ConversionDisplay.DisplayId = MainSideCamera1ConversionDisplay;

			sideCamera2Display.DisplayId = _sideCamera2Display;
			sideCamera2ConversionDisplay.DisplayId = _sideCamera2ConversionDisplay;

			mainSideCamera2Display.DisplayId = _sideCamera2Display;
			//mainSideCamera2ConversionDisplay.DisplayId = MainSideCamera2ConversionDisplay;

			sideCamera3Display.DisplayId = _sideCamera3Display;
			sideCamera3ConversionDisplay.DisplayId = _sideCamera3ConversionDisplay;

			mainSideCamera3Display.DisplayId = _sideCamera3Display;
			//mainSideCamera3ConversionDisplay.DisplayId = MainSideCamera3ConversionDisplay;

		}


		private void ManualButton_Click( object sender, RoutedEventArgs e )
		{

			//ManualWindow manualWindow = new ManualWindow( _modbus );
			//manualWindow.ShowDialog();

			ManualWindow manualWindow = new ManualWindow( _deviceControlService, _motionService );
			manualWindow.ShowDialog();

		}

		private void Click( object sender, RoutedEventArgs e )
		{
			MessageBox.Show( "버튼이 클릭되었습니다." );
			Logger.WriteLine( "버튼이 클릭되었습니다." );
		}

		private void GoodLamp( bool isGood )
		{
			if (!isGood)
			{
				Logger.WriteLine( "Reject" );

				GoodRejectLamp.Background = System.Windows.Media.Brushes.Red;
				GoodRejectText.Content = "Reject";
			}
			else
			{
				Logger.WriteLine( "Good" );
				GoodRejectLamp.Background = System.Windows.Media.Brushes.Green;
				GoodRejectText.Content = "Good";
			}
		}




		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e );

			// MILContext 해제
			MILContext.Instance.Dispose();
		}


		private async void ExitProgram( object sender, EventArgs e )
		{

			_cameraService.Dispose();

			///장비가 Stop 상태가 아니면 프로그램이 종료되지 않게 해야함

			var tasks = new[]
				{
					_infraredCamera.DisconnectAsync(),
					_sideCamera1.DisconnectAsync(),
					_sideCamera2.DisconnectAsync(),
					_sideCamera3.DisconnectAsync()
				};

			await Task.WhenAll( tasks );

			// 1. UI 요소의 DisplayId를 MIL.M_NULL로 설정하여 참조 해제
			if (infraredCameraDisplay != null)
			{
				infraredCameraDisplay.DisplayId = MIL.M_NULL;
			}

			if (infraredCameraConversionDisplay != null)
			{
				infraredCameraConversionDisplay.DisplayId = MIL.M_NULL;
			}

			if (mainInfraredCameraDisplay != null)
			{
				mainInfraredCameraDisplay.DisplayId = MIL.M_NULL;
			}

			//if (mainInfraredCameraConversionDisplay != null)
			//{
			//	mainInfraredCameraConversionDisplay.DisplayId = MIL.M_NULL;
			//}

			if (sideCamera1Display != null)
			{
				sideCamera1Display.DisplayId = MIL.M_NULL;
			}

			if (sideCamera1ConversionDisplay != null)
			{
				sideCamera1ConversionDisplay.DisplayId = MIL.M_NULL;
			}

			if (mainSideCamera1Display != null)
			{
				mainSideCamera1Display.DisplayId = MIL.M_NULL;
			}

			//if (mainSideCamera1ConversionDisplay != null)
			//{
			//	mainSideCamera1ConversionDisplay.DisplayId = MIL.M_NULL;
			//}

			if (sideCamera2Display != null)
			{
				sideCamera2Display.DisplayId = MIL.M_NULL;
			}

			if (sideCamera2ConversionDisplay != null)
			{
				sideCamera2ConversionDisplay.DisplayId = MIL.M_NULL;
			}

			if (mainSideCamera2Display != null)
			{
				mainSideCamera2Display.DisplayId = MIL.M_NULL;
			}

			//if (mainSideCamera2ConversionDisplay != null)
			//{
			//	mainSideCamera2ConversionDisplay.DisplayId = MIL.M_NULL;
			//}

			if (sideCamera3Display != null)
			{
				sideCamera3Display.DisplayId = MIL.M_NULL;
			}

			if (sideCamera3ConversionDisplay != null)
			{
				sideCamera3ConversionDisplay.DisplayId = MIL.M_NULL;
			}

			if (mainSideCamera3Display != null)
			{
				mainSideCamera3Display.DisplayId = MIL.M_NULL;
			}

			//if (mainSideCamera3ConversionDisplay != null)
			//{
			//	mainSideCamera3ConversionDisplay.DisplayId = MIL.M_NULL;
			//}

			// 2. disp 버퍼 해제
			if (_infraredCameraDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( _infraredCameraDisplay );
				_infraredCameraDisplay = MIL.M_NULL;
				Logger.WriteLine( "_infraredCameraDisplay 해제 완료." );
			}

			if (_infraredCameraConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( _infraredCameraConversionDisplay );
				_infraredCameraConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "_infraredCameraConversionDisplay 해제 완료." );
			}

			if (_sideCamera1Display != MIL.M_NULL)
			{
				MIL.MdispFree( _sideCamera1Display );
				_sideCamera1Display = MIL.M_NULL;
				Logger.WriteLine( "_sideCamera1Display 해제 완료." );
			}

			if (_sideCamera1ConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( _sideCamera1ConversionDisplay );
				_sideCamera1ConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "_sideCamera1ConversionDisplay 해제 완료." );
			}

			if (_sideCamera2Display != MIL.M_NULL)
			{
				MIL.MdispFree( _sideCamera2Display );
				_sideCamera2Display = MIL.M_NULL;
				Logger.WriteLine( "_sideCamera2Display 해제 완료." );
			}

			if (_sideCamera2ConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( _sideCamera2ConversionDisplay );
				_sideCamera2ConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "_sideCamera2ConversionDisplay 해제 완료." );
			}

			if (_sideCamera3Display != MIL.M_NULL)
			{
				MIL.MdispFree( _sideCamera3Display );
				_sideCamera3Display = MIL.M_NULL;
				Logger.WriteLine( "_sideCamera3Display 해제 완료." );
			}

			if (_sideCamera3ConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( _sideCamera3ConversionDisplay );
				_sideCamera3ConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "_sideCamera3ConversionDisplay 해제 완료." );
			}

			/// 어떻게 이미지를 해제할 것인지?
			/// 
			//3.이미지 버퍼 해제
			//if (_infraredCameraImage != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _infraredCameraImage );
			//	_infraredCameraImage = MIL.M_NULL;
			//	Logger.WriteLine( "_infraredCameraImage 해제 완료." );
			//}

			//if (_infraredCameraConversionImage != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _infraredCameraConversionImage );
			//	_infraredCameraConversionImage = MIL.M_NULL;
			//	Logger.WriteLine( "_infraredCameraConversionImage 해제 완료." );
			//}

			//if (_sideCamera1Image != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _sideCamera1Image );
			//	_sideCamera1Image = MIL.M_NULL;
			//	Logger.WriteLine( "_sideCamera1Image 해제 완료." );
			//}

			//if (_sideCamera1ConversionImage != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _sideCamera1ConversionImage );
			//	_sideCamera1ConversionImage = MIL.M_NULL;
			//	Logger.WriteLine( "_sideCamera1ConversionImage 해제 완료." );
			//}

			//if (_sideCamera2Image != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _sideCamera2Image );
			//	_sideCamera2Image = MIL.M_NULL;
			//	Logger.WriteLine( "_sideCamera2Image 해제 완료." );
			//}

			//if (_sideCamera2ConversionImage != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _sideCamera2ConversionImage );
			//	_sideCamera2ConversionImage = MIL.M_NULL;
			//	Logger.WriteLine( "_sideCamera2ConversionImage 해제 완료." );
			//}

			//if (_sideCamera3Image != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _sideCamera3Image );
			//	_sideCamera3Image = MIL.M_NULL;
			//	Logger.WriteLine( "_sideCamera3Image 해제 완료." );
			//}

			//if (_sideCamera3ConversionImage != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _sideCamera3ConversionImage );
			//	_sideCamera3ConversionImage = MIL.M_NULL;
			//	Logger.WriteLine( "_sideCamera3ConversionImage 해제 완료." );
			//}

			Logger.Shutdown();

			Application.Current.Shutdown();
		}

		private void DataReadButton_Click( object sender, RoutedEventArgs e )
		{
			//ushort[] data = modbus.ReadHoldingRegisters( 0, 0x00, 20 );
			//foreach (var item in data)
			//{
			//	Data.Log.WriteLine( $"{item}" );
			//}
			_modbus.WriteHoldingRegisters32( 0, 15, 1000000 );

			Logger.WriteLine($"{_modbus.ReadHoldingRegisters32( 0, 15, 1 )[0]}" );

			//modbus.ReadInputRegisters( 0, 0x00, 10 );

		}





		//private void Show3DButton_Click( object sender, RoutedEventArgs e )
		//{
		//	// 별도의 윈도우를 띄워서 3D 표시
		//	_3DView _3dview = new _3DView( (byte[])RawPixelData.Clone());
		//	_3dview.Show();
		//}




	}
}
