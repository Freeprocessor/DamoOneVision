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

		/*
		//public static void RunHSVThreshold( double hMin, double hMax, double sMin, double sMax, double vMin, double vMax, byte[ ] pixelData )
		//{
		//	MIL_ID MilImageRGB = MIL.M_NULL;
		//	MIL_ID MilImageHSV = MIL.M_NULL;
		//	MIL_ID MilImageBin = MIL.M_NULL;
		//	MIL_ID MilMaskRGB = MIL.M_NULL;
		//	MIL_ID MilImageMasked = MIL.M_NULL;

		//	try
		//	{
		//		Debug.WriteLine( "RunHSVThreshold 시작" );

		//		// RGB 이미지 버퍼 할당 및 데이터 설정
		//		MilImageRGB = MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height,
		//			8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
		//		if (MilImageRGB == MIL.M_NULL)
		//		{
		//			Debug.WriteLine( "MilImageRGB 할당 실패" );
		//			return;
		//		}
		//		MIL.MbufPut( MilImageRGB, pixelData );
		//		Debug.WriteLine( "RGB 이미지 버퍼 할당 및 데이터 설정 완료" );

		//		// HSV 이미지 버퍼 할당
		//		MilImageHSV = MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height,
		//			8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
		//		if (MilImageHSV == MIL.M_NULL)
		//		{
		//			Debug.WriteLine( "MilImageHSV 할당 실패" );
		//			MIL.MbufFree( MilImageRGB );
		//			return;
		//		}
		//		Debug.WriteLine( "HSV 이미지 버퍼 할당 완료" );

		//		// 색상 공간 변환 (RGB -> HSV)
		//		MIL.MimConvert( MilImageRGB, MilImageHSV, MIL.M_RGB_TO_HSV );
		//		Debug.WriteLine( "RGB -> HSV 변환 완료" );

		//		// HSV 채널 분리
		//		MIL_ID MilImageH = MIL.MbufChildColor(MilImageHSV, 0, MIL.M_NULL);
		//		MIL_ID MilImageS = MIL.MbufChildColor(MilImageHSV, 1, MIL.M_NULL);
		//		MIL_ID MilImageV = MIL.MbufChildColor(MilImageHSV, 2, MIL.M_NULL);
		//		if (MilImageH == MIL.M_NULL || MilImageS == MIL.M_NULL || MilImageV == MIL.M_NULL)
		//		{
		//			Debug.WriteLine( "HSV 채널 분리 실패" );
		//			MIL.MbufFree( MilImageRGB );
		//			MIL.MbufFree( MilImageHSV );
		//			return;
		//		}
		//		Debug.WriteLine( "HSV 채널 분리 완료" );

		//		// 이진화된 이미지 버퍼 할당 (단일 채널)
		//		MilImageBin = MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height,
		//			8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
		//		if (MilImageBin == MIL.M_NULL)
		//		{
		//			Debug.WriteLine( "MilImageBin 할당 실패" );
		//			CleanUp( MilImageRGB, MilImageHSV );
		//			MIL.MbufFree( MilImageH );
		//			MIL.MbufFree( MilImageS );
		//			MIL.MbufFree( MilImageV );
		//			return;
		//		}
		//		Debug.WriteLine( "이진화된 이미지 버퍼 할당 완료" );

		//		// H, S, V 채널에 대한 이진화 수행
		//		MIL_ID MilBinH = MIL.MbufAlloc2d(MilSystem, MILContext.Width, MILContext.Height,
		//	8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);
		//		MIL_ID MilBinS = MIL.MbufAlloc2d(MilSystem, MILContext.Width, MILContext.Height,
		//	8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);
		//		MIL_ID MilBinV = MIL.MbufAlloc2d(MilSystem, MILContext.Width, MILContext.Height,
		//	8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);

		//		MIL.MimBinarize( MilImageH, MilBinH, MIL.M_IN_RANGE, hMin, hMax );
		//		MIL.MimBinarize( MilImageS, MilBinS, MIL.M_IN_RANGE, sMin, sMax );
		//		MIL.MimBinarize( MilImageV, MilBinV, MIL.M_IN_RANGE, vMin, vMax );

		//		// 자식 버퍼 해제 (MilImageH, MilImageS, MilImageV)
		//		MIL.MbufFree( MilImageH );
		//		MIL.MbufFree( MilImageS );
		//		MIL.MbufFree( MilImageV );

		//		// 이진화된 결과들을 논리적 AND 연산으로 결합
		//		MIL.MimArith( MilBinH, MilBinS, MilImageBin, MIL.M_AND );
		//		MIL.MimArith( MilImageBin, MilBinV, MilImageBin, MIL.M_AND );
		//		Debug.WriteLine( "HSV 채널 이진화 및 결합 완료" );

		//		// 이진화된 채널 버퍼 해제
		//		MIL.MbufFree( MilBinH );
		//		MIL.MbufFree( MilBinS );
		//		MIL.MbufFree( MilBinV );

		//		// 이진화된 마스크를 0과 1로 정규화
		//		MIL.MimBinarize( MilImageBin, MilImageBin, MIL.M_NOT_EQUAL, 0, MIL.M_NULL );

		//		// 마스크 이미지를 RGB로 확장
		//		MilMaskRGB = MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height,
		//			8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
		//		if (MilMaskRGB == MIL.M_NULL)
		//		{
		//			Debug.WriteLine( "MilMaskRGB 할당 실패" );
		//			CleanUp( MilImageRGB, MilImageHSV, MilImageBin );
		//			return;
		//		}

		//		// 마스크 이미지를 각 밴드에 복사
		//		MIL_ID MilMaskBand0 = MIL.MbufChildColor(MilMaskRGB, 0, MIL.M_NULL);
		//		MIL_ID MilMaskBand1 = MIL.MbufChildColor(MilMaskRGB, 1, MIL.M_NULL);
		//		MIL_ID MilMaskBand2 = MIL.MbufChildColor(MilMaskRGB, 2, MIL.M_NULL);

		//		MIL.MbufCopy( MilImageBin, MilMaskBand0 );
		//		MIL.MbufCopy( MilImageBin, MilMaskBand1 );
		//		MIL.MbufCopy( MilImageBin, MilMaskBand2 );

		//		// 마스크 밴드 버퍼 해제 (자식 버퍼)
		//		MIL.MbufFree( MilMaskBand0 );
		//		MIL.MbufFree( MilMaskBand1 );
		//		MIL.MbufFree( MilMaskBand2 );

		//		MIL.MbufFree( MilImageBin ); // 이제 MilImageBin 버퍼도 해제 가능
		//		Debug.WriteLine( "마스크 이미지를 RGB로 확장 완료" );

		//		// 마스킹된 이미지를 저장할 버퍼 할당
		//		MilImageMasked = MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height,
		//			8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
		//		if (MilImageMasked == MIL.M_NULL)
		//		{
		//			Debug.WriteLine( "MilImageMasked 할당 실패" );
		//			CleanUp( MilImageRGB, MilImageHSV, MilMaskRGB );
		//			return;
		//		}

		//		// 원본 이미지와 마스크 이미지 곱하기
		//		MIL.MimArith( MilImageRGB, MilMaskRGB, MilImageMasked, MIL.M_MULT );
		//		Debug.WriteLine( "마스킹 작업 완료" );

		//		// 마스크 RGB 버퍼 해제
		//		MIL.MbufFree( MilMaskRGB );

		//		// 마스킹된 이미지의 픽셀 데이터를 추출
		//		byte[] maskedPixelData = ExtractPixelData(MilImageMasked, (int)MILContext.Width, (int)MILContext.Height);
		//		Debug.WriteLine( "마스킹된 픽셀 데이터 추출 완료" );

		//		// 이벤트 발생
		//		OnImageProcessed( maskedPixelData, (int) MILContext.Width, (int) MILContext.Height, PixelFormats.Rgb24 );
		//		Debug.WriteLine( "마스킹된 이미지 처리 이벤트 발생" );
		//	}
		//	catch (Exception ex)
		//	{
		//		Debug.WriteLine( $"RunHSVThreshold에서 예외 발생: {ex.Message}" );
		//	}
		//	finally
		//	{
		//		// MIL 리소스 해제
		//		if (MilImageMasked != MIL.M_NULL) MIL.MbufFree( MilImageMasked );
		//		if (MilImageHSV != MIL.M_NULL) MIL.MbufFree( MilImageHSV );
		//		if (MilImageRGB != MIL.M_NULL) MIL.MbufFree( MilImageRGB );
		//	}
		//}

		//public static void RunHSVThreshold( double hMin, double hMax, double sMin, double sMax, double vMin, double vMax, byte[ ] pixelData )
		//{
		//	MIL_ID MilImageRGB = MIL.M_NULL;
		//	MIL_ID MilImageHSL = MIL.M_NULL;
		//	MIL_ID MilImageBin = MIL.M_NULL;

		//	MIL_ID MilImageH = MIL.M_NULL;
		//	MIL_ID MilImageS = MIL.M_NULL;
		//	MIL_ID MilImageL = MIL.M_NULL;

		//	MIL_ID MilBinH = MIL.M_NULL;
		//	MIL_ID MilBinS = MIL.M_NULL;
		//	MIL_ID MilBinL = MIL.M_NULL;

		//	MIL_ID MilImageMasked = MIL.M_NULL;

		//	try
		//	{
		//		Debug.WriteLine( "RunHSVThreshold 시작" );

		//		// RGB 이미지 버퍼 할당 및 데이터 설정
		//		MilImageRGB = MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height,
		//			8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );

		//		if (MilImageRGB == MIL.M_NULL)
		//		{
		//			Debug.WriteLine( "MilImageRGB 할당 실패" );
		//			return;
		//		}
		//		MIL.MbufPut( MilImageRGB, pixelData );

		//		Debug.WriteLine( "RGB 이미지 버퍼 할당 및 데이터 설정 완료" );

		//		// HSV 이미지 버퍼 할당
		//		MilImageHSL = MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height,
		//			8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
		//		if (MilImageHSL == MIL.M_NULL)
		//		{
		//			Debug.WriteLine( "MilImageHSL 할당 실패" );
		//			MIL.MbufFree( MilImageRGB );
		//			return;
		//		}
		//		Debug.WriteLine( "HSL 이미지 버퍼 할당 완료" );

		//		// 색상 공간 변환 (RGB -> HSV)
		//		MIL.MimConvert( MilImageRGB, MilImageHSL, MIL.M_RGB_TO_HSL );
		//		Debug.WriteLine( "RGB -> HSL 변환 완료" );

		//		MIL.MbufChildColor2d( MilImageHSL, MIL.M_HUE, 0, 0, MILContext.Width, MILContext.Height, ref MilImageH );
		//		MIL.MbufChildColor2d( MilImageHSL, MIL.M_SATURATION, 0, 0, MILContext.Width, MILContext.Height, ref MilImageS );
		//		MIL.MbufChildColor2d( MilImageHSL, MIL.M_LUMINANCE, 0, 0, MILContext.Width, MILContext.Height, ref MilImageL );


		//		if (MilImageH == MIL.M_NULL || MilImageS == MIL.M_NULL || MilImageL == MIL.M_NULL)
		//		{
		//			Debug.WriteLine( "HSV 채널 분리 실패" );
		//			MIL.MbufFree( MilImageRGB );
		//			MIL.MbufFree( MilImageHSL );
		//			return;
		//		}
		//		Debug.WriteLine( "HSV 채널 분리 완료" );



		//		// 이진화된 이미지 버퍼 할당 (단일 채널)
		//		MilImageBin = MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height,
		//			8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
		//		if (MilImageBin == MIL.M_NULL)
		//		{
		//			Debug.WriteLine( "MilImageBin 할당 실패" );
		//			CleanUp( MilImageRGB, MilImageHSL );
		//			MIL.MbufFree( MilImageH );
		//			MIL.MbufFree( MilImageS );
		//			MIL.MbufFree( MilImageL );
		//			return;
		//		}
		//		Debug.WriteLine( "이진화된 이미지 버퍼 할당 완료" );

		//		MIL.MbufClone( MilImageH, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilBinH );
		//		MIL.MbufClone( MilImageH, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilBinS );
		//		MIL.MbufClone( MilImageH, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilBinL );

		//		// H, S, V 채널에 대한 이진화 수행
		//	//	MIL_ID MilBinH = MIL.MbufAlloc2d(MilSystem, MILContext.Width, MILContext.Height,
		//	//8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);
		//	//	MIL_ID MilBinS = MIL.MbufAlloc2d(MilSystem, MILContext.Width, MILContext.Height,
		//	//8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);
		//	//	MIL_ID MilBinV = MIL.MbufAlloc2d(MilSystem, MILContext.Width, MILContext.Height,
		//	//8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);

		//		MIL.MimBinarize( MilImageH, MilBinH, MIL.M_FIXED + MIL.M_IN_RANGE, hMin, hMax );
		//		MIL.MimBinarize( MilImageS, MilBinS, MIL.M_FIXED + MIL.M_IN_RANGE, sMin, sMax );
		//		MIL.MimBinarize( MilImageL, MilBinL, MIL.M_FIXED + MIL.M_IN_RANGE, vMin, vMax );

		//		// 자식 버퍼 해제 (MilImageH, MilImageS, MilImageV)


		//		// 이진화된 결과들을 논리적 AND 연산으로 결합
		//		MIL.MimArith( MilBinH, MilBinS, MilImageBin, MIL.M_AND );
		//		MIL.MimArith( MilImageBin, MilBinL, MilImageBin, MIL.M_AND );
		//		Debug.WriteLine( "HSV 채널 이진화 및 결합 완료" );

		//		MIL.MbufFree( MilImageH );
		//		MIL.MbufFree( MilImageS );
		//		MIL.MbufFree( MilImageL );

		//		// 이진화된 채널 버퍼 해제
		//		MIL.MbufFree( MilBinH );
		//		MIL.MbufFree( MilBinS );
		//		MIL.MbufFree( MilBinL );

		//		// 이진화된 마스크를 0과 1로 정규화
		//		//MIL.MimBinarize( MilImageBin, MilImageBin, MIL.M_NOT_EQUAL, 0, MIL.M_NULL );

		//		// 마스킹된 이미지를 저장할 버퍼 할당
		//		if (MilImageMasked == MIL.M_NULL)
		//		{
		//			Debug.WriteLine( "MilImageMasked 할당 실패" );
		//			CleanUp( MilImageRGB, MilImageHSL );
		//			return;
		//		}

		//		MIL.MimConvert( MilImageBin, MilImageMasked, MIL.M_L_TO_RGB );

		//		// 원본 이미지와 마스크 이미지 곱하기
		//		MIL.MimArith( MilImageRGB, MilImageBin, MilImageMasked, MIL.M_MULT );
		//		Debug.WriteLine( "마스킹 작업 완료" );

		//		MIL.MbufFree( MilImageBin ); // 이제 MilImageBin 버퍼도 해제 가능

		//		// 마스킹된 이미지의 픽셀 데이터를 추출
		//		//byte[] maskedPixelData = ExtractPixelData(MilImageMasked, (int)MILContext.Width, (int)MILContext.Height);
		//		byte[] maskedPixelData = ExtractPixelData(MilImageMasked, (int)MILContext.Width, (int)MILContext.Height);
		//		Debug.WriteLine( "마스킹된 픽셀 데이터 추출 완료" );

		//		// 이벤트 발생
		//		OnImageProcessed( maskedPixelData, (int) MILContext.Width, (int) MILContext.Height, PixelFormats.Rgb24 );
		//		Debug.WriteLine( "마스킹된 이미지 처리 이벤트 발생" );
		//	}
		//	catch (Exception ex)
		//	{
		//		Debug.WriteLine( $"RunHSVThreshold에서 예외 발생: {ex.Message}" );
		//	}
		//	finally
		//	{
		//		// MIL 리소스 해제
		//		if (MilImageMasked != MIL.M_NULL) MIL.MbufFree( MilImageMasked );
		//		if (MilImageHSL != MIL.M_NULL) MIL.MbufFree( MilImageHSL );
		//		if (MilImageRGB != MIL.M_NULL) MIL.MbufFree( MilImageRGB );
		//	}
		//}

		//public static void RunHSLThreshold( double hMin, double hMax, double sMin, double sMax, double vMin, double vMax, byte[ ] pixelData )
		//{
		//	MIL_ID MilImageRGB = MIL.M_NULL;
		//	MIL_ID MilImageHSL = MIL.M_NULL;
		//	MIL_ID MilImageBin = MIL.M_NULL;

		//	MIL_ID MilImageH = MIL.M_NULL;
		//	MIL_ID MilImageS = MIL.M_NULL;
		//	MIL_ID MilImageL = MIL.M_NULL;

		//	MIL_ID MilBinH = MIL.M_NULL;
		//	MIL_ID MilBinS = MIL.M_NULL;
		//	MIL_ID MilBinL = MIL.M_NULL;
		//	MIL_ID MilBinHS = MIL.M_NULL;

		//	MIL_ID MilImageMasked = MIL.M_NULL;

		//	try
		//	{
		//		Debug.WriteLine( "RunHSVThreshold 시작" );

		//		// RGB 이미지 버퍼 할당 및 데이터 설정
		//		MilImageRGB = MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height,
		//			8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );

		//		if (MilImageRGB == MIL.M_NULL)
		//		{
		//			Debug.WriteLine( "MilImageRGB 할당 실패" );
		//			return;
		//		}
		//		MIL.MbufPut( MilImageRGB, pixelData );

		//		Debug.WriteLine( "RGB 이미지 버퍼 할당 및 데이터 설정 완료" );

		//		// HSV 이미지 버퍼 할당
		//		MilImageHSL = MIL.MbufAllocColor( MilSystem, 3, MILContext.Width, MILContext.Height,
		//			8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );

		//		MIL.MbufClear( MilImageHSL, MIL.M_COLOR_BLACK );

		//		MIL.MbufChildColor2d( MilImageHSL, MIL.M_HUE, 0, 0, MILContext.Width, MILContext.Height, ref MilImageH );
		//		MIL.MbufChildColor2d( MilImageHSL, MIL.M_SATURATION, 0, 0, MILContext.Width, MILContext.Height, ref MilImageS );
		//		MIL.MbufChildColor2d( MilImageHSL, MIL.M_LUMINANCE, 0, 0, MILContext.Width, MILContext.Height, ref MilImageL );



		//		MIL.MbufClone( MilImageH, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilBinH );

		//		// Post-Alloc Block for MilBinH
		//		MIL.MbufClear( MilBinH, 0.0 );

		//		MIL.MbufClone( MilImageS, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilBinS );

		//		// Post-Alloc Block for MilBinS
		//		MIL.MbufClear( MilBinS, 0.0 );

		//		MIL.MbufClone( MilImageL, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilBinL );

		//		// Post-Alloc Block for MilBinL
		//		MIL.MbufClear( MilBinL, 0.0 );

		//		MIL.MbufClone( MilBinS, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilBinHS );

		//		// Post-Alloc Block for BinHS
		//		MIL.MbufClear( MilBinHS, 0.0 );

		//		MIL.MbufClone( MilBinL, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilImageBin );

		//		// Post-Alloc Block for BinHS
		//		MIL.MbufClear( MilImageBin, 0.0 );

		//		MIL.MbufAllocColor( MilSystem, 1, MILContext.Width, MILContext.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref MilImageMasked );

		//		// Post-Alloc Block for MimArith's destination
		//		MIL.MbufClear( MilImageMasked, 0.0 );



		//		// 색상 공간 변환 (RGB -> HSV)
		//		MIL.MimConvert( MilImageRGB, MilImageHSL, MIL.M_RGB_TO_HSL );
		//		//MIL.MimConvert( MilImageHSL, MilImageRGB, MIL.M_HSL_TO_RGB );
		//		Debug.WriteLine( "RGB -> HSL 변환 완료" );



		//		MIL.MimBinarize( MilImageH, MilBinH, MIL.M_FIXED + MIL.M_IN_RANGE, hMin, hMax );
		//		MIL.MimBinarize( MilImageS, MilBinS, MIL.M_FIXED + MIL.M_IN_RANGE, sMin, sMax );
		//		MIL.MimBinarize( MilImageL, MilBinL, MIL.M_FIXED + MIL.M_IN_RANGE, vMin, vMax );

		//		//int width = 0;
		//		//int height = 0;
		//		//int sizeBit = 0;
		//		//int nbBands = 0;

		//		//MIL.MbufInquire( MilImageH, MIL.M_WIDTH, ref width );
		//		//MIL.MbufInquire( MilImageH, MIL.M_HEIGHT, ref height );
		//		//MIL.MbufInquire( MilImageH, MIL.M_SIZE_BIT, ref sizeBit );
		//		//MIL.MbufInquire( MilImageH, MIL.M_SIZE_BAND, ref nbBands );

		//		// 자식 버퍼 해제 (MilImageH, MilImageS, MilImageV)


		//		// 이진화된 결과들을 논리적 AND 연산으로 결합
		//		MIL.MimArith( MilBinH, MilBinS, MilBinHS, MIL.M_AND );
		//		MIL.MimArith( MilBinHS, MilBinL, MilImageBin, MIL.M_AND );
		//		//MIL.MimArith( MilImageRGB, MilImageBin, MilImageMasked, MIL.M_AND );
		//		Debug.WriteLine( "HSL 채널 이진화 및 결합 완료" );



		//		// 원본 이미지와 마스크 이미지 곱하기
		//		MIL.MimArith( MilImageRGB, MilImageBin, MilImageMasked, MIL.M_MULT );
		//		Debug.WriteLine( "마스킹 작업 완료" );

		//		//MIL.MimConvert( MilImageMasked, MilImageMasked, MIL.M_HSL_TO_RGB );



		//		// 마스킹된 이미지의 픽셀 데이터를 추출
		//		//byte[] maskedPixelData = ExtractPixelData(MilImageMasked, (int)MILContext.Width, (int)MILContext.Height);
		//		byte[] maskedPixelData = ExtractPixelData(MilImageMasked, (int)MILContext.Width, (int)MILContext.Height);
		//		Debug.WriteLine( "마스킹된 픽셀 데이터 추출 완료" );

		//		// 이벤트 발생
		//		OnImageProcessed( maskedPixelData, (int) MILContext.Width, (int) MILContext.Height, PixelFormats.Rgb24 );
		//		Debug.WriteLine( "마스킹된 이미지 처리 이벤트 발생" );

		//		MIL.MbufFree( MilImageBin ); // 이제 MilImageBin 버퍼도 해제 가능
		//		MIL.MbufFree( MilImageH );
		//		MIL.MbufFree( MilImageS );
		//		MIL.MbufFree( MilImageL );

		//		// 이진화된 채널 버퍼 해제
		//		MIL.MbufFree( MilBinH );
		//		MIL.MbufFree( MilBinS );
		//		MIL.MbufFree( MilBinL );
		//		MIL.MbufFree( MilBinHS );
		//	}
		//	catch (Exception ex)
		//	{
		//		Debug.WriteLine( $"RunHSVThreshold에서 예외 발생: {ex.Message}" );
		//	}
		//	finally
		//	{
		//		// MIL 리소스 해제
		//		if (MilImageMasked != MIL.M_NULL) MIL.MbufFree( MilImageMasked );
		//		if (MilImageHSL != MIL.M_NULL) MIL.MbufFree( MilImageHSL );
		//		if (MilImageRGB != MIL.M_NULL) MIL.MbufFree( MilImageRGB );
		//	}
		//}
		*/
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



		private static void CleanUp( params MIL_ID[ ] milIds )
		{
			foreach (var milId in milIds)
			{
				if (milId != MIL.M_NULL)
				{
					MIL.MbufFree( milId );
				}
			}
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
