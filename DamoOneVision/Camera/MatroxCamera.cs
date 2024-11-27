using DamoOneVision.Data;
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

			// 버퍼이미지를 Scale히여 16bit 이미지로 변환
			ushort [] ushortScaleImageData = MilImageShortScale(MilImage);


			// Scale된 이미지 데이터 Buffer에 전송
			MIL.MbufPut( MilImage, ushortScaleImageData );


			// Scale된 이미지 데이터를 byte로 변환
			// TODO : 어떻게 사용할건지 확인해야함
			byte [] byteImageData = ShortToByte(ushortScaleImageData);


			MIL_INT SizeByte = 0;
			//버퍼에 쓴 Scale 데이터를 byte로 변환
			MIL.MbufInquire( MilImage, MIL.M_SIZE_BYTE, ref SizeByte );
			byte[] imageData = new byte[SizeByte];
			
			MIL.MbufGet( MilImage, imageData );


			/*//------------------------------------------------------------------------------------------------

			//MIL_INT offsetX = 100;
			//MIL_INT offsetY = 50;
			//MIL_INT width = 200;
			//MIL_INT height = 150;

			//MIL_ID MilChildImage = MIL.M_NULL;
			//MIL.MbufChild2d( MilImageScale, offsetX, offsetY, width, height, ref MilChildImage );

			//템플릿 저장 테스트 코드(템플릿을 크롭했으므로 주석처리함)
			//string filePath = @".\cropped_image.tif";
			//MIL.MbufExport( filePath, MIL.M_TIFF, MilImage8bitScale );

			//MIL.MbufFree( MilChildImage );

			//------------------------------------------------------------------------------------------------*/
			/*
			//Template Matching 테스트 코드
			//위에 주석문에서 저장한 이미지를 다시 불러옴


			//MIL_ID MilTemplateImage = MIL.M_NULL;
			//string templateImagePath = @".\cropped_image.tif";
			//MIL.MbufRestore( templateImagePath, MilSystem, ref MilTemplateImage );


			//MIL_ID MilMatchContext = MIL.M_NULL;
			//MIL_ID MilMatchResult = MIL.M_NULL;
			//MIL.MpatAlloc( MilSystem, MIL.M_NORMALIZED, MIL.M_DEFAULT, ref MilMatchContext );
			//MIL.MpatAllocResult( MilSystem, MIL.M_DEFAULT, ref MilMatchResult );

			//// 템플릿 정의
			//MIL.MpatDefine(
			//	MilMatchContext,
			//	MIL.M_REGULAR_MODEL,
			//	MilTemplateImage,
			//	0.0, // OffX
			//	0.0, // OffY
			//	0.0, // SizeX (0.0이면 전체 이미지 사용)
			//	0.0, // SizeY (0.0이면 전체 이미지 사용)
			//	MIL.M_DEFAULT // ControlFlag
			//);
			//// 템플릿 전처리 (수정된 부분)
			//MIL.MpatPreprocess( MilSystem, MilMatchContext, MIL.M_DEFAULT );

			//MIL.MpatFind( MilMatchContext, MilImageScale, MilMatchResult );


			//MIL_INT numOccurrences = 0;
			//MIL.MpatGetResult( MilMatchResult, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref numOccurrences );

			//MIL.MbufFree( MilImageScale );

			//if (numOccurrences > 0)
			//{
			//	// 그래픽 디스플레이 초기화
			//	MIL_ID MilDisplay = MIL.M_NULL;
			//	MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WINDOWED, ref MilDisplay );
			//	MIL.MdispSelect( MilDisplay, MilImageScale );

			//	// 오버레이 활성화
			//	MIL.MdispControl( MilDisplay, MIL.M_OVERLAY, MIL.M_ENABLE );
			//	MIL_ID MilOverlayImage = MIL.M_NULL;
			//	MIL.MdispInquire( MilDisplay, MIL.M_OVERLAY_ID, ref MilOverlayImage );

			//	// 그래픽 설정
			//	MIL.MgraColor( MIL.M_DEFAULT, MIL.M_COLOR_GREEN );

			//	// 템플릿 크기 가져오기
			//	MIL_INT templateWidth = 0, templateHeight = 0;
			//	MIL.MbufInquire( MilTemplateImage, MIL.M_SIZE_X, ref templateWidth );
			//	MIL.MbufInquire( MilTemplateImage, MIL.M_SIZE_Y, ref templateHeight );

			//	// 매칭된 모든 위치에 사각형 그리기
			//	for (MIL_INT i = 0; i < numOccurrences; i++)
			//	{
			//		double posX = 0, posY = 0;
			//		MIL.MpatGetResult( MilMatchResult, MIL.M_POSITION_X + i, ref posX );
			//		MIL.MpatGetResult( MilMatchResult, MIL.M_POSITION_Y + i, ref posY );

			//		// 매칭된 위치에 사각형 그리기
			//		MIL.MgraRect( MIL.M_DEFAULT, MilOverlayImage,
			//			posX, posY,
			//			posX + templateWidth, posY + templateHeight );
			//	}
			//}
			//else
			//{
			//	Console.WriteLine( "매칭된 템플릿이 없습니다." );
			//}

			*/

			return imageData;
		}

		private ushort[ ] MilImageShortScale( MIL_ID MilImage )
		{
			ushort [] ImageData = new ushort[ MILContext.Width * MILContext.Height ];

			MIL.MbufGet( MilImage, ImageData );

			//var distinctNumbersDesc = ImageData.Distinct().OrderByDescending( x => x ).ToArray();

			//ImageData[ 0 ] = distinctNumbersDesc[ 1 ];

			ushort MinPixelValue = ImageData.Min();
			ushort MaxPixelValue = ImageData.Max();


			for (int i = 0; i < ImageData.Length; i++)
			{
				ImageData[ i ] = (ushort) (((double) (ImageData[ i ] - MinPixelValue) / (double) (MaxPixelValue - MinPixelValue)) * 65535);
			}

			return ImageData;
		}

		private byte[ ] ShortToByte( ushort[ ] ushortImageData )
		{
			byte [] ImageData = new byte[ MILContext.Width * MILContext.Height ];

			for (int i = 0; i < ImageData.Length; i++)
			{
				ImageData[ i ] = (byte) ((double) ((double) ushortImageData[ i ] / 65535) * 255);
			}


			return ImageData;
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
