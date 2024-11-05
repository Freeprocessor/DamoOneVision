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

using LiteDB;
using DamoOneVision.Camera;

namespace DamoOneVision
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	///
	public partial class MainWindow : Window
	{
		private ICamera camera;

		private DispatcherTimer timer;

		// 이미지 표시용 WriteableBitmap
		private WriteableBitmap bitmap;


		private int frameCount = 0;
		private DateTime fpsStartTime = DateTime.Now;
		private double currentFps = 0;

		private CancellationTokenSource cts;
		private Task captureTask;

		public MainWindow( )
		{
			InitializeComponent();

			// 윈도우 종료 이벤트 핸들러 추가
			this.Closing += Window_Closing;
		}
		private string DetectCameraModel( )
		{
			// 카메라 모델을 결정하는 로직을 구현
			// 예를 들어, 사용자 입력이나 설정 파일을 통해 결정
			// 또는 연결된 카메라를 검색하여 모델명 확인

			// 예시로 수동 설정
			//return "Matrox"; // 또는 "Spinnaker"
			return "Spinnaker";
		}
		private void ConnectButton_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				// 카메라 모델 결정 (예: 사용자 선택 또는 자동 감지)
				string cameraModel = DetectCameraModel();

				if (cameraModel == "Matrox")
				{
					camera = new MatroxCamera();
				}
				else if (cameraModel == "Spinnaker")
				{
					camera = new SpinnakerCamera();
				}
				else
				{
					MessageBox.Show( "지원되지 않는 카메라 모델입니다." );
					return;
				}

				if (camera.Connect())
				{
					// CancellationTokenSource 초기화
					cts = new CancellationTokenSource();

					// 이미지 캡처 Task 시작
					captureTask = Task.Run( ( ) => CaptureImages( cts.Token ), cts.Token );

					// 버튼 상태 변경
					ConnectButton.IsEnabled = false;
					DisconnectButton.IsEnabled = true;
				}
				else
				{
					MessageBox.Show( "카메라 연결 실패" );
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show( $"카메라 연결 오류\n{ex.Message}" );
				// 리소스 해제
				DisconnectCamera();
			}
		}

		private async Task CaptureImages( CancellationToken token )
		{
			frameCount = 0;
			fpsStartTime = DateTime.Now;
			currentFps = 0;

			while (!token.IsCancellationRequested)
			{
				try
				{
					// 이미지 획득
					byte[] pixelData = camera.CaptureImage();

					if (pixelData != null)
					{
						// UI 스레드에서 이미지 표시
						await Dispatcher.InvokeAsync( ( ) => DisplayImage( pixelData ) );

						// FPS 계산
						frameCount++;
						TimeSpan elapsed = DateTime.Now - fpsStartTime;

						if (elapsed.TotalSeconds >= 1.0)
						{
							currentFps = frameCount / elapsed.TotalSeconds;
							frameCount = 0;
							fpsStartTime = DateTime.Now;

							// FPS를 화면에 표시
							await Dispatcher.InvokeAsync( ( ) =>
							{
								FpsLabel.Content = $"FPS: {currentFps:F2}";
							} );
						}
					}
				}
				catch (Exception ex)
				{
					// 예외 처리
					await Dispatcher.InvokeAsync( ( ) =>
					{
						MessageBox.Show( $"이미지 획득 오류: {ex.Message}" );
					} );
				}
			}
		}

		private void DisconnectButton_Click( object sender, RoutedEventArgs e )
		{
			DisconnectCamera();
		}

		private void DisconnectCamera( )
		{
			try
			{
				// CancellationTokenSource 취소
				if (cts != null)
				{
					cts.Cancel();
					cts = null;
				}

				// 캡처 Task 완료 대기
				if (captureTask != null)
				{
					captureTask.Wait();
					captureTask = null;
				}

				// 카메라 해제
				if (camera != null)
				{
					camera.Disconnect();
					camera = null;
				}

				// 이미지 초기화
				VisionImage.Source = null;
				bitmap = null;

				// FPS 레이블 초기화
				Dispatcher.Invoke( ( ) =>
				{
					FpsLabel.Content = "FPS: 0";
				} );

				// 버튼 상태 변경 (UI 스레드에서 실행)
				Dispatcher.Invoke( ( ) =>
				{
					ConnectButton.IsEnabled = true;
					DisconnectButton.IsEnabled = false;
				} );
			}
			catch (Exception ex)
			{
				MessageBox.Show( $"카메라 연결 종료 오류: {ex.Message}" );
			}
		}

		private void Timer_Tick( object sender, EventArgs e )
		{
			try
			{
				// 이미지 획득
				byte[] pixelData = camera.CaptureImage();

				if (pixelData != null)
				{
					// 이미지 표시
					DisplayImage( pixelData );

					// FPS 계산
					frameCount++;
					TimeSpan elapsed = DateTime.Now - fpsStartTime;

					if (elapsed.TotalSeconds >= 1.0)
					{
						currentFps = frameCount / elapsed.TotalSeconds;
						frameCount = 0;
						fpsStartTime = DateTime.Now;

						// FPS를 화면에 표시하거나 로그에 출력
						Dispatcher.Invoke( ( ) =>
						{
							FpsLabel.Content = $"FPS: {currentFps:F2}";
						} );
					}
				}
			}
			catch (Exception ex)
			{
				// 예외 처리
			}
		}

		private void DisplayImage( byte[ ] pixelData )
		{
			int width = camera.GetWidth();
			int height = camera.GetHeight();

			// 첫 실행 시 WriteableBitmap 생성
			if (bitmap == null)
			{
				bitmap = new WriteableBitmap( width, height, 96, 96, PixelFormats.Gray8, null );
				VisionImage.Source = bitmap;
			}

			// WriteableBitmap의 버퍼에 직접 쓰기
			bitmap.Lock();
			try
			{
				bitmap.WritePixels( new Int32Rect( 0, 0, width, height ), pixelData, width, 0 );
			}
			finally
			{
				bitmap.Unlock();
			}
		}

		private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			// 애플리케이션 종료 시 카메라 연결 종료
			DisconnectCamera();
		}
		private void Click( object sender, RoutedEventArgs e )
		{
			MessageBox.Show( "ㅋ" );
		}

		private void ExitProgram( object sender, EventArgs e )
		{
			DisconnectCamera();
			Close();
		}
	}
}
