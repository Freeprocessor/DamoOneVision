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



		public static async Task<InfraredInspectionResult> InfraredCameraModel( bool isSetting, bool isBinarized, MIL_ID BinarizedImage, MIL_ID InfraredCameraScaleImage, MIL_ID InfraredCameraImage, MIL_ID InfraredDisplay, InfraredCameraModel infraredCameraModel )
		{
			if (InfraredCameraImage == MIL.M_NULL)
			{
				Logger.WriteLine( "InfraredCameraImage가 유효하지 않습니다." );
				return null;
			}
			//return true;
			//Logger.WriteLine( "InfraredCameraModel 호출" );
			bool moonCutGood = false;
			bool circleGood = false;
			bool overHeatGood = false;
			bool underHeatGood = false;

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

			// 기준 ROI 평균 온도 측정
			double currentReferenceAvg = CalculateReferenceRoiAverage(InfraredCameraImage);

			// 모델 등록 시 저장된 기준 온도와의 차이 계산
			double delta = currentReferenceAvg - infraredCameraModel.ReferenceBaseTemperature;

			// 기준 온도 차이만큼 임계값을 동적으로 보정
			double dynamicThreshold = infraredCameraModel.BinarizedThreshold + delta;

			double thresholdForMil = (dynamicThreshold + 273.15) * 100;

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


			/// InfraredCameraImage의 클론 생성
			//MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BinarizedImage );
			MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilAnnulusImage );
			MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref AnnulusAndBinarized );
			MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref AnnulusAndImage );

			/// Black으로 초기화 
			MIL.MbufClear( MilAnnulusImage, MIL.M_COLOR_BLACK );
			MIL.MbufClear( AnnulusAndBinarized, MIL.M_COLOR_BLACK );
			MIL.MbufClear( AnnulusAndImage, MIL.M_COLOR_BLACK );

			/// 이진화 이미지 생성 
			MIL.MimBinarize( InfraredCameraImage, BinarizedImage, MIL.M_GREATER, thresholdForMil, MIL.M_NULL );

			// 로그 기록 (권장)
			Logger.WriteLine( $"기준 온도: {infraredCameraModel.ReferenceBaseTemperature}, 현재 온도: {currentReferenceAvg}, 동적 임계값: {dynamicThreshold}" );

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

			/// 그래픽 컨텍스트 생성(디스플레이 오버레이)
			MIL.MgraAlloc( MilSystem, ref GraphicsContext );
			MIL.MgraAlloc( MilSystem, ref AnnulusContext );
			MIL.MgraAlloc( MilSystem, ref SettingGraphicsContext );


			/// 원찾기 
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

			/// 도넛모양 도형 생성
			MIL.MgraColor( AnnulusContext, MIL.M_COLOR_WHITE );
			MIL.MgraArcFill( AnnulusContext, MilAnnulusImage, DetectCirclrCenterX, DetectCirclrCenterY, Radius, Radius, -360.0, 360.0 );
			MIL.MgraColor( AnnulusContext, MIL.M_COLOR_BLACK );
			MIL.MgraArcFill( AnnulusContext, MilAnnulusImage, DetectCirclrCenterX, DetectCirclrCenterY, smallRadius, smallRadius, -360.0, 360.0 );

			/// 생성된 도넛모양 도형과 이진화 이미지, 흑백이미지 AND연산 
			MIL.MimArith( MilAnnulusImage, BinarizedImage, AnnulusAndBinarized, MIL.M_AND );
			MIL.MimArith( MilAnnulusImage, InfraredCameraImage, AnnulusAndImage, MIL.M_AND );

			int imageWidth = 0;
			int imageHeight = 0;

			MIL.MbufInquire( AnnulusAndImage, MIL.M_SIZE_X, ref imageWidth );
			MIL.MbufInquire( AnnulusAndImage, MIL.M_SIZE_Y, ref imageHeight );

			/// 온도데이터를 담고 있는 AnnulusAndImage를 ushort 배열로 변환
			ushort[ ] ImageData = new ushort[imageWidth*imageHeight];
			MIL.MbufGet( AnnulusAndImage, ImageData );
			///
			//MIL.MdispSelect( InfraredDisplay, AnnulusAndBinarized );

			/// 이진화 + 도넛 AND 연산한 이미지에서 Blob 검출
			MIL.MblobAlloc( MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobContext );
			MIL.MblobAllocResult( MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobResult );

			MIL.MblobControl( BlobContext, MIL.M_BOX, MIL.M_ENABLE );
			MIL.MblobControl( BlobContext, MIL.M_FERETS, MIL.M_ENABLE );
			//면적이 큰 순서대로 Sort
			MIL.MblobControl( BlobContext, MIL.M_SORT1, MIL.M_BOX_AREA );
			MIL.MblobControl( BlobContext, MIL.M_NUMBER_OF_HOLES, MIL.M_ENABLE );
			//MIL.MblobControl( BlobContext, MIL.M_CONVEX_HULL, MIL.M_ENABLE );

			MIL.MblobCalculate( BlobContext, AnnulusAndBinarized, MIL.M_NULL, BlobResult );


			/// 블롭 개수 가져오기
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

				/// M_BLOB_INDEX 속성 가져오기
				/// 블롭 인덱스는 보통 0부터 시작
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
			Logger.WriteLine( $"블롭의 최대 길이 : {MaxLangth} \n" );


			if (selectedBlobCount == 0 || Radius == 0 || Radius < infraredCameraModel.CircleMinRadius)
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


			if ( (FillRatio > infraredCameraModel.CircleMinAreaRatio) && (FillRatio < infraredCameraModel.CircleMaxAreaRatio) && (MaxLangth + infraredCameraModel.CircleAreaMinLength) >= (Radius * 2))
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
				if (ImageData[ i ] >= dynamicThreshold)
				{
					sum += ImageData[ i ];
					divnum++;
				}
			}

			/// 아래 구문으로 대체
			if (divnum != 0)
			{
				avg = (int)(sum / divnum);
			}

			double avgCelsius = divnum != 0 ? (sum / divnum) / 100.0 - 273.15 : 0.0;
			Logger.WriteLine( $"평균 온도(℃): {avgCelsius}" );



			MIL.MdispControl( InfraredDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_DEFAULT );

			double[] sectorTemp = new double[12];

			double sectorTotalSum = 0.0;
			for (int i = 0; i < 12; i++)
			{
				if (Radius == 0 || Radius < infraredCameraModel.CircleMinRadius)
				{
					Logger.WriteLine( "Radius가 0이거나 최소 원주보다 작습니다." );
					circleGood = false;
					break;
				}
				// 시작 각도: 시계 방향으로 -각도로 표현
				double span = 30.0;
				double start = -i * span;

				MIL_ID sectorMask = MIL.M_NULL;
				MIL_ID graCtx = MIL.M_NULL;
				MIL_ID sectorImage = MIL.M_NULL;

				// 1. 마스크 이미지 생성 및 초기화
				MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT,
					MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref sectorMask );
				MIL.MbufClear( sectorMask, MIL.M_COLOR_BLACK );

				MIL.MgraAlloc( MilSystem, ref graCtx );
				MIL.MgraColor( graCtx, MIL.M_COLOR_WHITE );

				// 2. 섹터 영역 그리기 (외곽 호)
				MIL.MgraArcFill( graCtx, sectorMask, DetectCirclrCenterX, DetectCirclrCenterY, Radius, Radius,
					start, span );

				// 3. 내부 원 제거 (도넛 형태 유지)
				MIL.MgraColor( graCtx, MIL.M_COLOR_BLACK );
				MIL.MgraArcFill( graCtx, sectorMask, DetectCirclrCenterX, DetectCirclrCenterY, smallRadius, smallRadius,
					start, span );

				// 4. 섹터 마스크와 실제 이미지 AND
				MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT,
					MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref sectorImage );
				MIL.MimArith( InfraredCameraImage, sectorMask, sectorImage, MIL.M_AND );

				// 5. 데이터 가져오기
				int w = 0, h = 0;
				MIL.MbufInquire( sectorImage, MIL.M_SIZE_X, ref w );
				MIL.MbufInquire( sectorImage, MIL.M_SIZE_Y, ref h );
				ushort[] sectorData = new ushort[w * h];
				MIL.MbufGet( sectorImage, sectorData );

				// 6. 평균 온도 계산
				var temps = sectorData.Where(v => v > 0).Select(v => (v / 100.0) - 273.15);
				double sectorAvg = temps.Any() ? temps.Average() : 0;
				Logger.WriteLine( $"[Sector {i + 1}] 평균 온도: {sectorAvg:F2} ℃" );
				sectorTotalSum += sectorAvg;
				sectorTemp[i] = sectorAvg;
				// 7. 그래픽 오버레이에 두 개의 선 추가 (피자 조각 형태)
				//MIL.MgraColor( GraphicsContext, MIL.M_COLOR_YELLOW );
				double angleStartRad = start * Math.PI / 180.0;
				double angleEndRad = (start + span) * Math.PI / 180.0;

				// [🧠 수정된 코드: 내부 반지름부터 외부 반지름까지 선 그리기]
				double x1_start = DetectCirclrCenterX + smallRadius * Math.Cos(angleStartRad);
				double y1_start = DetectCirclrCenterY + smallRadius * Math.Sin(angleStartRad);
				double x1_end = DetectCirclrCenterX + Radius * Math.Cos(angleStartRad);
				double y1_end = DetectCirclrCenterY + Radius * Math.Sin(angleStartRad);

				double x2_start = DetectCirclrCenterX + smallRadius * Math.Cos(angleEndRad);
				double y2_start = DetectCirclrCenterY + smallRadius * Math.Sin(angleEndRad);
				double x2_end = DetectCirclrCenterX + Radius * Math.Cos(angleEndRad);
				double y2_end = DetectCirclrCenterY + Radius * Math.Sin(angleEndRad);

				MIL.MgraLine( GraphicsContext, MilOverlayImage, x1_start, y1_start, x1_end, y1_end );
				MIL.MgraLine( GraphicsContext, MilOverlayImage, x2_start, y2_start, x2_end, y2_end );

				// 리소스 해제
				MIL.MbufFree( sectorMask );
				MIL.MbufFree( sectorImage );
				MIL.MgraFree( graCtx );
			}
			double sectorTotalAvg = sectorTotalSum / 12;
			Logger.WriteLine( $"12개 섹터 평균 온도: {sectorTotalAvg:F2} ℃" );


			/// 임계온도 이상 값 검출이 아닌 전체 면적 기준 평균 온도로 변경
			//if ( avgCelsius > infraredCameraModel.AvgTemperatureMin )
			//{
			//	underHeatGood = true;
			//}
			//else
			//{
			//	underHeatGood = false;
			//	Logger.WriteLine( "UnderHeat Error" );
			//}

			//if ( avgCelsius < infraredCameraModel.AvgTemperatureMax )
			//{
			//	overHeatGood = true;
			//}
			//else
			//{
			//	overHeatGood = false;
			//	Logger.WriteLine( "OverHeat Error" );
			//}


			/// 평균 온도 기준으로 섹터별 온도 비교
			bool[] underHeatSector = new bool[12];
			bool[] overHeatSector = new bool[12];
			///
			for (int i = 0; i < sectorTemp.Length; i++)
			{
				if (sectorTemp[i] > infraredCameraModel.AvgTemperatureMin)
				{
					underHeatSector[i] = true;
				}
				else
				{
					underHeatSector[ i] = false;
					Logger.WriteLine( $"Sector {i + 1} UnderHeat Error" );
				}

				if (sectorTemp[i] < infraredCameraModel.AvgTemperatureMax)
				{
					overHeatSector[i] = true;
				}
				else
				{
					overHeatSector[i] = false;
					Logger.WriteLine( $"Sector {i + 1} OverHeat Error" );
				}
			}

			int underHeatCount = underHeatSector.Count( x => x == false );
			int overHeatCount = overHeatSector.Count( x => x == false );

			/// 카운트값 추후 변수로 변경 예정
			if ( underHeatCount > 0 )
			{
				underHeatGood = false;
			}
			else
			{
				underHeatGood = true;
			}

			if( overHeatCount > 8)
			{
				overHeatGood = false;
			}
			else
			{
				overHeatGood = true;
			}

			//Logger.WriteLine( $"avg: {avg}" );




			//isGood = blobGood && circleGood && moonCutGood && temperatureGood;
			/*

			MIL.MblobSelect( BlobContext, BlobResult, BlobResult, MIL.M_SIZE_X, MIL.M_GREATER_OR_EQUAL, Radius * 2 - 20 );
			MIL.MblobSelect( BlobContext, BlobResult, BlobResult, MIL.M_SIZE_X, MIL.M_LESS_OR_EQUAL, Radius * 2 + 20 );
			*/
			

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




			//MIL.MbufFree( MilOverlayImage );
			//if (BinarizedImage != MIL.M_NULL)
			//{
			//	MIL.MbufFree( BinarizedImage );
			//	BinarizedImage = MIL.M_NULL;
			//}
			// [🧠 NEW] 도넛 섹터별 평균 온도 계산 (30도씩 12조각)
			

			MIL.MblobFree( BlobResult );
			MIL.MblobFree( BlobContext );
			MIL.MgraFree( GraphicsContext );
			MIL.MgraFree( SettingGraphicsContext );
			MIL.MgraFree( AnnulusContext );
			MIL.MmeasFree( MeasMarker );

			MIL.MbufFree( AnnulusAndBinarized );
			MIL.MbufFree( AnnulusAndImage );
			MIL.MbufFree( MilAnnulusImage );

			//BinarizedImage = MIL.M_NULL;
			var inspectionResult = new InfraredInspectionResult
			{
				MoonCutIssue = !moonCutGood,
				CircleIssue = !circleGood,
				OverHeatIssue = !overHeatGood,
				UnderHeatIssue = !underHeatGood,

				FillRatio = FillRatio,
				AverageTemperature = avgCelsius,
				Radius = Radius,
				MaxBlobLength = MaxLangth
			};

			return inspectionResult;
		}

		private static double CalculateReferenceRoiAverage( MIL_ID image )
		{
			int refX = 610, refY = 10, refW = 20, refH = 20;

			MIL_ID roi = MIL.M_NULL;
			MIL.MbufChild2d( image, refX, refY, refW, refH, ref roi );

			ushort[] roiData = new ushort[refW * refH];
			MIL.MbufGet( roi, roiData );
			MIL.MbufFree( roi );

			// ushort[] → double로 변환 후 평균
			return roiData.Select( v => (v / 100.0) - 273.15 ).Average();
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
