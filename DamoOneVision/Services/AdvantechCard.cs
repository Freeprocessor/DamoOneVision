using Advantech.Adam;
using Advantech.Common;
using Advantech.Protocol;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

using System.Diagnostics;

namespace DamoOneVision.Services
{
	internal class AdvantechCard
	{
		private AdamSocket _adamSocket = new AdamSocket();


		public event Func<Task> TriggerDetected;

		/// <summary>
		/// Advantech IP 주소
		/// </summary>
		private string _ip ;

		/// <summary>
		/// Advantech Port 번호
		/// </summary>
		private int _port ;

		/// <summary>
		/// Advantech Card 연결 상태
		/// </summary>
		private bool _isConnected = false;

		/// <summary>
		/// Advantech Card Input Status
		/// </summary>
		public bool[ ] ReadCoil = new bool[8];

		/// <summary>
		/// Advantech Card Output Status
		/// </summary>
		public bool WriteCoil = false;

		/// <summary>
		/// Trigger Reading Status
		/// </summary>
		private bool _isTriggerReading = false;

		/// <summary>
		/// Trigger Reading OFF 요청
		/// </summary>
		private bool _triggerReadingStop = false;

		public AdvantechCard( string ip, int port )
		{
			_ip = ip;
			_port = port;
			ConnectAsync();
		}


		// TCP 모드로 열기 (Adam6000, 6200, 4000 시리즈 등)
		/// <summary>
		/// Advantech Card 연결
		/// </summary>
		public async void ConnectAsync( )
		{
			//Advantech Card Timeout 설정
			_adamSocket.SetTimeout( 1000, 1000, 1000 );

			// Connect to the ADAM-6000 device
			await Task.Run( () => _isConnected = _adamSocket.Connect( _ip, ProtocolType.Tcp, _port ) );

			if (_isConnected)
			{
				Logger.WriteLine( "Adventech Connected" );
			}
			else
			{
				Logger.WriteLine( "Advantech Connect Fail" );
			}

			
		}

		/// <summary>
		/// Advantech Card 연결 해제
		/// </summary>
		public async void DisconnectAsync( )
		{
			await Task.Run( ( ) =>
			{
				Logger.WriteLine( "Advantech Disconnect" );
				_adamSocket.Disconnect();
				_adamSocket = null;
				_isConnected = false;
			} );
		}

		/// <summary>
		/// Advantech Card Digital Output Write
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool WriteBit( int channel, bool value )
		{
			if (_isConnected)
			{
				return _adamSocket.DigitalOutput().SetValue( channel, value );
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Advantech Card Digital Input Read
		/// </summary>
		public async void ReadBitAsync( )
		{
			Logger.WriteLine( "ReadBitAsync Start" );
			while (true)
			{
				if (_isConnected)
				{
					bool[] value;
					_adamSocket.Modbus().ReadCoilStatus( 1, 8, out value );

					ReadCoil = value;

					_adamSocket.DigitalOutput().SetValue( 0, WriteCoil );
				}
				else
				{
					Logger.WriteLine( "Advantech Not Connected. ReadBitAsync Stop" );
					break;
				}
				await Task.Delay( 1 );
			}
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
					if (ReadCoil[ 0 ] == true)
					{
						// Convyer Delay
						await Task.Delay( 1450 );
						if(TriggerDetected != null)
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
		private async Task TriggerReadingStopAsync( )
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
	}
}
