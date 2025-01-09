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
		}

		public void DisconnectButton_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				Modbus.master.Dispose();
				Debug.WriteLine( "Disconnect Success\r\n\r\n" );
			}
			catch (Exception ex)
			{
				MessageBox.Show( $"Modbus 연결 해제 중 오류 발생: {ex.Message}" );
			}
		}



		private void TowerLampREDONButton_MouseDown( object sender, RoutedEventArgs e )
		{
			bool[] towerlamp;
			modbus.WriteSingleCoil( 0, 0, true );
			while( true ) 
			{
				towerlamp = modbus.ReadInputs( 0, 0, 1 );
				if (towerlamp[0] == true )
				{
					modbus.WriteSingleCoil( 0, 0, false );
					break;
				}
				Thread.Sleep( 100 );
			}
			Thread.Sleep( 100 );
			//modbus.
		}

		private async void TowerLampREDOFFButton_MouseDown( object sender, RoutedEventArgs e )
		{
			modbus.WriteSingleCoil( 0, 1, false );
		}

		public async void SelfHoldingHandShacking( int num )
		{

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
