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
using System.Windows.Input;
using System.Runtime.CompilerServices;


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

		//private MIL_ID _infraredCameraImage;
		//private MIL_ID _infraredCameraConversionImage;

		//private MIL_ID _sideCamera1Image;
		//private MIL_ID _sideCamera1ConversionImage;

		//private MIL_ID _sideCamera2Image;
		//private MIL_ID _sideCamera2ConversionImage;

		//private MIL_ID _sideCamera3Image;
		//private MIL_ID _sideCamera3ConversionImage;



		private MainViewModel _mainViewModel;

		private ManualViewModel _manualViewModel;

		private SettingViewModel _settingViewModel;


		private MainUserControl _mainUserControl;

		private ManualUserControl _manualUserControl;

		private SettingUserControl _settingUserControl;

		private readonly MilSystemService _milSystemService;

		private CameraService _cameraService;




		public MainWindow( MainViewModel mainViewModel ,ManualViewModel manualViewModel, SettingViewModel settingViewModel, MilSystemService milSystemService, CameraService cameraService )
		{
			InitializeComponent();


			_mainViewModel = mainViewModel;
			_manualViewModel = manualViewModel;
			_settingViewModel = settingViewModel;

			/// ViewModel을 DataContext로 설정
			this.DataContext = _mainViewModel;

			_milSystemService = milSystemService;

			_cameraService = cameraService;


			InitMILDisplay();


			_mainUserControl = new MainUserControl( _mainViewModel );

			_manualUserControl = new ManualUserControl( _manualViewModel );

			_settingUserControl = new SettingUserControl( _settingViewModel );

			MainContent.Content = _mainUserControl;

			// ManualUserControl에서 이벤트 구독 (예: "돌아가기" 신호)
			_manualUserControl.GoMainRequested += ( s, e ) =>
			{
				MainContent.Content = _mainUserControl;
				_manualViewModel.PositionReadStop();
			};

			_settingUserControl.GoMainRequested += ( s, e ) =>
			{
				MainContent.Content = _mainUserControl;
			};


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
			_mainViewModel?.StartClockAsync();

		}

		private void MainWindow_Closing( object? sender, EventArgs e )
		{
			_mainViewModel?.StopClock();
			//row new NotImplementedException();
		}


		private void InitMILDisplay()
		{

			InfraredCameraDisplay.DisplayId = _milSystemService.InfraredDisplay;
			InfraredCameraConversionDisplay.DisplayId = _milSystemService.InfraredConversionDisplay;

			//mainInfraredCameraDisplay.DisplayId = _milSystemService.InfraredDisplay;
			//mainInfraredCameraConversionDisplay.DisplayId = MainInfraredCameraConversionDisplay;

			//sideCamera1Display.DisplayId = _sideCamera1Display;
			//sideCamera1ConversionDisplay.DisplayId = _sideCamera1ConversionDisplay;

			//mainSideCamera1Display.DisplayId = _sideCamera1Display;
			//mainSideCamera1ConversionDisplay.DisplayId = MainSideCamera1ConversionDisplay;

			//sideCamera2Display.DisplayId = _sideCamera2Display;
			//sideCamera2ConversionDisplay.DisplayId = _sideCamera2ConversionDisplay;

			//mainSideCamera2Display.DisplayId = _sideCamera2Display;
			//mainSideCamera2ConversionDisplay.DisplayId = MainSideCamera2ConversionDisplay;

			//sideCamera3Display.DisplayId = _sideCamera3Display;
			//sideCamera3ConversionDisplay.DisplayId = _sideCamera3ConversionDisplay;

			//mainSideCamera3Display.DisplayId = _sideCamera3Display;
			//mainSideCamera3ConversionDisplay.DisplayId = MainSideCamera3ConversionDisplay;

		}

		private void MILWPFDisplay_MouseMove( object sender, MouseEventArgs e )
		{
			const int imageWidth = 640;
			const int imageHeight = 480;
			// 컨트롤 내 마우스 위치 가져오기
			var pos = e.GetPosition(InfraredCameraDisplay);
			double mouseX = pos.X;
			double mouseY = pos.Y;

			// 디스플레이 크기
			double displayWidth = InfraredCameraDisplay.ActualWidth;
			double displayHeight = InfraredCameraDisplay.ActualHeight;

			// 종횡비 계산
			double imageAspect = (double)imageWidth / imageHeight;
			double displayAspect = displayWidth / displayHeight;

			double scale, offsetX = 0, offsetY = 0;
			if (displayAspect > imageAspect)
			{
				scale = displayHeight / imageHeight;
				double scaledImageWidth = imageWidth * scale;
				offsetX = (displayWidth - scaledImageWidth) / 2.0;
			}
			else
			{
				scale = displayWidth / imageWidth;
				double scaledImageHeight = imageHeight * scale;
				offsetY = (displayHeight - scaledImageHeight) / 2.0;
			}

			// 실제 이미지 좌표 계산 (반올림 적용)
			int ix = (int)Math.Round((mouseX - offsetX) / scale);
			int iy = (int)Math.Round((mouseY - offsetY) / scale);

			// 경계 체크 (0 <= ix < imageWidth, 0 <= iy < imageHeight)
			if (ix < 0) ix = 0;
			else if (ix >= imageWidth) ix = imageWidth - 1;
			if (iy < 0) iy = 0;
			else if (iy >= imageHeight) iy = imageHeight - 1;

			//Logger.WriteLine( $"마우스: ({mouseX:F1}, {mouseY:F1}) => 이미지 좌표: ({ix}, {iy})" );

			// 이미지 데이터가 ushort[] 배열이라고 가정
			ushort[] imageData = _cameraService.ImageData();
			if (imageData != null && imageData.Length >= imageWidth * imageHeight)
			{
				int index = iy * imageWidth + ix;
				ushort pixelValue = imageData[index];
				//Logger.WriteLine( $"온도 값: {(double)(pixelValue-27315)/100}" );
				_mainViewModel.CurrentTemperature = $"{(double) (pixelValue - 27315) / 100}°C";

			}
			else
			{
				//Logger.WriteLine( "ImageData가 null이거나 크기가 올바르지 않습니다." );
			}
		}


		private void ManualButton_Click( object sender, RoutedEventArgs e )
		{
			// Manual 화면으로 전환
			MainContent.Content = _manualUserControl;
			_manualViewModel.PositionReadStart();

		}


		private void ModelButton_Click( object sender, RoutedEventArgs e )
		{
			// Manual 화면으로 전환
			MainContent.Content = _settingUserControl;
			_settingViewModel.UpdateCameraSettings();

		}

		private void Click( object sender, RoutedEventArgs e )
		{
			MessageBox.Show( "버튼이 클릭되었습니다." );
			Logger.WriteLine( "버튼이 클릭되었습니다." );
		}


		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e );

			// MILContext 해제
			
		}


		private async void ExitProgram( object sender, EventArgs e )
		{
			await Task.Delay( 1 );
			Application.Current.Shutdown();
		}


		//private void Show3DButton_Click( object sender, RoutedEventArgs e )
		//{
		//	// 별도의 윈도우를 띄워서 3D 표시
		//	_3DView _3dview = new _3DView( (byte[])RawPixelData.Clone());
		//	_3dview.Show();
		//}

	}
}
