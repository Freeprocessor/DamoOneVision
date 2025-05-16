using DamoOneVision.Ajinextek.Common;
using DamoOneVision.Ajinextek.Motion;
using DamoOneVision.Models;
using System.Windows;
using System.Windows.Threading;

namespace DamoOneVision.Services
{
	public class MotionService
	{

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

		bool isXAxisHome = false;
		bool isZAxisHome = false;

		public int CameraDelay { get; set; }

		private double _lastPosition = 0;
		private DateTime _lastTime = DateTime.Now;

		public double ConveyorSpeed { get; set; }

		private readonly DispatcherTimer _positionTimer;
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
			_positionTimer = new DispatcherTimer( DispatcherPriority.Normal, Application.Current.Dispatcher );
			_positionTimer.Interval = TimeSpan.FromMilliseconds( 200 ); // 0.2초마다 업데이트
			_positionTimer.Tick += PositionTimer_Tick;

		}

		public void ConveyorReadStart( )
		{
			_lastTime = DateTime.Now;
			_positionTimer.Start();
			Logger.WriteLine( "ConveyorReadStart" );
		}

		public void ConveyorReadStop( )
		{
			_positionTimer.Stop();
		}

		public void SetModel( MotionModel motionModel )
		{
			_motionModel = motionModel;

			CameraDelay = (int) (_motionModel.XAxisMoveAcceleration * 1000.0 );
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
				Logger.WriteLine( $"AxlOpen 실패, 에러 코드: {openResult}" );
				return false;
			}
			else
			{
				// 라이브러리 열림
				int isOpened = CAXL.AxlIsOpened();
				if (isOpened == 1)
				{
					Logger.WriteLine( "AXL 라이브러리가 정상적으로 로드되었습니다!" );
				}
				else
				{
					Logger.WriteLine( "AXL 라이브러리 로드에 실패했습니다." );
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
				Logger.WriteLine( "AxlOpen 실패" );
				return;
			}
			//++ 지정한 Mot파일의 설정값들로 모션보드의 설정값들을 일괄변경 적용합니다.
			if (CAXM.AxmMotLoadParaAll( motFilePath ) != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				_isInitialized = false;
				Logger.WriteLine( "AxmMotLoadParaAll 실패" );
				return;
			}
			else
			{
				//++ 모션보드의 설정값들을 모두 적용하였습니다.
				Logger.WriteLine( "모션보드 설정값을 모두 적용하였습니다." );
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
				Logger.WriteLine( "모션 라이브러리가 초기화되지 않았습니다!" );
				return;
			}

			uint duRetCode = 0;

			await Task.Run( ( ) =>
			{
				duRetCode = CAXM.AxmMoveStartPos( X, position, velocity, acceleration, deceleration );
			} );

			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( $"AxmMoveStartPos return error[Code:{duRetCode}]" );
			}
		}

		public async Task ZAxisMoveToPosition( double position, double velocity, double acceleration, double deceleration )
		{
			if (!isZAxisHome)
				return;
			if (!_isInitialized)
			{
				Logger.WriteLine( "모션 라이브러리가 초기화되지 않았습니다!" );
				return;
			}

			uint duRetCode = 0;

			await Task.Run( ( ) =>
			{
				duRetCode = CAXM.AxmMoveStartPos( Z, position, velocity, acceleration, deceleration );
			} );

			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( $"AxmMoveStartPos return error[Code:{duRetCode}]" );
			}
		}

		public void XAxisHome( )
		{
			if (isXAxisHome)
				return;
			Logger.WriteLine( "X-Axis Home" );
			uint duRetCode = 0;
			// 지정축의 원점 검색 파라미터를 설정합니다.
			duRetCode = CAXM.AxmHomeSetMethod( X, _motionModel.XAxisOriginDirection, _motionModel.XAxisOriginSensor, _motionModel.XAxisOriginUseZPhase, _motionModel.XAxisOriginDelay, _motionModel.XAxisOriginOffset );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( $"AxmHomeSetStart return error[Code:{duRetCode}]" );
			}
			// 지정축의 원점 검색 속도 파라이터를 설정합니다.
			duRetCode = CAXM.AxmHomeSetVel( X, _motionModel.XAxisOriginSpeed1, _motionModel.XAxisOriginSpeed2, _motionModel.XAxisOriginCreepSpeed, _motionModel.XAxisOriginZPhaseSpeed, _motionModel.XAxisOriginAcceleration, _motionModel.XAxisOriginAcceleration );
			Logger.WriteLine( $"{_motionModel.XAxisOriginSpeed1},{_motionModel.XAxisOriginSpeed2}" );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( $"AxmHomeGetResult return error[Code:{duRetCode}]" );
			}
			// 원점 검색을 실행합니다.
			duRetCode = CAXM.AxmHomeSetStart( X );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( $"AxmHomeGetResult return error[Code:{duRetCode}]" );
			}

			Logger.WriteLine( "X-Axis Home End" );
			isXAxisHome = true;
		}

		public void ZAxisHome( )
		{
			if (isZAxisHome)
				return;
			Logger.WriteLine( "Z-Axis Home" );
			uint duRetCode = 0;
			// 지정축의 원점 검색 파라미터를 설정합니다.
			duRetCode = CAXM.AxmHomeSetMethod( Z, _motionModel.ZAxisOriginDirection, _motionModel.ZAxisOriginSensor, _motionModel.ZAxisOriginUseZPhase, _motionModel.ZAxisOriginDelay, _motionModel.ZAxisOriginOffset );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( $"AxmHomeSetStart return error[Code:{duRetCode}]" );
			}
			// 지정축의 원점 검색을 시작합니다.
			duRetCode = CAXM.AxmHomeSetVel( Z, _motionModel.ZAxisOriginSpeed1, _motionModel.ZAxisOriginSpeed2, _motionModel.ZAxisOriginCreepSpeed, _motionModel.ZAxisOriginZPhaseSpeed, _motionModel.ZAxisOriginAcceleration, _motionModel.ZAxisOriginAcceleration );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( $"AxmHomeGetResult return error[Code:{duRetCode}]" );
			}
			duRetCode = CAXM.AxmHomeSetStart( Z );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				Logger.WriteLine( $"AxmHomeGetResult return error[Code:{duRetCode}]" );
			}

			Logger.WriteLine( "Z-Axis Home End" );
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
			Logger.WriteLine( $"Conveyor Speed : {ConveyorSpeed} mm/s" );
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
			int axisNo = 1;
			double pulsePerRevolution = 1000;
			double distancePerRevolution = 300;

			double pulsePerMm = pulsePerRevolution / distancePerRevolution;

			// 현재 위치 (엔코더 단위)
			double currentPulse = 0;
			CAXM.AxmStatusGetActPos( axisNo, ref currentPulse );

			// 현재 시간
			DateTime now = DateTime.Now;
			double elapsedSeconds = (now - _lastTime).TotalSeconds;

			// 이동 거리 계산
			double deltaPulse = currentPulse - _lastPosition;
			double distanceMm = deltaPulse / pulsePerMm;

			// 속도 계산
			double speedMmPerSec = distanceMm / elapsedSeconds;

			// 상태 갱신
			_lastTime = now;
			_lastPosition = 0; // 다음 계산을 위해 기준값 0으로 고정

			// 💡 위치를 0으로 초기화 (커맨드/액츄얼 모두)
			CAXM.AxmStatusSetPosMatch( axisNo, 0.0 );

			speedMmPerSec = Math.Round( speedMmPerSec, 2 );
			speedMmPerSec = Math.Abs( speedMmPerSec );


			ConveyorSpeed = speedMmPerSec;
			//Logger.WriteLine( $"Conveyor Speed: {ConveyorSpeed} mm/s" );
			//Logger.WriteLine( $"Speed: {ConveyorSpeed} mm/s" );
			return speedMmPerSec;
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
				Logger.WriteLine( "JogStart: dir 값이 잘못되었습니다." );
				return;
			}

			uint duRetCode   = 0;

			double dVelocity = Math.Abs(5000);
			double dAccel    = Math.Abs(0.5);
			double dDecel    = Math.Abs(0.5);

			//++ 지정한 축을 (+)방향으로 지정한 속도/가속도/감속도로 모션구동합니다.
			duRetCode = CAXM.AxmMoveVel( axisNum, dVelocity * dir, dAccel, dDecel );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
				Logger.WriteLine( $"AxmMoveVel return error[Code:{0:duRetCode}]" );

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

		private void PositionTimer_Tick( object? sender, EventArgs e )
		{
			GetConveyorSpeed();
		}
	}
}
