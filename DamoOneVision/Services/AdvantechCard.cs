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
		AdamSocket adamSocket = new AdamSocket();

		public string Ip { get; set; }
		public int Port { get; set; }

		public bool isConnected = false;

		public bool[ ] ReadCoil = new bool[8];

		public AdvantechCard( )
		{

		}


		// TCP 모드로 열기 (Adam6000, 6200, 4000 시리즈 등)
		public void Connect( )
		{
			adamSocket.SetTimeout( 1000, 1000, 1000 );
			Ip = "192.168.2.20";
			Port = 502;
			isConnected = adamSocket.Connect( Ip, ProtocolType.Tcp, Port );

			if (isConnected)
			{
				Logger.WriteLine( "Adventech Connected" );
			}
			else
			{
				Logger.WriteLine( "Advantech Not Connected" );
			}
		}

		public void Disconnect( )
		{
			adamSocket.Disconnect();
			adamSocket = null;
			isConnected = false;
		}

		public bool WriteBit( int channel, bool value )
		{
			if (isConnected)
			{
				return adamSocket.DigitalOutput().SetValue( channel, value );
			}
			else
			{
				return false;
			}
		}

		public async void ReadBitAsync( )
		{
			Logger.WriteLine( "ReadBitAsync Start" );
			while (true)
			{
				if (isConnected)
				{
					bool[] value;
					adamSocket.Modbus().ReadCoilStatus( 1, 8, out value );

					ReadCoil = value;
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
