using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DamoOneVision.Camera
{
	public class MatroxCamera : ICamera
	{
		// Matrox SDK 관련 필드
		private MIL_ID MilApplication = MIL.M_NULL;
		private MIL_ID MilSystem = MIL.M_NULL;
		private MIL_ID MilDigitizer = MIL.M_NULL;
		private MIL_ID MilImage = MIL.M_NULL;

		public bool Connect( )
		{
			try
			{
				// MIL 초기화 및 자원 할당
				MIL.MappAlloc( MIL.M_DEFAULT, ref MilApplication );

				MIL.MappControl( MIL.M_ERROR, MIL.M_PRINT_DISABLE );
				MIL.MappControl( MIL.M_ERROR, MIL.M_THROW_EXCEPTION );

				MIL.MsysAlloc( MIL.M_SYSTEM_GIGE_VISION, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilSystem );
				MIL.MdigAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref MilDigitizer );
				MIL.MbufAlloc2d( MilSystem,
								MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_X, MIL.M_NULL ),
								MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_Y, MIL.M_NULL ),
								8 + MIL.M_UNSIGNED,
								MIL.M_IMAGE + MIL.M_DISP + MIL.M_GRAB,
								ref MilImage );

				// 프레임 레이트 설정 (예: 30 FPS)
				MIL.MdigControl( MilDigitizer, MIL.M_FRAME_RATE, 30.0 );

				return true;
			}
			catch (Exception ex)
			{
				// 예외 처리
				MessageBox.Show( $"Matrox 카메라 연결 오류: {ex.Message}" );
				return false;
			}
		}

		public void Disconnect( )
		{
			try
			{
				// MIL 자원 해제
				if (MilImage != MIL.M_NULL)
				{
					MIL.MbufFree( MilImage );
					MilImage = MIL.M_NULL;
				}
				if (MilDigitizer != MIL.M_NULL)
				{
					MIL.MdigFree( MilDigitizer );
					MilDigitizer = MIL.M_NULL;
				}
				if (MilSystem != MIL.M_NULL)
				{
					MIL.MsysFree( MilSystem );
					MilSystem = MIL.M_NULL;
				}
				if (MilApplication != MIL.M_NULL)
				{
					MIL.MappFree( MilApplication );
					MilApplication = MIL.M_NULL;
				}
			}
			catch (Exception ex)
			{
				// 예외 처리
				MessageBox.Show( $"Matrox 카메라 연결 종료 오류: {ex.Message}" );
			}
		}

		public byte[ ] CaptureImage( )
		{
			try
			{
				MIL.MdigGrab( MilDigitizer, MilImage );

				int width = GetWidth();
				int height = GetHeight();
				int bufferSize = width * height;
				byte[] pixelData = new byte[bufferSize];

				MIL.MbufGet( MilImage, pixelData );

				return pixelData;
			}
			catch (Exception ex)
			{
				// 예외 처리
				MessageBox.Show( $"Matrox 이미지 획득 오류: {ex.Message}" );
				return null;
			}
		}

		public int GetWidth( )
		{
			return (int) MIL.MbufInquire( MilImage, MIL.M_SIZE_X, MIL.M_NULL );
		}

		public int GetHeight( )
		{
			return (int) MIL.MbufInquire( MilImage, MIL.M_SIZE_Y, MIL.M_NULL );
		}
	}
}
