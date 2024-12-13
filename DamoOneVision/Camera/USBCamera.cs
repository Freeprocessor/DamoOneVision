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
using System.Diagnostics;

namespace DamoOneVision.Camera
{
	public class USBCamera : ICamera
	{
		// Matrox SDK 관련 필드
		private MIL_ID MilSystem = MIL.M_NULL;
		//private MIL_ID MilImage = MIL.M_NULL;

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
				Debug.WriteLine( "카메라를 열 수 없습니다." );
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

		}

		public byte[ ] CaptureImage( )
		{
			// 이미지 캡처
			if (capture != null)
			{
				capture.Read( frame );
				if (frame.Empty())
				{
					Debug.WriteLine( "프레임을 읽을 수 없습니다." );
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
			//Debug.WriteLine( "" );
			MIL_ID MilImageLocal = MIL.M_NULL;
			byte[] imageData = null;

			try
			{
				MILContext.Width = mat.Width;
				MILContext.Height = mat.Height;
				MILContext.NbBands = mat.Channels();
				MILContext.DataType = 8;

				Cv2.CvtColor( mat, mat, ColorConversionCodes.BGR2RGB );

				// 픽셀 포맷 및 속성 설정
				MIL_INT attribute = MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED;

				// MIL 버퍼 할당
				//TODO 필요한 구문이 아닌 것 같으니 최적화 대상
				if(MILContext.NbBands == 1)
				{
					//gray

				}
				else if(MILContext.NbBands == 3)
				{
					//color
					attribute += MIL.M_RGB24;
				}
				else
				{
					Debug.WriteLine( "지원하지 않는 채널 수입니다." );
					return null;
				}
				MIL.MbufAllocColor( MilSystem, MILContext.NbBands, MILContext.Width, MILContext.Height, MILContext.DataType, MIL.M_IMAGE + MIL.M_PROC, ref MilImageLocal );

				// OpenCV Mat 데이터가 연속적인지 확인
				if (!mat.IsContinuous())
				{
					mat = mat.Clone();
				}

				//MIL.MbufPut( MilImageLocal, matData );


				// OpenCV Mat 데이터를 바이트 배열로 가져오기
				int bufferSize = mat.Width * mat.Height * mat.Channels();
				byte[] matData = new byte[bufferSize];
				Marshal.Copy( mat.Data, matData, 0, bufferSize );

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
	}
}
