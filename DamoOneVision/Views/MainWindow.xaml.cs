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

		/// <summary>
		/// Advantech 카드 서비스
		/// </summary>
		private AdvantechCard _advantechCard;


		private MIL_ID MilSystem = MIL.M_NULL;

		private MIL_ID InfraredCameraDisplay;
		private MIL_ID InfraredCameraConversionDisplay;
		private MIL_ID MainInfraredCameraDisplay;
		private MIL_ID MainInfraredCameraConversionDisplay;

		private MIL_ID SideCamera1Display;
		private MIL_ID SideCamera1ConversionDisplay;
		private MIL_ID MainSideCamera1Display;
		private MIL_ID MainSideCamera1ConversionDisplay;

		private MIL_ID SideCamera2Display;
		private MIL_ID SideCamera2ConversionDisplay;
		private MIL_ID MainSideCamera2Display;
		private MIL_ID MainSideCamera2ConversionDisplay;

		private MIL_ID SideCamera3Display;
		private MIL_ID SideCamera3ConversionDisplay;
		private MIL_ID MainSideCamera3Display;
		private MIL_ID MainSideCamera3ConversionDisplay;

		private MIL_ID _infraredCameraImage;
		private MIL_ID _infraredCameraConversionImage;

		private MIL_ID _sideCamera1Image;
		private MIL_ID _sideCamera1ConversionImage;

		private MIL_ID _sideCamera2Image;
		private MIL_ID _sideCamera2ConversionImage;

		private MIL_ID _sideCamera3Image;
		private MIL_ID _sideCamera3ConversionImage;



		private MainViewModel _viewModel;




		public MainWindow( )
		{
			InitializeComponent();

			_modbus = new ModbusService( "192.168.2.11", 502 );

			_advantechCard = new AdvantechCard( "192.168.2.20", 502 );

			_infraredCamera = new CameraManager( "Matrox", "InfraredCamera" );
			_sideCamera1 = new CameraManager( "Matrox", "SideCamera1" );
			_sideCamera2 = new CameraManager( "Matrox", "SideCamera2" );
			_sideCamera3 = new CameraManager( "Matrox", "SideCamera3" );

			InitMILSystem();

			_viewModel = new MainViewModel( _modbus, _advantechCard, _infraredCamera, _sideCamera1, _sideCamera2 ,_sideCamera3, 
				_infraredCameraImage, _infraredCameraConversionImage, _sideCamera1Image, _sideCamera1ConversionImage, _sideCamera2Image, _sideCamera2ConversionImage, _sideCamera3Image, _sideCamera3ConversionImage);
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

			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref InfraredCameraDisplay );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref InfraredCameraConversionDisplay );

			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref MainInfraredCameraDisplay );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref MainInfraredCameraConversionDisplay );

			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref SideCamera1Display );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref SideCamera1ConversionDisplay );

			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref MainSideCamera1Display );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref MainSideCamera1ConversionDisplay );

			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref SideCamera2Display );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref SideCamera2ConversionDisplay );

			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref MainSideCamera2Display );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref MainSideCamera2ConversionDisplay );

			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref SideCamera3Display );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref SideCamera3ConversionDisplay );

			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref MainSideCamera3Display );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref MainSideCamera3ConversionDisplay );

			MIL.MdispControl( InfraredCameraDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( InfraredCameraDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( InfraredCameraDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( InfraredCameraConversionDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( InfraredCameraConversionDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( InfraredCameraConversionDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( MainInfraredCameraDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( MainInfraredCameraDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( MainInfraredCameraDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( MainInfraredCameraConversionDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( MainInfraredCameraConversionDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( MainInfraredCameraConversionDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( SideCamera1Display, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( SideCamera1Display, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( SideCamera1Display, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( SideCamera1ConversionDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( SideCamera1ConversionDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( SideCamera1ConversionDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( MainSideCamera1Display, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( MainSideCamera1Display, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( MainSideCamera1Display, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( MainSideCamera1ConversionDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( MainSideCamera1ConversionDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( MainSideCamera1ConversionDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( SideCamera2Display, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( SideCamera2Display, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( SideCamera2Display, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( SideCamera2ConversionDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( SideCamera2ConversionDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( SideCamera2ConversionDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( MainSideCamera2Display, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( MainSideCamera2Display, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( MainSideCamera2Display, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( MainSideCamera2ConversionDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( MainSideCamera2ConversionDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( MainSideCamera2ConversionDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( SideCamera3Display, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( SideCamera3Display, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( SideCamera3Display, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( SideCamera3ConversionDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( SideCamera3ConversionDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( SideCamera3ConversionDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( MainSideCamera3Display, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( MainSideCamera3Display, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( MainSideCamera3Display, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );

			MIL.MdispControl( MainSideCamera3ConversionDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( MainSideCamera3ConversionDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( MainSideCamera3ConversionDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( MainSideCamera3ConversionDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );


			/// 컬러맵 설정은 필요에 따라 변경 가능
			MIL.MdispLut( InfraredCameraDisplay, MIL.M_COLORMAP_JET );
			MIL.MdispLut( MainInfraredCameraDisplay, MIL.M_COLORMAP_JET );


			infraredCameraDisplay.DisplayId = InfraredCameraDisplay;
			infraredCameraConversionDisplay.DisplayId = InfraredCameraConversionDisplay;

			mainInfraredCameraDisplay.DisplayId = MainInfraredCameraDisplay;
			//mainInfraredCameraConversionDisplay.DisplayId = MainInfraredCameraConversionDisplay;

			sideCamera1Display.DisplayId = SideCamera1Display;
			sideCamera1ConversionDisplay.DisplayId = SideCamera1ConversionDisplay;

			mainSideCamera1Display.DisplayId = MainSideCamera1Display;
			//mainSideCamera1ConversionDisplay.DisplayId = MainSideCamera1ConversionDisplay;

			sideCamera2Display.DisplayId = SideCamera2Display;
			sideCamera2ConversionDisplay.DisplayId = SideCamera2ConversionDisplay;

			mainSideCamera2Display.DisplayId = MainSideCamera2Display;
			//mainSideCamera2ConversionDisplay.DisplayId = MainSideCamera2ConversionDisplay;

			sideCamera3Display.DisplayId = SideCamera3Display;
			sideCamera3ConversionDisplay.DisplayId = SideCamera3ConversionDisplay;

			mainSideCamera3Display.DisplayId = MainSideCamera3Display;
			//mainSideCamera3ConversionDisplay.DisplayId = MainSideCamera3ConversionDisplay;

		}


		//private async void ConnectButton_Click( object sender, RoutedEventArgs e )
		//{
		//	await ConnectAction();
		//}




		private void ManualButton_Click( object sender, RoutedEventArgs e )
		{

			ManualWindow manualWindow = new ManualWindow( _modbus );
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

			///장비가 Stop 상태가 아니면 프로그램이 종료되지 않게 해야함
			//await StopAction();


			if (_infraredCamera.IsConnected) _infraredCameraImage = _infraredCamera.ReciveImage();
			if (_sideCamera1.IsConnected) _sideCamera1Image = _sideCamera1.ReciveImage();
			if (_sideCamera2.IsConnected) _sideCamera2Image = _sideCamera2.ReciveImage();
			if (_sideCamera3.IsConnected) _sideCamera3Image = _sideCamera3.ReciveImage();

			var tasks = new[]
				{
					_infraredCamera.DisconnectAsync(),
					_sideCamera1.DisconnectAsync(),
					_sideCamera2.DisconnectAsync(),
					_sideCamera3.DisconnectAsync()
				};

			await Task.WhenAll( tasks );

			//InfraredCamera.DisconnectAsync();
			//SideCamera1.DisconnectAsync();
			//SideCamera2.DisconnectAsync();
			//SideCamera3.DisconnectAsync();

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
			if (InfraredCameraDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( InfraredCameraDisplay );
				InfraredCameraDisplay = MIL.M_NULL;
				Logger.WriteLine( "InfraredCameraDisplay 해제 완료." );
			}

			if (InfraredCameraConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( InfraredCameraConversionDisplay );
				InfraredCameraConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "InfraredCameraConversionDisplay 해제 완료." );
			}

			if (MainInfraredCameraDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( MainInfraredCameraDisplay );
				MainInfraredCameraDisplay = MIL.M_NULL;
				Logger.WriteLine( "MainInfraredCameraDisplay 해제 완료." );
			}

			if (MainInfraredCameraConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( MainInfraredCameraConversionDisplay );
				MainInfraredCameraConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "MainInfraredCameraConversionDisplay 해제 완료." );
			}

			if (SideCamera1Display != MIL.M_NULL)
			{
				MIL.MdispFree( SideCamera1Display );
				SideCamera1Display = MIL.M_NULL;
				Logger.WriteLine( "SideCamera1Display 해제 완료." );
			}

			if (SideCamera1ConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( SideCamera1ConversionDisplay );
				SideCamera1ConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "SideCamera1ConversionDisplay 해제 완료." );
			}

			if (MainSideCamera1Display != MIL.M_NULL)
			{
				MIL.MdispFree( MainSideCamera1Display );
				MainSideCamera1Display = MIL.M_NULL;
				Logger.WriteLine( "MainSideCamera1Display 해제 완료." );
			}

			if (MainSideCamera1ConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( MainSideCamera1ConversionDisplay );
				MainSideCamera1ConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "MainSideCamera1ConversionDisplay 해제 완료." );
			}

			if (SideCamera2Display != MIL.M_NULL)
			{
				MIL.MdispFree( SideCamera2Display );
				SideCamera2Display = MIL.M_NULL;
				Logger.WriteLine( "SideCamera2Display 해제 완료." );
			}

			if (SideCamera2ConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( SideCamera2ConversionDisplay );
				SideCamera2ConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "SideCamera2ConversionDisplay 해제 완료." );
			}

			if (MainSideCamera2Display != MIL.M_NULL)
			{
				MIL.MdispFree( MainSideCamera2Display );
				MainSideCamera2Display = MIL.M_NULL;
				Logger.WriteLine( "MainSideCamera2Display 해제 완료." );
			}

			if (MainSideCamera2ConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( MainSideCamera2ConversionDisplay );
				MainSideCamera2ConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "MainSideCamera2ConversionDisplay 해제 완료." );
			}

			if (SideCamera3Display != MIL.M_NULL)
			{
				MIL.MdispFree( SideCamera3Display );
				SideCamera3Display = MIL.M_NULL;
				Logger.WriteLine( "SideCamera3Display 해제 완료." );
			}

			if (SideCamera3ConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( SideCamera3ConversionDisplay );
				SideCamera3ConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "SideCamera3ConversionDisplay 해제 완료." );
			}

			if (MainSideCamera3Display != MIL.M_NULL)
			{
				MIL.MdispFree( MainSideCamera3Display );
				MainSideCamera3Display = MIL.M_NULL;
				Logger.WriteLine( "MainSideCamera3Display 해제 완료." );
			}

			if (MainSideCamera3ConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( MainSideCamera3ConversionDisplay );
				MainSideCamera3ConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "MainSideCamera3ConversionDisplay 해제 완료." );
			}

			/// 어떻게 이미지를 해제할 것인지?
			/// 
			//3.이미지 버퍼 해제
			if (_infraredCameraImage != MIL.M_NULL)
			{
				MIL.MbufFree( _infraredCameraImage );
				_infraredCameraImage = MIL.M_NULL;
				Logger.WriteLine( "InfraredCameraImage 해제 완료." );
			}

			if (_infraredCameraConversionImage != MIL.M_NULL)
			{
				MIL.MbufFree( _infraredCameraConversionImage );
				_infraredCameraConversionImage = MIL.M_NULL;
				Logger.WriteLine( "InfraredCameraConversionImage 해제 완료." );
			}

			if (_sideCamera1Image != MIL.M_NULL)
			{
				MIL.MbufFree( _sideCamera1Image );
				_sideCamera1Image = MIL.M_NULL;
				Logger.WriteLine( "SideCamera1Image 해제 완료." );
			}

			if (_sideCamera1ConversionImage != MIL.M_NULL)
			{
				MIL.MbufFree( _sideCamera1ConversionImage );
				_sideCamera1ConversionImage = MIL.M_NULL;
				Logger.WriteLine( "SideCamera1ConversionImage 해제 완료." );
			}

			if (_sideCamera2Image != MIL.M_NULL)
			{
				MIL.MbufFree( _sideCamera2Image );
				_sideCamera2Image = MIL.M_NULL;
				Logger.WriteLine( "SideCamera2Image 해제 완료." );
			}

			if (_sideCamera2ConversionImage != MIL.M_NULL)
			{
				MIL.MbufFree( _sideCamera2ConversionImage );
				_sideCamera2ConversionImage = MIL.M_NULL;
				Logger.WriteLine( "SideCamera2ConversionImage 해제 완료." );
			}

			if (_sideCamera3Image != MIL.M_NULL)
			{
				MIL.MbufFree( _sideCamera3Image );
				_sideCamera3Image = MIL.M_NULL;
				Logger.WriteLine( "SideCamera3Image 해제 완료." );
			}

			if (_sideCamera3ConversionImage != MIL.M_NULL)
			{
				MIL.MbufFree( _sideCamera3ConversionImage );
				_sideCamera3ConversionImage = MIL.M_NULL;
				Logger.WriteLine( "SideCamera3ConversionImage 해제 완료." );
				//}

				Logger.Shutdown();
			}

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
