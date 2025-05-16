using DamoOneVision.Models;
using System.Diagnostics;

namespace DamoOneVision.Services
{
	public class DeviceControlService
	{
		public event Func<Task<bool>> TriggerDetected;

		const int X = 0;
		//const uint Y = 1;
		const int Z = 2;

		/// <summary>
		/// Trigger Reading Status
		/// </summary>
		private bool _isTriggerReading = false;

		/// <summary>
		/// Trigger Reading OFF 요청
		/// </summary>
		private bool _triggerReadingStop = false;

		private bool _isGood = false;

		const int EMSTOPSW = 0x00;
		const int INVERTERALARM = 0x03;
		const int VISIONTRIGGER1 = 0x06;
		const int VISIONTRIGGER2 = 0x07;

		const int TOWERLAMP_RED = 0x00;
		const int TOWERLAMP_YELLOW = 0x01;
		const int TOWERLAMP_GREEN = 0x02;
		const int EJECTOR = 0x03;
		const int SIDECV = 0x04;
		const int MAINCV = 0x06;


		ModbusService _modbus;
		AdvantechCardService _advantechCard;
		MotionService _motionService;

		bool _isError = false;

		public DeviceControlService( ModbusService modbus, AdvantechCardService advantechCard, MotionService motionService )
		{
			_modbus = modbus;
			_advantechCard = advantechCard;
			_motionService = motionService;

			Connect();

		}

		public void ConveyorReadStart( )
		{
			_motionService.ConveyorReadStart();
		}

		public void ConveyorReadStop( )
		{
			_motionService.ConveyorReadStop();
		}

		public void Connect( )
		{
			//_modbus.ConnectAsync();
			_advantechCard.ConnectAsync();
		}

		public void Disconnect( )
		{
			//_modbus.DisconnectAsync();
			_advantechCard.DisconnectAsync();
		}

		public void TowerLampAsync( string status )
		{
			if (status == "STOP")
			{
				_isError = false;
				_advantechCard.WriteCoil[ TOWERLAMP_RED ] = false;
				_advantechCard.WriteCoil[ TOWERLAMP_YELLOW ] = true;
				_advantechCard.WriteCoil[ TOWERLAMP_GREEN ] = false;
			}
			else if (status == "START")
			{
				_isError = false;
				_advantechCard.WriteCoil[ TOWERLAMP_RED ] = false;
				_advantechCard.WriteCoil[ TOWERLAMP_YELLOW ] = false;
				_advantechCard.WriteCoil[ TOWERLAMP_GREEN ] = true;
			}
			else if (status == "ERROR")
			{
				_isError = true;
				TowerLampErrorAsync();
			}
		}

		private async void TowerLampErrorAsync( )
		{
			while (_isError)
			{
				_advantechCard.WriteCoil[ TOWERLAMP_RED ] = !_advantechCard.WriteCoil[ TOWERLAMP_RED ];
				_advantechCard.WriteCoil[ TOWERLAMP_YELLOW ] = false;
				_advantechCard.WriteCoil[ TOWERLAMP_GREEN ] = false;

				await Task.Delay( 500 );
			}
		}

		public void MainCVOn( )
		{
			_advantechCard.WriteCoil[ MAINCV ] = true;
		}

		public void MainCVOff( )
		{
			_advantechCard.WriteCoil[ MAINCV ] = false;
		}

		public void SideCVOn( )
		{
			_advantechCard.WriteCoil[ SIDECV ] = true;
		}

		public void SideCVOff( )
		{
			_advantechCard.WriteCoil[ SIDECV ] = false;
		}

		//public async void Vision1LampONAsync( )
		//{
		//	await _modbus.SelfHolding( 0x20, 0x20 );
		//}

		//public async void Vision1LampOFFAsync( )
		//{
		//	await _modbus.SelfHolding( 0x21, 0x21 );
		//}

		//public async void Vision2LampONAsync( )
		//{
		//	await _modbus.SelfHolding( 0x22, 0x22 );
		//}

		//public async void Vision2LampOFFAsync( )
		//{
		//	await _modbus.SelfHolding( 0x23, 0x23 );
		//}

		//public async void Vision3LampONAsync( )
		//{
		//	await _modbus.SelfHolding( 0x24, 0x24 );
		//}

		//public async void Vision3LampOFFAsync( )
		//{
		//	await _modbus.SelfHolding( 0x25, 0x25 );
		//}

		public void EjectorManualON( )
		{
			_advantechCard.WriteCoil[ EJECTOR ] = true;
		}

		public void EjectorManualOFF( )
		{
			_advantechCard.WriteCoil[ EJECTOR ] = false;
		}

		public void SetModel( MotionModel motionModel )
		{
			_motionService.SetModel( motionModel );
		}

		public async Task ZAxisMoveWorkPos( )
		{
			await _motionService.ZAxisMoveWorkPos();
		}

		public async Task ZAxisMoveEndPos( )
		{
			await _motionService.ZAxisMoveEndPos();
		}

		/// <summary>
		/// Advantech Card Trigger(DI0) Read Start Async
		/// </summary>
		public async void TriggerReadingStartAsync( )
		{

			_isTriggerReading = true;
			_triggerReadingStop = false;
			await Task.Run( async ( ) =>
			{
				//modbus.WriteSingleCoil( 0, 0x2A, true );
				Logger.WriteLine( "TriggerReadingAsync Start" );
				// 제품 간섭 대기
				while (_advantechCard.ReadCoil[ VISIONTRIGGER1 ] == true)
				{
					await Task.Delay( 100 );
				}

				while (true)
				{
					if (_triggerReadingStop)
					{

						break;
					}
					/// Trigger-1 ON
					if (_advantechCard.ReadCoil[ VISIONTRIGGER1 ] == true)
					{
						/// Tast
						await TriggerActionAsync();
						Logger.WriteLine( "Trigger Detected." );
						//await TriggerAtionTestAsync();
					}

				}
				//modbus.WriteSingleCoil( 0, 0x2A, false );


				_isTriggerReading = false;
			} );
		}

		private async Task TriggerActionAsync( )
		{
			var sw = new Stopwatch();
			sw.Start();
			var ejector = new Ejector( _advantechCard, _motionService );

			// Convyer Delay
			//await Task.Delay( 350 );
			ejector.EjectActionAsync();
			//Logger.WriteLine( "Trigger Detected" );
			if (TriggerDetected != null)
			{
				await _motionService.XAxisMoveWaitPos();
				_ = _motionService.XAxisMoveEndPos();
				//Logger.WriteLine( $"Tracking Start : {sw.ElapsedMilliseconds} ms" );
				//Logger.WriteLine( "{_motionService.CameraDelay}" );
				await Task.Delay( _motionService.CameraDelay );
				_isGood = await TriggerDetected();
				ejector.IsGood = _isGood;
				//_isGood = await TriggerDetected();

				//Logger.WriteLine( $"Capture Complete : {sw.ElapsedMilliseconds} ms" );
				_motionService.XAxisStop();
				//Logger.WriteLine( $"Tracking Stop {sw.ElapsedMilliseconds} ms" );
				await _motionService.XAxisWaitingStop();
				//Logger.WriteLine( $"Tracking Stop Complete {sw.ElapsedMilliseconds} ms" );
				Logger.WriteLine( $"Current Position {_motionService.XAxisGetCommandPosition()} pulse)" );
				await _motionService.XAxisMoveWaitPos();
				//Logger.WriteLine( $"Wait Move Complete : {sw.ElapsedMilliseconds} ms" );
			}
			//modbus.WriteSingleCoil( 0, 0x06, false );
			//while (modbus.ReadInputs( 0, 0x06, 1 )[ 0 ]) ;
			while (_advantechCard.ReadCoil[ VISIONTRIGGER1 ]) ;
			sw.Stop();
			Logger.WriteLine( $"TriggerReadingAsync End (total: {sw.ElapsedMilliseconds} ms)" );
		}

		private async Task TriggerAtionTestAsync( )
		{
			if (TriggerDetected != null)
			{
				await _motionService.XAxisMoveWaitPos();
				MainCVOff();
				await Task.Delay( 300 );
				_isGood = await TriggerDetected();
				await Task.Delay( 300 );
				_isGood = await TriggerDetected();
				await Task.Delay( 300 );
				_isGood = await TriggerDetected();
				await Task.Delay( 300 );
				_isGood = await TriggerDetected();
				await Task.Delay( 300 );
				_isGood = await TriggerDetected();
				await Task.Delay( 300 );
				_isGood = await TriggerDetected();
				await Task.Delay( 300 );
				_isGood = await TriggerDetected();
				await Task.Delay( 300 );
				_isGood = await TriggerDetected();
				await Task.Delay( 300 );
				_isGood = await TriggerDetected();
				await Task.Delay( 300 );
				_isGood = await TriggerDetected();
				await Task.Delay( 300 );
				_isGood = await TriggerDetected();
				await Task.Delay( 300 );
				_isGood = await TriggerDetected();
				await Task.Delay( 300 );
				MainCVOn();
				while (_advantechCard.ReadCoil[ VISIONTRIGGER1 ]) ;
			}
		}


		/// <summary>
		/// Advantech Card Trigger(DI0) Read Stop Async
		/// </summary>
		/// <returns></returns>
		public async Task TriggerReadingStopAsync( )
		{
			_triggerReadingStop = true;
			await Task.Run( ( ) =>
			{
				while (_isTriggerReading)
				{
					System.Threading.Thread.Sleep( 1000 );
				}
			} );
			Logger.WriteLine( "TriggerReadingAsync Stop" );
		}

		public async Task MachineStartAction( )
		{


			/// Vision Lamp ON
			/// 
			//await _modbus.SelfHolding( 0x20, 0x20 );
			//await _modbus.SelfHolding( 0x22, 0x22 );
			//await _modbus.SelfHolding( 0x24, 0x24 );


			await _motionService.XAxisMoveWaitPos();
			await _motionService.ZAxisMoveWorkPos();
			MainCVOn();
			SideCVOn();
			//Logger.WriteLine( "Trigger Reading Start." );
			TriggerReadingStartAsync();
			TowerLampAsync( "START" );
			Logger.WriteLine( "Machine Start." );
		}

		public async Task MachineStopAction( )
		{

			await TriggerReadingStopAsync();
			//Logger.WriteLine( "Trigger Reading Stop." );
			MainCVOff();
			SideCVOff();
			//Logger.WriteLine( "C/V OFF" );
			TowerLampAsync( "STOP" );
			Logger.WriteLine( "Machine Stop." );
		}


	}

	public class Ejector : IDisposable
	{
		public bool IsGood;
		AdvantechCardService _advantechCard;
		MotionService _motionService;

		const int EJECTOR = 0x03;

		public Ejector( AdvantechCardService advantechCard, MotionService motionService )
		{
			_advantechCard = advantechCard;
			_motionService = motionService;
		}

		public async void EjectActionAsync( )
		{
			await Task.Run( async ( ) =>
			{

				int ejectorDelay = 0;
				int ejecttorDistance = 530;
				ejectorDelay = (int) (ejecttorDistance / _motionService.ConveyorSpeed * 1000.00);
				Logger.WriteLine( $"[Ejector] Eject Delay 설정값: {_motionService.ConveyorSpeed} ms" );
				int safeDelay = Math.Max(0, ejectorDelay); // 음수 방지
				await Task.Delay( safeDelay );
				if (IsGood)
				{
					return;
				}
				_advantechCard.WriteCoil[ EJECTOR ] = true;
				await Task.Delay( 500 );
				_advantechCard.WriteCoil[ EJECTOR ] = false;
			} );
		}
		public void Dispose( )
		{
		}

	}
}
