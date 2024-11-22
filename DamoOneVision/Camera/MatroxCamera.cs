using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace DamoOneVision.Camera
{
	public class MatroxCamera : ICamera
	{
		// Matrox SDK 관련 필드
		private MIL_ID MilSystem = MIL.M_NULL;
		private MIL_ID MilDigitizer = MIL.M_NULL;
		private MIL_ID MilImage = MIL.M_NULL;



		public bool Connect( )
		{
			// MILContext에서 MilSystem 가져오기
			MilSystem = MILContext.Instance.MilSystem;

			// 디지타이저(카메라) 할당
			MIL.MdigAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref MilDigitizer );

			return MilDigitizer != MIL.M_NULL;
		}

		public void Disconnect( )
		{
			if (MilDigitizer != MIL.M_NULL)
			{
				MIL.MdigFree( MilDigitizer );
				MilDigitizer = MIL.M_NULL;
			}

			if (MilImage != MIL.M_NULL)
			{
				MIL.MbufFree( MilImage );
				MilImage = MIL.M_NULL;
			}

		}

		public byte[ ] CaptureImage( )
		{
			// 이미지 버퍼 할당
			if (MilImage == MIL.M_NULL)
			{
				// 이미지 크기 및 속성 가져오기
				MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_X, ref MILContext.Width );
				MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_Y, ref MILContext.Height );
				MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_BAND, ref MILContext.NbBands );
				MIL.MdigInquire( MilDigitizer, MIL.M_TYPE, ref MILContext.DataType );

				// 원하는 프레임 레이트 설정 (예: 30fps)
				//double desiredFrameRate = 30.0;

				// 디지타이저의 프레임 레이트를 설정
				//MIL.MdigControl( MilDigitizer, MIL.M_GRAB_FRAME_RATE, desiredFrameRate );

				// 프로그레시브 모드 설정
				//MIL.MdigControl(MilDigitizer, MIL.M_GRAB_FIELD_NUM, 1);

				// 이미지 버퍼 할당
				MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height, MILContext.DataType, MIL.M_IMAGE + MIL.M_GRAB, ref MilImage );

			}

			// 이미지 캡처
			MIL.MdigGrab( MilDigitizer, MilImage );


			ushort[] imageData = new ushort[ MILContext.Width * MILContext.Height ];
			MIL.MbufGet2d( MilImage, 0, 0, MILContext.Width, MILContext.Height, imageData );

			ushort MinPixelValue = imageData.Min();
			ushort MaxPixelValue = imageData.Max();

			// 온도 범위 정의
			double MinTemp = -20.0;
			double MaxTemp = 175.0;
			MIL_ID lutId = MIL.MbufAlloc1d(MIL.M_DEFAULT_HOST, 65536, 16 + MIL.M_UNSIGNED, MIL.M_LUT, MIL.M_NULL);



			//// 픽셀 값을 온도 값으로 매핑하기 위한 스케일링 인자 계산
			//double ScaleFactor = (MaxTemp - MinTemp) / (MaxPixelValue - MinPixelValue);
			//double Offset = MinTemp - (MinPixelValue * ScaleFactor);

			//// 픽셀 값을 온도 값으로 변환하여 8비트로 매핑
			//byte[] imageData8Bit = new byte[MILContext.Width * MILContext.Height];
			//for (int i = 0; i < imageData.Length; i++)
			//{
			//	double tempValue = imageData[i] * ScaleFactor + Offset;
			//	// 온도 값을 0~255 범위로 스케일링
			//	double scaledValue = ((tempValue - MinTemp) / (MaxTemp - MinTemp)) * 255.0;
			//	// 범위를 벗어나는 값 클리핑
			//	if (scaledValue < 0) scaledValue = 0;
			//	if (scaledValue > 255) scaledValue = 255;
			//	imageData8Bit[ i ] = (byte) scaledValue;
			//}







			//MIL.MimClip( MilImage, MilImage, MIL.M_INSIDE, MinTemp, MaxTemp, 0.0, 0.0 );
			//MIL.MimArith( MilImage, MinTemp, MilImage, MIL.M_SUB_CONST );
			//double scale = 255.0 / (MaxTemp - MinTemp);
			//MIL.MimArith( MilImage, scale, MilImage, MIL.M_MULT_CONST );


			//// 컬러맵 생성 및 적용 (Jet 컬러맵 사용)
			//MIL_ID MilLut = MIL.M_NULL;
			//MIL.MbufAllocColor( MilSystem, 3, 256, 1, 8 + MIL.M_UNSIGNED, MIL.M_LUT, ref MilLut );

			//// Jet 컬러 맵 생성
			//byte[] jetR = new byte[256];
			//byte[] jetG = new byte[256];
			//byte[] jetB = new byte[256];
			//for (int i = 0; i < 256; i++)
			//{
			//	double value = i / 255.0;
			//	jetR[ i ] = (byte) (255 * Math.Clamp( 1.5 - Math.Abs( 4 * (value - 0.75) ), 0, 1 ));
			//	jetG[ i ] = (byte) (255 * Math.Clamp( 1.5 - Math.Abs( 4 * (value - 0.5) ), 0, 1 ));
			//	jetB[ i ] = (byte) (255 * Math.Clamp( 1.5 - Math.Abs( 4 * (value - 0.25) ), 0, 1 ));
			//}

			//// LUT에 컬러 맵 데이터 설정
			//MIL.MbufPutColor( MilLut, MIL.M_PLANAR, MIL.M_RED, jetR );
			//MIL.MbufPutColor( MilLut, MIL.M_PLANAR, MIL.M_GREEN, jetG );
			//MIL.MbufPutColor( MilLut, MIL.M_PLANAR, MIL.M_BLUE, jetB );

			//// LUT 매핑 적용
			//MIL.MimLutMap( MilGrayImage, MilColorImage, MilLut, MIL.M_DEFAULT );


			// 컬러 이미지 데이터를 바이트 배열로 가져오기 (BGR24 포맷)
			byte[] colorImageData = new byte[MILContext.Width * MILContext.Height * 3];
			MIL.MbufGetColor( MilImageColor, MIL.M_PACKED + MIL.M_BGR24, MIL.M_ALL_BANDS, colorImageData );

			// 이미지 데이터 가져오기
			//MIL_INT SizeByte = 0;

			//MIL.MbufInquire( MilImage8Bit, MIL.M_SIZE_BYTE, ref SizeByte );

			//byte[] imageData = new byte[SizeByte];

			//MIL.MbufGet( MilImage8Bit, imageData );

			return colorImageData;
		}

		public int GetWidth( )
		{
			return (int) MIL.MbufInquire( MilImage, MIL.M_SIZE_X, MIL.M_NULL );
		}

		public int GetHeight( )
		{
			return (int) MIL.MbufInquire( MilImage, MIL.M_SIZE_Y, MIL.M_NULL );
		}
		public int GetNbBands( )
		{
			return (int) MIL.MbufInquire( MilImage, MIL.M_SIZE_BAND, MIL.M_NULL );
		}
		public int GetDataType( )
		{
			return (int) MIL.MbufInquire( MilImage, MIL.M_TYPE, MIL.M_NULL );
		}
	}
}
