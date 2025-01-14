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

using DamoOneVision.Data;
using static OpenCvSharp.FileStorage;

namespace DamoOneVision
{
    /// <summary>
    /// ManualWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualWindow : Window
    {
		private Modbus modbus;

		public ManualWindow( Modbus modbus )
        {
			this.modbus = modbus;
			InitializeComponent();

		}




		private async void TowerLampREDONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 0, 0 );
			TowerLampREDONButton.Background = Brushes.Green;
			TowerLampREDOFFButton.Background = Brushes.LightGray;
		}

		private async void TowerLampREDOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 1, 1 );
			TowerLampREDONButton.Background = Brushes.LightGray;
			TowerLampREDOFFButton.Background = Brushes.MediumVioletRed;
		}

		private async void TowerLampYELONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 2, 2 );
		}

		private async void TowerLampYELOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 3, 3 );
		}

		private async void TowerLampGRNONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 4, 4 );
		}

		private async void TowerLampGRNOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 5, 5 );
		}

		private async void MainCVONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 0x10, 0x10 );
		}

		private async void MainCVOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 0x11, 0x11 );
		}

		private async void Vision1LampONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 0x20, 0x20 );
		}

		private async void Vision1LampOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 0x21, 0x21 );
		}

		private async void Vision2LampONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 0x22, 0x22 );
		}

		private async void Vision2LampOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 0x23, 0x23 );
		}

		private async void Vision3LampONButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 0x24, 0x24 );
		}

		private async void Vision3LampOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 0x25, 0x25 );
		}

		private async void ReadStatus( ushort StartAddress )
		{
			await Task.Run( ( ) =>
			{
				bool[] result = modbus.ReadInputs( 0, StartAddress, 1 );
				if (result[ 0 ])
				{
					Data.Log.WriteLine( "ON" );
				}
				else
				{
					Data.Log.WriteLine( "OFF" );
				}
			} );

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
			await modbus.SelfHolding( 0x1a, 0x1a );
		}
		private async void EjectManualOFFButton_MouseClick( object sender, RoutedEventArgs e )
		{
			await modbus.SelfHolding( 0x1b, 0x1b );
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
						Data.Log.WriteLine( "SelfHolding operation timed out." );
						throw new TimeoutException( "SelfHolding operation timed out." );
					}
					//Thread.Sleep( 10 );
				}
			} );
		}






	}



}
