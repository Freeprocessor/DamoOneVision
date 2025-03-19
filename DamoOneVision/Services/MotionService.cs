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
        

		public MotionService( )
		{

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

		public void XAxisJogStop( )
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
