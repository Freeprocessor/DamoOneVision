using DamoOneVision.Services;
using Matrox.MatroxImagingLibrary;
using System.Diagnostics;

namespace DamoOneVision.Camera
{
	//public class MILContext : IDisposable
	public class MILContext : IDisposable
	//자원 해제를 위한 IDisposable 인터페이스 구현
	//Dispose 메서드를 구현하여 자원을 해제
	//비관리 자원을 명시적으로 해제할 수 있음
	{
		private static MILContext? instance;

		private bool isDisposed = false;

		public static MILContext Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MILContext();
				}
				GC.KeepAlive( instance );
				return instance;
			}
		}
		~MILContext( )
		{
			Logger.WriteLine( "[MILContext] Finalizer 호출됨! GC가 Dispose보다 먼저 호출했을 수 있음" );
		}

		//public static MIL_INT Width;

		//public static MIL_INT Height;

		//public static MIL_INT NbBands;

		//public static MIL_INT DataType;


		// 속성 대신 필드로 선언
		public MIL_ID MilApplication = MIL.M_NULL;
		public MIL_ID MilSystem = MIL.M_NULL;

		private MILContext( )
		{
			// MIL 애플리케이션 및 시스템 초기화
			MIL.MappAlloc( MIL.M_DEFAULT, ref MilApplication );

			// MilApplication이 유효한지 확인
			if (MilApplication != MIL.M_NULL)
			{
				//MIL.MsysAlloc( MIL.M_SYSTEM_USB3_VISION, MIL.M_NULL, MIL.M_DEFAULT, ref MilSystem );

				MIL.MsysAlloc( MIL.M_SYSTEM_GIGE_VISION, MIL.M_NULL, MIL.M_DEFAULT, ref MilSystem );
			}
			else
			{
				throw new Exception( "MilApplication을 초기화할 수 없습니다." );
			}

			// MilSystem이 유효한지 확인
			if (MilSystem == MIL.M_NULL)
			{
				throw new Exception( "MilSystem을 초기화할 수 없습니다." );
			}

			MIL.MsysControl( MilSystem, MIL.M_DISCOVER_DEVICE, MIL.M_DEFAULT );
			//MIL.MappControl( MIL.M_DEFAULT, MIL.M_DEBUG_LEVEL, MIL.M_DEBUG_ALL );
			MIL.MappControl( MIL.M_ERROR, MIL.M_PRINT_DISABLE );
			//MIL.MappControl( MIL.M_ERROR, MIL );
		}

		public void Dispose( )
		{
			Debug.WriteLine( "[MILContext] Dispose 시작" );
			if (isDisposed)
			{
				Debug.WriteLine( "[MILContext] Dispose 이미 완료 됨" );
				return;
			}

			// MilSystem 해제
			if (MilSystem != MIL.M_NULL)
			{
				MIL.MsysFree( MilSystem );
				MilSystem = MIL.M_NULL;
				Debug.WriteLine( "[MILContext] MsysFree 호출" );
			}

			// MilApplication 해제
			if (MilApplication != MIL.M_NULL)
			{
				MIL.MappFree( MilApplication );
				MilApplication = MIL.M_NULL;
				Debug.WriteLine( "[MILContext] MappFree 호출" );
			}

			isDisposed = true;
			instance = null;
			Debug.WriteLine( "[MILContext] Dispose 완료" );
		}
	}
}
