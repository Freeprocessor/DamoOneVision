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
		private CameraManager InfraredCamera;
		private CameraManager SideCamera1;
		private CameraManager SideCamera2;
		private CameraManager SideCamera3;

		public ObservableCollection<string> ImagePaths { get; set; }

		private string appFolder = "";
		private string imageFolder = "";
		private string modelfolder = "";
		private string modelfile = "";

		ModbusService modbus = new ModbusService( "192.168.2.11", 502 );

		//private bool isTriggered = false;
		private bool triggerReadingOFFRequire = false;
		private bool triggerReadingStatus = false;

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

		private MIL_ID InfraredCameraImage;
		private MIL_ID InfraredCameraConversionImage;

		private MIL_ID SideCamera1Image;
		private MIL_ID SideCamera1ConversionImage;

		private MIL_ID SideCamera2Image;
		private MIL_ID SideCamera2ConversionImage;

		private MIL_ID SideCamera3Image;
		private MIL_ID SideCamera3ConversionImage;

		private bool isContinuous = false; // Continuous 모드 상태
		private bool isCapturing = false;  // 이미지 캡처 중인지 여부
		private bool isConnected = false;

		private readonly JsonHandler _jsonHandler;
		public ObservableCollection<InfraredCameraModel> InfraredCameraModels { get; set; }

		private InfraredCameraModel currentInfraredCameraModel;

		// Setting
		SettingManager settingManager;

		AdvantechCard advantechCard = new AdvantechCard();

		public MainWindow( )
		{
			InitializeComponent();
			InitLocalAppFolder();
			InitMILSystem();
			StartClockAsync();
			//DATA BINDING
			this.DataContext = this;

			settingManager = new SettingManager();
			_jsonHandler = new JsonHandler( modelfile );
			InfraredCameraModels = new ObservableCollection<InfraredCameraModel>();
			LoadInfraredModelsAsync();

			// 윈도우 종료 이벤트 핸들러 추가
			this.Closing += Window_Closing;

			InfraredCamera = new CameraManager( "Matrox", "InfraredCamera" );
			SideCamera1 = new CameraManager( "Matrox", "SideCamera1" );
			SideCamera2 = new CameraManager( "Matrox", "SideCamera2" );
			SideCamera3 = new CameraManager( "Matrox", "SideCamera3" );

			advantechCard.Connect();
			advantechCard.ReadBitAsync();

			//cameraManager.ImageCaptured += OnImageCaptured;

		}


		// 모델 데이터를 JSON 파일로 저장하는 메서드eh
		private async void SaveInfraredModelsAsync( )
		{
			var data = new InfraredCameraModelData { InfraredCameraModels = new List<InfraredCameraModel>(InfraredCameraModels) };
			await _jsonHandler.SaveInfraredModelsAsync( data );
		}



		// JSON 파일에서 모델 데이터를 불러오는 메서드
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


		private async void ConnectButton_Click( object sender, RoutedEventArgs e )
		{
			await ConnectAction();
		}

		private async Task ConnectAction( )
		{
			if (isConnected)
			{
				Logger.WriteLine( "이미 카메라가 연결되어 있습니다." );
				return;
			}
			ConnectButton.IsEnabled = false;
			DisconnectButton.IsEnabled = false;
			try
			{
				var tasks = new[]
				{
					InfraredCamera.ConnectAsync( ),
					SideCamera1.ConnectAsync( ),
					SideCamera2.ConnectAsync( ),
					SideCamera3.ConnectAsync( )
				};

				await Task.WhenAll( tasks );

				ConnectButton.IsEnabled = false;
				DisconnectButton.IsEnabled = true;
				isConnected = true;

			}
			catch (Exception ex)
			{
				MessageBox.Show( $"카메라 연결 오류\n{ex.Message}" );
				Logger.WriteLine( $"카메라 연결 오류\n{ex.Message}" );
				var tasks = new[]
				{
					InfraredCamera.DisconnectAsync(),
					SideCamera1.DisconnectAsync(),
					SideCamera2.DisconnectAsync(),
					SideCamera3.DisconnectAsync()
				};

				await Task.WhenAll( tasks );

			}
		}

		private async void DisconnectButton_Click( object sender, RoutedEventArgs e )
		{
			if (!isConnected)
			{
				Logger.WriteLine( "카메라가 연결되어 있지 않습니다." );
				return;
			}
			ConnectButton.IsEnabled = false;
			DisconnectButton.IsEnabled = false;

			InfraredCameraImage = InfraredCamera.ReciveImage();
			SideCamera1Image = SideCamera1.ReciveImage();
			SideCamera2Image = SideCamera2.ReciveImage();
			SideCamera3Image = SideCamera3.ReciveImage();

			var tasks = new[]
				{
					InfraredCamera.DisconnectAsync(),
					SideCamera1.DisconnectAsync(),
					SideCamera2.DisconnectAsync(),
					SideCamera3.DisconnectAsync()
				};

			await Task.WhenAll( tasks );

			if (InfraredCameraImage != MIL.M_NULL)
			{
				MIL.MbufFree( InfraredCameraImage );
				InfraredCameraImage = MIL.M_NULL;
				Logger.WriteLine( "InfraredCameraImage 해제 완료." );
			}

			if (InfraredCameraConversionImage != MIL.M_NULL)
			{
				MIL.MbufFree( InfraredCameraConversionImage );
				InfraredCameraConversionImage = MIL.M_NULL;
				Logger.WriteLine( "InfraredCameraConversionImage 해제 완료." );
			}

			if (SideCamera1Image != MIL.M_NULL)
			{
				MIL.MbufFree( SideCamera1Image );
				SideCamera1Image = MIL.M_NULL;
				Logger.WriteLine( "SideCamera1Image 해제 완료." );
			}

			if (SideCamera1ConversionImage != MIL.M_NULL)
			{
				MIL.MbufFree( SideCamera1ConversionImage );
				SideCamera1ConversionImage = MIL.M_NULL;
				Logger.WriteLine( "SideCamera1ConversionImage 해제 완료." );
			}

			if (SideCamera2Image != MIL.M_NULL)
			{
				MIL.MbufFree( SideCamera2Image );
				SideCamera2Image = MIL.M_NULL;
				Logger.WriteLine( "SideCamera2Image 해제 완료." );
			}

			if (SideCamera2ConversionImage != MIL.M_NULL)
			{
				MIL.MbufFree( SideCamera2ConversionImage );
				SideCamera2ConversionImage = MIL.M_NULL;
				Logger.WriteLine( "SideCamera2ConversionImage 해제 완료." );
			}

			if (SideCamera3Image != MIL.M_NULL)
			{
				MIL.MbufFree( SideCamera3Image );
				SideCamera3Image = MIL.M_NULL;
				Logger.WriteLine( "SideCamera3Image 해제 완료." );
			}

			if (SideCamera3ConversionImage != MIL.M_NULL)
			{
				MIL.MbufFree( SideCamera3ConversionImage );
				SideCamera3ConversionImage = MIL.M_NULL;
				Logger.WriteLine( "SideCamera3ConversionImage 해제 완료." );
			}

			ConnectButton.IsEnabled = true;
			DisconnectButton.IsEnabled = false;
			isConnected = false;
		}

		// 모델 수정 버튼 클릭 이벤트 핸들러
		private void EditModelButton_Click( object sender, RoutedEventArgs e )
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

		private async void TriggerButton_Click( object sender, RoutedEventArgs e )
		{
			await VisionTrigger();
		}

		private async void TriggerReadingAsync( )
		{
			
			triggerReadingStatus = true;
			triggerReadingOFFRequire = false;
			await Task.Run( async ( ) =>
			{
				//modbus.WriteSingleCoil( 0, 0x2A, true );
				Logger.WriteLine( "TriggerReadingAsync Start" );

				while (!triggerReadingOFFRequire)
				{
					/// Trigger-1 ON
					if (advantechCard.ReadCoil[0] == true)
					{
						await Task.Delay( 1450 );
						await VisionTrigger();
						//modbus.WriteSingleCoil( 0, 0x06, false );
						//while (modbus.ReadInputs( 0, 0x06, 1 )[ 0 ]) ;
					}

				}
				//modbus.WriteSingleCoil( 0, 0x2A, false );
				Logger.WriteLine( "TriggerReadingAsync Stop" );

				triggerReadingStatus = false;
			} );
		}

		

		public async Task VisionTrigger()
		{
			Stopwatch TectTime = new Stopwatch();
			TectTime.Start();

			if ((!InfraredCamera.IsConnected && !SideCamera1.IsConnected && !SideCamera2.IsConnected && !SideCamera3.IsConnected) &&
				(InfraredCameraImage == MIL.M_NULL && SideCamera1Image == MIL.M_NULL && SideCamera2Image == MIL.M_NULL && SideCamera3Image == MIL.M_NULL))
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
				if (!isCapturing)
				{
					isCapturing = true;


					if (InfraredCamera.IsConnected && SideCamera1.IsConnected && SideCamera2.IsConnected && SideCamera3.IsConnected)
					{
						try
						{
							// 카메라에서 이미지 캡처
							var tasks = new[]
							{
								InfraredCamera.CaptureSingleImageAsync(),
								SideCamera1.CaptureSingleImageAsync(),
								SideCamera2.CaptureSingleImageAsync(),
								SideCamera3.CaptureSingleImageAsync()
							};
							await Task.WhenAll( tasks );

							Logger.WriteLine( "카메라 이미지 캡처 완료" );

							InfraredCameraImage = InfraredCamera.ReciveImage();
							SideCamera1Image = SideCamera1.ReciveImage();
							SideCamera2Image = SideCamera2.ReciveImage();
							SideCamera3Image = SideCamera3.ReciveImage();

							Logger.WriteLine( "카메라 이미지 수신 완료" );
						}
						catch (Exception ex)
						{
							Logger.WriteLine( $"이미지 캡쳐 중 오류 발생: {ex.Message}" );
							MessageBox.Show( $"이미지 캡쳐 중 오류 발생: {ex.Message}" );

						}


						try
						{
							
							MIL.MdispSelect( InfraredCameraDisplay, InfraredCameraImage );
							MIL.MdispSelect( MainInfraredCameraDisplay, InfraredCameraImage );
							MIL.MdispSelect( SideCamera1Display, SideCamera1Image );
							MIL.MdispSelect( MainSideCamera1Display, SideCamera1Image );
							MIL.MdispSelect( SideCamera2Display, SideCamera2Image );
							MIL.MdispSelect( MainSideCamera2Display, SideCamera2Image );
							MIL.MdispSelect( SideCamera3Display, SideCamera3Image );
							MIL.MdispSelect( MainSideCamera3Display, SideCamera3Image );

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
						if (InfraredCameraImage != MIL.M_NULL && SideCamera1Image != MIL.M_NULL && SideCamera2Image != MIL.M_NULL && SideCamera3Image != MIL.M_NULL)
						{
							// 여기서 pixelData에 대한 추가 처리(예: HSLThreshold 등) 호출 가능
							// 예: Conversion.RunHSLThreshold(hMin, hMax, sMin, sMax, lMin, lMax, pixelData);
							// 처리 후 다시 DisplayImage(pixelData)로 화면에 갱신할 수 있음
							bool isGood = true;

							if (InfraredCameraConversionImage == MIL.M_NULL) MIL.MbufFree( InfraredCameraConversionImage );
							InfraredCameraConversionImage = MIL.M_NULL;
							if (SideCamera1ConversionImage == MIL.M_NULL) MIL.MbufFree( SideCamera1ConversionImage );
							SideCamera1ConversionImage = MIL.M_NULL;
							if (SideCamera2ConversionImage == MIL.M_NULL) MIL.MbufFree( SideCamera2ConversionImage );
							SideCamera2ConversionImage = MIL.M_NULL;
							if (SideCamera3ConversionImage == MIL.M_NULL) MIL.MbufFree( SideCamera3ConversionImage );
							SideCamera3ConversionImage = MIL.M_NULL;



							//InfraredCameraConversionImage = Conversion.InfraredCameraModel( InfraredCameraImage, ref isGood, currentInfraredCameraModel );
							//await Task.Run( ( ) => Conversion.SideCameraModel( SideCamera1Image, MainSideCamera1Display ) );

							var tasks = new[]
							{
								Conversion.SideCameraModel( SideCamera1Image, MainSideCamera1Display ),
								Conversion.SideCameraModel( SideCamera2Image, MainSideCamera2Display ),
								Conversion.SideCameraModel( SideCamera3Image, MainSideCamera3Display )
							};
							bool[] result = await Task.WhenAll( tasks );

							isGood = result[ 0 ] && result[ 1 ] && result[ 2 ];
							//MIL.MdispSelect( InfraredCameraConversionDisplay, InfraredCameraConversionImage );
							Logger.WriteLine( "이미지 처리 완료" );

							if (!Dispatcher.CheckAccess())
							{
								// UI 스레드에서 실행되도록 Dispatcher를 사용하여 호출
								Dispatcher.Invoke( ( ) => GoodLamp( isGood ) );
							}
							if (!isGood) EjectAction();

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
						isCapturing = false;
					}

				}

			} );
			TectTime.Stop();
			Logger.WriteLine( $"이미지 처리 시간: {TectTime.ElapsedMilliseconds}ms" );


		}

		private async void EjectAction()
		{
			await Task.Run( async ( ) =>
			{
				await Task.Delay( 3000 );
				advantechCard.WriteCoil = true ;
				await Task.Delay( 500 );
				advantechCard.WriteCoil = false;
			} );
		}

		private void TeachingButton_Click( object sender, RoutedEventArgs e )
		{
			MIL_ID TeachingImage = MIL.M_NULL;
			MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref TeachingImage );
			// 템플릿 학습 윈도우 열기
			if (MIL.MbufInquire( InfraredCameraImage, MIL.M_TYPE, MIL.M_NULL ) != MIL.M_NULL)
			{
				MessageBox.Show( "이미지가 캡처되지 않았습니다." );
				Logger.WriteLine( "이미지가 캡처되지 않았습니다." );
				return;
			}
			TeachingWindow teachingWindow = new TeachingWindow(TeachingImage, (int)InfraredCamera.Width(), (int)InfraredCamera.Height(), (int)InfraredCamera.NbBands(), (int)InfraredCamera.DataType() );
			teachingWindow.ShowDialog();

		}

		private void ManualButton_Click( object sender, RoutedEventArgs e )
		{

			ManualWindow manualWindow = new ManualWindow( modbus );
			manualWindow.ShowDialog();

		}

		private void Click( object sender, RoutedEventArgs e )
		{
			MessageBox.Show( "버튼이 클릭되었습니다." );
			Logger.WriteLine( "버튼이 클릭되었습니다." );
		}
		
		private async void StartButton_Click( object sender, RoutedEventArgs e )
		{

			await ConnectAction();

			await modbus.SelfHolding( 1, 1 );
			await modbus.SelfHolding( 4, 4 );

			await modbus.SelfHolding( 0x20, 0x20 );
			await modbus.SelfHolding( 0x22, 0x22 );
			await modbus.SelfHolding( 0x24, 0x24 );

			//modbus.WriteHoldingRegisters32( 0, 0x00, 20000 );
			int pos = 85000;
			int speed = 20000;
			modbus.HoldingRegister32[ 0x00 ] = pos;
			modbus.HoldingRegister32[ 0x01 ] = speed;

			await Task.Delay( 100 );

			if (modbus.InputRegister32[ 0x00 ] != pos)
			{
				await Task.Run( ( ) =>
				{
					//modbus.WriteSingleCoil( 0, 0x0A, true );
					modbus.OutputCoil[ 0x0A ] = true;
					Logger.WriteLine( "Servo Move Start" );
					var startTime = DateTime.Now;
					while (true)
					{
						//bool[] coil = modbus.ReadInputs( 0, 0x0A, 1 );
						if (modbus.InputCoil[ 0x0A ])
						{
							//modbus.WriteSingleCoil( 0, 0x0A, false );
							modbus.OutputCoil[ 0x0A ] = false;
							Logger.WriteLine( "Servo Moveing..." );
							break;
						}
						if ((DateTime.Now - startTime).TotalMilliseconds > 15000) // 10초 타임아웃
						{
							modbus.OutputCoil[ 0x0A ] = false;
							Logger.WriteLine( "SelfHolding operation timed out." );
							//throw new TimeoutException( "SelfHolding operation timed out." );
							break;
						}
						Thread.Sleep( 10 );
					}
					startTime = DateTime.Now;
					Logger.WriteLine( "Servo Move Complete 대기" );
					//modbus.WriteSingleCoil( 0, 0x0B, true );
					modbus.OutputCoil[ 0x0B ] = false;
					while (true)
					{
						//bool[] coil = modbus.ReadInputs( 0, 0x0B, 1 );
						if (modbus.InputCoil[ 0x0B ])
						{
							//modbus.WriteSingleCoil( 0, 0x0B, false );
							modbus.OutputCoil[ 0x0B ] = false;
							Logger.WriteLine( "Servo Move Complete" );
							break;
						}
						if ((DateTime.Now - startTime).TotalMilliseconds > 15000) // 10초 타임아웃
						{
							modbus.OutputCoil[ 0x0B ] = false;
							Logger.WriteLine( "SelfHolding operation timed out." );
							//throw new TimeoutException( "SelfHolding operation timed out." );
							break;
						}
						Thread.Sleep( 10 );
					}
				} );
			}

			await modbus.SelfHolding( 0x10, 0x10 );


			Logger.WriteLine( "Trigger Reading Start." );
			TriggerReadingAsync();
			Logger.WriteLine( "Machine Start." );
		}

		private async void StopButton_Click( object sender, RoutedEventArgs e )
		{
			await StopAction();
		}

		private async Task StopAction( )
		{
			triggerReadingOFFRequire = true;
			await Task.Run( ( ) =>
			{
				while (triggerReadingStatus)
				{
					System.Threading.Thread.Sleep( 1000 );
				}
			} );

			Logger.WriteLine( "Trigger Reading Stop." );
			await modbus.SelfHolding( 0x11, 0x11 );
			Logger.WriteLine( "C/V OFF" );
			await modbus.SelfHolding( 0x21, 0x21 );
			await modbus.SelfHolding( 0x23, 0x23 );
			await modbus.SelfHolding( 0x25, 0x25 );

			await modbus.SelfHolding( 5, 5 );
			await modbus.SelfHolding( 0, 0 );
			Logger.WriteLine( "Machine Stop." );
		}






		//private void OnImageCaptured( byte[ ] pixelData )
		//{
		//	Dispatcher.Invoke( ( ) =>
		//	{
		//		DisplayImage( (byte[ ])pixelData.Clone() );

		//		// FPS 계산
		//		frameCount++;
		//		TimeSpan elapsed = DateTime.Now - fpsStartTime;

		//		if (elapsed.TotalSeconds >= 1.0)
		//		{
		//			currentFps = frameCount / elapsed.TotalSeconds;
		//			frameCount = 0;
		//			fpsStartTime = DateTime.Now;

		//			FpsLabel.Content = $"FPS: {currentFps:F2}";
		//			Debug.WriteLine( $"FPS 업데이트: {currentFps:F2}" );
		//		}
		//	} );
		//}


		//private void ContinuousMenuItem_Checked( object sender, RoutedEventArgs e )
		//{
		//	isContinuous = true;

		//	if (cameraManager.IsConnected)
		//	{
		//		cameraManager.StartContinuousCapture();
		//	}
		//}

		//private void ContinuousMenuItem_Unchecked( object sender, RoutedEventArgs e )
		//{
		//	isContinuous = false;

		//	if (cameraManager.IsConnected)
		//	{
		//		cameraManager.StopContinuousCapture();
		//	}
		//}



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

		private void LoadModel( string modelData )
		{
			// 여기에 모델 로딩 로직을 구현하세요
			DeserializeModelData( modelData );
			MessageBox.Show( "모델이 로드되었습니다 : {modelData}");
			Logger.WriteLine( "모델이 로드되었습니다 : {modelData}" );


			// 예를 들어, modelData를 역직렬화하여 애플리케이션의 상태나 설정에 적용할 수 있습니다.
		}

		private void ListBox_SelectionChanged( object sender, System.Windows.Controls.SelectionChangedEventArgs e )
		{
			if (e.AddedItems.Count > 0)
			{
				string? selectedImagePath = e.AddedItems[0] as string;
				if (!string.IsNullOrEmpty( selectedImagePath ) && File.Exists( selectedImagePath ))
				{
					// 선택된 이미지를 VisionImage에 표시
					try
					{
						InfraredCameraImage = InfraredCamera.LoadImage( MilSystem, selectedImagePath);
						MIL.MdispSelect( InfraredCameraDisplay, InfraredCameraImage );
					}
					catch (Exception ex)
					{
						MessageBox.Show( $"이미지를 불러오는 중 오류 발생: {ex.Message}" );
						Logger.WriteLine( $"이미지를 불러오는 중 오류 발생: {ex.Message}" );
					}
				}
			}
		}

		private void LoadAllTriggeredImagesButton_Click( object sender, RoutedEventArgs e )
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

		private void DeserializeModelData( string serializedData )
		{
			var items = JsonConvert.DeserializeObject<List<ComboBoxItemViewModel>>(serializedData);
		}


		private async void StartClockAsync( )
		{
			await Task.Run( ( ) =>
			{
				while (true)
				{
					Task.Run( ( ) =>
					{
						Dispatcher.Invoke( ( ) =>
						{
							TimeLabel.Content = DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" );
						} );
					} );

					System.Threading.Thread.Sleep( 1000 );
				}
			} );

		}

		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e );

			// MILContext 해제
			MILContext.Instance.Dispose();
		}

		private async void Window_Closing( object? sender, System.ComponentModel.CancelEventArgs e )
		{

			//Logger.WriteLine( "Window_Closing 이벤트 발생" );

		}

		private async void ExitProgram( object sender, EventArgs e )
		{
			await StopAction();


			if (InfraredCamera.IsConnected) InfraredCameraImage = InfraredCamera.ReciveImage();
			if (SideCamera1.IsConnected) SideCamera1Image = SideCamera1.ReciveImage();
			if (SideCamera2.IsConnected) SideCamera2Image = SideCamera2.ReciveImage();
			if (SideCamera3.IsConnected) SideCamera3Image = SideCamera3.ReciveImage();

			var tasks = new[]
				{
					InfraredCamera.DisconnectAsync(),
					SideCamera1.DisconnectAsync(),
					SideCamera2.DisconnectAsync(),
					SideCamera3.DisconnectAsync()
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

			// 3. 이미지 버퍼 해제
			if (InfraredCameraImage != MIL.M_NULL)
			{
				MIL.MbufFree( InfraredCameraImage );
				InfraredCameraImage = MIL.M_NULL;
				Logger.WriteLine( "InfraredCameraImage 해제 완료." );
			}

			if (InfraredCameraConversionImage != MIL.M_NULL)
			{
				MIL.MbufFree( InfraredCameraConversionImage );
				InfraredCameraConversionImage = MIL.M_NULL;
				Logger.WriteLine( "InfraredCameraConversionImage 해제 완료." );
			}

			if (SideCamera1Image != MIL.M_NULL)
			{
				MIL.MbufFree( SideCamera1Image );
				SideCamera1Image = MIL.M_NULL;
				Logger.WriteLine( "SideCamera1Image 해제 완료." );
			}

			if (SideCamera1ConversionImage != MIL.M_NULL)
			{
				MIL.MbufFree( SideCamera1ConversionImage );
				SideCamera1ConversionImage = MIL.M_NULL;
				Logger.WriteLine( "SideCamera1ConversionImage 해제 완료." );
			}

			if (SideCamera2Image != MIL.M_NULL)
			{
				MIL.MbufFree( SideCamera2Image );
				SideCamera2Image = MIL.M_NULL;
				Logger.WriteLine( "SideCamera2Image 해제 완료." );
			}

			if (SideCamera2ConversionImage != MIL.M_NULL)
			{
				MIL.MbufFree( SideCamera2ConversionImage );
				SideCamera2ConversionImage = MIL.M_NULL;
				Logger.WriteLine( "SideCamera2ConversionImage 해제 완료." );
			}

			if (SideCamera3Image != MIL.M_NULL)
			{
				MIL.MbufFree( SideCamera3Image );
				SideCamera3Image = MIL.M_NULL;
				Logger.WriteLine( "SideCamera3Image 해제 완료." );
			}

			if (SideCamera3ConversionImage != MIL.M_NULL)
			{
				MIL.MbufFree( SideCamera3ConversionImage );
				SideCamera3ConversionImage = MIL.M_NULL;
				Logger.WriteLine( "SideCamera3ConversionImage 해제 완료." );
			}

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
			modbus.WriteHoldingRegisters32( 0, 15, 1000000 );

			Logger.WriteLine($"{modbus.ReadHoldingRegisters32( 0, 15, 1 )[0]}" );

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
