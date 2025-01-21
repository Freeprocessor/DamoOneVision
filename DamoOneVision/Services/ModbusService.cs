using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static OpenCvSharp.FileStorage;
using System.Windows.Threading;
using DamoOneVision.Utilities;

namespace DamoOneVision.Services
{
	public class ModbusService : IDisposable
	{
		private string _ip="";
		private int _port=0;
		private TcpClient _tcpClient;
		private IModbusMaster _master;
		private bool _connected;
		//public bool isConnected;

		public bool[] InputCoil = new bool[0x20];
		public bool[] OutputCoil = new bool[0x20];


		public ModbusService( string ip, int port )
		{
			_ip = ip;
			_port = port;
			ConnectAsync();
		}

		public bool IsConnected => _connected && _tcpClient?.Connected == true;

		public async Task ConnectAsync( )
		{
			if(_connected) return;

			try
			{
				_tcpClient = new TcpClient();      // TCP Client 선언
				await _tcpClient.ConnectAsync( _ip, _port ); // TCP Client 연결
				var factory = new ModbusFactory();          // ModbusFactory 선언
				_master = factory.CreateMaster( _tcpClient ); // IModbusMaster 초기화
				_connected = true;
			}
			catch (Exception ex)
			{
				Logger.WriteLine( ex.Message );
				_connected = false;
			}

			_modbusPollingAsync();

		}

		public async Task DisconnectAsync( )
		{
			if (!_connected) return;
			_tcpClient.Close();
			_tcpClient = null;
			_connected = false;
			// _master = null;
		}

		private async void _modbusPollingAsync( )
		{
			while (_connected)
			{
				ReadInputs( 0, 0, 32 ).CopyTo( InputCoil, 0 );
				WriteMultipleCoils( 0, 0, OutputCoil );

				//ReadInputRegisters

				await Task.Delay( 1 );
			}
		}


		// Coil Write Multiple
		public int WriteMultipleCoils( byte station, ushort startAddress, bool[ ] data )
		{
			if (!IsConnected) throw new Exception( "Modbus is not connected." );
			try
			{
				_master.WriteMultipleCoils( station, startAddress, data );
				return 0;
			}
			catch (Exception ex)
			{
				Logger.WriteLine( ex.Message );
				return -1;
			}
		}

		// Coil Write Multiple
		public bool[ ] ReadInputs( byte station, ushort startAddress, ushort num )
		{
			if (!IsConnected) throw new Exception( "Modbus is not connected." );
			try
			{
				return _master.ReadInputs( station, startAddress, num );
			}
			catch (Exception ex)
			{
				Logger.WriteLine( ex.Message );
				return null;
			}
		}

		public bool[ ] ReadCoils( byte station, ushort startAddress, ushort num )
		{
			if (!IsConnected) throw new Exception( "Modbus is not connected." );

			try
			{
				return _master.ReadCoils( station, startAddress, num );
			}
			catch (Exception ex)
			{
				Logger.WriteLine( ex.Message );
				return null;
			}
		}

		public int WriteSingleCoil( byte station, ushort startAddress, bool data )
		{
			if (!IsConnected) throw new Exception( "Modbus is not connected." );
			try
			{
				_master.WriteSingleCoil( station, startAddress, data );
				return 0;
			}
			catch (Exception ex)
			{
				Logger.WriteLine( ex.Message );
				return -1;
			}
		}

		// Read Input Register 
		public ushort[ ] ReadInputRegisters( byte station, ushort startAddress, ushort numInputs )
		{
			if (!IsConnected) throw new Exception( "Modbus is not connected." );
			try
			{
				return _master.ReadInputRegisters( station, startAddress, numInputs );
			}
			catch (Exception ex)
			{
				Logger.WriteLine( ex.Message );
				return null;
			}
		}

		// Read Input Register 32bit 
		/// <summary>
		/// Read 32bit input registers
		/// </summary>
		/// <param name="station"></param>
		/// <param name="startAddress"></param>
		/// <param name="numInputs"></param>
		/// <returns></returns>
		/// TODO : 예외처리 필요
		public int[ ] ReadInputRegisters32( byte station, ushort startAddress, ushort numInputs )
		{
			int j = 0;
			int[] result = new int[numInputs];
			numInputs = (ushort) (numInputs * 2);
			ushort[] registers = _master.ReadInputRegisters( station, startAddress, numInputs );

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
			if (!IsConnected) throw new Exception( "Modbus is not connected." );

			try
			{
				return _master.ReadHoldingRegisters( station, startAddress, numInputs );
			}
			catch (Exception ex)
			{
				Logger.WriteLine( ex.Message );
				return null;
			}
		}

		/// <summary>
		/// Read 32bit holding registers
		/// </summary>
		/// <param name="station"></param>
		/// <param name="startAddress"></param>
		/// <param name="numInputs"></param>
		/// <returns></returns>
		/// TODO : 예외처리 필요
		// Read Holding Register 32bit
		public int[ ] ReadHoldingRegisters32( byte station, ushort startAddress, ushort numInputs )
		{
			int j = 0;
			int[] result = new int[numInputs];
			numInputs = (ushort) (numInputs * 2);
			ushort[] registers = _master.ReadHoldingRegisters( station, startAddress, numInputs );

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
			if (!IsConnected) throw new Exception( "Modbus is not connected." );
			try
			{
				_master.WriteSingleRegister( station, startAddress, data );
				return 0;
			}
			catch (Exception ex)
			{
				Logger.WriteLine( ex.Message );
				return -1;
			}
		}

		public int WriteHoldingRegisters32( byte station, ushort startAddress, int data )
		{
			ushort[] registers = new ushort[ 2 ];
			registers[ 0 ] = (ushort) (data & 0xFFFF);
			registers[ 1 ] = (ushort) (data >> 16);
			_master.WriteMultipleRegisters( station, startAddress, registers );
			return 0;
		}

		public void Dispose( )
		{
			if (_tcpClient != null)
			{
				_tcpClient.Close();
				_tcpClient = null;
			}
			_connected = false;
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
						Logger.WriteLine( "SelfHolding operation timed out." );
						throw new TimeoutException( "SelfHolding operation timed out." );
					}
					//Thread.Sleep( 10 );
				}
			} );
		}



	}
}
