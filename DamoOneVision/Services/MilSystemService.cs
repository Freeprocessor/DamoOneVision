using DamoOneVision.Camera;
using Matrox.MatroxImagingLibrary;

namespace DamoOneVision.Services
{
	public class MilSystemService : IAsyncDisposable
	{
		public MIL_ID MilSystem;

		// 카메라 Display ID들
		public MIL_ID InfraredDisplay;
		public MIL_ID InfraredConversionDisplay;

		public MIL_ID SideCam1Display;
		public MIL_ID SideCam1ConversionDisplay;

		public MIL_ID SideCam2Display;
		public MIL_ID SideCam2ConversionDisplay;

		public MIL_ID SideCam3Display;
		public MIL_ID SideCam3ConversionDisplay;


		public MilSystemService( )
		{
			MilSystem = MILContext.Instance.MilSystem;

			InitializeDisplays();
		}

		private void InitializeDisplays( )
		{
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref InfraredDisplay );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref InfraredConversionDisplay );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref SideCam1Display );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref SideCam1ConversionDisplay );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref SideCam2Display );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref SideCam2ConversionDisplay );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref SideCam3Display );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref SideCam3ConversionDisplay );

			// 예: 공통 Display Control들
			// Infrared
			SetupDisplay( InfraredDisplay, isInfrared: true );
			SetupDisplay( InfraredConversionDisplay, isInfrared: true );

			// side1
			SetupDisplay( SideCam1Display );
			SetupDisplay( SideCam1ConversionDisplay );

			// side2
			SetupDisplay( SideCam2Display );
			SetupDisplay( SideCam2ConversionDisplay );

			// side3
			SetupDisplay( SideCam3Display );
			SetupDisplay( SideCam3ConversionDisplay );
		}

		private void SetupDisplay( MIL_ID displayId, bool isInfrared = false )
		{
			MIL.MdispControl( displayId, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( displayId, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( displayId, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( displayId, MIL.M_MOUSE_USE, MIL.M_DISABLE );

			if (isInfrared)
			{
				// 필요시 LUT 설정
				MIL.MdispLut( displayId, MIL.M_COLORMAP_JET );
			}
		}


		public async ValueTask DisposeAsync( )
		{
			/*
			// 1. UI 요소의 DisplayId를 MIL.M_NULL로 설정하여 참조 해제
			if (infraredCameraDisplay != null)
			{
				infraredCameraDisplay.DisplayId = MIL.M_NULL;
			}

			if (infraredCameraConversionDisplay != null)
			{
				infraredCameraConversionDisplay.DisplayId = MIL.M_NULL;
			}

			if (mainInfraredCameraDisplay != null)
			{
				mainInfraredCameraDisplay.DisplayId = MIL.M_NULL;
			}

			//if (mainInfraredCameraConversionDisplay != null)
			//{
			//	mainInfraredCameraConversionDisplay.DisplayId = MIL.M_NULL;
			//}

			//if (sideCamera1Display != null)
			//{
			//	sideCamera1Display.DisplayId = MIL.M_NULL;
			//}

			//if (sideCamera1ConversionDisplay != null)
			//{
			//	sideCamera1ConversionDisplay.DisplayId = MIL.M_NULL;
			//}

			//if (mainSideCamera1Display != null)
			//{
			//	mainSideCamera1Display.DisplayId = MIL.M_NULL;
			//}

			//if (mainSideCamera1ConversionDisplay != null)
			//{
			//	mainSideCamera1ConversionDisplay.DisplayId = MIL.M_NULL;
			//}

			//if (sideCamera2Display != null)
			//{
			//	sideCamera2Display.DisplayId = MIL.M_NULL;
			//}

			//if (sideCamera2ConversionDisplay != null)
			//{
			//	sideCamera2ConversionDisplay.DisplayId = MIL.M_NULL;
			//}

			//if (mainSideCamera2Display != null)
			//{
			//	mainSideCamera2Display.DisplayId = MIL.M_NULL;
			//}

			//if (mainSideCamera2ConversionDisplay != null)
			//{
			//	mainSideCamera2ConversionDisplay.DisplayId = MIL.M_NULL;
			//}

			//if (sideCamera3Display != null)
			//{
			//	sideCamera3Display.DisplayId = MIL.M_NULL;
			//}

			//if (sideCamera3ConversionDisplay != null)
			//{
			//	sideCamera3ConversionDisplay.DisplayId = MIL.M_NULL;
			//}

			//if (mainSideCamera3Display != null)
			//{
			//	mainSideCamera3Display.DisplayId = MIL.M_NULL;
			//}

			//if (mainSideCamera3ConversionDisplay != null)
			//{
			//	mainSideCamera3ConversionDisplay.DisplayId = MIL.M_NULL;
			//}
			*/
			// 2. disp 버퍼 해제

			if (InfraredDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( InfraredDisplay );
				InfraredDisplay = MIL.M_NULL;
				Logger.WriteLine( "InfraredDisplay 해제 완료." );
			}

			if (InfraredConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( InfraredConversionDisplay );
				InfraredConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "InfraredConversionDisplay 해제 완료." );
			}

			if (SideCam1Display != MIL.M_NULL)
			{
				MIL.MdispFree( SideCam1Display );
				SideCam1Display = MIL.M_NULL;
				Logger.WriteLine( "SideCam1Display 해제 완료." );
			}

			if (SideCam1ConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( SideCam1ConversionDisplay );
				SideCam1ConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "SideCam1ConversionDisplay 해제 완료." );
			}

			if (SideCam2Display != MIL.M_NULL)
			{
				MIL.MdispFree( SideCam2Display );
				SideCam2Display = MIL.M_NULL;
				Logger.WriteLine( "SideCam2Display 해제 완료." );
			}

			if (SideCam2ConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( SideCam2ConversionDisplay );
				SideCam2ConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "SideCam2ConversionDisplay 해제 완료." );
			}

			if (SideCam3Display != MIL.M_NULL)
			{
				MIL.MdispFree( SideCam3Display );
				SideCam3Display = MIL.M_NULL;
				Logger.WriteLine( "SideCam3Display 해제 완료." );
			}

			if (SideCam3ConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( SideCam3ConversionDisplay );
				SideCam3ConversionDisplay = MIL.M_NULL;
				Logger.WriteLine( "SideCam3ConversionDisplay 해제 완료." );
			}

			/// 어떻게 이미지를 해제할 것인지?
			/// 
			//3.이미지 버퍼 해제
			//if (_infraredCameraImage != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _infraredCameraImage );
			//	_infraredCameraImage = MIL.M_NULL;
			//	Logger.WriteLine( "_infraredCameraImage 해제 완료." );
			//}

			//if (_infraredCameraConversionImage != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _infraredCameraConversionImage );
			//	_infraredCameraConversionImage = MIL.M_NULL;
			//	Logger.WriteLine( "_infraredCameraConversionImage 해제 완료." );
			//}

			//if (_sideCamera1Image != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _sideCamera1Image );
			//	_sideCamera1Image = MIL.M_NULL;
			//	Logger.WriteLine( "_sideCamera1Image 해제 완료." );
			//}

			//if (_sideCamera1ConversionImage != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _sideCamera1ConversionImage );
			//	_sideCamera1ConversionImage = MIL.M_NULL;
			//	Logger.WriteLine( "_sideCamera1ConversionImage 해제 완료." );
			//}

			//if (_sideCamera2Image != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _sideCamera2Image );
			//	_sideCamera2Image = MIL.M_NULL;
			//	Logger.WriteLine( "_sideCamera2Image 해제 완료." );
			//}

			//if (_sideCamera2ConversionImage != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _sideCamera2ConversionImage );
			//	_sideCamera2ConversionImage = MIL.M_NULL;
			//	Logger.WriteLine( "_sideCamera2ConversionImage 해제 완료." );
			//}

			//if (_sideCamera3Image != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _sideCamera3Image );
			//	_sideCamera3Image = MIL.M_NULL;
			//	Logger.WriteLine( "_sideCamera3Image 해제 완료." );
			//}

			//if (_sideCamera3ConversionImage != MIL.M_NULL)
			//{
			//	MIL.MbufFree( _sideCamera3ConversionImage );
			//	_sideCamera3ConversionImage = MIL.M_NULL;
			//	Logger.WriteLine( "_sideCamera3ConversionImage 해제 완료." );
			//}
			//await Task.Delay( 100 );
		}

	}
}
