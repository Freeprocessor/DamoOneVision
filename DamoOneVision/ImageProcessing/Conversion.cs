using DamoOneVision.Camera;
using DamoOneVision.Models;
using DamoOneVision.Services;
using Matrox.MatroxImagingLibrary;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using Windows.Devices.Radios;

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

			if ((Math.Abs( posy1 - posy2 ) - Math.Abs( posy3 - posy4 )) > 15)
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

			Stopwatch sw = new Stopwatch();	
			sw.Start();

			if (InfraredCameraImage == MIL.M_NULL)
			{
				sw.Stop();
				Logger.WriteLine( "InfraredCameraImage가 유효하지 않습니다." );
				return null;
			}


			long ImageWidth = 0;
			long ImageHeight = 0;
			MIL.MbufInquire( InfraredCameraImage, MIL.M_SIZE_X, ref ImageWidth );
			MIL.MbufInquire( InfraredCameraImage, MIL.M_SIZE_Y, ref ImageHeight );

			// 해상도 바뀌면 그때만 Free→Alloc, 매 프레임엔 Clear()
			ReusableMilBuffers.EnsureSize( MilSystem, (int) ImageWidth, (int) ImageHeight );
			ReusableMilBuffers.Clear();    // ← 입력프레임 처리 전 초기화


			//MIL_ID CircleMeasMarker = MIL.M_NULL;
			MIL_ID GraphicsContext = MIL.M_NULL;
			MIL_ID SettingGraphicsContext = MIL.M_NULL;

			MIL_ID MilOverlayImage = MIL.M_NULL;

			// 기준 ROI 평균 온도 측정
			double currentReferenceAvg = CalculateReferenceRoiAverage(InfraredCameraImage);

			// 모델 등록 시 저장된 기준 온도와의 차이 계산
			double delta = currentReferenceAvg - infraredCameraModel.ReferenceBaseTemperature;

			// 기준 온도 차이만큼 임계값을 동적으로 보정
			double dynamicThreshold = infraredCameraModel.BinarizedThreshold + delta;
			double thresholdForMil = (dynamicThreshold + 273.15) * 100;

			// 로그 기록 (권장)
			Logger.WriteLine( $"기준 온도: {infraredCameraModel.ReferenceBaseTemperature}, 현재 온도: {currentReferenceAvg}, 동적 임계값: {dynamicThreshold}" );

			/// 오버레이 이미지 생성
			MIL.MdispInquire( InfraredDisplay, MIL.M_OVERLAY_ID, ref MilOverlayImage );
			MIL.MdispControl( InfraredDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_DEFAULT );

			/// 이진화 이미지 생성 
			MIL.MimBinarize( InfraredCameraImage, BinarizedImage, MIL.M_GREATER, thresholdForMil, MIL.M_NULL );

			/// 그래픽 컨텍스트 생성(디스플레이 오버레이)
			MIL.MgraAlloc( MilSystem, ref GraphicsContext );
			MIL.MgraAlloc( MilSystem, ref SettingGraphicsContext );

			/// 원찾기 
			double[] CircleResult = MeasFindCircleRadius( BinarizedImage, MilOverlayImage, infraredCameraModel );
			double Radius = CircleResult[0];
			double DetectCirclrCenterX = CircleResult[1];
			double DetectCirclrCenterY = CircleResult[2];

			double smallRadius = Radius - infraredCameraModel.CircleInnerRadius;

			/// 블롭 계산
			double[] BlobResult = FindBlob(BinarizedImage, MilOverlayImage, DetectCirclrCenterX, DetectCirclrCenterY, Radius, smallRadius);
			double AreaSum = BlobResult[0];
			double MaxLangth = BlobResult[1];
			int selectedBlobCount = (int) BlobResult[2];

			/// 섹터 연산
			const int sectorCount = 60; // 섹터 개수
			(int neighborBadCnt, double[] sectorTemp) = await SectorCalculate( InfraredCameraImage, sectorCount, Radius, smallRadius, DetectCirclrCenterX, DetectCirclrCenterY, infraredCameraModel);

			double avgTemp = sectorTemp.Length == 0      // 빈 배열 방지
               ? 0.0
			   : sectorTemp.Average();       // 평균 구하기
			bool neighborGood = neighborBadCnt == 0;

			Logger.WriteLine($"Neighbor ΔT > {infraredCameraModel.NeighborDiffLim:F1} ℃ 섹터 수: {neighborBadCnt}" );



			/// MoonCut 검사
			bool circleGood = false;
			double ReferenceArea = 0.0;
			double FillRatio = 0.0;

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

			/// MoonCut 최소길이 검사
			bool moonCutGood = false;

			if ((FillRatio > infraredCameraModel.CircleMinAreaRatio) && (FillRatio < infraredCameraModel.CircleMaxAreaRatio) && (MaxLangth + infraredCameraModel.CircleAreaMinLength) >= (Radius * 2))
			{
				moonCutGood = true;
			}
			else
			{
				moonCutGood = false;
			}


			/// 섹터별 온도 검사
			bool[] underHeatSector = new bool[sectorCount];
			bool[] overHeatSector = new bool[sectorCount];
			///
			for (int i = 0; i < sectorTemp.Length; i++)
			{
				if (sectorTemp[ i ] > infraredCameraModel.AvgTemperatureMin)
				{
					underHeatSector[ i ] = true;
				}
				else
				{
					underHeatSector[ i ] = false;
					//Logger.WriteLine( $"Sector {i + 1} UnderHeat Error" );
				}

				if (sectorTemp[ i ] < infraredCameraModel.AvgTemperatureMax)
				{
					overHeatSector[ i ] = true;
				}
				else
				{
					overHeatSector[ i ] = false;
					//Logger.WriteLine( $"Sector {i + 1} OverHeat Error" );
				}
			}

			int underHeatCount = underHeatSector.Count( x => x == false );
			int overHeatCount = overHeatSector.Count( x => x == false );

			Logger.WriteLine( $"UnderHeat Count: {underHeatCount}" );
			Logger.WriteLine( $"OverHeat Count: {overHeatCount}" );

			bool underHeatGood = false;
			bool overHeatGood = false;
			if (underHeatCount >= infraredCameraModel.UnderHeatCountLim)
			{
				underHeatGood = false;
			}
			else
			{
				underHeatGood = true;
			}

			if (overHeatCount >= infraredCameraModel.OverHeatCountLim)
			{
				overHeatGood = false;
			}
			else
			{
				overHeatGood = true;
			}


			/// 최대, 최소온도 편차 검사
			bool tempdivGood = false;
			// ① NaN 제거
			var clean = sectorTemp.Where(v => !double.IsNaN(v)).ToArray();

			if (clean.Length == 0)        // ② 빈 배열 체크
			{
				Console.WriteLine( "값이 없습니다." );
			}
			double max = clean.Max();
			double min = clean.Min();

			double tempDiv = max - min;
			if ( tempDiv > infraredCameraModel.TempDivLim )
			{
				tempdivGood = false;
			}
			else
			{
				tempdivGood = true;
			}



			DrawSetors( MilOverlayImage, sectorCount, Radius, smallRadius, DetectCirclrCenterX, DetectCirclrCenterY );

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


			MIL.MgraFree( GraphicsContext );
			MIL.MgraFree( SettingGraphicsContext );

			var inspectionResult = new InfraredInspectionResult
			{
				MoonCutIssue = !moonCutGood,
				CircleIssue = !circleGood,
				OverHeatIssue = !overHeatGood,
				UnderHeatIssue = !underHeatGood,
				TemperatureIssue = !tempdivGood,
				NeighborTempIssue = !neighborGood,

				FillRatio = FillRatio,
				AverageTemperature = avgTemp,
				TempeDiv = tempDiv,
				Radius = Radius,
				MaxBlobLength = MaxLangth
			};

			sw.Stop();
			Logger.WriteLine( $"검사 시간: {sw.ElapsedMilliseconds} ms" );

			return inspectionResult;
		}

		private static double[] ModFindCircleRadius( MIL_ID BinarizedImage, InfraredCameraModel infraredCameraModel, ref double Radius, ref double DetectCirclrCenterX, ref double DetectCirclrCenterY )
		{

			/// 원 찾기 (mod)
			/// 

			//MIL_ID ResultobjectforModCircleShapeContext = MIL.M_NULL;

			//MIL.MmodAllocResult( MilSystem, MIL.M_SHAPE_CIRCLE, ref ResultobjectforModCircleShapeContext );

			//MIL.MmodAlloc( MilSystem, MIL.M_SHAPE_CIRCLE, MIL.M_DEFAULT, ref ModCircleShape );
			//MIL.MmodDefine( ModCircleShape, MIL.M_CIRCLE, MIL.M_DEFAULT, 140.0, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT );

			//MIL.MmodControl( ModCircleShape, MIL.M_CONTEXT, MIL.M_DETAIL_LEVEL, MIL.M_HIGH );
			//MIL.MmodControl( ModCircleShape, MIL.M_CONTEXT, MIL.M_SMOOTHNESS, 20.0 );

			//MIL.MmodPreprocess( ModCircleShape, MIL.M_DEFAULT );

			//MIL.MmodFind( ModCircleShape, MimConvolImage, ResultobjectforModCircleShapeContext );

			//try
			//{
			//	MIL.MmodGetResult( ResultobjectforModCircleShapeContext, 0, MIL.M_POSITION_X + MIL.M_TYPE_MIL_DOUBLE, ref DetectCirclrCenterX );
			//	MIL.MmodGetResult( ResultobjectforModCircleShapeContext, 0, MIL.M_POSITION_Y + MIL.M_TYPE_MIL_DOUBLE, ref DetectCirclrCenterY );
			//	MIL.MmodGetResult( ResultobjectforModCircleShapeContext, 0, MIL.M_RADIUS + MIL.M_TYPE_MIL_DOUBLE, ref Radius );
			//}
			//catch (Exception ex)
			//{
			//	Logger.WriteLine( $"ModCircleShape에서 예외 발생: {ex.Message}" );
			//}

			//MIL.MmodFree( ModCircleShape );
			//MIL.MmodFree( ResultobjectforModCircleShapeContext );

			//MIL.MbufFree( MimConvolImage );
			return new double[] { Radius, DetectCirclrCenterX, DetectCirclrCenterY };
		}


		private static double[] MeasFindCircleRadius( MIL_ID BinarizedImage, MIL_ID MilOverlayImage, InfraredCameraModel infraredCameraModel )
		{

			Stopwatch sw = new Stopwatch( );
			sw.Start( );

			MIL_ID MeasMarker = MIL.M_NULL;
			MIL_ID GraphicsContext = MIL.M_NULL;

			double Radius = 0.0;
			double DetectCirclrCenterX = 0.0;
			double DetectCirclrCenterY = 0.0;

			MIL.MgraAlloc( MilSystem, ref GraphicsContext );

			MIL.MmeasAllocMarker( MilSystem, MIL.M_CIRCLE, MIL.M_DEFAULT, ref MeasMarker );

			MIL.MmeasSetScore( MeasMarker, MIL.M_RADIUS_SCORE, 0.0, MIL.M_MAX_POSSIBLE_VALUE, MIL.M_MAX_POSSIBLE_VALUE, MIL.M_MAX_POSSIBLE_VALUE, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT );

			//MIL.MmeasSetScore( MeasMarker, MIL.M_RADIUS_SCORE, 0.0, 0.0, 0.0, MIL.M_MAX_POSSIBLE_VALUE, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT );


			MIL.MmeasSetMarker( MeasMarker, MIL.M_CIRCLE_ACCURACY, MIL.M_HIGH, MIL.M_NULL );
			//MIL.MmeasSetMarker( MeasMarker, MIL.M_SUB_PIXEL, MIL.M_ENABLE, MIL.M_NULL );
			MIL.MmeasSetMarker( MeasMarker, MIL.M_FILTER_TYPE, MIL.M_SHEN, MIL.M_NULL );
			MIL.MmeasSetMarker( MeasMarker, MIL.M_FILTER_SMOOTHNESS, 15.0, MIL.M_NULL );
			MIL.MmeasSetMarker( MeasMarker, MIL.M_EDGEVALUE_MIN, 6.0, MIL.M_NULL );




			MIL.MmeasSetMarker( MeasMarker, MIL.M_SEARCH_REGION_INPUT_UNITS, MIL.M_PIXEL, MIL.M_NULL );
			MIL.MmeasSetMarker( MeasMarker, MIL.M_RING_CENTER, infraredCameraModel.CircleCenterX, infraredCameraModel.CircleCenterY );
			MIL.MmeasSetMarker( MeasMarker, MIL.M_RING_RADII, infraredCameraModel.CircleMinRadius, infraredCameraModel.CircleMaxRadius );

			//	MIL.MmeasSetMarker( MeasMarker, MIL.M_MINIMUM_CIRCULAR_ARC_POINTS, 64, MIL.M_NULL );
			MIL.MmeasSetMarker( MeasMarker, MIL.M_MAX_ASSOCIATION_DISTANCE, MIL.M_DEFAULT, MIL.M_NULL );
			//MIL.MmeasSetMarker( MeasMarker, MIL.M_CIRCULAR_FIT_ERROR_MAX, 0.5, MIL.M_NULL );


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


			sw.Stop( );
			Logger.WriteLine( $"MeasFind  : {sw.ElapsedMilliseconds} ms" );

			MIL.MmeasDraw( GraphicsContext, MeasMarker, MilOverlayImage, MIL.M_DRAW_EDGES, MIL.M_DEFAULT, MIL.M_DEFAULT );

			MIL.MmeasFree( MeasMarker );
			MIL.MgraFree( GraphicsContext );

			double[] result = new double[3];

			result[0]= Radius;
			result[1]= DetectCirclrCenterX;
			result[2]= DetectCirclrCenterY;

			return result;
		}

		private static double[] FindBlob( MIL_ID BinarizedImage, MIL_ID MilOverlayImage, double DetectCirclrCenterX, double DetectCirclrCenterY, double Radius, double smallRadius )
		{
			Stopwatch sw = new Stopwatch( );
			sw.Start( );

			MIL_ID MilAnnulusImage = MIL.M_NULL;
			MIL_ID AnnulusAndBinarized = MIL.M_NULL;

			MIL_ID AnnulusContext = MIL.M_NULL;
			MIL_ID GraphicsContext = MIL.M_NULL;

			MIL_ID BlobContext = MIL.M_NULL;
			MIL_ID BlobResult = MIL.M_NULL;

			MIL.MgraAlloc( MilSystem, ref AnnulusContext );
			MIL.MgraAlloc( MilSystem, ref GraphicsContext );


			/// InfraredCameraImage의 클론 생성
			MIL.MbufClone( BinarizedImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilAnnulusImage );
			MIL.MbufClone( BinarizedImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref AnnulusAndBinarized );
			//MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref AnnulusAndImage );


			/// Black으로 초기화 
			MIL.MbufClear( MilAnnulusImage, MIL.M_COLOR_BLACK );
			MIL.MbufClear( AnnulusAndBinarized, MIL.M_COLOR_BLACK );
			//MIL.MbufClear( AnnulusAndImage, MIL.M_COLOR_BLACK );

			/// 도넛모양 도형 생성
			MIL.MgraColor( AnnulusContext, MIL.M_COLOR_WHITE );
			MIL.MgraArcFill( AnnulusContext, MilAnnulusImage, DetectCirclrCenterX, DetectCirclrCenterY, Radius, Radius, -360.0, 360.0 );
			MIL.MgraColor( AnnulusContext, MIL.M_COLOR_BLACK );
			MIL.MgraArcFill( AnnulusContext, MilAnnulusImage, DetectCirclrCenterX, DetectCirclrCenterY, smallRadius, smallRadius, -360.0, 360.0 );


			/// 생성된 도넛모양 도형과 이진화 이미지, 흑백이미지 AND연산 
			MIL.MimArith( MilAnnulusImage, BinarizedImage, AnnulusAndBinarized, MIL.M_AND );

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

			sw.Stop( );
			//Logger.WriteLine( $"블롭의 전체 면적 : {AreaSum}\n" );
			//Logger.WriteLine( $"블롭의 최대 길이 : {MaxLangth} \n" );
			Logger.WriteLine( $"Blob      : {sw.ElapsedMilliseconds} ms" );

			MIL.MblobDraw( GraphicsContext, BlobResult, MilOverlayImage, MIL.M_DRAW_BOX, MIL.M_DEFAULT, MIL.M_DEFAULT );

			MIL.MbufFree( MilAnnulusImage );
			MIL.MbufFree( AnnulusAndBinarized );

			MIL.MgraFree( AnnulusContext );
			MIL.MgraFree( GraphicsContext );

			MIL.MblobFree( BlobContext );
			MIL.MblobFree( BlobResult );

			double[] result = new double[4];
			result[0] = AreaSum;          // 블롭 면적 합계
			result[1] = MaxLangth;        // 블롭 최대 길이
			result[2] = selectedBlobCount; // 선택된 블롭 개수
			result[3] = MaxArea;         // 블롭 최대 면적 (추가)

			return result;
		}

		private static async Task<(int, double[ ])> SectorCalculate( MIL_ID InfraredCameraImage, int SectorCount, double Radius, double smallRadius, double DetectCirclrCenterX, double DetectCirclrCenterY, InfraredCameraModel infraredCameraModel )
		{
			Stopwatch sw = new Stopwatch( );
			sw.Start( );

			MIL_INT ImageWidth = 0;
			MIL_INT ImageHeight = 0;

			MIL.MbufInquire( InfraredCameraImage, MIL.M_SIZE_X, ref ImageWidth );
			MIL.MbufInquire( InfraredCameraImage, MIL.M_SIZE_Y, ref ImageHeight );

			double sectorTotalSum = 0.0;
			List<Task> tasks = new();

			object sumLock = new();                  // ★ 공유 데이터 보호 (sectorTotalSum, sectorTemp)

			double     span       = 360.0 / SectorCount;   // 6°
			double[] sectorTemp = new double[SectorCount];
			// ★ for → Task.Run 내부로 캡처
			for (int i = 0; i < SectorCount; i++)
			{
				int idx = i;                         // ★ 클로저 변수 고정

				tasks.Add( Task.Run( ( ) =>
				{
					// MIL 스레드 컨텍스트 확보 (기존 그대로)
					//MIL_ID tctx = MIL.M_NULL;
					//MIL.MthrAlloc( MilSystem, MIL.M_THREAD,MIL.M_NULL, MIL.M_NULL, MIL.M_DEFAULT, ref tctx );

					if (Radius == 0 || Radius < infraredCameraModel.CircleMinRadius)
						return;   // early-exit

					/* --- 1) 각 Task 전용 버퍼/그래픽 컨텍스트 가져오기 --- */
					MIL_ID mask   = ReusableMilBuffers.AcquireMask(idx);
					MIL_ID sector = ReusableMilBuffers.AcquireSector(idx);
					MIL_ID gctx   = ReusableMilBuffers.AcquireGctx(idx);     // 섹터별 전용 gctx

					MIL.MbufClear( mask, MIL.M_COLOR_BLACK );    // ← 반드시 초기화
					MIL.MbufClear( sector, MIL.M_COLOR_BLACK );

					/* --- 2) 마스크 작성 & AND --- */
					double start = (idx - 1) * span;
					double end   = idx * span;

					MIL.MgraColor( gctx, MIL.M_COLOR_WHITE );
					MIL.MgraArcFill( gctx, mask,
									DetectCirclrCenterX, DetectCirclrCenterY,
									Radius, Radius, start, end );

					MIL.MgraColor( gctx, MIL.M_COLOR_BLACK );
					MIL.MgraArcFill( gctx, mask,
									DetectCirclrCenterX, DetectCirclrCenterY,
									smallRadius, smallRadius, start, end );

					MIL.MimArith( InfraredCameraImage, mask, sector, MIL.M_AND );


					/* --- 3) 평균 온도 계산 --- */
					ushort[] sectorData = new ushort[ImageWidth * ImageHeight];
					MIL.MbufGet( sector, sectorData );

					var temps     = sectorData.Where(v => v > 0)
							  .Select(v => v / 100.0 - 273.15);
					double sAvg   = temps.Any() ? temps.Average() : 0.0;

					lock (sumLock)
					{
						sectorTotalSum += sAvg;
						sectorTemp[ idx ] = sAvg;
					}
				} ) );
			}

			// ★ 모든 Task 완료 대기 (동기식이면 Task.WaitAll, 비동기면 await)
			try
			{
				await Task.WhenAll( tasks ).ConfigureAwait( false );
			}
			catch (Exception ex)
			{
				Logger.WriteLine( $"섹터 처리 중 예외: {ex}" );
				//throw;                        // 상위에서 추가 처리할 경우
			}

			double sectorTotalAvg = sectorTotalSum / SectorCount;
			Logger.WriteLine( $"{SectorCount}개 섹터 평균 온도: {sectorTotalAvg:F2} ℃" );
			/// 인접섹터 온도 비교
			// ── 1) NaN·Inf 제거
			double[] neighborDiff   = new double[SectorCount];
			bool[]   neighborIssue  = new bool[SectorCount];
			int      neighborBadCnt = 0;

			for (int i = 0; i < SectorCount; i++)
			{
				int next = (i + 1) % SectorCount;
				double diff = Math.Abs(sectorTemp[i] - sectorTemp[next]);

				neighborDiff[ i ] = diff;
				neighborIssue[ i ] = diff > infraredCameraModel.NeighborDiffLim;

				if (neighborIssue[ i ]) neighborBadCnt++;
			}
			sw.Stop( );
			Logger.WriteLine( $"SectorCalculate: {sw.ElapsedMilliseconds} ms" );

			return (neighborBadCnt, sectorTemp);  // 섹터별 온도 차이로 불량 섹터 수 반환
		}

		//private static int DrawSetors( MIL_ID MilOverlayImage, int SectorCount, double Radius, double SmallRadius, double DetectCirclrCenterX, double DetectCirclrCenterY )
		//{
		//	///불량섹터 표시
		//	double     span       = 360.0 / SectorCount;   // 6°
		//	for (int i = 0; i < SectorCount; i++)
		//	{
		//		MIL_ID gctx   = ReusableMilBuffers.AcquireGctx(i);     // 섹터별 전용 gctx
		//		double start = (i - 1) * span;
		//		/* --- 4) 분할선 표시 (원래 코드 유지) --- */
		//		double a0 = start * Math.PI / 180.0;
		//		double a1 = (start + span) * Math.PI / 180.0;

		//		double x1 = DetectCirclrCenterX + SmallRadius * Math.Cos(a0);
		//		double y1 = DetectCirclrCenterY + SmallRadius * Math.Sin(a0);
		//		double x2 = DetectCirclrCenterX + Radius      * Math.Cos(a0);
		//		double y2 = DetectCirclrCenterY + Radius      * Math.Sin(a0);

		//		double x3 = DetectCirclrCenterX + SmallRadius * Math.Cos(a1);
		//		double y3 = DetectCirclrCenterY + SmallRadius * Math.Sin(a1);
		//		double x4 = DetectCirclrCenterX + Radius      * Math.Cos(a1);
		//		double y4 = DetectCirclrCenterY + Radius      * Math.Sin(a1);

		//		MIL.MgraLine( gctx, MilOverlayImage, x1, y1, x2, y2 );
		//		MIL.MgraLine( gctx, MilOverlayImage, x3, y3, x4, y4 );
		//	}

		//	//for (int i = 0; i < 36; i++)
		//	//{
		//	//	if (!neighborIssue[ i ]) continue;

		//	//	double span  = 10.0;
		//	//	double start = (i - 1) * span;
		//	//	double end   = i * span;

		//	//	// 빨강색으로 두꺼운 선
		//	//	MIL.MgraColor( GraphicsContext, MIL.M_COLOR_RED );
		//	//	MIL.MgraArcFill( GraphicsContext, MilOverlayImage,
		//	//		DetectCirclrCenterX, DetectCirclrCenterY,
		//	//		Radius, Radius, start, end );
		//	//	MIL.MgraColor( GraphicsContext, MIL.M_COLOR_WHITE );
		//	//}
		//	return 0;
		//}
		/// <summary>
		/// 60·48·36 등 원하는 개수의 섹터 분할선을
		/// 8-인수 MgraLines 한 번으로 그린다.
		/// ─ MIL .NET Wrapper 10.40~10.60 (xStart·yStart·xEnd·yEnd 배열 버전)
		/// </summary>
		private static int DrawSetors(
				MIL_ID MilOverlayImage,
				int sectorCount,
				double radius,
				double smallRadius,
				double cx, double cy )
		{
			/* ── 1) 반지름·센터가 변하지 않으면 다시 그릴 필요 없음 */
			double prevR = double.NaN, prevRin = double.NaN, prevCx = double.NaN, prevCy = double.NaN;
			if (radius == prevR && smallRadius == prevRin && cx == prevCx && cy == prevCy)
				return 0;

			prevR = radius; prevRin = smallRadius; prevCx = cx; prevCy = cy;

			/* ── 2) 선 좌표 배열 준비 -------------- */
			int lineCnt      = sectorCount * 2;          // 안쪽→바깥 + 바깥 호
			double[] xs1     = new double[lineCnt];
			double[] ys1     = new double[lineCnt];
			double[] xe1     = new double[lineCnt];
			double[] ye1     = new double[lineCnt];

			double span      = 2.0 * Math.PI / sectorCount;

			for (int i = 0; i < sectorCount; ++i)
			{
				double a0 = i * span;
				double a1 = a0 + span;

				/* 안쪽 → 바깥 */
				xs1[ 2 * i ] = cx + smallRadius * Math.Cos( a0 );
				ys1[ 2 * i ] = cy + smallRadius * Math.Sin( a0 );
				xe1[ 2 * i ] = cx + radius * Math.Cos( a0 );
				ye1[ 2 * i ] = cy + radius * Math.Sin( a0 );

				/* 바깥 호  (옵션) */
				xs1[ 2 * i + 1 ] = cx + radius * Math.Cos( a0 );
				ys1[ 2 * i + 1 ] = cy + radius * Math.Sin( a0 );
				xe1[ 2 * i + 1 ] = cx + radius * Math.Cos( a1 );
				ye1[ 2 * i + 1 ] = cy + radius * Math.Sin( a1 );
			}

			/* ── 3) 그래픽 컨텍스트는 한 번만 생성 */
			MIL_ID gctx = MIL.M_NULL;
			if (gctx == MIL.M_NULL)
				MIL.MgraAlloc( MilSystem, ref gctx );

			/* ── 4) 오버레이 클리어 후 한 방에 그리기 */

			MIL.MgraLines(
				gctx,                     // ① ContextGraId
				MilOverlayImage,          // ② DestImageBufOrListGraId
				(MIL_INT) lineCnt,         // ③ NumberOfLines    ★ 세 번째!
				xs1, ys1,                   // ④ XPtr, ⑤ YPtr   (시작점들)
				xe1, ye1,                   // ⑥ X2Ptr,⑦ Y2Ptr  (끝점들)
				MIL.M_DEFAULT );           // ⑧ ControlFlag

			MIL.MgraFree( gctx ); // 그래픽 컨텍스트 해제

			return 0;
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
