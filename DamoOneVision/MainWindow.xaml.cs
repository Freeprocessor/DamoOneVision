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
using Microsoft.Win32;
using System.Diagnostics;


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
		private CameraManager cameraManager;

		private WriteableBitmap bitmap;
		private byte[ ] RawPixelData;

		private int frameCount = 0;
		private DateTime fpsStartTime = DateTime.Now;
		private double currentFps = 0;

		private bool isContinuous = false; // Continuous 모드 상태
		private bool isCapturing = false;  // 이미지 캡처 중인지 여부


		public MainWindow( )
		{
			InitializeComponent();

			// 윈도우 종료 이벤트 핸들러 추가
			this.Closing += Window_Closing;

			cameraManager = new CameraManager();
			cameraManager.ImageCaptured += OnImageCaptured;

			//templateMatcher = new TemplateMatcher();
		}
		private string DetectCameraModel( )
		{
			// 카메라 모델을 결정하는 로직을 구현
			// 예를 들어, 사용자 입력이나 설정 파일을 통해 결정
			// 또는 연결된 카메라를 검색하여 모델명 확인

			// 예시로 수동 설정
			return "USB"; // 또는 "Matrox"
							 //return "Spinnaker";
							 //return "USB";
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
					Debug.WriteLine( $"FPS 업데이트: {currentFps:F2}" );
				}
			} );
		}

		/*
		private void OnImageCaptured( byte[ ] pixelData )
		{
			Dispatcher.Invoke( ( ) =>
			{
				DisplayImage( pixelData );

				// 템플릿 매칭 수행
				if (templateMatcher != null)
				{
					double posX, posY, score;
					templateMatcher.FindTemplate( pixelData, cameraManager.GetWidth(), cameraManager.GetHeight(), out posX, out posY, out score );

					if (score > 0.8) // 임계값은 필요에 따라 조정
					{
						// 템플릿이 발견된 위치에 표시하거나 처리
						Debug.WriteLine( $"템플릿 발견: X={posX}, Y={posY}, Score={score}" );
					}
				}

				// FPS 계산 코드
			} );
		}*/

		private void DisplayImage( byte[ ] pixelData )
		{
			int width = (int)MILContext.Width;
			int height = (int)MILContext.Height;
			int bytesPerPixel = (int)MILContext.DataType*(int)MILContext.NbBands / 8;

			if (bitmap == null || bitmap.PixelWidth != width || bitmap.PixelHeight != height )
			{
				bitmap = new WriteableBitmap( width, height, 96, 96, getPixelFormat(), null );
				VisionImage.Source = bitmap;
			}

			bitmap.Lock();
			try
			{
				// 스트라이드 계산
				int stride = width * bytesPerPixel;

				// 픽셀 데이터를 WriteableBitmap에 쓰기
				bitmap.WritePixels( new Int32Rect( 0, 0, width, height ), pixelData, stride, 0 );
				this.RawPixelData = pixelData;
			}
			finally
			{
				bitmap.Unlock();
			}
		}

		private static PixelFormat getPixelFormat()
		{
			PixelFormat pixelFormat;
			if (MILContext.DataType == 8 && MILContext.NbBands == 3)
			{
				pixelFormat = PixelFormats.Rgb24;
			}
			else if (MILContext.DataType == 8 && MILContext.NbBands == 1)
			{
				pixelFormat = PixelFormats.Gray8;
			}
			else if (MILContext.DataType == 16 && MILContext.NbBands == 1)
			{
				pixelFormat = PixelFormats.Gray16;
			}
			else
			{
				pixelFormat = PixelFormats.Default;
			}


			return pixelFormat;
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
		private void ContinuousMenuItem_Checked( object sender, RoutedEventArgs e )
		{
			isContinuous = true;

			if (cameraManager.IsConnected)
			{
				cameraManager.StartContinuousCapture();
			}
		}

		private void ContinuousMenuItem_Unchecked( object sender, RoutedEventArgs e )
		{
			isContinuous = false;

			if (cameraManager.IsConnected)
			{
				cameraManager.StopContinuousCapture();
			}
		}

		private async void TriggerButton_Click( object sender, RoutedEventArgs e )
		{
			if (!cameraManager.IsConnected)
			{
				MessageBox.Show( "카메라가 연결되어 있지 않습니다." );
				return;
			}

			if (!isContinuous && !isCapturing)
			{
				isCapturing = true;

				try
				{
					// 단일 이미지 캡처
					byte[] pixelData = await cameraManager.CaptureSingleImageAsync();

					if (pixelData != null)
					{
						DisplayImage( pixelData );
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show( $"이미지 캡처 중 오류 발생: {ex.Message}" );
				}
				finally
				{
					isCapturing = false;
				}
			}
		}
		
		private void TeachingButton_Click( object sender, RoutedEventArgs e )
		{
			// 템플릿 학습 윈도우 열기
			if (this.RawPixelData == null)
			{
				MessageBox.Show( "이미지가 캡처되지 않았습니다." );
				return;
			}
			TeachingWindow teachingWindow = new TeachingWindow((byte[])this.RawPixelData.Clone(), (int)MILContext.Width, (int)MILContext.Height, getPixelFormat());
			teachingWindow.ShowDialog();

			//if (teachingWindow.TemplateImageData != null)
			//{
				// 템플릿 학습
				//templateMatcher.TeachTemplate( teachingWindow.TemplateImageData, teachingWindow.TemplateWidth, teachingWindow.TemplateHeight );
			//	MessageBox.Show( "템플릿이 학습되었습니다." );
			//}
		}

		//private void FileLoadingButton_Click( object sender, RoutedEventArgs e )
		//{
		//	OpenFileDialog openFileDialog = new OpenFileDialog();
		//	if (openFileDialog.ShowDialog() == true)
		//	{
		//		// 파일 선택 후 처리 로직을 여기에 구현
		//		MessageBox.Show( $"선택된 파일: {openFileDialog.FileName}" );
		//	}
		//}
		protected async override void OnClosed( EventArgs e )
		{
			base.OnClosed( e );

			// 리소스 해제
			//templateMatcher?.Dispose();
			await cameraManager?.DisconnectAsync();

			// MILContext 해제
			MILContext.Instance.Dispose();
		}

		private void LoadModelButton_Click( object sender, RoutedEventArgs e )
		{

		}
	}
}
