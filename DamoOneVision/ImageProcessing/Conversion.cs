﻿using DamoOneVision.Camera;
using DamoOneVision.Data;
using DamoOneVision.Services;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DamoOneVision.ImageProcessing
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


		public static async Task SideCameraModel( MIL_ID SideCameraImage, MIL_ID SideCameraDisplay )
		{

			string appFolder = string.Empty;
			string SideRightImage = string.Empty;
			string SideLeftImage = string.Empty;
			string localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			appFolder = System.IO.Path.Combine( localAppData, "DamoOneVision" );
			SideRightImage = System.IO.Path.Combine( appFolder, "SideRight.mim" );
			SideLeftImage = System.IO.Path.Combine( appFolder, "SideLeft.mim" );

			MIL_ID PatContext = MIL.M_NULL;
			MIL_ID PatResult = MIL.M_NULL;
			MIL_ID MilOverlayImage = MIL.M_NULL;


			MIL.MdispInquire( SideCameraDisplay, MIL.M_OVERLAY_ID, ref MilOverlayImage );
			MIL.MdispControl( SideCameraDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_DEFAULT );

			//MIL.MbufClear( MilOverlayImage, 0 );

			double posx1 = 0;
			double posy1 = 0;
			double posx2 = 0;
			double posy2 = 0;

			if (BinarizedImage != MIL.M_NULL)
			{
				MIL.MbufFree( BinarizedImage );
				BinarizedImage = MIL.M_NULL;
			}

			MIL_ID SideRight = MIL.M_NULL;
			MIL_ID SideLeft = MIL.M_NULL;

			MIL.MbufImport( SideRightImage, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MilSystem, ref SideRight );
			MIL.MbufImport( SideLeftImage, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MilSystem, ref SideLeft );

			MIL.MpatAlloc( MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref PatContext );
			MIL.MpatAllocResult( MilSystem, MIL.M_DEFAULT, ref PatResult );



			MIL.MpatDefine( PatContext, MIL.M_REGULAR_MODEL, SideRight, 0.0, 0.0, 30.0, 20.0, MIL.M_DEFAULT );
			MIL.MpatDefine( PatContext, MIL.M_REGULAR_MODEL, SideLeft, 0.0, 0.0, 30.0, 20.0, MIL.M_DEFAULT );

			// Control Block for Pat Context
			MIL.MpatControl( PatContext, 0, MIL.M_REFERENCE_X, 6.984375 );
			MIL.MpatControl( PatContext, 0, MIL.M_REFERENCE_Y, 13.1875 );
			MIL.MpatControl( PatContext, 1, MIL.M_REFERENCE_X, 22.875 );
			MIL.MpatControl( PatContext, 1, MIL.M_REFERENCE_Y, 14.96875 );


			MIL.MpatPreprocess( PatContext, MIL.M_DEFAULT, MIL.M_NULL );
			MIL.MpatFind( PatContext, SideCameraImage, PatResult );

			MIL.MpatGetResult( PatResult, 0, MIL.M_POSITION_X + MIL.M_TYPE_MIL_DOUBLE, ref posx1 );
			MIL.MpatGetResult( PatResult, 0, MIL.M_POSITION_Y + MIL.M_TYPE_MIL_DOUBLE, ref posy1 );
			MIL.MpatGetResult( PatResult, 1, MIL.M_POSITION_X + MIL.M_TYPE_MIL_DOUBLE, ref posx2 );
			MIL.MpatGetResult( PatResult, 1, MIL.M_POSITION_Y + MIL.M_TYPE_MIL_DOUBLE, ref posy2 );

			///예외처리 필요

			//Logger.WriteLine( $"posx1:{posx1}, posy1:{posy1}, posx2:{posx2}, posy2:{posy2}" );

			MIL.MgraColor( MIL.M_DEFAULT, MIL.M_COLOR_GREEN );
			MIL.MpatDraw( MIL.M_DEFAULT, PatResult, MilOverlayImage, MIL.M_DRAW_POSITION + MIL.M_DRAW_BOX, MIL.M_ALL, MIL.M_DEFAULT );
			MIL.MgraLine( MIL.M_DEFAULT, MilOverlayImage, posx1, posy1, posx2, posy2 );



			MIL.MpatFree( PatResult );
			MIL.MpatFree( PatContext );

			MIL.MbufFree( SideRight );
			MIL.MbufFree( SideLeft );

			//MIL.MbufFree( MilOverlayImage );



			//return SideCameraImage;
		}



		public static MIL_ID InfraredCameraModel( MIL_ID InfraredCameraImage, ref bool isGood, InfraredCameraModel infraredCameraModels )
		{

			if (BinarizedImage != MIL.M_NULL)
			{
				MIL.MbufFree( BinarizedImage );
				BinarizedImage = MIL.M_NULL;
			}

			MIL_ID CircleMeasMarker = MIL.M_NULL;
			MIL_ID BlobResult = MIL.M_NULL;
			MIL_ID BlobContext = MIL.M_NULL;
			MIL_ID GraphicsContext = MIL.M_NULL;

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
			MIL.MimBinarize( InfraredCameraImage, BinarizedImage, MIL.M_GREATER, infraredCameraModels.BinarizedThreshold, MIL.M_NULL );

			///

			MIL.MmeasAllocMarker( MilSystem, MIL.M_CIRCLE, MIL.M_DEFAULT, ref CircleMeasMarker );

			MIL.MmeasSetMarker( CircleMeasMarker, MIL.M_SEARCH_REGION_INPUT_UNITS, MIL.M_PIXEL, MIL.M_NULL );
			MIL.MmeasSetMarker( CircleMeasMarker, MIL.M_RING_CENTER, infraredCameraModels.CircleCenterX, infraredCameraModels.CircleCenterY );
			MIL.MmeasSetMarker( CircleMeasMarker, MIL.M_RING_RADII, infraredCameraModels.CircleMinRadius, infraredCameraModels.CircleMaxRadius );

			MIL.MmeasFindMarker( MIL.M_DEFAULT, BinarizedImage, CircleMeasMarker, MIL.M_DEFAULT );

			MIL.MmeasGetResult( CircleMeasMarker, MIL.M_RADIUS, ref Radius, nint.Zero );
			MIL.MmeasGetResult( CircleMeasMarker, MIL.M_POSITION, ref XPos, ref YPos );
			MIL.MmeasGetResult( CircleMeasMarker, MIL.M_NUMBER, ref Number, nint.Zero );

			Logger.WriteLine( $"Radius: {Radius}, XPos: {XPos}, YPos: {YPos}, Number: {Number}" );


			///
			MIL.MgraAlloc( MilSystem, ref GraphicsContext );

			MIL.MblobAllocResult( MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobResult );

			MIL.MblobAlloc( MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobContext );

			MIL.MblobControl( BlobContext, MIL.M_BOX, MIL.M_ENABLE );
			MIL.MblobControl( BlobContext, MIL.M_NUMBER_OF_HOLES, MIL.M_ENABLE );

			MIL.MblobCalculate( BlobContext, BinarizedImage, MIL.M_NULL, BlobResult );

			MIL_INT selectedBlobCount = 0;

			MIL.MblobGetResult( BlobContext, BlobResult, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref selectedBlobCount );

			Logger.WriteLine( $"Blob Number: {selectedBlobCount}" );

			for (MIL_INT i = 0; i < selectedBlobCount; i++)
			{
				MIL_INT blobIndex = 0;

				// M_BLOB_INDEX 속성 가져오기
				// 블롭 인덱스는 보통 1부터 시작
				MIL.MblobGetResult( BlobContext, BlobResult, MIL.M_BLOB_INDEX( i ), ref blobIndex );

				Logger.WriteLine( "블롭 {i}}의 인덱스: {blobIndex}\n" );
			}



			MIL.MblobSelect( BlobContext, BlobResult, BlobResult, MIL.M_SIZE_X, MIL.M_GREATER_OR_EQUAL, Radius * 2 - 20 );
			MIL.MblobSelect( BlobContext, BlobResult, BlobResult, MIL.M_SIZE_X, MIL.M_LESS_OR_EQUAL, Radius * 2 + 20 );



			MIL.MblobDraw( GraphicsContext, BlobResult, BinarizedImage, MIL.M_DRAW_BOX, MIL.M_DEFAULT, MIL.M_DEFAULT );

			//MIL.MblobGetResult( BlobResult, MIL.M_DEFAULT, MIL.M_NUMBER_OF_HOLES , ref holenumber );
			//MIL.MblobGetResult( BlobResult, M_GENERAL, M_NUMBER + M_TYPE_MIL_INT, &Number0 );
			//MIL.MblobGetResult( BlobResult, MIL.M_GENERAL, MIL.M_NUMBER_OF_HOLES + MIL.M_TYPE_MIL_INT, ref holenumber );

			try
			{
				MIL.MblobGetResult( BlobResult, MIL.M_BLOB_INDEX( 1 ), MIL.M_NUMBER_OF_HOLES + MIL.M_TYPE_MIL_INT, ref holenumber );
			}
			catch (Exception ex)
			{
				Logger.WriteLine( $"MblobGetResult에서 예외 발생: {ex.Message}" );
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
			MIL.MgraFree( GraphicsContext );
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
					Logger.WriteLine( "milImage가 유효하지 않습니다." );
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
				Logger.WriteLine( $"ExtractPixelData에서 예외 발생: {ex.Message}" );
				throw;
			}
		}

	}


}