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
		private static MIL_ID MilImageRGB = MIL.M_NULL;
		private static MIL_ID MilImageHSV = MIL.M_NULL;
		private static MIL_ID MilImageBin = MIL.M_NULL;

		// 이미지 처리 완료 시 발생하는 이벤트
		//
		public static event EventHandler<ImageProcessedEventArgs> ImageProcessed;

		static Conversion( )
		{

			MilSystem = MILContext.Instance.MilSystem;

		}


		private static void OnImageProcessed( byte[ ] processedPixelData, int width, int height, PixelFormat pixelFormat )
		{
			ImageProcessed?.Invoke( null, new ImageProcessedEventArgs( processedPixelData, width, height, pixelFormat ) );
		}


		public static void RunHSVThreshold( double hMin, double hMax, double sMin, double sMax, double vMin, double vMax, byte[ ] pixelData )
		{

			try
			{

				Debug.WriteLine( "RunHSVThreshold 시작" );

				// RGB 이미지 버퍼 할당 및 데이터 설정
				MilImageRGB = MIL.MbufAllocColor( MilSystem, MILContext.NbBands, MILContext.Width, MILContext.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
				MIL.MbufPut( MilImageRGB, pixelData ); // pixelData를 MilImageRGB에 전송
				Debug.WriteLine( "RGB 이미지 버퍼 할당 및 데이터 설정 완료" );

				// HSV 이미지 버퍼 할당
				MilImageHSV = MIL.MbufAllocColor( MilSystem, MILContext.NbBands, MILContext.Width, MILContext.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
				Debug.WriteLine( "HSV 이미지 버퍼 할당 완료" );

				// 색상 공간 변환 (RGB -> HSV)
				MIL.MimConvert( MilImageRGB, MilImageHSV, MIL.M_RGB_TO_HSV );
				Debug.WriteLine( "RGB -> HSV 변환 완료" );

				// 이진화된 이미지 버퍼 할당 (단일 채널)
				MilImageBin = MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height, 1 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );
				Debug.WriteLine( "이진화된 이미지 버퍼 할당 완료" );

				// HSV 채널 분리
				MIL_ID MilImageH = MIL.MbufChildColor(MilImageHSV, 0, MIL.M_NULL);
				MIL_ID MilImageS = MIL.MbufChildColor(MilImageHSV, 1, MIL.M_NULL);
				MIL_ID MilImageV = MIL.MbufChildColor(MilImageHSV, 2, MIL.M_NULL);
				Debug.WriteLine( "HSV 채널 분리 완료" );

				// 각 채널에 대한 이진화 결과를 저장할 버퍼 할당
				MIL_ID MilBinH = MIL.MbufAlloc2d(MilSystem, MILContext.Width, MILContext.Height, 1 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);
				MIL_ID MilBinS = MIL.MbufAlloc2d(MilSystem, MILContext.Width, MILContext.Height, 1 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);
				MIL_ID MilBinV = MIL.MbufAlloc2d(MilSystem, MILContext.Width, MILContext.Height, 1 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);
				Debug.WriteLine( "이진화 결과 저장용 버퍼 할당 완료" );

				// H 채널 이진화
				MIL.MimBinarize( MilImageH, MilBinH, MIL.M_IN_RANGE, hMin, hMax );
				Debug.WriteLine( "H 채널 이진화 완료" );

				// S 채널 이진화
				MIL.MimBinarize( MilImageS, MilBinS, MIL.M_IN_RANGE, sMin, sMax );
				Debug.WriteLine( "S 채널 이진화 완료" );

				// V 채널 이진화
				MIL.MimBinarize( MilImageV, MilBinV, MIL.M_IN_RANGE, vMin, vMax );
				Debug.WriteLine( "V 채널 이진화 완료" );

				// 이진화된 결과들을 논리적 AND 연산으로 결합
				MIL.MimArith( MilBinH, MilBinS, MilImageBin, MIL.M_AND );
				MIL.MimArith( MilImageBin, MilBinV, MilImageBin, MIL.M_AND );
				Debug.WriteLine( "H, S, V 채널 AND 연산 완료" );

				// 이진화된 픽셀 데이터를 byte[]로 추출
				byte[] processedPixelData = ExtractPixelData(MilImageBin, (int)MILContext.Width, (int)MILContext.Height);
				Debug.WriteLine( "픽셀 데이터 추출 완료" );

				// 이벤트 발생
				OnImageProcessed( processedPixelData, (int) MILContext.Width, (int) MILContext.Height, PixelFormats.Gray8 );
				Debug.WriteLine( "ImageProcessed 이벤트 발생" );

				// 자식 버퍼 해제
				MIL.MbufFree( MilImageH );
				MIL.MbufFree( MilImageS );
				MIL.MbufFree( MilImageV );
				MIL.MbufFree( MilBinH );
				MIL.MbufFree( MilBinS );
				MIL.MbufFree( MilBinV );
			}
			catch (Exception ex)
			{
				// 예외 발생 시 로그 출력
				Debug.WriteLine( $"Exception in RunHSVThreshold: {ex.Message}" );
				// 필요 시 사용자에게 알림 추가
			}
			finally
			{
				//MIL_INT SizeByte = 0;
				//MIL.MbufInquire( MilImageBin, MIL.M_SIZE_BYTE, ref SizeByte );
				//byte[] processedPixelData= new byte[SizeByte]; ; // 실제 구현 필요
				//MIL.MbufGet( MilImageBin, processedPixelData );
				//int width = (int)MILContext.Width;
				//int height = (int)MILContext.Height;
				//PixelFormat pixelFormat = PixelFormats.Gray8; // 예시, 실제 포맷에 맞게 설정

				//OnImageProcessed( processedPixelData, width, height, pixelFormat );
				// MIL 리소스 해제
				if (MilImageRGB != MIL.M_NULL) MIL.MbufFree( MilImageRGB );
				if (MilImageHSV != MIL.M_NULL) MIL.MbufFree( MilImageHSV );
				if (MilImageBin != MIL.M_NULL) MIL.MbufFree( MilImageBin );
				//if (MilImage != MIL.M_NULL) MIL.MbufFree( MilImage );
			}
		}

		private static byte[ ] ExtractPixelData( MIL_ID milImage, int width, int height )
		{
			int size = width * height;
			byte[] pixelData = new byte[size];
			MIL.MbufGet( milImage, pixelData );
			return pixelData;
		}

	}


}
