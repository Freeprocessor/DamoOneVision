using System;
using System.Collections.Generic;
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

		const int X = 0;
		const int Z = 1;

		private bool _isInitialized = false; // 라이브러리 초기화 여부

		public MotionService( )
		{
			InitLibrary();
			ServoOn( X );
			ServoOn( Z );


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
			// ※ [CAUTION] 아래와 다른 Mot파일(모션 설정파일)을 사용할 경우 경로를 변경하십시요.
			String szFilePath = "C:\\Program Files\\EzSoftware RM\\EzSoftware\\MotionDefault.mot";
			//++ AXL(AjineXtek Library)을 사용가능하게 하고 장착된 보드들을 초기화합니다.
			if (CAXL.AxlOpen( 7 ) != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				_isInitialized = false;
				return;
			}
			//++ 지정한 Mot파일의 설정값들로 모션보드의 설정값들을 일괄변경 적용합니다.
			if (CAXM.AxmMotLoadParaAll( szFilePath ) != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				_isInitialized = false;
				return;
			}
			else
			{
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
				duRetCode = CAXM.AxmMoveStartPos( 0, position, velocity, acceleration, deceleration );
			} );

			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				MessageBox.Show( $"AxmMoveStartPos return error[Code:{duRetCode}]", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
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

		private void JogPStart( )
		{

		}

		public void XAxisJogPStart( )
		{
			uint duRetCode   = 0;

			double dVelocity = Math.Abs(100);
			double dAccel    = Math.Abs(100);
			double dDecel    = Math.Abs(100);

			//++ 지정한 축을 (+)방향으로 지정한 속도/가속도/감속도로 모션구동합니다.
			duRetCode = CAXM.AxmMoveVel( 0, dVelocity, dAccel, dDecel );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
				MessageBox.Show( String.Format( "AxmMoveVel return error[Code:{0:d}]", duRetCode ) );

			Logger.WriteLine( "JogPX_Start" );
		}

		public void XAxisJogNStart( )
		{
			uint duRetCode   = 0;

			//double dVelocity = Math.Abs(Convert.ToDouble(edtJogVel.Value));
			//double dAccel    = Math.Abs(Convert.ToDouble(edtJogAcc.Value));
			//double dDecel    = Math.Abs(Convert.ToDouble(edtJogDec.Value));
			double dVelocity = Math.Abs(100);
			double dAccel    = Math.Abs(100);
			double dDecel    = Math.Abs(100);

			//++ 지정한 축을 (+)방향으로 지정한 속도/가속도/감속도로 모션구동합니다.
			duRetCode = CAXM.AxmMoveVel( 0, -dVelocity, dAccel, dDecel );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
				MessageBox.Show( String.Format( "AxmMoveVel return error[Code:{0:d}]", duRetCode ) );

			Logger.WriteLine( "JogPX_Start" );
		}

		public void XAxisStop( )
		{
			//++ 지정한 축의 Jog구동(모션구동)을 종료합니다.  
			CAXM.AxmMoveSStop( 0 );
		}



		public void ZAxisJogPStart( )
		{
			uint duRetCode   = 0;

			double dVelocity = Math.Abs(100);
			double dAccel    = Math.Abs(100);
			double dDecel    = Math.Abs(100);

			//++ 지정한 축을 (+)방향으로 지정한 속도/가속도/감속도로 모션구동합니다.
			duRetCode = CAXM.AxmMoveVel( 1, dVelocity, dAccel, dDecel );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
				MessageBox.Show( String.Format( "AxmMoveVel return error[Code:{0:d}]", duRetCode ) );

			Logger.WriteLine( "JogPX_Start" );
		}

		public void ZAxisJogNStart( )
		{
			uint duRetCode   = 0;

			double dVelocity = Math.Abs(100);
			double dAccel    = Math.Abs(100);
			double dDecel    = Math.Abs(100);

			//++ 지정한 축을 (+)방향으로 지정한 속도/가속도/감속도로 모션구동합니다.
			duRetCode = CAXM.AxmMoveVel( 1, -dVelocity, dAccel, dDecel );
			if (duRetCode != (uint) AXT_FUNC_RESULT.AXT_RT_SUCCESS)
				MessageBox.Show( String.Format( "AxmMoveVel return error[Code:{0:d}]", duRetCode ) );

			Logger.WriteLine( "JogPX_Start" );
		}

		public void ZAxisJogStop( )
		{
			//++ 지정한 축의 Jog구동(모션구동)을 종료합니다.  
			CAXM.AxmMoveSStop( 1 );
		}
	}
}
