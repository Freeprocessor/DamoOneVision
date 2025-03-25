using DamoOneVision.Ajinextek.Motion;
using DamoOneVision.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamoOneVision.Services
{
	public class DeviceControlService
	{
		public event Func<Task> TriggerDetected;

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

		const int EMSTOPSW = 0x00;
		const int INVERTERALARM = 0x03;
		const int VISIONTRIGGER1 = 0x06;
		const int VISIONTRIGGER2 = 0x07;

		const int TOWERLAMP_RED = 0x00;
		const int TOWERLAMP_YELLOW = 0x01;
		const int TOWERLAMP_GREEN = 0x02;
		const int EJECTOR = 0x03;
		const int MAINCV = 0x06;


		ModbusService _modbus;
		AdvantechCardService _advantechCard;
		MotionService _motionService;

		bool _isError = false;

		public DeviceControlService( ModbusService modbus, AdvantechCardService advantechCard, MotionService motionService)
		{
			_modbus = modbus;
			_advantechCard = advantechCard;
			_motionService = motionService;

			Connect();

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

		public void TowerLampAsync(string status)
		{
			if(status == "STOP")
			{
				_isError = false;
				_advantechCard.WriteCoil[ TOWERLAMP_RED ] = false;
				_advantechCard.WriteCoil[ TOWERLAMP_YELLOW ] = true;
				_advantechCard.WriteCoil[ TOWERLAMP_GREEN ] = false;
			}
			else if(status == "START")
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

		private async void TowerLampErrorAsync()
		{
			while(_isError)
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

		public async void EjectActionAsync( )
		{
			await Task.Run( async ( ) =>
			{
				await Task.Delay( 3000 );
				_advantechCard.WriteCoil[ EJECTOR ] = true;
				await Task.Delay( 500 );
				_advantechCard.WriteCoil[ EJECTOR ] = false;
			} );
		}

		public void SetModel( MotionModel motionModel )
		{
			_motionService.SetModel( motionModel );
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

				while (true)
				{
					if (_triggerReadingStop)
					{

						break;
					}
					/// Trigger-1 ON
					if (_advantechCard.ReadCoil[ VISIONTRIGGER1 ] == true)
					{


						// Convyer Delay
						//await Task.Delay( 350 );
						Logger.WriteLine( "Trigger Detected" );
						if (TriggerDetected != null)
						{
							await _motionService.XAxisMoveWaitPos();
							_ = _motionService.XAxisMoveEndPos();
							await Task.Delay( _motionService.CameraDelay );
							await TriggerDetected();
							_motionService.XAxisStop();
							await _motionService.XAxisWaitingStop();
							await _motionService.XAxisMoveWaitPos();
						}
						//modbus.WriteSingleCoil( 0, 0x06, false );
						//while (modbus.ReadInputs( 0, 0x06, 1 )[ 0 ]) ;
					}

				}
				//modbus.WriteSingleCoil( 0, 0x2A, false );


				_isTriggerReading = false;
			} );
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
			//Logger.WriteLine( "C/V OFF" );
			TowerLampAsync( "STOP" );
			Logger.WriteLine( "Machine Stop." );
		}


	}
}
