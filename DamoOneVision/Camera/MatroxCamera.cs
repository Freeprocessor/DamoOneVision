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

			ushort[] ushortimageData = new ushort[ MILContext.Width * MILContext.Height ];

			// 이미지 데이터 가져오기
			MIL.MbufGet( MilImage, ushortimageData );


			//ushort 데이터에서 최대, 최소값을 추출 
			ushort MinPixelValue = ushortimageData.Min();
			ushort MaxPixelValue = ushortimageData.Max();
			//double ScaleFactor =  65535.0 * (MaxPixelValue - MinPixelValue);

			//최대, 최소값 기준으로 데이터값을 0~65535로 스케일링
			for (int i=0; i < ushortimageData.Length; i++)
			{
				ushortimageData[ i ] = (ushort) (((double) (ushortimageData[ i ] - MinPixelValue) / (double) (MaxPixelValue - MinPixelValue)) * 65535);
			}

			

			

			MIL_ID MilImageScale = MIL.M_NULL;
			// ushort 데이터를 할당할 버퍼	생성
			MilImageScale = MIL.MbufAllocColor( MilSystem, 1, MILContext.Width, MILContext.Height,
					16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );

			// ushort 데이터를 버퍼에 쓰기
			MIL.MbufPut( MilImageScale, ushortimageData );

			MIL_INT SizeByte = 0;
			//버퍼에 쓴 ushort 데이터를 byte로 변환
			MIL.MbufInquire( MilImageScale, MIL.M_SIZE_BYTE, ref SizeByte );
			byte[] imageData = new byte[SizeByte];
			
			MIL.MbufGet( MilImageScale, imageData );


			MIL_INT offsetX = 100;
			MIL_INT offsetY = 50;
			MIL_INT width = 200;
			MIL_INT height = 150;

			MIL_ID MilChildImage = MIL.M_NULL;
			MIL.MbufChild2d( MilImageScale, offsetX, offsetY, width, height, ref MilChildImage );

			//템플릿 저장 테스트 코드(템플릿을 크롭했으므로 주석처리함)
			//string filePath = @".\cropped_image.tif";
			//MIL.MbufExport( filePath, MIL.M_TIFF, MilChildImage );

			MIL.MbufFree( MilChildImage );

			//Template Matching 테스트 코드
			//위에 주석문에서 저장한 이미지를 다시 불러옴
			MIL_ID MilTemplateImage = MIL.M_NULL;
			string templateImagePath = @".\cropped_image.tif";
			MIL.MbufRestore( templateImagePath, MilSystem, ref MilTemplateImage );


			MIL_ID MilMatchContext = MIL.M_NULL;
			MIL_ID MilMatchResult = MIL.M_NULL;
			MIL.MpatAlloc( MilSystem, MIL.M_NORMALIZED, MIL.M_DEFAULT, ref MilMatchContext );
			MIL.MpatAllocResult( MilSystem, MIL.M_DEFAULT, ref MilMatchResult );

			// 템플릿 학습
			MIL.MpatDefine( MilMatchContext, MIL.M_REGULAR_MODEL, MilTemplateImage, 0, 0, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_NULL );
			MIL.MpatPreprocModel( MilMatchContext, MIL.M_DEFAULT );



			MIL.MbufFree( MilImageScale );



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
