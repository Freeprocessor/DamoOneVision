using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static OpenCvSharp.FileStorage;
using System.Windows.Threading;
using Advantech.Adam;
using System.Windows;
using DamoOneVision.Views;

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

		private bool _lifeBitStatus = false;
		private bool _readServoPosition = false;

		//public bool[ ] InputCoil = new bool[0x40];
		//public bool[ ] OutputCoil = new bool[0x40];

		public bool[ ] InputCoil = new bool[0x10];
		public bool[ ] OutputCoil = new bool[0x10];

		public ushort[ ] InputRegister = new ushort[0x20];
		public ushort[ ] HoldingRegister = new ushort[0x20];

		public int[ ] InputRegister32 = new int[0x10];
		public int[ ] HoldingRegister32 = new int[0x10];

		int Lifebit = 0x01;
		//int Lifebit = 0x2F;


		public ModbusService( string ip, int port )
		{
			_ip = ip;
			_port = port;
		}

		public bool IsConnected => _connected && _tcpClient?.Connected == true;

		public async void ConnectAsync( )
		{
			if(_connected) return;

			try
			{
				_tcpClient = new TcpClient();      // TCP Client 선언
				await _tcpClient.ConnectAsync( _ip, _port ); // TCP Client 연결
				var factory = new ModbusFactory();          // ModbusFactory 선언
				_master = factory.CreateMaster( _tcpClient ); // IModbusMaster 초기화
				_connected = true;
				Logger.WriteLine("Modbus Connect Success");
			}
			catch (Exception ex)
			{
				Logger.WriteLine( ex.Message );
				_connected = false;
			}

			_modbusPollingAsync();
			//StartLifeRegAsync();
			//GVAsync();
			//ServoCurrentPositionAsync();
		}

		public async void DisconnectAsync( )
		{
			await Task.Run( ( ) =>
			{
				if (!_connected) return;
				_tcpClient.Close();
				_tcpClient = null;
				_connected = false;
				// _master = null;
			} );
		}

		private async void _modbusPollingAsync( )
		{
			while (_connected)
			{
				_modbusHoldingRegister32();
				WriteHoldingRegisters( 0, 0x00, HoldingRegister );

				InputRegister = ReadInputRegisters( 0, 0x00, 0x20 );
				_modbusInputRegister32();

				//InputCoil = ReadInputs( 0, 0, 0x40 );
				InputCoil = ReadInputs( 0, 0, 0x10 );
				WriteMultipleCoils( 0, 0, OutputCoil );

				await Task.Delay( 50 );
			}
		}

		private int _modbusInputRegister32( )
		{
			// 2) Holding Register 16비트 배열 읽기
			ushort[] registers = InputRegister;

			// 3) 2개씩 묶어서 32비트(int)로 변환
			int length = InputRegister32.Length;
			int[] result = new int[length];
			for (int i = 0; i < length; i++)
			{
				// Low word + (High word << 16)
				result[ i ] = registers[ 2 * i ] + (registers[ 2 * i + 1 ] << 16);
			}

			for (int i = 0; i < length; i++)
			{
				InputRegister32[ i ] = result[ i ];
			}

			return 0;
		}

		private int _modbusHoldingRegister32( )
		{
			// 1) dataArray.Length개의 int -> 각각 2개의 16비트로 변환
			int length = HoldingRegister32.Length;
			ushort[] registers = new ushort[length * 2];

			// 2) for문으로 int → 2 x ushort (Low Word, High Word)
			for (int i = 0; i < length; i++)
			{
				registers[ 2 * i ] = (ushort) (HoldingRegister32[ i ] & 0xFFFF);
				registers[ 2 * i + 1 ] = (ushort) (HoldingRegister32[ i ] >> 16);
			}

			for (int i=0; i<length*2; i++)
			{
				HoldingRegister[ i ] = registers[ i ];
			}

			return 0;
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

		public int WriteHoldingRegisters( byte station, ushort startAddress, ushort[] data )
		{
			if (!IsConnected) throw new Exception( "Modbus is not connected." );
			try
			{
				_master.WriteMultipleRegisters( station, startAddress, data );
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
			try
			{
				if (!IsConnected) throw new Exception( "Modbus is not connected." );
				await Task.Run( ( ) =>
				{
					//WriteSingleCoil( 0, output, true );
					OutputCoil[ output ] = true;
					var startTime = DateTime.Now;
					while (true)
					{
						if (InputCoil[ input ])
						{
							//WriteSingleCoil( 0, output, false );
							OutputCoil[ output ] = false;
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
			catch (Exception ex)
			{
				Logger.WriteLine( ex.Message );
			}

		}

		public async Task SelfHoldingRegister( ushort input, ushort output )
		{
			try
			{
				if (!IsConnected) throw new Exception( "Modbus is not connected." );
				await Task.Run( ( ) =>
				{
					//WriteSingleRegister( 0, output, 1 );
					HoldingRegister[ output ] = 1;
					var startTime = DateTime.Now;
					while (true)
					{
						if (InputCoil[ input ])
						{
							//WriteSingleRegister( 0, output, 0 );
							HoldingRegister[ output ] = 0;
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
			catch(Exception ex)
			{
				Logger.WriteLine( ex.Message );
			}

		}

		public async Task SelfHoldingRegister32( ushort input, ushort output )
		{
			try
			{
				if (!IsConnected) throw new Exception( "Modbus is not connected." );
				await Task.Run( ( ) =>
				{
					//WriteSingleRegister( 0, output, 1 );
					HoldingRegister32[ output ] = 1;
					var startTime = DateTime.Now;
					while (true)
					{
						if (InputCoil[ input ])
						{
							//WriteSingleRegister( 0, output, 0 );
							HoldingRegister32[ output ] = 0;
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
			catch (Exception ex)
			{
				Logger.WriteLine( ex.Message );
			}

		}

		//private async void TriggerDelayCalculationAsync( )
		//{
		//	await Task.Run( ( ) =>
		//	{
		//		Logger.WriteLine( "TriggerDelayCalculationAsync Start" );
		//		while (_connected)
		//		{
		//			int delay = 0;
		//			double distance = 200;
		//			double speed = 0;
		//			double time = 0;
		//			//delay = modbus.ReadInputRegisters( 0, 0x04, 1 )[ 0 ];
		//			delay = InputRegister[ 0x04 ];
		//			if (delay == 0)
		//			{
		//				Logger.WriteLine( "Trigger Delay Devide 0" );
		//				System.Threading.Thread.Sleep( 1000 );
		//				continue;
		//			}
		//			speed = 40.0 / (double) delay;
		//			time = distance / speed;

		//			//Log.WriteLine( $"Speed: {speed}, Time: {time}" );

		//			//modbus.WriteSingleRegister( 0, 0x04, (ushort) time );
		//			HoldingRegister[ 0x04 ] = (ushort) time;

		//		}
		//		Logger.WriteLine( "TriggerDelayCalculationAsync Stop" );
		//	} );
		//}

		private async void StartLifeBitAsync( )
		{
			try
			{
				if (!IsConnected) throw new Exception( "Modbus is not connected." );
				await Task.Run( ( ) =>
				{
					_lifeBitStatus = true;
					Logger.WriteLine( "LifeBit ON" );
					ushort i = 0;
					while (_connected)
					{
						///
						if (InputCoil[ Lifebit ])
						{
							OutputCoil[ Lifebit ] = false;

							Application.Current?.Dispatcher?.BeginInvoke( DispatcherPriority.Background, new Action( ( ) =>
							{
								if (Application.Current.MainWindow is MainWindow mainWindow)
								{
									mainWindow.pcLifeBit.Fill = System.Windows.Media.Brushes.White;
									mainWindow.plcLifeBit.Fill = System.Windows.Media.Brushes.Green;
								}
							} ) );
						}
						else
						{
							OutputCoil[ Lifebit ] = true;
							Application.Current?.Dispatcher?.BeginInvoke( DispatcherPriority.Background, new Action( ( ) =>
							{
								if (Application.Current.MainWindow is MainWindow mainWindow)
								{
									mainWindow.pcLifeBit.Fill = System.Windows.Media.Brushes.Green;
									mainWindow.plcLifeBit.Fill = System.Windows.Media.Brushes.White;
								}
							} ) );
						}


						System.Threading.Thread.Sleep( 1000 );
					}
					_lifeBitStatus = false;
					Logger.WriteLine( "LifeBit OFF" );
				} );
			}
			catch (Exception ex)
			{
				Logger.WriteLine( ex.Message );
			}
		}

		//private async void StartLifeRegAsync( )
		//{
		//	try
		//	{
		//		if (!IsConnected) throw new Exception( "Modbus is not connected." );
		//		await Task.Run( ( ) =>
		//		{
		//			_lifeBitStatus = true;
		//			Logger.WriteLine( "LifeBit ON" );
		//			ushort i = 0;
		//			while (_connected)
		//			{
		//				///
		//				if (HoldingRegister32[0x0f]==1)
		//				{
		//					HoldingRegister32[ 0x0f ] = 0;

		//					Application.Current?.Dispatcher?.BeginInvoke( DispatcherPriority.Background, new Action( ( ) =>
		//					{
		//						if (Application.Current.MainWindow is MainWindow mainWindow)
		//						{
		//							mainWindow.pcLifeBit.Fill = System.Windows.Media.Brushes.White;
		//							mainWindow.plcLifeBit.Fill = System.Windows.Media.Brushes.Green;
		//						}
		//					} ) );
		//				}
		//				else
		//				{
		//					HoldingRegister32[ 0x0f ] = 1;
		//					Application.Current?.Dispatcher?.BeginInvoke( DispatcherPriority.Background, new Action( ( ) =>
		//					{
		//						if (Application.Current.MainWindow is MainWindow mainWindow)
		//						{
		//							mainWindow.pcLifeBit.Fill = System.Windows.Media.Brushes.Green;
		//							mainWindow.plcLifeBit.Fill = System.Windows.Media.Brushes.White;
		//						}
		//					} ) );
		//				}


		//				System.Threading.Thread.Sleep( 1000 );
		//			}
		//			_lifeBitStatus = false;
		//			Logger.WriteLine( "LifeBit OFF" );
		//		} );
		//	}
		//	catch (Exception ex)
		//	{
		//		Logger.WriteLine( ex.Message );
		//	}
		//}

		//private async void ServoCurrentPositionAsync( )
		//{
		//	try
		//	{
		//		if (!IsConnected) throw new Exception( "Modbus is not connected." );
		//		await Task.Run( ( ) =>
		//		{
		//			Logger.WriteLine( "ServoCurrentPosition Start" );
		//			while (_connected)
		//			{
		//				string CurrentPosition = InputRegister32[0].ToString();
		//				Application.Current?.Dispatcher?.BeginInvoke( DispatcherPriority.Background, new Action( ( ) =>
		//				{
		//					if (Application.Current.MainWindow is MainWindow mainWindow)
		//					{
		//						mainWindow.ServoPosition.Content = CurrentPosition;
		//					}
		//				} ) );
		//				Thread.Sleep( 100 );
		//			}
		//			Logger.WriteLine( "ServoCurrentPosition Stop" );
		//		} );
		//	}
		//	catch (Exception ex)
		//	{
		//		Logger.WriteLine( ex.Message );
		//	}

		//}

		//private async void GVAsync( )
		//{
		//	try
		//	{
		//		if (!IsConnected) throw new Exception( "Modbus is not connected." );
		//		await Task.Run( ( ) =>
		//		{
		//			Logger.WriteLine( "ServoCurrentPosition Start" );
		//			while (_connected)
		//			{
		//				string GV0 = InputRegister[0].ToString();
		//				string GV1 = InputRegister[0].ToString();
		//				string GV2 = InputRegister[0].ToString();
		//				Application.Current?.Dispatcher?.BeginInvoke( DispatcherPriority.Background, new Action( ( ) =>
		//				{
		//					if (Application.Current.MainWindow is RobotManualWindow robotManualWindow)
		//					{
		//						robotManualWindow.GV1Label.Content = GV0;
		//						robotManualWindow.GV1Label.Content = GV1;
		//						robotManualWindow.GV1Label.Content = GV2;
		//					}
		//				} ) );
		//				Thread.Sleep( 100 );
		//			}
		//			Logger.WriteLine( "ServoCurrentPosition Stop" );
		//		} );
		//	}
		//	catch (Exception ex)
		//	{
		//		Logger.WriteLine( ex.Message );
		//	}

		//}





	}
}
