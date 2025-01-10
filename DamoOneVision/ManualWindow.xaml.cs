using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using NModbus;
using NModbus.Device;
using static OpenCvSharp.FileStorage;

namespace DamoOneVision
{
    /// <summary>
    /// ManualWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualWindow : Window
    {
		Modbus modbus = new Modbus();

		bool lifeBitOFFRequire = false;
		bool lifeBitOFF = false;
		bool PCLifeBit = false;
		bool PLCLifeBit = false;

		public ManualWindow()
        {
            InitializeComponent();

			modbus.Ip = "192.168.2.11";
			modbus.Port = 502;

			ModbusIPTextBox.Text = modbus.Ip;
			ModbusPortTextBox.Text = modbus.Port.ToString();

		}
		public void ConnectButton_Click( object sender, RoutedEventArgs e )
		{
			IPAddress sip;
			int port;
			var ipaddress = ModbusIPTextBox.Text;
			bool ValidIp = IPAddress.TryParse( ipaddress, out sip );

			modbus.Ip = sip.ToString();
			int.TryParse( ModbusPortTextBox.Text, out port );
			modbus.Port = port;
			try
			{
				modbus.Connect();
				Debug.WriteLine( "Connect Success\r\n\r\n" );
				MessageBox.Show( $"Modbus 연결 성공" );
			}
			catch (Exception ex)
			{
				MessageBox.Show( $"Modbus 연결 중 오류 발생: {ex.Message}" );
			}
			lifeBitOFFRequire = true;
			StartLifeBitAsync();
		}

		public void DisconnectButton_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				Modbus.master.Dispose();
				lifeBitOFFRequire = true;

				Debug.WriteLine( "Disconnect Success\r\n\r\n" );
				
			}
			catch (Exception ex)
			{
				MessageBox.Show( $"Modbus 연결 해제 중 오류 발생: {ex.Message}" );
			}

			lifeBitOFFRequire = false;
			while (!lifeBitOFF)
			{
				System.Threading.Thread.Sleep( 1000 );
			}
			lifeBitOFF = false;

		}



		private async void TowerLampREDONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 0, 0 );
		}

		private async void TowerLampREDOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 1, 1 );		
		}

		private async void TowerLampYELONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 2, 2 );
		}

		private async void TowerLampYELOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 3, 3 );
		}

		private async void TowerLampGRNONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 4, 4 );
		}

		private async void TowerLampGRNOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 5, 5 );
		}

		private async void MainCVONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 0x10, 0x10 );
		}

		private async void MainCVOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 0x11, 0x11 );
		}

		private async void Vision1LampONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 0x20, 0x20 );
		}

		private async void Vision1LampOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 0x21, 0x21 );
		}

		private async void Vision2LampONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 0x22, 0x22 );
		}

		private async void Vision2LampOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 0x23, 0x23 );
		}

		private async void Vision3LampONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 0x24, 0x24 );
		}

		private async void Vision3LampOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 0x25, 0x25 );
		}





		//private async void EjectONButton_MouseClick( object sender, RoutedEventArgs e )
		//{
		//	await SelfHolding( 0x18, 0x18 );
		//}
		//private async void EjectOFFButton_MouseClick( object sender, RoutedEventArgs e )
		//{
		//	await SelfHolding( 0x19, 0x19 );
		//}

		private async void EjectManualONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 0x1a, 0x1a );
		}
		private async void EjectManualOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await SelfHolding( 0x1b, 0x1b );
		}
		private async void EjectRoutineButton_MouseClick( object sender, RoutedEventArgs e )
		{
			bool[] coil;

			await Task.Run( ( ) =>
			{
				modbus.WriteSingleCoil( 0, 0x18, true );
				var startTime = DateTime.Now;
				while (true)
				{
					coil = modbus.ReadInputs( 0, 0x18, 1 );
					if (coil[ 0 ] == true)
					{
						modbus.WriteSingleCoil( 0, 0x19, true );
						while (true)
						{
							coil = modbus.ReadInputs( 0, 0x19, 1 );
							if (coil[ 0 ] == false)
							{
								break;
							}
						}
						break;
					}
					if ((DateTime.Now - startTime).TotalMilliseconds > 10000) // 10초 타임아웃
					{
						throw new TimeoutException( "SelfHolding operation timed out." );
					}
					//Thread.Sleep( 10 );
				}
			} );
		}


		private async Task SelfHolding(ushort input, ushort output )
		{
			bool[] coil;
			
			await Task.Run( ( ) =>
			{
				modbus.WriteSingleCoil( 0, output, true );
				var startTime = DateTime.Now;
				while (true)
				{
					coil = modbus.ReadInputs( 0, input, 1 );
					if (coil[ 0 ] == true)
					{
						modbus.WriteSingleCoil( 0, output, false );
						break;
					}
					if ((DateTime.Now - startTime).TotalMilliseconds > 5000) // 5초 타임아웃
					{
						throw new TimeoutException( "SelfHolding operation timed out." );
					}
					//Thread.Sleep( 10 );
				}
			} );
		}

		private async void StartLifeBitAsync( )
		{
			await Task.Run( ( ) =>
			{
				while (lifeBitOFFRequire)
				{
					Dispatcher.Invoke( ( ) =>
					{
						if (PCLifeBit)
						{
							modbus.WriteSingleCoil( 0, 0x0f, false );
							PCLifeBit = false;
							pcLifeBit.Fill = Brushes.Green;
						}
						else
						{
							modbus.WriteSingleCoil( 0, 0x0f, true );
							PCLifeBit = true;
							pcLifeBit.Fill = Brushes.White;
						}

						PLCLifeBit = modbus.ReadInputs( 0, 0x0f, 1 )[ 0 ];

						if ( PLCLifeBit )
						{
							plcLifeBit.Fill = Brushes.Green;
						}
						else
						{
							plcLifeBit.Fill = Brushes.White;
						}

					} );
					System.Threading.Thread.Sleep( 1000 );
				}
				lifeBitOFF = true;
			} );


		}



	}

	class Modbus
	{
		private static string ip="";
		private static int port=0;
		private static TcpClient tcpClient;
		public static IModbusMaster master;

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
		}

		public void ConnectionCheck( )
		{
			if (tcpClient.Connected)
			{

			}
		}

		// Coil Write Multiple
		public int WriteMultipleCoils( byte station, ushort startAddress, bool[ ] data )
		{
			master.WriteMultipleCoils( station, startAddress, data );
			return 0;
		}

		// Coil Write Multiple
		public bool[ ] ReadInputs( byte station, ushort startAddress, ushort num )
		{

			bool[] result = master.ReadInputs( station, startAddress, num );
			return result;
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

	}

}
