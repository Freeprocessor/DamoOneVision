using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamoOneVision.Camera
{
	internal class Conversion
	{

		private MIL_ID MilSystem = MIL.M_NULL;
		private MIL_ID MilDisplay = MIL.M_NULL;
		private MIL_ID MilImageRGB = MIL.M_NULL;
		private MIL_ID MilImageHSV = MIL.M_NULL;
		private MIL_ID MilImageBin = MIL.M_NULL;

		public Conversion( )
		{

			MilSystem = MILContext.Instance.MilSystem;

		}


		private void RunHSVThreshold( double hMin, double hMax, double sMin, double sMax, double vMin, double vMax)
		{
			// HSV 임계값 설정 (원하는 값으로 수정 가능)
			//double hMin = 0.0;    // Hue 최소값
			//double hMax = 180.0;  // Hue 최대값
			//double sMin = 0.0;    // Saturation 최소값
			//double sMax = 255.0;  // Saturation 최대값
			//double vMin = 0.0;    // Value 최소값
			//double vMax = 255.0;  // Value 최대값

			try
			{
				// RGB 이미지 로드
				// TODo: 이미지 파일 경로를 수정하세요.
				MilImageRGB = MIL.MbufRestore( "IMG_1160.jpg", MilSystem, MIL.M_NULL );

				// 이미지 크기 및 채널 수 가져오기
				int sizeX = (int)MIL.MbufInquire(MilImageRGB, MIL.M_SIZE_X, MIL.M_NULL);
				int sizeY = (int)MIL.MbufInquire(MilImageRGB, MIL.M_SIZE_Y, MIL.M_NULL);
				int bands = (int)MIL.MbufInquire(MilImageRGB, MIL.M_SIZE_BAND, MIL.M_NULL);

				// HSV 이미지 버퍼 할당
				MilImageHSV = MIL.MbufAllocColor( MilSystem, bands, sizeX, sizeY, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );

				// 색상 공간 변환 (RGB -> HSV)
				MIL.MimConvert( MilImageRGB, MilImageHSV, MIL.M_RGB_TO_HSV );

				// 이진화된 이미지 버퍼 할당 (단일 채널)
				MilImageBin = MIL.MbufAlloc2d( MilSystem, sizeX, sizeY, 1 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL );

				// HSV 채널 분리
				MIL_ID MilImageH = MIL.MbufChildColor(MilImageHSV, 0, MIL.M_NULL);
				MIL_ID MilImageS = MIL.MbufChildColor(MilImageHSV, 1, MIL.M_NULL);
				MIL_ID MilImageV = MIL.MbufChildColor(MilImageHSV, 2, MIL.M_NULL);

				// 각 채널에 대한 이진화 결과를 저장할 버퍼 할당
				MIL_ID MilBinH = MIL.MbufAlloc2d(MilSystem, sizeX, sizeY, 1 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);
				MIL_ID MilBinS = MIL.MbufAlloc2d(MilSystem, sizeX, sizeY, 1 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);
				MIL_ID MilBinV = MIL.MbufAlloc2d(MilSystem, sizeX, sizeY, 1 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, MIL.M_NULL);

				// H 채널 이진화
				MIL.MimBinarize( MilImageH, MilBinH, MIL.M_IN_RANGE, hMin, hMax);

				// S 채널 이진화
				MIL.MimBinarize( MilImageS, MilBinS, MIL.M_IN_RANGE, sMin, sMax );

				// V 채널 이진화
				MIL.MimBinarize( MilImageV, MilBinV, MIL.M_IN_RANGE, vMin, vMax );

				// 이진화된 결과들을 논리적 AND 연산으로 결합
				MIL.MimArith( MilBinH, MilBinS, MilImageBin, MIL.M_AND );
				MIL.MimArith( MilImageBin, MilBinV, MilImageBin, MIL.M_AND );

				// WPF에서 이미지를 표시하기 위해 픽셀 데이터를 가져옴
				//DisplayMilImage( MilImageBin, sizeX, sizeY );

				// 자식 버퍼 해제
				MIL.MbufFree( MilImageH );
				MIL.MbufFree( MilImageS );
				MIL.MbufFree( MilImageV );
				MIL.MbufFree( MilBinH );
				MIL.MbufFree( MilBinS );
				MIL.MbufFree( MilBinV );
			}
			finally
			{
				// MIL 리소스 해제
				if (MilImageRGB != MIL.M_NULL) MIL.MbufFree( MilImageRGB );
				if (MilImageHSV != MIL.M_NULL) MIL.MbufFree( MilImageHSV );
				if (MilImageBin != MIL.M_NULL) MIL.MbufFree( MilImageBin );
			}
		}

	}
}
