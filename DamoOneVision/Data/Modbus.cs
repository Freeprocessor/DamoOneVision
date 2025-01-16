using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static OpenCvSharp.FileStorage;
using System.Windows.Threading;

namespace DamoOneVision.Data
{
	public class Modbus
	{
		private static string ip="";
		private static int port=0;
		private static TcpClient tcpClient;
		public static IModbusMaster master;
		public bool isConnected = false;




		ushort TowerLampREDONAddress = 0x00;
		ushort TowerLampREDOFFAddress = 0x01;
		ushort TowerLampYELONAddress = 0x02;
		ushort TowerLampYELOFFAddress = 0x03;
		ushort TowerLampGRNONAddress = 0x04;
		ushort TowerLampGRNOFFAddress = 0x05;
		ushort MainCVONAddress = 0x10;
		ushort MainCVOFFAddress = 0x11;
		ushort EjectONAddress = 0x18;
		ushort EjectOFFAddress = 0x19;
		ushort EjectManualONAddress = 0x1a;
		ushort EjectManualOFFAddress = 0x1b;
		ushort Vision1LampONAddress = 0x20;
		ushort Vision1LampOFFAddress = 0x21;
		ushort Vision2LampONAddress = 0x22;
		ushort Vision2LampOFFAddress = 0x23;
		ushort Vision3LampONAddress = 0x24;

		public Modbus( )
		{
			ip = "";
			port = 0;
		}

		public string Ip { get { return ip; } set { ip = value; } }
		public int Port { get { return port; } set { port = value; } }


		public void Connect( )
		{
			tcpClient = new TcpClient( ip, port );      // TCP Client 선언
			var factory = new ModbusFactory();          // ModbusFactory 선언
			master = factory.CreateMaster( tcpClient ); // IModbusMaster 초기화
			isConnected = true;
		}

		public bool ConnectionCheck( )
		{
			if (tcpClient.Connected)
			{
				isConnected = true;
			}
			else
			{
				isConnected = false;
				Data.Log.WriteLine( "Connection Fail" );
			}
			return isConnected;
		}

		// Coil Write Multiple
		public int WriteMultipleCoils( byte station, ushort startAddress, bool[ ] data )
		{
			if (ConnectionCheck())
			{
				master.WriteMultipleCoils( station, startAddress, data );
				return 0;
			}
			else
			{
				return -1;
			}

		}

		// Coil Write Multiple
		public bool[ ] ReadInputs( byte station, ushort startAddress, ushort num )
		{
			if (ConnectionCheck())
			{
				bool[] result = master.ReadInputs( station, startAddress, num );
				return result;
			}
			else
			{
				return null;
			}

		}

		public bool[ ] ReadCoils( byte station, ushort startAddress, ushort num )
		{
			bool[] result = master.ReadCoils( station, startAddress, num );
			return result;
		}

		public int WriteSingleCoil( byte station, ushort startAddress, bool data )
		{
			master.WriteSingleCoil( station, startAddress, data );
			return 0;
		}

		// Read Input Register 
		public ushort[ ] ReadInputRegisters( byte station, ushort startAddress, ushort numInputs )
		{
			ushort[] registers = master.ReadInputRegisters( station, startAddress, numInputs );
			return registers;
		}

		// Read Input Register 32bit 
		public int[ ] ReadInputRegisters32( byte station, ushort startAddress, ushort numInputs )
		{
			int j = 0;
			int[] result = new int[numInputs];
			numInputs = (ushort) (numInputs * 2);
			ushort[] registers = master.ReadInputRegisters( station, startAddress, numInputs );

			// 16bit 받아서 bit shift 연산 후 더함 = 32bit
			for (int i = 0; i < registers.Length / 2; i++)
			{
				result[ i ] = registers[ j ] + (registers[ j + 1 ] << 16);
				j += 2;
			}
			return result;
		}

		// Read Holding Register
		public ushort[ ] ReadHoldingRegisters( byte station, ushort startAddress, ushort numInputs )
		{
			ushort[] registers = master.ReadHoldingRegisters( station, startAddress, numInputs );
			return registers;
		}

		// Read Holding Register 32bit
		public int[ ] ReadHoldingRegisters32( byte station, ushort startAddress, ushort numInputs )
		{
			int j = 0;
			int[] result = new int[numInputs];
			numInputs = (ushort) (numInputs * 2);
			ushort[] registers = master.ReadHoldingRegisters( station, startAddress, numInputs );

			// 16bit 받아서 bit shift 연산 후 더함 = 32bit
			for (int i = 0; i < registers.Length / 2; i++)
			{
				result[ i ] = registers[ j ] + (registers[ j + 1 ] << 16);
				j += 2;
			}
			return result;
		}

		public int WriteSingleRegister( byte station, ushort startAddress, ushort data )
		{
			master.WriteSingleRegister( station, startAddress, data );
			return 0;
		}

		public int WriteHoldingRegisters32( byte station, ushort startAddress, int data )
		{
			ushort[] registers = new ushort[ 2 ];
			registers[ 0 ] = (ushort) (data & 0xFFFF);
			registers[ 1 ] = (ushort) (data >> 16);
			master.WriteMultipleRegisters( station, startAddress, registers );
			return 0;
		}

		public async Task SelfHolding( ushort input, ushort output )
		{
			bool[] coil;

			await Task.Run( ( ) =>
			{
				WriteSingleCoil( 0, output, true );
				var startTime = DateTime.Now;
				while (true)
				{
					coil = ReadInputs( 0, input, 1 );
					if (coil[ 0 ] == true)
					{
						WriteSingleCoil( 0, output, false );
						break;
					}
					if ((DateTime.Now - startTime).TotalMilliseconds > 5000) // 5초 타임아웃
					{
						Data.Log.WriteLine( "SelfHolding operation timed out." );
						throw new TimeoutException( "SelfHolding operation timed out." );
					}
					//Thread.Sleep( 10 );
				}
			} );
		}



	}
}
