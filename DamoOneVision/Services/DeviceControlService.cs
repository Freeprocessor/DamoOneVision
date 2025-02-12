using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamoOneVision.Services
{
	internal class DeviceControlService
	{
		public event Func<Task> TriggerDetected;

		/// <summary>
		/// Trigger Reading Status
		/// </summary>
		private bool _isTriggerReading = false;

		/// <summary>
		/// Trigger Reading OFF 요청
		/// </summary>
		private bool _triggerReadingStop = false;


		ModbusService _modbus;
		AdvantechCardService _advantechCard;

		bool _isError = false;

		public DeviceControlService( ModbusService modbus, AdvantechCardService advantechCard)
		{
			_modbus = modbus;
			_advantechCard = advantechCard;

			Connect();

		}

		public void Connect( )
		{
			_modbus.ConnectAsync();
			_advantechCard.ConnectAsync();
		}

		public void Disconnect( )
		{
			_modbus.DisconnectAsync();
			_advantechCard.DisconnectAsync();
		}

		public async Task TowerLampAsync(string status)
		{
			if(status == "STOP")
			{
				_isError = false;
				var tasks = new[]
				{
				_modbus.SelfHolding( 1, 1 ),
				_modbus.SelfHolding( 2, 2 ),
				_modbus.SelfHolding( 5, 5 )
				};
				await Task.WhenAll( tasks );
			}
			else if(status == "START")
			{
				_isError = false;
				var tasks = new[]
				{
				_modbus.SelfHolding( 1, 1 ),
				_modbus.SelfHolding( 3, 3 ),
				_modbus.SelfHolding( 4, 4 )
				};
				await Task.WhenAll( tasks );
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
				var tasks1 = new[]
				{
				_modbus.SelfHolding( 0, 0 ),
				_modbus.SelfHolding( 3, 3 ),
				_modbus.SelfHolding( 5, 5 )
				};
				await Task.WhenAll( tasks1 );

				await Task.Delay( 500 );

				var tasks2 = new[]
				{
				_modbus.SelfHolding( 1, 1 ),
				_modbus.SelfHolding( 3, 3 ),
				_modbus.SelfHolding( 5, 5 )
				};
				await Task.WhenAll( tasks2 );

				await Task.Delay( 500 );
			}
		}

		public async void MainCVRunAsync( )
		{
			await _modbus.SelfHolding( 0x10, 0x10 );
		}

		public async void MainCVStopAsync( )
		{
			await _modbus.SelfHolding( 0x11, 0x11 );
		}

		public async void Vision1LampONAsync( )
		{
			await _modbus.SelfHolding( 0x20, 0x20 );
		}

		public async void Vision1LampOFFAsync( )
		{
			await _modbus.SelfHolding( 0x21, 0x21 );
		}

		public async void Vision2LampONAsync( )
		{
			await _modbus.SelfHolding( 0x22, 0x22 );
		}

		public async void Vision2LampOFFAsync( )
		{
			await _modbus.SelfHolding( 0x23, 0x23 );
		}

		public async void Vision3LampONAsync( )
		{
			await _modbus.SelfHolding( 0x24, 0x24 );
		}

		public async void Vision3LampOFFAsync( )
		{
			await _modbus.SelfHolding( 0x25, 0x25 );
		}

		public void EjectorManualON( )
		{
			_advantechCard.WriteBit( 0x01, true );
		}

		public void EjectorManualOFF( )
		{
			_advantechCard.WriteBit( 0x01, false );
		}

		public async void EjectActionAsync( )
		{
			await Task.Run( async ( ) =>
			{
				await Task.Delay( 3000 );
				_advantechCard.WriteCoil = true;
				await Task.Delay( 500 );
				_advantechCard.WriteCoil = false;
			} );
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
					if (_advantechCard.ReadCoil[ 0 ] == true)
					{
						// Convyer Delay
						await Task.Delay( 350 );
						if (TriggerDetected != null)
						{
							await TriggerDetected();
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
			///이걸 어떻게 해야할까?

			await _modbus.SelfHolding( 1, 1 );
			await _modbus.SelfHolding( 4, 4 );

			await _modbus.SelfHolding( 0x20, 0x20 );
			await _modbus.SelfHolding( 0x22, 0x22 );
			await _modbus.SelfHolding( 0x24, 0x24 );

			//modbus.WriteHoldingRegisters32( 0, 0x00, 20000 );
			int pos = 108000;
			int speed = 20000;
			_modbus.HoldingRegister32[ 0x00 ] = pos;
			_modbus.HoldingRegister32[ 0x01 ] = speed;

			await Task.Delay( 100 );

			if (_modbus.InputRegister32[ 0x00 ] != pos)
			{
				await Task.Run( ( ) =>
				{
					//modbus.WriteSingleCoil( 0, 0x0A, true );
					_modbus.OutputCoil[ 0x0A ] = true;
					Logger.WriteLine( "Servo Move Start" );
					var startTime = DateTime.Now;
					while (true)
					{
						//bool[] coil = modbus.ReadInputs( 0, 0x0A, 1 );
						if (_modbus.InputCoil[ 0x0A ])
						{
							//modbus.WriteSingleCoil( 0, 0x0A, false );
							_modbus.OutputCoil[ 0x0A ] = false;
							Logger.WriteLine( "Servo Moveing..." );
							break;
						}
						if ((DateTime.Now - startTime).TotalMilliseconds > 15000) // 10초 타임아웃
						{
							_modbus.OutputCoil[ 0x0A ] = false;
							Logger.WriteLine( "SelfHolding operation timed out." );
							//throw new TimeoutException( "SelfHolding operation timed out." );
							break;
						}
						Thread.Sleep( 10 );
					}
					startTime = DateTime.Now;
					Logger.WriteLine( "Servo Move Complete 대기" );
					//modbus.WriteSingleCoil( 0, 0x0B, true );
					_modbus.OutputCoil[ 0x0B ] = false;
					while (true)
					{
						//bool[] coil = modbus.ReadInputs( 0, 0x0B, 1 );
						if (_modbus.InputCoil[ 0x0B ])
						{
							//modbus.WriteSingleCoil( 0, 0x0B, false );
							_modbus.OutputCoil[ 0x0B ] = false;
							Logger.WriteLine( "Servo Move Complete" );
							break;
						}
						if ((DateTime.Now - startTime).TotalMilliseconds > 15000) // 10초 타임아웃
						{
							_modbus.OutputCoil[ 0x0B ] = false;
							Logger.WriteLine( "SelfHolding operation timed out." );
							//throw new TimeoutException( "SelfHolding operation timed out." );
							break;
						}
						Thread.Sleep( 10 );
					}
				} );
			}

			await _modbus.SelfHolding( 0x10, 0x10 );


			Logger.WriteLine( "Trigger Reading Start." );
			TriggerReadingStartAsync();
			Logger.WriteLine( "Machine Start." );
		}

		public async Task MachineStopAction( )
		{


			Logger.WriteLine( "Trigger Reading Stop." );
			await _modbus.SelfHolding( 0x11, 0x11 );
			Logger.WriteLine( "C/V OFF" );
			await _modbus.SelfHolding( 0x21, 0x21 );
			await _modbus.SelfHolding( 0x23, 0x23 );
			await _modbus.SelfHolding( 0x25, 0x25 );

			await _modbus.SelfHolding( 5, 5 );
			await _modbus.SelfHolding( 0, 0 );
			Logger.WriteLine( "Machine Stop." );
		}


	}
}
