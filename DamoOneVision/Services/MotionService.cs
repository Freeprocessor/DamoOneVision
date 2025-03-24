using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DamoOneVision.Ajinextek.Common;
using DamoOneVision.Ajinextek.Motion;

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

		public double XAxisWaitingPostion { get; set; }
		public double XAxisEndPostion { get; set; }
		public double XAxisTrackingSpeed { get; set; }
		public double XAxisReturnSpeed { get; set; }
		public double XAxisAcceleration { get; set; }
		public double XAxisDeceleration { get; set; }
		public double XAxisJogSpeed { get; set; }
		public double XAxisJogAcceleration { get; set; }
		public double XAxisJogDeceleration { get; set; }






		private bool _isInitialized = false; // 라이브러리 초기화 여부

		//public bool MotionStopRequested { get; set; }

		public MotionService( )
		{

			XAxisWaitingPostion = 1000;
			XAxisEndPostion = 100000;
			XAxisTrackingSpeed = 10000;
			XAxisReturnSpeed = 100000;
			XAxisAcceleration = 0.1;
			XAxisDeceleration = 0.1;

			InitLibrary();
			ServoOn( X );
			ServoOn( Z );

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
				MessageBox.Show( "모션 라이브러리 해제 완료", "Info", MessageBoxButton.OK, MessageBoxImage.Information );
			}
		}

		/// <summary>
		/// 지정 위치로 이동
		/// </summary>
		public async Task XAxisMoveToPosition( double position, double velocity, double acceleration, double deceleration )
		{
			if (!_isInitialized)
			{
				MessageBox.Show( "모션 라이브러리가 초기화되지 않았습니다!", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return;
			}

			uint duRetCode = 0;

			await Task.Run( ( ) =>
			{
				duRetCode = CAXM.AxmMoveStartPos( X, position, velocity, acceleration, deceleration );
			} );

			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				MessageBox.Show( $"AxmMoveStartPos return error[Code:{duRetCode}]", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}

		public async Task ZAxisMoveToPosition( double position, double velocity, double acceleration, double deceleration )
		{
			if (!_isInitialized)
			{
				MessageBox.Show( "모션 라이브러리가 초기화되지 않았습니다!", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return;
			}

			uint duRetCode = 0;

			await Task.Run( ( ) =>
			{
				duRetCode = CAXM.AxmMoveStartPos( Z, position, velocity, acceleration, deceleration );
			} );

			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				MessageBox.Show( $"AxmMoveStartPos return error[Code:{duRetCode}]", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}

		public void XAxisHome( )
		{
			Logger.WriteLine( "X-Axis Home" );
			uint duRetCode = 0;
			// 지정축의 원점 검색 파라미터를 설정합니다.
			duRetCode = CAXM.AxmHomeSetMethod( X, CCW, LIMIT_NEGATIVE, NONZPHASE, 1000, 0 );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				MessageBox.Show( $"AxmHomeSetStart return error[Code:{duRetCode}]", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			// 지정축의 원점 검색 속도 파라이터를 설정합니다.
			duRetCode = CAXM.AxmHomeSetVel( X, 10000, 10000, 1000, 500, 0.1, 0.1 );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				MessageBox.Show( $"AxmHomeGetResult return error[Code:{duRetCode}]", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			// 원점 검색을 실행합니다.
			duRetCode = CAXM.AxmHomeSetStart( X );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				MessageBox.Show( $"AxmHomeGetResult return error[Code:{duRetCode}]", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}

			Logger.WriteLine( "X-Axis Home End" );
		}

		public void ZAxisHome( )
		{
			Logger.WriteLine( "Z-Axis Home" );
			uint duRetCode = 0;
			// 지정축의 원점 검색 파라미터를 설정합니다.
			duRetCode = CAXM.AxmHomeSetMethod( Z, CCW, LIMIT_NEGATIVE, ZPHASE_POSITIVE, 1000, 0 );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				MessageBox.Show( $"AxmHomeSetStart return error[Code:{duRetCode}]", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			// 지정축의 원점 검색을 시작합니다.
			duRetCode = CAXM.AxmHomeSetVel( Z, 10000, 10000, 1000, 500, 0.1, 0.1 );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				MessageBox.Show( $"AxmHomeGetResult return error[Code:{duRetCode}]", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			duRetCode = CAXM.AxmHomeSetStart( Z );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				MessageBox.Show( $"AxmHomeGetResult return error[Code:{duRetCode}]", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}

			Logger.WriteLine( "Z-Axis Home End" );
		}

		public async Task XAxisMoveWaitPos( )
		{
			await XAxisMoveToPosition( XAxisWaitingPostion, XAxisReturnSpeed, XAxisAcceleration, XAxisDeceleration );
			await XAxisWaitingStop();
		}

		public async Task XAxisMoveEndPos( )
		{
			await XAxisMoveToPosition( XAxisEndPostion, XAxisReturnSpeed, XAxisAcceleration, XAxisDeceleration );
			//await XAxisWaitingStop();
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
			});
		}

		public async void ZAxisWaitingStop( )
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
		/// 지정한 축의 실제 위치 변화를 기반으로 속도를 계산합니다.
		/// </summary>
		/// <param name="axisNo">측정할 축 번호</param>
		/// <param name="intervalMs">측정 간격 (ms)</param>
		/// <returns>초당 이동 거리 (단위는 설정된 유닛 기준)</returns>
		public async static Task<double> GetEncoderVelocity( int axisNo, int intervalMs = 100 )
		{
			double pos1 = 0.0, pos2 = 0.0;

			// 1) 현재 실제 위치 읽기
			CAXM.AxmStatusGetActPos( axisNo, ref pos1 );

			// 2) 잠깐 대기
			await Task.Delay( intervalMs );

			// 3) 다시 실제 위치 읽기
			CAXM.AxmStatusGetActPos( axisNo, ref pos2 );

			// 4) 위치 변화량 / 시간으로 속도 계산 (초 단위)
			double delta = pos2 - pos1;
			double seconds = intervalMs / 1000.0;

			return delta / seconds; // 단위: mm/s 또는 pulse/s (축 설정에 따라)
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

		public void ServoOn( int axisNum )
		{
			//++ 지정한 축의 서보온을 설정합니다.
			CAXM.AxmSignalServoOn( axisNum, 1 );
		}

		public void ServoOff( int axisNum )
		{
			//++ 지정한 축의 서보온을 설정합니다.
			CAXM.AxmSignalServoOn( axisNum, 0 );
		}

		private void JogStart( int axisNum, int dir )
		{
			if(!((dir == 1) || (dir == -1)))
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
				MessageBox.Show( String.Format( "AxmMoveVel return error[Code:{0:d}]", duRetCode ) );

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
	}
}
