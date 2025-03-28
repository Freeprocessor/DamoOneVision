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

		private MainUserControl _mainUserControl;

		private ManualUserControl _manualUserControl;

		private readonly MilSystemService _milSystemService;




		public MainWindow( MainViewModel mainViewModel ,ManualViewModel manualViewModel, MilSystemService milSystemService )
		{
			InitializeComponent();


			_mainViewModel = mainViewModel;
			_manualViewModel = manualViewModel;

			/// ViewModel을 DataContext로 설정
			this.DataContext = _mainViewModel;

			_milSystemService = milSystemService;


			InitMILDisplay();


			_mainUserControl = new MainUserControl( _mainViewModel );

			_manualUserControl = new ManualUserControl( _manualViewModel );

			MainContent.Content = _mainUserControl;

			// ManualUserControl에서 이벤트 구독 (예: "돌아가기" 신호)
			_manualUserControl.GoMainRequested += ( s, e ) =>
			{
				MainContent.Content = _mainUserControl;
				_manualViewModel.PositionReadStop();
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

			infraredCameraDisplay.DisplayId = _milSystemService.InfraredDisplay;
			infraredCameraConversionDisplay.DisplayId = _milSystemService.InfraredConversionDisplay;

			mainInfraredCameraDisplay.DisplayId = _milSystemService.InfraredDisplay;
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


		private void ManualButton_Click( object sender, RoutedEventArgs e )
		{
			// Manual 화면으로 전환
			MainContent.Content = _manualUserControl;
			_manualViewModel.PositionReadStart();

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

				//GoodRejectLamp.Background = System.Windows.Media.Brushes.Red;
				//GoodRejectText.Content = "Reject";
			}
			else
			{
				Logger.WriteLine( "Good" );
				//GoodRejectLamp.Background = System.Windows.Media.Brushes.Green;
				//GoodRejectText.Content = "Good";
			}
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
