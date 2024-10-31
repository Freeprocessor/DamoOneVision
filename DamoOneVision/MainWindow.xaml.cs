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
		public MainWindow( )
		{
			InitializeComponent();

			this.Loaded += MainWindow_Loaded;
			this.Closing += MainWindow_Closing;
		}
		private void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
			try
			{
				// MIL 초기화 및 자원 할당
				MIL.MappAlloc( MIL.M_DEFAULT, ref MilApplication );
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
			}
			catch (Exception ex)
			{
				MessageBox.Show( $"MIL 초기화 오류: {ex.Message}" );
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
				// 예외 처리
			}
		}

		private void DisplayImage( )
		{
			// 이미지 크기 가져오기
			int width = (int)MIL.MbufInquire(MilImage, MIL.M_SIZE_X, MIL.M_NULL);
			int height = (int)MIL.MbufInquire(MilImage, MIL.M_SIZE_Y, MIL.M_NULL);

			// 픽셀 데이터 버퍼 생성
			int bufferSize = width * height;
			byte[] pixelData = new byte[bufferSize];

			// MIL 이미지 버퍼에서 픽셀 데이터 가져오기
			MIL.MbufGet( MilImage, pixelData );

			// WriteableBitmap 생성
			WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
			bitmap.WritePixels( new Int32Rect( 0, 0, width, height ), pixelData, width, 0 );

			// 이미지 컨트롤에 표시
			visionImage.Source = bitmap;
		}

		private void MainWindow_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			// 타이머 중지
			timer.Stop();

			// MIL 자원 해제
			if (MilImage != MIL.M_NULL)
				MIL.MbufFree( MilImage );
			if (MilDigitizer != MIL.M_NULL)
				MIL.MdigFree( MilDigitizer );
			if (MilSystem != MIL.M_NULL)
				MIL.MsysFree( MilSystem );
			if (MilApplication != MIL.M_NULL)
				MIL.MappFree( MilApplication );
		}
	}
}