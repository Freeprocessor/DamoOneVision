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

namespace DamoOneVision
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	///
	public partial class MainWindow : Window
	{
		private MIL_ID MilApplication = MIL.M_NULL;
		private MIL_ID MilSystem = MIL.M_NULL;
		private MIL_ID MilDigitizer = MIL.M_NULL;
		private MIL_ID MilImage = MIL.M_NULL;

		private DispatcherTimer timer;

		// 이미지 표시용 WriteableBitmap
		private WriteableBitmap bitmap;
		public MainWindow( )
		{
			InitializeComponent();

			// 윈도우 종료 이벤트 핸들러 추가
			this.Closing += Window_Closing;
		}
		private void ConnectButton_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				// MIL 초기화 및 자원 할당
				MIL.MappAlloc( MIL.M_DEFAULT, ref MilApplication );

				MIL.MappControl( MIL.M_ERROR, MIL.M_PRINT_DISABLE );
				MIL.MappControl( MIL.M_ERROR, MIL.M_THROW_EXCEPTION );

				MIL.MsysAlloc( MIL.M_SYSTEM_GIGE_VISION, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilSystem );
				MIL.MdigAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref MilDigitizer );
				MIL.MbufAlloc2d( MilSystem,
								MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_X, MIL.M_NULL ),
								MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_Y, MIL.M_NULL ),
								8 + MIL.M_UNSIGNED,
								MIL.M_IMAGE + MIL.M_DISP + MIL.M_GRAB,
								ref MilImage );

				// 타이머 설정
				timer = new DispatcherTimer();
				timer.Interval = TimeSpan.FromMilliseconds( 33 ); // 약 30fps
				timer.Tick += Timer_Tick;
				timer.Start();

				// 버튼 상태 변경
				ConnectButton.IsEnabled = false;
				DisconnectButton.IsEnabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show( $"카메라 연결 오류\n{ex.Message}" );
				// 리소스 해제
				DisconnectCamera();
			}
			finally
			{

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
				// 타이머 중지
				if (timer != null)
				{
					timer.Stop();
					timer.Tick -= Timer_Tick;
					timer = null;
				}

				// MIL 자원 해제
				if (MilImage != MIL.M_NULL)
				{
					MIL.MbufFree( MilImage );
					MilImage = MIL.M_NULL;
				}
				if (MilDigitizer != MIL.M_NULL)
				{
					MIL.MdigFree( MilDigitizer );
					MilDigitizer = MIL.M_NULL;
				}
				if (MilSystem != MIL.M_NULL)
				{
					MIL.MsysFree( MilSystem );
					MilSystem = MIL.M_NULL;
				}
				if (MilApplication != MIL.M_NULL)
				{
					MIL.MappFree( MilApplication );
					MilApplication = MIL.M_NULL;
				}

				// 이미지 초기화
				VisionImage.Source = null;
				bitmap = null;

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
				MIL.MdigGrab( MilDigitizer, MilImage );

				// 이미지 표시
				DisplayImage();
			}
			catch (Exception ex)
			{
				// 예외 처리 (필요 시 로그 또는 메시지 표시)
			}
		}

		private void DisplayImage( )
		{
			// 이미지 크기 가져오기
			int width = (int)MIL.MbufInquire(MilImage, MIL.M_SIZE_X, MIL.M_NULL);
			int height = (int)MIL.MbufInquire(MilImage, MIL.M_SIZE_Y, MIL.M_NULL);

			// 첫 실행 시 WriteableBitmap 생성
			if (bitmap == null)
			{
				bitmap = new WriteableBitmap( width, height, 96, 96, System.Windows.Media.PixelFormats.Gray8, null );
				VisionImage.Source = bitmap;
			}

			// 픽셀 데이터 버퍼 생성
			int bufferSize = width * height;
			byte[] pixelData = new byte[bufferSize];

			// MIL 이미지 버퍼에서 픽셀 데이터 가져오기
			MIL.MbufGet( MilImage, pixelData );

			// WritePixels를 사용하여 픽셀 데이터 업데이트
			bitmap.WritePixels( new Int32Rect( 0, 0, width, height ), pixelData, width, 0 );
		}

		private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			// 애플리케이션 종료 시 카메라 연결 종료
			DisconnectCamera();
		}
	}
}
