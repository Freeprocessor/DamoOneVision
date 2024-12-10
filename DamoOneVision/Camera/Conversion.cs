using DamoOneVision.Data;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DamoOneVision.Camera
{

	internal static class Conversion
	{

		private static MIL_ID MilSystem = MIL.M_NULL;


		// 이미지 처리 완료 시 발생하는 이벤트
		public static event EventHandler<ImageProcessedEventArgs> ImageProcessed;

		static Conversion( )
		{

			MilSystem = MILContext.Instance.MilSystem;

		}


		public static void OnImageProcessed( byte[ ] processedPixelData )
		{
			ImageProcessed?.Invoke( null, new ImageProcessedEventArgs( processedPixelData ) );
		}

		public static void RunHSLThreshold( double hMin, double hMax, double sMin, double sMax, double lMin, double lMax, byte[ ] pixelData )
		{
			MIL_ID MilImageRGB = MIL.M_NULL;
			MIL_ID MilImageHSL = MIL.M_NULL;
			MIL_ID MilImageBin = MIL.M_NULL;

			MIL_ID MilImageH = MIL.M_NULL;
			MIL_ID MilImageS = MIL.M_NULL;
			MIL_ID MilImageL = MIL.M_NULL;

			MIL_ID MilBinH = MIL.M_NULL;
			MIL_ID MilBinS = MIL.M_NULL;
			MIL_ID MilBinL = MIL.M_NULL;
			MIL_ID MilBinHS = MIL.M_NULL;

			MIL_ID MilMaskRGB = MIL.M_NULL;
			MIL_ID MilImageMasked = MIL.M_NULL;

			try
			{
				Debug.WriteLine( "RunHSLThreshold 시작" );

				// RGB 이미지 버퍼 할당 및 데이터 설정
				MilImageRGB = MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height,
					8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );

				if (MilImageRGB == MIL.M_NULL)
				{
					Debug.WriteLine( "MilImageRGB 할당 실패" );
					return;
				}
				MIL.MbufPut( MilImageRGB, pixelData );

				Debug.WriteLine( "RGB 이미지 버퍼 할당 및 데이터 설정 완료" );

				// HSL 이미지 버퍼 할당
				MilImageHSL = MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height,
					8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );

				// 색상 공간 변환 (RGB -> HSL)
				MIL.MimConvert( MilImageRGB, MilImageHSL, MIL.M_RGB_TO_HSL );
				Debug.WriteLine( "RGB -> HSL 변환 완료" );

				// HSL 채널 분리
				MIL.MbufChildColor2d( MilImageHSL, MIL.M_HUE, 0, 0, MILContext.Width, MILContext.Height, ref MilImageH );
				MIL.MbufChildColor2d( MilImageHSL, MIL.M_SATURATION, 0, 0, MILContext.Width, MILContext.Height, ref MilImageS );
				MIL.MbufChildColor2d( MilImageHSL, MIL.M_LUMINANCE, 0, 0, MILContext.Width, MILContext.Height, ref MilImageL );

				// 각 채널 이진화
				MilBinH = MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height,
					8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
				MilBinS = MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height,
					8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
				MilBinL = MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height,
					8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );

				MIL.MimBinarize( MilImageH, MilBinH, MIL.M_IN_RANGE, hMin, hMax );
				MIL.MimBinarize( MilImageS, MilBinS, MIL.M_IN_RANGE, sMin, sMax );
				MIL.MimBinarize( MilImageL, MilBinL, MIL.M_IN_RANGE, lMin, lMax );

				// 자식 버퍼 해제
				MIL.MbufFree( MilImageH );
				MIL.MbufFree( MilImageS );
				MIL.MbufFree( MilImageL );

				// 이진화된 결과들을 논리적 AND 연산으로 결합
				MilBinHS = MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height,
					8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
				MIL.MimArith( MilBinH, MilBinS, MilBinHS, MIL.M_AND );
				MIL.MbufFree( MilBinH );
				MIL.MbufFree( MilBinS );

				MilImageBin = MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height,
					8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
				MIL.MimArith( MilBinHS, MilBinL, MilImageBin, MIL.M_AND );
				MIL.MbufFree( MilBinHS );
				MIL.MbufFree( MilBinL );

				Debug.WriteLine( "HSL 채널 이진화 및 결합 완료" );

				// 이진화된 마스크를 0과 1로 정규화
				MIL.MimBinarize( MilImageBin, MilImageBin, MIL.M_NOT_EQUAL, 0, MIL.M_NULL );

				// 마스크 이미지를 RGB로 확장
				MilMaskRGB = MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height,
					8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );

				if (MilMaskRGB == MIL.M_NULL)
				{
					Debug.WriteLine( "MilMaskRGB 할당 실패" );
					// 필요한 자원 해제 후 반환
					return;
				}

				// 마스크 이미지를 각 밴드에 복사
				MIL_ID MilMaskBand0 = MIL.MbufChildColor(MilMaskRGB, 0, MIL.M_NULL);
				MIL_ID MilMaskBand1 = MIL.MbufChildColor(MilMaskRGB, 1, MIL.M_NULL);
				MIL_ID MilMaskBand2 = MIL.MbufChildColor(MilMaskRGB, 2, MIL.M_NULL);

				MIL.MbufCopy( MilImageBin, MilMaskBand0 );
				MIL.MbufCopy( MilImageBin, MilMaskBand1 );
				MIL.MbufCopy( MilImageBin, MilMaskBand2 );

				// 자식 버퍼 해제
				MIL.MbufFree( MilMaskBand0 );
				MIL.MbufFree( MilMaskBand1 );
				MIL.MbufFree( MilMaskBand2 );

				// 마스킹된 이미지를 저장할 버퍼 할당
				MilImageMasked = MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height,
					8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );

				if (MilImageMasked == MIL.M_NULL)
				{
					Debug.WriteLine( "MilImageMasked 할당 실패" );
					// 필요한 자원 해제 후 반환
					return;
				}

				// 원본 이미지와 마스크 이미지 곱하기
				//MIL.MimConvert( MilImageRGB, MilImageHSL, MIL.M_RGB_TO_HSL );
				MIL.MimArith( MilImageRGB, MilMaskRGB, MilImageMasked, MIL.M_MULT );
				Debug.WriteLine( "마스킹 작업 완료" );

				// 마스크 RGB 버퍼 해제
				MIL.MbufFree( MilMaskRGB );
				MIL.MbufFree( MilImageBin );

				// 마스킹된 이미지의 픽셀 데이터를 추출
				byte[] maskedPixelData = ExtractPixelData(MilImageMasked, (int)MILContext.Width, (int)MILContext.Height);

				// 필요하다면 픽셀 데이터의 채널 순서 변경 (BGR -> RGB)
				//SwapBlueAndRedChannels( maskedPixelData );

				// 이벤트 발생
				//OnImageProcessed( maskedPixelData, (int) MILContext.Width, (int) MILContext.Height, PixelFormats.Rgb24 );
				Debug.WriteLine( "마스킹된 이미지 처리 이벤트 발생" );

				// 마스킹된 이미지 버퍼 해제
				MIL.MbufFree( MilImageMasked );

				// HSL 이미지 버퍼 해제
				MIL.MbufFree( MilImageHSL );

				// RGB 이미지 버퍼 해제
				MIL.MbufFree( MilImageRGB );
			}
			catch (Exception ex)
			{
				Debug.WriteLine( $"RunHSLThreshold에서 예외 발생: {ex.Message}" );
			}
		}


		public static void RunClip(MIL_ID ClipType, ushort LowerLimit, ushort UpperLimit, ushort WriteLow, ushort WriteHigh, byte[ ]PixelData)
		{

			MIL_ID MilImage = MIL.M_NULL;


			MilImage = MIL.MbufAllocColor( MilSystem, 1, MILContext.Width, MILContext.Height,
					16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
			MIL.MbufPut( MilImage, PixelData );

			try
			{
				MIL.MimClip( MilImage, MilImage, ClipType, LowerLimit, UpperLimit, WriteLow, WriteHigh );
			}
			catch(Exception ex)
			{
				Debug.WriteLine( $"Clip에서 예외 발생: {ex.Message}" );
			}

			MIL.MbufGet( MilImage, PixelData );

			//OnImageProcessed( PixelData, (int) MILContext.Width, (int) MILContext.Height, PixelFormats.Gray16 );

			MIL.MbufFree( MilImage );

			Debug.WriteLine( $"RunClip 동작 완료" );
		}

		public static void PattenMatching(int patXpos, int patYpos, int patWidth, int patHeight)
		{
			MIL_ID PatContext = MIL.M_NULL;
			MIL_ID PatResult = MIL.M_NULL;
			MIL_ID PatModel = MIL.M_NULL;

			MIL_ID MilImage = MIL.M_NULL;

			MIL.MbufImport("저장한 템플릿 파일 위치", MIL.M_BMP, MIL.M_RESTORE, MilSystem, MilImage );

			MIL.MpatAlloc( MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref PatContext );
			MIL.MpatAllocResult( MilSystem, MIL.M_DEFAULT, ref PatResult );

			MIL.MpatDefine( PatContext, MIL.M_REGULAR_MODEL, MilImage , patXpos, patYpos, patWidth, patHeight, MIL.M_DEFAULT);





		}

		public static byte[ ] Model1( byte[ ] imageData, ref bool isGood)
		{
			MIL_ID MilImage = MIL.M_NULL;
			MIL_ID MilColorImage = MIL.M_NULL;
			MIL_ID Mil8bitImage = MIL.M_NULL;
			MIL_ID BinarizedImage = MIL.M_NULL;
			MIL_ID CircleMeasMarker = MIL.M_NULL;
			MIL_ID BlobResult = MIL.M_NULL;
			MIL_ID BlobContext = MIL.M_NULL;

			double Radius = 0;
			double XPos = 0;
			double YPos = 0;
			double Number = 0;

			int holenumber = 0;

			MIL.MbufAllocColor( MilSystem, MILContext.NbBands, MILContext.Width, MILContext.Height,MILContext.DataType, MIL.M_IMAGE + MIL.M_PROC, ref MilImage );
			MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height, 8, MIL.M_IMAGE + MIL.M_PROC, ref MilColorImage );
			MIL.MbufAllocColor( MilSystem, 1, MILContext.Width, MILContext.Height, 8, MIL.M_IMAGE + MIL.M_PROC, ref Mil8bitImage );
			MIL.MbufAllocColor( MilSystem, 1, MILContext.Width, MILContext.Height, 8, MIL.M_IMAGE + MIL.M_PROC, ref BinarizedImage );

			MIL.MbufPut( MilImage, imageData );
			//16bit 이미지를 8bit Color 이미지로 변환
			MIL.MimConvert( MilImage, MilColorImage, MIL.M_L_TO_RGB );
			//8bit Color 이미지를 8bit Gray 이미지로 변환
			MIL.MimConvert( MilColorImage, Mil8bitImage, MIL.M_RGB_TO_L );

			MIL.MimBinarize( Mil8bitImage, BinarizedImage, MIL.M_GREATER, 90, MIL.M_NULL );


			///

			MIL.MmeasAllocMarker( MilSystem, MIL.M_CIRCLE, MIL.M_DEFAULT, ref CircleMeasMarker );

			MIL.MmeasSetMarker( CircleMeasMarker, MIL.M_SEARCH_REGION_INPUT_UNITS, MIL.M_PIXEL, MIL.M_NULL );
			MIL.MmeasSetMarker( CircleMeasMarker, MIL.M_RING_CENTER, 219.0, 184.0 );
			MIL.MmeasSetMarker( CircleMeasMarker, MIL.M_RING_RADII, 115.0, 183.5 );

			MIL.MmeasFindMarker( MIL.M_DEFAULT, BinarizedImage, CircleMeasMarker, MIL.M_DEFAULT );

			MIL.MmeasGetResult( CircleMeasMarker, MIL.M_RADIUS, ref Radius, IntPtr.Zero );
			MIL.MmeasGetResult( CircleMeasMarker, MIL.M_POSITION, ref XPos, ref YPos );	
			MIL.MmeasGetResult( CircleMeasMarker, MIL.M_NUMBER, ref Number, IntPtr.Zero );

			Debug.WriteLine( $"Radius: {Radius}, XPos: {XPos}, YPos: {YPos}, Number: {Number}" );


			///

			MIL.MblobAllocResult( MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobResult );

			MIL.MblobAlloc( MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobContext );

			MIL.MblobControl( BlobContext, MIL.M_BOX, MIL.M_ENABLE );
			MIL.MblobControl( BlobContext, MIL.M_NUMBER_OF_HOLES, MIL.M_ENABLE );

			MIL.MblobCalculate( BlobContext, BinarizedImage, MIL.M_NULL, BlobResult );

			//MIL.MblobGetResult( BlobResult, MIL.M_DEFAULT, MIL.M_NUMBER_OF_HOLES , ref holenumber );
			//MIL.MblobGetResult( BlobResult, M_GENERAL, M_NUMBER + M_TYPE_MIL_INT, &Number0 );
			//MIL.MblobGetResult( BlobResult, MIL.M_GENERAL, MIL.M_NUMBER_OF_HOLES + MIL.M_TYPE_MIL_INT, ref holenumber );
			MIL.MblobGetResult( BlobResult, MIL.M_BLOB_INDEX( 0 ), MIL.M_NUMBER_OF_HOLES + MIL.M_TYPE_MIL_INT, ref holenumber );

			if(holenumber == 1)
			{
				isGood = true;
			}
			else
			{
				isGood = false;
			}

			//MIL.MbufPut( BinarizedImage, imageData );
			MIL.MbufGet( BinarizedImage, imageData );


			MIL.MbufFree( MilImage );
			MIL.MbufFree( MilColorImage );
			MIL.MbufFree( Mil8bitImage );
			MIL.MbufFree( BinarizedImage );
			//MIL.MbufFree( CircleMeasMarker );

			return imageData;
		}



		private static byte[ ] ExtractPixelData( MIL_ID milImage, int width, int height )
		{
			try
			{
				if (milImage == MIL.M_NULL)
				{
					Debug.WriteLine( "milImage가 유효하지 않습니다." );
					throw new ArgumentNullException( nameof( milImage ) );
				}

				// 이미지의 속성 가져오기
				MIL_INT nbBands = 0;
				MIL_INT sizeBit = 0;

				MIL.MbufInquire( milImage, MIL.M_SIZE_BIT, ref sizeBit );
				MIL.MbufInquire( milImage, MIL.M_SIZE_BAND, ref nbBands );

				// 픽셀당 바이트 수 계산
				int bitsPerPixel = (int)sizeBit;
				int bytesPerPixel = bitsPerPixel / 8;

				// 전체 이미지 크기 계산
				int size = width * height * (int)nbBands * bytesPerPixel;

				// 픽셀 데이터 배열 할당
				byte[] pixelData = new byte[size];

				// 이미지 데이터 가져오기
				MIL.MbufGet( milImage, pixelData );

				return pixelData;
			}
			catch (Exception ex)
			{
				Debug.WriteLine( $"ExtractPixelData에서 예외 발생: {ex.Message}" );
				throw;
			}
		}

	}


}
