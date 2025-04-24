using DamoOneVision.Camera;
using DamoOneVision.Models;
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
		const double K = 27315;

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


		public static async Task<bool> SideCameraModel( MIL_ID SideCameraImage, MIL_ID SideCameraDisplay )
		{

			string appFolder = string.Empty;
			string SideRightTopImage = string.Empty;
			string SideLeftTopImage = string.Empty;
			string SideRightBottomImage = string.Empty;
			string SideLeftBottomImage = string.Empty;
			string localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			appFolder = System.IO.Path.Combine( localAppData, "DamoOneVision" );
			SideRightTopImage = System.IO.Path.Combine( appFolder, "SideRightTop.mim" );
			SideLeftTopImage = System.IO.Path.Combine( appFolder, "SideLeftTop.mim" );
			SideRightBottomImage = System.IO.Path.Combine( appFolder, "SideRightBottom.mim" );
			SideLeftBottomImage = System.IO.Path.Combine( appFolder, "SideLeftBottom.mim" );

			MIL_ID PatContext = MIL.M_NULL;
			MIL_ID PatResult = MIL.M_NULL;
			MIL_ID MilOverlayImage = MIL.M_NULL;
			MIL_ID BinarizedImage = MIL.M_NULL;

			MIL.MdispInquire( SideCameraDisplay, MIL.M_OVERLAY_ID, ref MilOverlayImage );
			MIL.MdispControl( SideCameraDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_DEFAULT );

			//MIL.MbufClear( MilOverlayImage, 0 );

			double posx1 = 0;
			double posy1 = 0;
			double posx2 = 0;
			double posy2 = 0;
			double posx3 = 0;
			double posy3 = 0;
			double posx4 = 0;
			double posy4 = 0;

			if (BinarizedImage != MIL.M_NULL)
			{
				MIL.MbufFree( BinarizedImage );
				BinarizedImage = MIL.M_NULL;
			}

			MIL_ID SideRightTop = MIL.M_NULL;
			MIL_ID SideLeftTop = MIL.M_NULL;
			MIL_ID SideRightBottom = MIL.M_NULL;
			MIL_ID SideLeftBottom = MIL.M_NULL;


			MIL.MbufImport( SideRightTopImage, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MilSystem, ref SideRightTop );
			MIL.MbufImport( SideLeftTopImage, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MilSystem, ref SideLeftTop );
			MIL.MbufImport( SideRightBottomImage, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MilSystem, ref SideRightBottom );
			MIL.MbufImport( SideLeftBottomImage, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MilSystem, ref SideLeftBottom );

			MIL.MpatAlloc( MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref PatContext );
			MIL.MpatAllocResult( MilSystem, MIL.M_DEFAULT, ref PatResult );

			MIL.MpatDefine( PatContext, MIL.M_REGULAR_MODEL, SideLeftTop, 0.0, 0.0, 65.0, 45.0, MIL.M_DEFAULT );
			MIL.MpatDefine( PatContext, MIL.M_REGULAR_MODEL, SideRightTop, 0.0, 0.0, 65.0, 45.0, MIL.M_DEFAULT );
			MIL.MpatDefine( PatContext, MIL.M_REGULAR_MODEL, SideLeftBottom, 0.0, 0.0, 65.0, 45.0, MIL.M_DEFAULT );
			MIL.MpatDefine( PatContext, MIL.M_REGULAR_MODEL, SideRightBottom, 0.0, 0.0, 65.0, 45.0, MIL.M_DEFAULT );

			// Control Block for Pat Context
			MIL.MpatControl( PatContext, 0, MIL.M_REFERENCE_X, 12.0 );
			MIL.MpatControl( PatContext, 0, MIL.M_REFERENCE_Y, 23.8125 );
			MIL.MpatControl( PatContext, 1, MIL.M_REFERENCE_X, 51.03125 );
			MIL.MpatControl( PatContext, 1, MIL.M_REFERENCE_Y, 22.9375 );
			MIL.MpatControl( PatContext, 2, MIL.M_REFERENCE_X, 35.875 );
			MIL.MpatControl( PatContext, 2, MIL.M_REFERENCE_Y, 18.125 );
			MIL.MpatControl( PatContext, 3, MIL.M_REFERENCE_X, 29.0625 );
			MIL.MpatControl( PatContext, 3, MIL.M_REFERENCE_Y, 21.5 );



			MIL.MpatPreprocess( PatContext, MIL.M_DEFAULT, MIL.M_NULL );
			MIL.MpatFind( PatContext, SideCameraImage, PatResult );

			MIL.MpatGetResult( PatResult, 0, MIL.M_POSITION_X + MIL.M_TYPE_MIL_DOUBLE, ref posx1 );
			MIL.MpatGetResult( PatResult, 0, MIL.M_POSITION_Y + MIL.M_TYPE_MIL_DOUBLE, ref posy1 );
			MIL.MpatGetResult( PatResult, 1, MIL.M_POSITION_X + MIL.M_TYPE_MIL_DOUBLE, ref posx2 );
			MIL.MpatGetResult( PatResult, 1, MIL.M_POSITION_Y + MIL.M_TYPE_MIL_DOUBLE, ref posy2 );
			MIL.MpatGetResult( PatResult, 2, MIL.M_POSITION_X + MIL.M_TYPE_MIL_DOUBLE, ref posx3 );
			MIL.MpatGetResult( PatResult, 2, MIL.M_POSITION_Y + MIL.M_TYPE_MIL_DOUBLE, ref posy3 );
			MIL.MpatGetResult( PatResult, 3, MIL.M_POSITION_X + MIL.M_TYPE_MIL_DOUBLE, ref posx4 );
			MIL.MpatGetResult( PatResult, 3, MIL.M_POSITION_Y + MIL.M_TYPE_MIL_DOUBLE, ref posy4 );


			///예외처리 필요

			//Logger.WriteLine( $"posx1:{posx1}, posy1:{posy1}, posx2:{posx2}, posy2:{posy2}" );

			MIL.MgraColor( MIL.M_DEFAULT, MIL.M_COLOR_GREEN );

			MIL.MpatDraw( MIL.M_DEFAULT, PatResult, MilOverlayImage, MIL.M_DRAW_POSITION + MIL.M_DRAW_BOX, MIL.M_ALL, MIL.M_DEFAULT );
			MIL.MgraLine( MIL.M_DEFAULT, MilOverlayImage, posx1, posy1, posx2, posy2 );
			MIL.MgraLine( MIL.M_DEFAULT, MilOverlayImage, posx3, posy3, posx4, posy4 );
			//MIL.MgraControl( MIL.M_DEFAULT, MIL.M_LINE_THICKNESS, 2 );


			MIL.MpatFree( PatResult );
			MIL.MpatFree( PatContext );

			MIL.MbufFree( SideRightTop );
			MIL.MbufFree( SideLeftTop );
			MIL.MbufFree( SideRightBottom );
			MIL.MbufFree( SideLeftBottom );

			if((Math.Abs(posy1 - posy2) - Math.Abs( posy3 - posy4 )) > 15)
			{
				return false;
			}
			else
			{
				return true;
			}

			//MIL.MbufFree( MilOverlayImage );



			//return SideCameraImage;
		}

		public static async Task<bool> ColorDetectionModel( )
		{
			bool isGood = false;

			return isGood;
		}



		public static async Task<bool> InfraredCameraModel( bool isSetting, bool isBinarized, MIL_ID BinarizedImage, MIL_ID InfraredCameraScaleImage, MIL_ID InfraredCameraImage, MIL_ID InfraredDisplay, InfraredCameraModel infraredCameraModel )
		{
			if (InfraredCameraImage == MIL.M_NULL)
			{
				Logger.WriteLine( "InfraredCameraImage가 유효하지 않습니다." );
				return false;
			}
			//return true;
			//Logger.WriteLine( "InfraredCameraModel 호출" );
			bool blobGood = false;
			bool circleGood = false;
			bool moonCutGood = false;
			bool isGood = false;
			bool temperatureGood = false;

			//MIL_ID CircleMeasMarker = MIL.M_NULL;
			MIL_ID BlobResult = MIL.M_NULL;
			MIL_ID BlobContext = MIL.M_NULL;
			MIL_ID GraphicsContext = MIL.M_NULL;
			MIL_ID AnnulusContext = MIL.M_NULL;
			MIL_ID SettingGraphicsContext = MIL.M_NULL;
			MIL_ID MeasMarker = MIL.M_NULL;
			MIL_ID MilOverlayImage = MIL.M_NULL;
			MIL_ID MilAnnulusImage = MIL.M_NULL;
			MIL_ID AnnulusAndBinarized = MIL.M_NULL;
			MIL_ID AnnulusAndImage = MIL.M_NULL;

			double ReferenceArea = 0.0;
			double Radius = 0.0;
			double FillRatio = 0;
			double DetectCirclrCenterX = 0.0;
			double DetectCirclrCenterY = 0.0;

			/*
			double Radius = 0;
			double XPos = 0;
			double YPos = 0;
			double Number = 0;

			int holenumber = 0;
			*/
			//if (BinarizedImage != MIL.M_NULL)
			//	MIL.MbufFree( BinarizedImage );
			// 
			MIL.MdispInquire( InfraredDisplay, MIL.M_OVERLAY_ID, ref MilOverlayImage );



			//MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BinarizedImage );
			MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilAnnulusImage );
			MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref AnnulusAndBinarized );
			MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref AnnulusAndImage );

			MIL.MbufClear( MilAnnulusImage, MIL.M_COLOR_BLACK );
			MIL.MbufClear( AnnulusAndBinarized, MIL.M_COLOR_BLACK );
			MIL.MbufClear( AnnulusAndImage, MIL.M_COLOR_BLACK );

			MIL.MimBinarize( InfraredCameraImage, BinarizedImage, MIL.M_GREATER, infraredCameraModel.BinarizedThreshold, MIL.M_NULL );

			///
			/*
			MIL.MmeasAllocMarker( MilSystem, MIL.M_CIRCLE, MIL.M_DEFAULT, ref CircleMeasMarker );

			MIL.MmeasSetMarker( CircleMeasMarker, MIL.M_SEARCH_REGION_INPUT_UNITS, MIL.M_PIXEL, MIL.M_NULL );
			MIL.MmeasSetMarker( CircleMeasMarker, MIL.M_RING_CENTER, infraredCameraModels.CircleCenterX, infraredCameraModels.CircleCenterY );
			MIL.MmeasSetMarker( CircleMeasMarker, MIL.M_RING_RADII, infraredCameraModels.CircleMinRadius, infraredCameraModels.CircleMaxRadius );

			MIL.MmeasFindMarker( MIL.M_DEFAULT, BinarizedImage, CircleMeasMarker, MIL.M_DEFAULT );

			MIL.MmeasGetResult( CircleMeasMarker, MIL.M_RADIUS, ref Radius, nint.Zero );
			MIL.MmeasGetResult( CircleMeasMarker, MIL.M_POSITION, ref XPos, ref YPos );
			MIL.MmeasGetResult( CircleMeasMarker, MIL.M_NUMBER, ref Number, nint.Zero );

			Logger.WriteLine( $"Radius: {Radius}, XPos: {XPos}, YPos: {YPos}, Number: {Number}" );

			*/
			///

			// 그래픽 컨텍스트 생성(디스플레이 오버레이)
			MIL.MgraAlloc( MilSystem, ref GraphicsContext );
			MIL.MgraAlloc( MilSystem, ref AnnulusContext );
			MIL.MgraAlloc( MilSystem, ref SettingGraphicsContext );


			// 원찾기 
			MIL.MmeasAllocMarker( MilSystem, MIL.M_CIRCLE, MIL.M_DEFAULT, ref MeasMarker );

			MIL.MmeasSetMarker( MeasMarker, MIL.M_SEARCH_REGION_INPUT_UNITS, MIL.M_PIXEL, MIL.M_NULL );
			MIL.MmeasSetMarker( MeasMarker, MIL.M_RING_CENTER, infraredCameraModel.CircleCenterX, infraredCameraModel.CircleCenterY );
			MIL.MmeasSetMarker( MeasMarker, MIL.M_RING_RADII, infraredCameraModel.CircleMinRadius, infraredCameraModel.CircleMaxRadius );
			MIL.MmeasFindMarker( MIL.M_DEFAULT, BinarizedImage, MeasMarker, MIL.M_DEFAULT );

			try
			{
				MIL.MmeasGetResult( MeasMarker, MIL.M_RADIUS + MIL.M_TYPE_MIL_DOUBLE, ref Radius, MIL.M_NULL );
				MIL.MmeasGetResultSingle( MeasMarker, MIL.M_POSITION + MIL.M_TYPE_MIL_DOUBLE, ref DetectCirclrCenterX, ref DetectCirclrCenterY, 0 );

				Logger.WriteLine( $"Radius: {Radius}, Circle X : {DetectCirclrCenterX}, Circle Y : {DetectCirclrCenterY}" );
			}
			catch (Exception ex)
			{
				Logger.WriteLine( $"MmeasGetResultSingle에서 예외 발생: {ex.Message}" );
			}

			double smallRadius = Radius - infraredCameraModel.CircleInnerRadius;
			// 도넛모양 도형 생성
			MIL.MgraColor( AnnulusContext, MIL.M_COLOR_WHITE );
			MIL.MgraArcFill( AnnulusContext, MilAnnulusImage, DetectCirclrCenterX, DetectCirclrCenterY, Radius, Radius, -360.0, 360.0 );
			MIL.MgraColor( AnnulusContext, MIL.M_COLOR_BLACK );
			MIL.MgraArcFill( AnnulusContext, MilAnnulusImage, DetectCirclrCenterX, DetectCirclrCenterY, smallRadius, smallRadius, -360.0, 360.0 );

			MIL.MimArith( MilAnnulusImage, BinarizedImage, AnnulusAndBinarized, MIL.M_AND );
			MIL.MimArith( MilAnnulusImage, InfraredCameraImage, AnnulusAndImage, MIL.M_AND );

			int imageWidth = 0;
			int imageHeight = 0;

			MIL.MbufInquire( AnnulusAndImage, MIL.M_SIZE_X, ref imageWidth );
			MIL.MbufInquire( AnnulusAndImage, MIL.M_SIZE_Y, ref imageHeight );


			ushort[ ] ImageData = new ushort[imageWidth*imageHeight];
			MIL.MbufGet( AnnulusAndImage, ImageData );
			///
			//MIL.MdispSelect( InfraredDisplay, AnnulusAndBinarized );


			MIL.MblobAlloc( MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobContext );
			MIL.MblobAllocResult( MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobResult );

			MIL.MblobControl( BlobContext, MIL.M_BOX, MIL.M_ENABLE );
			MIL.MblobControl( BlobContext, MIL.M_FERETS, MIL.M_ENABLE );
			//면적이 큰 순서대로 Sort
			MIL.MblobControl( BlobContext, MIL.M_SORT1, MIL.M_BOX_AREA );
			MIL.MblobControl( BlobContext, MIL.M_NUMBER_OF_HOLES, MIL.M_ENABLE );
			//MIL.MblobControl( BlobContext, MIL.M_CONVEX_HULL, MIL.M_ENABLE );

			MIL.MblobCalculate( BlobContext, AnnulusAndBinarized, MIL.M_NULL, BlobResult );


			// 블롭 개수 가져오기
			MIL_INT selectedBlobCount = 0;

			MIL.MblobGetResult( BlobResult, MIL.M_GENERAL, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref selectedBlobCount );

			//Logger.WriteLine( $"Blob Number: {selectedBlobCount}" );
			//int SelectBlob = 0;
			
			double AreaSum = 0.0;
			double Area = 0.0;
			double MaxArea = 0.0;
			double MaxLangth = 0.0;
			//double LastBlobTouchingImageBorders = 0.0;
			for (MIL_INT i = 0; i < selectedBlobCount; i++)
			{

				// M_BLOB_INDEX 속성 가져오기
				// 블롭 인덱스는 보통 0부터 시작
				MIL.MblobGetResult( BlobResult, MIL.M_BLOB_INDEX( i ), MIL.M_AREA + MIL.M_TYPE_MIL_DOUBLE, ref Area );
				
				//MIL.MblobGetResult( BlobResult, MIL.M_BLOB_INDEX( i ), MIL.M_BLOB_TOUCHING_IMAGE_BORDERS + MIL.M_TYPE_MIL_DOUBLE, ref BlobTouchingImageBorders );
				if (Area > MaxArea)
				{
					MIL.MblobGetResult( BlobResult, MIL.M_BLOB_INDEX( i ), MIL.M_FERET_MIN_DIAMETER + MIL.M_TYPE_MIL_DOUBLE, ref MaxLangth );
					MaxArea = Area;
				}
				AreaSum = Area + AreaSum;

			}
			Logger.WriteLine( $"블롭의 전체 면적 : {AreaSum}\n" );
			Logger.WriteLine( $"최소 길이 : {MaxLangth} \n" );

			if ( selectedBlobCount != 0 && ( MaxLangth + infraredCameraModel.CircleAreaMinLength ) >= ( Radius*2 ) )
			{
				blobGood = true;
			}
			else
			{
				Logger.WriteLine( "selectedBlobCount가 0입니다." );
				Logger.WriteLine( " 최소 길이 미달 " );
				blobGood = false;
			}

			

			

			if (Radius == 0 || Radius < infraredCameraModel.CircleMinRadius)
			{
				Logger.WriteLine( "Radius가 0이거나 최소 원주보다 작습니다." );
				circleGood = false;
			}
			else
			{
				ReferenceArea = (Math.PI * Radius * Radius) - (Math.PI * smallRadius * smallRadius);
				FillRatio = AreaSum / ReferenceArea;
				Logger.WriteLine( $"FillRatio: {FillRatio}" );
				circleGood = true;
			}


			if ( FillRatio > infraredCameraModel.CircleMinAreaRatio && FillRatio < infraredCameraModel.CircleMaxAreaRatio )
			{
				moonCutGood = true;
			}
			else
			{
				moonCutGood = false;
			}


			double sum = 0;
			int divnum = 0;
			int avg = 0;
			for (int i = 0; i < ImageData.Length; i++)
			{
				if (ImageData[ i ] >= infraredCameraModel.BinarizedThreshold)
				{
					sum += ImageData[ i ];
					divnum++;
				}
			}
			if (divnum != 0)
			{
				avg = (int)(sum / divnum);
			}


			if ( avg > infraredCameraModel.AvgTemperatureMin && avg < infraredCameraModel.AvgTemperatureMax )
			{
				temperatureGood = true;
			}
			else
			{
				temperatureGood = false;
				Logger.WriteLine("평균온도 이상");
			}

			Logger.WriteLine( $"avg: {avg}" );




			isGood = blobGood && circleGood && moonCutGood && temperatureGood;
			/*

			MIL.MblobSelect( BlobContext, BlobResult, BlobResult, MIL.M_SIZE_X, MIL.M_GREATER_OR_EQUAL, Radius * 2 - 20 );
			MIL.MblobSelect( BlobContext, BlobResult, BlobResult, MIL.M_SIZE_X, MIL.M_LESS_OR_EQUAL, Radius * 2 + 20 );
			*/
			MIL.MdispControl( InfraredDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_DEFAULT );

			if (isSetting)
			{
				MIL.MgraColor( SettingGraphicsContext, MIL.M_COLOR_GREEN );
				MIL.MgraArc( SettingGraphicsContext, MilOverlayImage, infraredCameraModel.CircleCenterX, infraredCameraModel.CircleCenterY, infraredCameraModel.CircleMinRadius, infraredCameraModel.CircleMinRadius, 0.0, 360.0 );
				MIL.MgraArc( SettingGraphicsContext, MilOverlayImage, infraredCameraModel.CircleCenterX, infraredCameraModel.CircleCenterY, infraredCameraModel.CircleMaxRadius, infraredCameraModel.CircleMaxRadius, 0.0, 360.0 );
			}

			if (isBinarized)
			{
				MIL.MdispLut( InfraredDisplay, MIL.M_COLORMAP_GRAYSCALE );
				MIL.MdispSelect( InfraredDisplay, BinarizedImage );
			}
			else
			{
				MIL.MdispLut( InfraredDisplay, MIL.M_COLORMAP_JET );
				MIL.MdispSelect( InfraredDisplay, InfraredCameraScaleImage );
			}

			MIL.MgraArc( GraphicsContext, MilOverlayImage, DetectCirclrCenterX, DetectCirclrCenterY, smallRadius, smallRadius, 0.0, 360.0 );
			MIL.MmeasDraw( GraphicsContext, MeasMarker, MilOverlayImage, MIL.M_DRAW_EDGES, MIL.M_DEFAULT, MIL.M_DEFAULT );
			MIL.MblobDraw( GraphicsContext, BlobResult, MilOverlayImage, MIL.M_DRAW_BOX, MIL.M_DEFAULT, MIL.M_DEFAULT );
			/*
			MIL.MblobGetResult( BlobResult, MIL.M_DEFAULT, MIL.M_NUMBER_OF_HOLES, ref holenumber );
			MIL.MblobGetResult( BlobResult, M_GENERAL, M_NUMBER + M_TYPE_MIL_INT, &Number0 );
			MIL.MblobGetResult( BlobResult, MIL.M_GENERAL, MIL.M_NUMBER_OF_HOLES + MIL.M_TYPE_MIL_INT, ref holenumber );

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

			MIL.MbufPut( BinarizedImage, imageData );
			MIL.MbufGet( BinarizedImage, imageData );
			MIL.MbufClone( BinarizedImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref InfraredCameraConversionImage );
			MIL.MmeasFree( CircleMeasMarker );
			*/



			MIL.MblobFree( BlobResult );
			MIL.MblobFree( BlobContext );
			MIL.MgraFree( GraphicsContext );
			MIL.MgraFree( SettingGraphicsContext );
			MIL.MgraFree( AnnulusContext );
			MIL.MmeasFree( MeasMarker );

			MIL.MbufFree( AnnulusAndBinarized );
			MIL.MbufFree( AnnulusAndImage );
			MIL.MbufFree( MilAnnulusImage );
			//MIL.MbufFree( MilOverlayImage );
			//if (BinarizedImage != MIL.M_NULL)
			//{
			//	MIL.MbufFree( BinarizedImage );
			//	BinarizedImage = MIL.M_NULL;
			//}

			//BinarizedImage = MIL.M_NULL;

			return isGood;
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
