using DamoOneVision.Ajinextek.Common;
using DamoOneVision.Ajinextek.Motion;
using DamoOneVision.Models;
using System;
using System.ComponentModel;
using System.Windows;

namespace DamoOneVision.Services
{
	public class MotionService : IDisposable
	{
		public event PropertyChangedEventHandler? PropertyChanged;


		// ★ UI-스레드 보장용 헬퍼
		private void Raise( string name )
		{
			// 현재 애플리케이션에 Dispatcher가 있고,
			// 호출 스레드가 UI 스레드가 아니라면
			var d = Application.Current?.Dispatcher;
			if (d != null && !d.CheckAccess())
				d.Invoke( ( ) => PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) ) );
			else
				PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		public const int X = 0;
		public const int Z = 2;

		const int CCW = 0;
		const int CW = 1;
		const int LIMIT_POSITIVE = 0;
		const int LIMIT_NEGATIVE = 1;
		const int HOME = 4;
		const int NONZPHASE = 0;
		const int ZPHASE_POSITIVE = 1;
		const int ZPHASE_NEGATIVE = 2;

		const int SERVOON = 0;
		const int ALARMRESET = 1;
		const int SERVOBREAK = 2;

		/* ―――――――― 엔코더/컨베이어 상수 ―――――――― */
		private const int    ENCODER_AXIS   = 1;        // 실제 엔코더 채널
		private const double PPR            = 1000.0;   // Pulse Per Revolution
		private const double DIST_PER_REVMM = 300.0;    // 1회전 이송 거리(mm)

		/* 32-bit 엔코더 카운터 범위(아진보드 기본) */
		private const long   MAX_COUNT      = int.MaxValue;   //  +2 147 483 647
		private const long   MIN_COUNT      = int.MinValue;   //  −2 147 483 648
		private const long   COUNT_RANGE    = (long)MAX_COUNT - MIN_COUNT + 1L;   // 4 294 967 296

		bool isXAxisHome = false;
		bool isZAxisHome = false;

		public int CameraDelay { get; set; }

		/* ―――――――― 내부 상태 ―――――――― */
		private double            _lastPulse     = 0.0;          // 직전 펄스
		private DateTime          _lastTime      = DateTime.Now; // 직전 시간
		private CancellationTokenSource? _cts;
		private Task?             _speedTask;


		/* ① 속도·축 위치 프로퍼티를 모두 여기서 보유 */
		private double _conveyorSpeed;
		public double ConveyorSpeed
		{
			get => _conveyorSpeed;
			private set      // ← 외부에서 못 바꾸도록 private set
			{
				if (Math.Abs( _conveyorSpeed - value ) > 0.01)
				{
					_conveyorSpeed = value;
					Raise( nameof( ConveyorSpeed ) );
				}
			}
		}

		private double _xCmdPos, _zCmdPos;
		public double XCmdPos
		{
			get => _xCmdPos;
			private set { _xCmdPos = value; Raise( nameof( XCmdPos ) ); }
		}
		public double ZCmdPos
		{
			get => _zCmdPos;
			private set { _zCmdPos = value; Raise( nameof( ZCmdPos ) ); }
		}

		private MotionModel _motionModel;

		private bool _isInitialized = false; // 라이브러리 초기화 여부

		//public bool MotionStopRequested { get; set; }

		public MotionService( )
		{
			//_motionModel = motionModel;
			//motionModel.XAxisWaitingPostion = 1000;
			//motionModel.XAxisEndPostion = 100000;
			//motionModel.XAxisTrackingSpeed = 10000;
			//motionModel.XAxisReturnSpeed = 100000;
			//motionModel.XAxisAcceleration = 0.1;
			//motionModel.XAxisDeceleration = 0.1;

			InitLibrary();
			ConveyorReadStart();
		}

		public void ConveyorReadStart( )
		{
			if (_cts != null) return;            // 이미 실행 중

			// 기준 위치·시간 확보
			CAXM.AxmStatusGetActPos( ENCODER_AXIS, ref _lastPulse );
			_lastTime = DateTime.Now;

			_cts = new CancellationTokenSource();
			_speedTask = Task.Run( async ( ) =>
			{
				while (!_cts.IsCancellationRequested)
				{
					GetConveyorSpeed();          // 10 Hz 주기로 속도 계산

					CAXM.AxmStatusGetCmdPos( X, ref _xCmdPos );
					CAXM.AxmStatusGetCmdPos( Z, ref _zCmdPos );
					Raise( nameof( XCmdPos ) );
					Raise( nameof( ZCmdPos ) );

					await Task.Delay( 100, _cts.Token );
				}
			}, _cts.Token );
			Logger.WriteLine( "INFO", "MotionService", "ConveyorReadStart" );
		}

		public void ConveyorReadStop( )
		{
			if (_cts == null) return;
			_cts.Cancel();
			try { _speedTask!.Wait(); }
			catch { /* TaskCanceledException 무시 */ }
			_cts = null;
			_speedTask = null;
		}

		public void SetModel( MotionModel motionModel )
		{
			_motionModel = motionModel;

			CameraDelay = (int) (_motionModel.XAxisMoveAcceleration * 1000.0 + 150 );
			XAxisServoOn();
			ZAxisServoOn();

			XAxisHome();
			ZAxisHome();
		}

		private bool MotionInit( )
		{
			uint openResult = CAXL.AxlOpen(7);

			if (openResult != 0)
			{
				Logger.WriteLine( "ERROR", "MotionService", $"AxlOpen 실패, 에러 코드: {openResult}" );
				return false;
			}
			else
			{
				// 라이브러리 열림
				int isOpened = CAXL.AxlIsOpened();
				if (isOpened == 1)
				{
					Logger.WriteLine( "INFO", "MotionService", "AXL 라이브러리가 정상적으로 로드되었습니다!" );
				}
				else
				{
					Logger.WriteLine( "ERROR", "MotionService", "AXL 라이브러리 로드에 실패했습니다." );
					return false;
				}

			}

			int axisCounts = 0;
			CAXM.AxmInfoGetAxisCount( ref axisCounts );

			return true;
		}

		public void InitLibrary( )
		{

			string localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			string appFolder = System.IO.Path.Combine( localAppData, "DamoOneVision" );
			string servoFolder = System.IO.Path.Combine( appFolder, "Servo" );
			string motFilePath = System.IO.Path.Combine( servoFolder, "DamoOne.mot" );


			// ※ [CAUTION] 아래와 다른 Mot파일(모션 설정파일)을 사용할 경우 경로를 변경하십시요.
			//String szFilePath = "C:\\Program Files\\EzSoftware RM\\EzSoftware\\MotionDefault.mot";
			//++ AXL(AjineXtek Library)을 사용가능하게 하고 장착된 보드들을 초기화합니다.
			if (CAXL.AxlOpen( 7 ) != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				_isInitialized = false;
				Logger.WriteLine( "ERROR", "MotionService", "AxlOpen 실패" );
				return;
			}
			//++ 지정한 Mot파일의 설정값들로 모션보드의 설정값들을 일괄변경 적용합니다.
			if (CAXM.AxmMotLoadParaAll( motFilePath ) != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				_isInitialized = false;
				Logger.WriteLine( "ERROR", "MotionService", "AxmMotLoadParaAll 실패" );
				return;
			}
			else
			{
				//++ 모션보드의 설정값들을 모두 적용하였습니다.
				Logger.WriteLine( "INFO", "MotionService", "모션보드 설정값을 모두 적용하였습니다." );
				_isInitialized = true;
			}

		}

		public void ReleaseLibrary( )
		{
			if (_isInitialized)
			{
				CAXL.AxlClose(); // 라이브러리 해제
				_isInitialized = false;
				//MessageBox.Show( "모션 라이브러리 해제 완료", "Info", MessageBoxButton.OK, MessageBoxImage.Information );
			}
		}

		/// <summary>
		/// 지정 위치로 이동
		/// </summary>
		public async Task XAxisMoveToPosition( double position, double velocity, double acceleration, double deceleration )
		{
			if (!isXAxisHome)
				return;
			if (!_isInitialized)
			{
				Logger.WriteLine( "ERROR", "MotionService", "모션 라이브러리가 초기화되지 않았습니다!" );
				return;
			}

			uint duRetCode = 0;

			await Task.Run( ( ) =>
			{
				duRetCode = CAXM.AxmMoveStartPos( X, position, velocity, acceleration, deceleration );
			} );

			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( "ERROR", "MotionService", $"AxmMoveStartPos return error[Code:{duRetCode}]" );
			}
		}

		public async Task ZAxisMoveToPosition( double position, double velocity, double acceleration, double deceleration )
		{
			if (!isZAxisHome)
				return;
			if (!_isInitialized)
			{
				Logger.WriteLine( "ERROR", "MotionService", "모션 라이브러리가 초기화되지 않았습니다!" );
				return;
			}

			uint duRetCode = 0;

			await Task.Run( ( ) =>
			{
				duRetCode = CAXM.AxmMoveStartPos( Z, position, velocity, acceleration, deceleration );
			} );

			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( "ERROR", "MotionService", $"AxmMoveStartPos return error[Code:{duRetCode}]" );
			}
		}

		public void XAxisHome( )
		{
			if (isXAxisHome)
				return;
			Logger.WriteLine( "INFO", "MotionService", "X-Axis Home" );
			uint duRetCode = 0;
			// 지정축의 원점 검색 파라미터를 설정합니다.
			duRetCode = CAXM.AxmHomeSetMethod( X, _motionModel.XAxisOriginDirection, _motionModel.XAxisOriginSensor, _motionModel.XAxisOriginUseZPhase, _motionModel.XAxisOriginDelay, _motionModel.XAxisOriginOffset );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( "ERROR", "MotionService", $"AxmHomeSetStart return error[Code:{duRetCode}]" );
			}
			// 지정축의 원점 검색 속도 파라이터를 설정합니다.
			duRetCode = CAXM.AxmHomeSetVel( X, _motionModel.XAxisOriginSpeed1, _motionModel.XAxisOriginSpeed2, _motionModel.XAxisOriginCreepSpeed, _motionModel.XAxisOriginZPhaseSpeed, _motionModel.XAxisOriginAcceleration, _motionModel.XAxisOriginAcceleration );
			Logger.WriteLine( "INFO", "MotionService", $"{_motionModel.XAxisOriginSpeed1},{_motionModel.XAxisOriginSpeed2}", 0 );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( "ERROR", "MotionService", $"AxmHomeGetResult return error[Code:{duRetCode}]" );
			}
			// 원점 검색을 실행합니다.
			duRetCode = CAXM.AxmHomeSetStart( X );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( "ERROR", "MotionService", $"AxmHomeGetResult return error[Code:{duRetCode}]" );
			}

			Logger.WriteLine( "INFO", "MotionService", "X-Axis Home End" );
			isXAxisHome = true;
		}

		public void ZAxisHome( )
		{
			if (isZAxisHome)
				return;
			Logger.WriteLine( "INFO", "MotionService", "Z-Axis Home" );
			uint duRetCode = 0;
			// 지정축의 원점 검색 파라미터를 설정합니다.
			duRetCode = CAXM.AxmHomeSetMethod( Z, _motionModel.ZAxisOriginDirection, _motionModel.ZAxisOriginSensor, _motionModel.ZAxisOriginUseZPhase, _motionModel.ZAxisOriginDelay, _motionModel.ZAxisOriginOffset );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( "ERROR", "MotionService", $"AxmHomeSetStart return error[Code:{duRetCode}]" );
			}
			// 지정축의 원점 검색을 시작합니다.
			duRetCode = CAXM.AxmHomeSetVel( Z, _motionModel.ZAxisOriginSpeed1, _motionModel.ZAxisOriginSpeed2, _motionModel.ZAxisOriginCreepSpeed, _motionModel.ZAxisOriginZPhaseSpeed, _motionModel.ZAxisOriginAcceleration, _motionModel.ZAxisOriginAcceleration );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( "ERROR", "MotionService", $"AxmHomeGetResult return error[Code:{duRetCode}]" );
			}
			duRetCode = CAXM.AxmHomeSetStart( Z );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( "ERROR", "MotionService", $"AxmHomeGetResult return error[Code:{duRetCode}]" );
			}

			Logger.WriteLine( "INFO", "MotionService", "Z-Axis Home End" );
			isZAxisHome = true;
		}

		public async Task XAxisMoveWaitPos( )
		{
			await XAxisMoveToPosition( _motionModel.XAxisWaitingPosition, _motionModel.XAxisReturnSpeed, _motionModel.XAxisReturnAcceleration, _motionModel.XAxisReturnDeceleration );
			await XAxisWaitingStop();
			//Logger.WriteLine( "XAxisMoveWaitPos" );
		}

		public async Task XAxisMoveEndPos( )
		{
			await XAxisMoveToPosition( _motionModel.XAxisEndPosition, ConveyorSpeed * 1000, _motionModel.XAxisMoveAcceleration, _motionModel.XAxisMoveDeceleration );
			Logger.WriteLine( "INFO", "MotionService", $"Conveyor Speed : {ConveyorSpeed} mm/s" );
			//await XAxisWaitingStop();
		}

		public async Task ZAxisMoveWorkPos( )
		{
			await ZAxisMoveToPosition( _motionModel.ZAxisWorkPosition, _motionModel.ZAxisSpeed, _motionModel.ZAxisAcceleration, _motionModel.ZAxisDeceleration );
			await ZAxisWaitingStop();
			//Logger.WriteLine( "ZAxisMoveWorkPos" );
		}

		public async Task ZAxisMoveEndPos( )
		{
			await ZAxisMoveToPosition( _motionModel.ZAxisEndPosition, _motionModel.ZAxisSpeed, _motionModel.ZAxisAcceleration, _motionModel.ZAxisDeceleration );
			await ZAxisWaitingStop();
		}

		public async Task XAxisWaitingStop( )
		{
			uint upStatus = 0;
			await Task.Run( ( ) =>
			{
				while (true)
				{
					CAXM.AxmStatusReadInMotion( X, ref upStatus );
					if (upStatus == 0)
					{
						break;
					}
				}
			} );
		}

		public async Task ZAxisWaitingStop( )
		{
			uint upStatus = 0;
			await Task.Run( ( ) =>
			{
				while (true)
				{

					CAXM.AxmStatusReadInMotion( Z, ref upStatus );
					if (upStatus == 0)
					{
						break;
					}
				}
			} );
		}



		/// <summary>
		/// 현재 엔코더 위치를 기준으로 mm/s 속도 계산
		/// </summary>
		/// <returns>속도 mm/s</returns>
		public double GetConveyorSpeed( )
		{
			/* 1) 현재 펄스 위치 읽기 */
			double curPulseDouble = 0.0;
			uint ret = CAXM.AxmStatusGetActPos(ENCODER_AXIS, ref curPulseDouble);
			if (ret != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				//Logger.WriteLine( $"[WARN] AxmStatusGetActPos 실패 : 0x{ret:X}" );
				return ConveyorSpeed;            // 오류 시 이전 값 유지
			}

			/* 2) 경과 시간(ms 단위 노이즈 차단) */
			DateTime now = DateTime.Now;
			double dtSec = (now - _lastTime).TotalSeconds;
			if (dtSec < 0.002)                   // 2 ms 이하 샘플은 무시
				return ConveyorSpeed;

			/* 3) 펄스 차이(오버플로우 보정) */
			long curPulse = (long)curPulseDouble;
			long lastPulse = (long)_lastPulse;
			long deltaPulse = curPulse - lastPulse;

			// |delta| 가 전체 범위의 절반보다 크면 오버/언더플로우로 판단
			if (deltaPulse > COUNT_RANGE / 2L)          // + → - 로 래핑된 경우
				deltaPulse -= COUNT_RANGE;
			else if (deltaPulse < -COUNT_RANGE / 2L)    // - → + 로 래핑된 경우
				deltaPulse += COUNT_RANGE;

			/* 4) 속도 계산 */
			double pulsePerMm = PPR / DIST_PER_REVMM;
			double distanceMm = deltaPulse / pulsePerMm;
			double speedMmSec = Math.Abs(distanceMm / dtSec);

			/* 5) 상태 갱신 */
			_lastPulse = curPulseDouble;
			_lastTime = now;
			ConveyorSpeed = Math.Round( speedMmSec, 2 );

			return ConveyorSpeed;
		}

		public double XAxisGetCommandPosition( )
		{
			double cmdPos = 0.0;
			CAXM.AxmStatusGetCmdPos( X, ref cmdPos ); // axisNo는 X축 번호
			return cmdPos;
		}

		public double ZAxisGetCommandPosition( )
		{
			double cmdPos = 0.0;
			CAXM.AxmStatusGetCmdPos( Z, ref cmdPos ); // axisNo는 X축 번호
			return cmdPos;
		}

		public void XAxisServoOn( )
		{
			//++ 지정한 축의 서보온을 설정합니다.

			CAXM.AxmSignalWriteOutputBit( X, SERVOON, 1 );
		}

		public void XAxisServoOff( )
		{
			//++ 지정한 축의 서보온을 설정합니다.

			CAXM.AxmSignalWriteOutputBit( X, SERVOON, 0 );
			isXAxisHome = false;
		}
		public void ZAxisServoOn( )
		{
			//++ 지정한 축의 서보온을 설정합니다.

			CAXM.AxmSignalWriteOutputBit( Z, SERVOON, 1 );
			XAxisBreakOff();
		}

		public void ZAxisServoOff( )
		{
			//++ 지정한 축의 서보온을 설정합니다.

			CAXM.AxmSignalWriteOutputBit( Z, SERVOON, 0 );
			XAxisBreakOn();
			isXAxisHome = false;
		}



		public void XAxisBreakOn( )
		{
			//++ 지정한 축의 브레이크를 설정합니다.
			CAXM.AxmSignalWriteOutputBit( Z, SERVOBREAK, 0 );
		}

		public void XAxisBreakOff( )
		{
			//++ 지정한 축의 브레이크를 설정합니다.
			CAXM.AxmSignalWriteOutputBit( Z, SERVOBREAK, 1 );
		}

		private void JogStart( int axisNum, int dir )
		{
			if (!((dir == 1) || (dir == -1)))
			{
				Logger.WriteLine( "ERROR", "MotionService", "JogStart: dir 값이 잘못되었습니다." );
				return;
			}

			uint duRetCode   = 0;

			double dVelocity = Math.Abs(5000);
			double dAccel    = Math.Abs(0.5);
			double dDecel    = Math.Abs(0.5);

			//++ 지정한 축을 (+)방향으로 지정한 속도/가속도/감속도로 모션구동합니다.
			duRetCode = CAXM.AxmMoveVel( axisNum, dVelocity * dir, dAccel, dDecel );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
				Logger.WriteLine( "ERROR", "MotionService", $"AxmMoveVel return error[Code:{0:duRetCode}]" );

			//Logger.WriteLine( "Jog Start" );

		}



		public void XAxisJogPStart( )
		{
			//Logger.WriteLine( "X-Axis Jog+ Start" );
			JogStart( X, 1 );
		}

		public void XAxisJogNStart( )
		{
			//Logger.WriteLine( "X-Axis Jog- Start" );
			JogStart( X, -1 );
		}

		public void XAxisStop( )
		{
			//++ 지정한 축의 Jog구동(모션구동)을 종료합니다.  
			//Logger.WriteLine( "X-Axis Stop" );
			CAXM.AxmMoveSStop( X );
		}



		public void ZAxisJogPStart( )
		{
			//Logger.WriteLine( "Z-Axis Jog+ Start" );
			JogStart( Z, 1 );
		}

		public void ZAxisJogNStart( )
		{
			//Logger.WriteLine( "Z-Axis Jog- Start" );
			JogStart( Z, -1 );
		}

		public void ZAxisStop( )
		{
			//++ 지정한 축의 Jog구동(모션구동)을 종료합니다.  
			//Logger.WriteLine( "Z-Axis Stop" );
			CAXM.AxmMoveSStop( Z );
		}

		public void Dispose( )
		{
			ConveyorReadStop();
			Logger.WriteLine( "INFO", "MotionService", "Motion Service Dispose" );
			GC.SuppressFinalize( this );
		}
	}
}
