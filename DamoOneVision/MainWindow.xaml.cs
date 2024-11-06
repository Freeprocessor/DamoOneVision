using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Matrox.MatroxImagingLibrary;
using System.Windows.Threading;
using System.Runtime.InteropServices;

using SpinnakerNET;
using SpinnakerNET.GenApi;

//using LiteDB;
using DamoOneVision.Camera;
using DamoOneVision.Data;
using System.IO;


namespace DamoOneVision
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	///
	public partial class MainWindow : Window
	{
		//private ICamera camera;
		private CameraManager cameraManager;

		private WriteableBitmap bitmap;
		private int frameCount = 0;
		private DateTime fpsStartTime = DateTime.Now;
		private double currentFps = 0;

		public MainWindow( )
		{
			InitializeComponent();

			// 윈도우 종료 이벤트 핸들러 추가
			this.Closing += Window_Closing;

			cameraManager = new CameraManager();
			cameraManager.ImageCaptured += OnImageCaptured;
		}
		private string DetectCameraModel( )
		{
			// 카메라 모델을 결정하는 로직을 구현
			// 예를 들어, 사용자 입력이나 설정 파일을 통해 결정
			// 또는 연결된 카메라를 검색하여 모델명 확인

			// 예시로 수동 설정
			return "Matrox"; // 또는 "Spinnaker"
			//return "Spinnaker";
		}
		private async void ConnectButton_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				string cameraModel = DetectCameraModel();

				cameraManager.Connect( cameraModel );

				ConnectButton.IsEnabled = false;
				DisconnectButton.IsEnabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show( $"카메라 연결 오류\n{ex.Message}" );
				await cameraManager.DisconnectAsync();
			}
		}

		private async void DisconnectButton_Click( object sender, RoutedEventArgs e )
		{
			await cameraManager.DisconnectAsync();

			VisionImage.Source = null;
			bitmap = null;
			FpsLabel.Content = "FPS: 0";

			ConnectButton.IsEnabled = true;
			DisconnectButton.IsEnabled = false;
		}

		private void OnImageCaptured( byte[ ] pixelData )
		{
			Dispatcher.Invoke( ( ) =>
			{
				DisplayImage( pixelData );

				// FPS 계산
				frameCount++;
				TimeSpan elapsed = DateTime.Now - fpsStartTime;

				if (elapsed.TotalSeconds >= 1.0)
				{
					currentFps = frameCount / elapsed.TotalSeconds;
					frameCount = 0;
					fpsStartTime = DateTime.Now;

					FpsLabel.Content = $"FPS: {currentFps:F2}";
					Console.WriteLine( $"FPS 업데이트: {currentFps:F2}" );
				}
			} );
		}

		private void DisplayImage( byte[ ] pixelData )
		{
			int width = cameraManager.GetWidth();
			int height = cameraManager.GetHeight();

			// 픽셀 포맷을 컬러로 변경 (예: Bgr24)
			PixelFormat pixelFormat = PixelFormats.Gray8;
			int bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;

			if (bitmap == null || bitmap.PixelWidth != width || bitmap.PixelHeight != height || bitmap.Format != pixelFormat)
			{
				bitmap = new WriteableBitmap( width, height, 96, 96, pixelFormat, null );
				VisionImage.Source = bitmap;
			}

			bitmap.Lock();
			try
			{
				// 스트라이드 계산
				int stride = width * bytesPerPixel;

				// 픽셀 데이터를 WriteableBitmap에 쓰기
				bitmap.WritePixels( new Int32Rect( 0, 0, width, height ), pixelData, stride, 0 );
			}
			finally
			{
				bitmap.Unlock();
			}
		}

		private async void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			await cameraManager.DisconnectAsync();
		}

		private void Click( object sender, RoutedEventArgs e )
		{
			MessageBox.Show( "버튼이 클릭되었습니다." );
		}

		private void ExitProgram( object sender, EventArgs e )
		{
			Application.Current.Shutdown();
		}
	}
}
