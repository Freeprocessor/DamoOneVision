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
	internal class AdvantechCardService
	{
		private AdamSocket _adamSocket = new AdamSocket();

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


		public AdvantechCardService( string ip, int port )
		{
			_ip = ip;
			_port = port;
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


	}
}
