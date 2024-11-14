using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace DamoOneVision.Camera
{
	public class USBCamera : ICamera
	{
		// Matrox SDK 관련 필드
		private MIL_ID MilSystem = MIL.M_NULL;
		private MIL_ID MilImage = MIL.M_NULL;

		//OpenCV 관련 필드
		private VideoCapture capture;
		private Mat frame;

		public bool Connect( )
		{
			// MILContext에서 MilSystem 가져오기
			MilSystem = MILContext.Instance.MilSystem;

			// OpenCV VideoCapture 초기화 (0은 기본 카메라를 의미)
			capture = new VideoCapture( 0 );
			if (!capture.IsOpened())
			{
				Console.WriteLine( "카메라를 열 수 없습니다." );
				return false;
			}

			frame = new Mat();

			return true;
		}

		public void Disconnect( )
		{
			// OpenCV 리소스 해제
			if (capture != null)
			{
				capture.Release();
				capture.Dispose();
				capture = null;
			}

			if (frame != null)
			{
				frame.Dispose();
				frame = null;
			}

			// MIL 리소스 해제
			if (MilImage != MIL.M_NULL)
			{
				MIL.MbufFree( MilImage );
				MilImage = MIL.M_NULL;
			}

		}

		public byte[ ] CaptureImage( )
		{
			// 이미지 캡처
			if (capture != null)
			{
				capture.Read( frame );
				if (frame.Empty())
				{
					Console.WriteLine( "프레임을 읽을 수 없습니다." );
					return null;
				}

				// OpenCV Mat를 MIL 버퍼로 변환
				byte[] imageData = ConvertMatToMilBuffer(frame);
				//byte[] imageData = frame;
				return imageData;
			}

			return null;
		}

		private byte[ ] ConvertMatToMilBuffer( Mat mat )
		{
			//Cv2.CvtColor( mat, mat, ColorConversionCodes.BGR2GRAY );
			//Console.WriteLine( "" );
			MIL_ID MilImageLocal = MIL.M_NULL;
			byte[] imageData = null;

			try
			{
				int width = mat.Width;
				int height = mat.Height;
				int channels = mat.Channels();


				//Data Type
				int milType = 8+MIL.M_UNSIGNED;

				// 픽셀 포맷 및 속성 설정
				MIL_INT attribute = MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED;

				// MIL 버퍼 할당
				//TODO 필요한 구문이 아닌 것 같으니 최적화 대상
				if(channels == 1)
				{
					//gray
					MilImageLocal = MIL.MbufAllocColor( MilSystem, channels, width, height, milType, attribute, MIL.M_NULL );
				}
				else if(channels ==3)
				{
					//color
					attribute += MIL.M_BGR24;
					MilImageLocal = MIL.MbufAllocColor( MilSystem, channels, width, height, milType, attribute, MIL.M_NULL );
				}
				else
				{
					Console.WriteLine( "지원하지 않는 채널 수입니다." );
					return null;
				}

				// OpenCV Mat 데이터가 연속적인지 확인
				if (!mat.IsContinuous())
				{
					mat = mat.Clone();
				}

				//MIL.MbufPut( MilImageLocal, matData );


				// OpenCV Mat 데이터를 바이트 배열로 가져오기
				int bufferSize = width * height * channels;
				byte[] matData = new byte[bufferSize];
				Marshal.Copy( mat.Data, matData, 0, bufferSize );
				MilImage = MIL.MbufAlloc2d( MilSystem, width, height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );

				MIL.MbufPut( this.MilImage, matData );

				imageData = matData;
			}
			finally
			{
				if (MilImageLocal != MIL.M_NULL)
				{
					MIL.MbufFree( MilImageLocal );
				}
			}

			return imageData;
		}

		public int GetWidth( )
		{
			return (int) MIL.MbufInquire( MilImage, MIL.M_SIZE_X, MIL.M_NULL );
		}

		public int GetHeight( )
		{
			return (int) MIL.MbufInquire( MilImage, MIL.M_SIZE_Y, MIL.M_NULL );
		}
	}
}
