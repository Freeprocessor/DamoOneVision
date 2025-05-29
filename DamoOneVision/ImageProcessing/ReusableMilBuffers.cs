// ───────────────────────────────────────────────────────────────
//  ReusableMilBuffers.cs  (Sector-Pool 버전)
// ───────────────────────────────────────────────────────────────
using System;
using Matrox.MatroxImagingLibrary;

namespace DamoOneVision.Services
{
	internal static class ReusableMilBuffers
	{
		private const int SECTOR_MAX = 60;             // ⬅ 필요하면 변경
		private static MIL_ID[] _mask   = new MIL_ID[SECTOR_MAX];
		private static MIL_ID[] _sector = new MIL_ID[SECTOR_MAX];

		private static MIL_ID[] _gctx = new MIL_ID[SECTOR_MAX];
		private static MIL_ID _sysId;
		private static int _w, _h;
		private static readonly object _lock = new();

		/* ───── 외부 노출 ───── */
		public static MIL_ID AcquireGctx( int i ) => _gctx[ i ];
		[System.Runtime.CompilerServices.MethodImpl(
			System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining )]
		public static MIL_ID AcquireMask( int i ) => _mask[ i ];
		[System.Runtime.CompilerServices.MethodImpl(
			System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining )]
		public static MIL_ID AcquireSector( int i ) => _sector[ i ];

		/* ───── 해상도 변경 시 호출 ───── */
		public static void EnsureSize( MIL_ID sys, int w, int h )
		{
			lock (_lock)
			{
				if (_mask[ 0 ] != MIL.M_NULL && w == _w && h == _h) return;

				Free();                      // 크기가 달라지면 전부 해제
				_sysId = sys; _w = w; _h = h;

				/* 버퍼 할당 */
				for (int i = 0; i < SECTOR_MAX; i++)
				{
					MIL.MbufAllocColor( _sysId, 1, _w, _h,
									   16 + MIL.M_UNSIGNED,
									   MIL.M_IMAGE + MIL.M_PROC,
									   ref _mask[ i ] );
					MIL.MbufAllocColor( _sysId, 1, _w, _h,
									   16 + MIL.M_UNSIGNED,
									   MIL.M_IMAGE + MIL.M_PROC,
									   ref _sector[ i ] );
					MIL.MgraAlloc( _sysId, ref _gctx[ i ] );      // ❶ 각 인덱스별로 생성
				}
			}
		}

		/* ───── 프레임마다 호출 ───── */
		public static void Clear( )
		{
			if (_mask[ 0 ] == MIL.M_NULL)
				throw new InvalidOperationException( "EnsureSize() 먼저 호출하세요" );

			for (int i = 0; i < SECTOR_MAX; i++)
			{
				MIL.MbufClear( _mask[ i ], MIL.M_COLOR_BLACK );
				MIL.MbufClear( _sector[ i ], MIL.M_COLOR_BLACK );
			}
		}

		/* ───── 프로그램 종료 ───── */
		public static void Free( )
		{
			for (int i = 0; i < SECTOR_MAX; i++)
			{
				if (_mask[ i ] != MIL.M_NULL) MIL.MbufFree( _mask[ i ] );
				if (_sector[ i ] != MIL.M_NULL) MIL.MbufFree( _sector[ i ] );
				if (_gctx[ i ] != MIL.M_NULL) MIL.MgraFree( _gctx[ i ] );
				_mask[ i ] = _sector[ i ] = _gctx[ i ] = MIL.M_NULL;
			}
			_w = _h = 0;
		}
	}
}
