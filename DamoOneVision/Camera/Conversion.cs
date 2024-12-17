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
		private static MIL_ID BinarizedImage = MIL.M_NULL;

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




		public static MIL_ID InfraredCameraModel( MIL_ID InfraredCameraImage, ref bool isGood, int threshold)
		{

			if(BinarizedImage != MIL.M_NULL)
			{
				MIL.MbufFree( BinarizedImage );
				BinarizedImage = MIL.M_NULL;
			}

			MIL_ID CircleMeasMarker = MIL.M_NULL;
			MIL_ID BlobResult = MIL.M_NULL;
			MIL_ID BlobContext = MIL.M_NULL;

			double Radius = 0;
			double XPos = 0;
			double YPos = 0;
			double Number = 0;

			int holenumber = 0;

			//MIL.MbufAllocColor( MilSystem, MILContext.NbBands, MILContext.Width, MILContext.Height,MILContext.DataType, MIL.M_IMAGE + MIL.M_PROC, ref MilImage );
			//MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height, 8, MIL.M_IMAGE + MIL.M_PROC, ref MilColorImage );
			//MIL.MbufAllocColor( MilSystem, 1, MILContext.Width, MILContext.Height, 8, MIL.M_IMAGE + MIL.M_PROC, ref Mil8bitImage );

			MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BinarizedImage );
			//MIL.MbufAllocColor( MilSystem, 1, MILContext.Width, MILContext.Height, 16, MIL.M_IMAGE + MIL.M_PROC, ref BinarizedImage );

			//MIL.MbufPut( MilImage, imageData );
			//16bit 이미지를 8bit Color 이미지로 변환
			//MIL.MimConvert( MilImage, MilColorImage, MIL.M_L_TO_RGB );
			////8bit Color 이미지를 8bit Gray 이미지로 변환
			//MIL.MimConvert( MilColorImage, Mil8bitImage, MIL.M_RGB_TO_L );

			//MIL.MimBinarize( Mil8bitImage, BinarizedImage, MIL.M_GREATER, 90, MIL.M_NULL );
			MIL.MimBinarize( InfraredCameraImage, BinarizedImage, MIL.M_GREATER, threshold, MIL.M_NULL );

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
			try
			{
				MIL.MblobGetResult( BlobResult, MIL.M_BLOB_INDEX( 0 ), MIL.M_NUMBER_OF_HOLES + MIL.M_TYPE_MIL_INT, ref holenumber );
			}
			catch (Exception ex)
			{
				Debug.WriteLine( $"MblobGetResult에서 예외 발생: {ex.Message}" );
			}


			if (holenumber == 1)
			{
				isGood = true;
			}
			else
			{
				isGood = false;
			}

			//MIL.MbufPut( BinarizedImage, imageData );
			//MIL.MbufGet( BinarizedImage, imageData );
			//MIL.MbufClone( BinarizedImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref InfraredCameraConversionImage );
			MIL.MmeasFree( CircleMeasMarker );
			MIL.MblobFree( BlobResult );
			MIL.MblobFree( BlobContext );
			//MIL.MblobFree( BlobResult );
			//MIL.MbufFree( BinarizedImage );

			//BinarizedImage = MIL.M_NULL;
			//MIL.MbufFree( CircleMeasMarker );

			return BinarizedImage;
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
