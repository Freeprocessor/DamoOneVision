using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

				// 이미지 버퍼 할당
				MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height, MILContext.DataType, MIL.M_IMAGE + MIL.M_GRAB, ref MilImage );
			}


			// 이미지 캡처
			MIL.MdigGrab( MilDigitizer, MilImage );

			// 이미지 데이터 가져오기
			MIL_INT SizeByte = 0;
			MIL.MbufInquire( MilImage, MIL.M_SIZE_BYTE, ref SizeByte );
			byte[] imageData = new byte[SizeByte];
			MIL.MbufGet( MilImage, imageData );

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
