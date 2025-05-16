using DamoOneVision.Services;
using System.Windows;

namespace DamoOneVision.Views
{
	/// <summary>
	/// RobotManualWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class RobotManualWindow : Window
	{
		private readonly ModbusService _modbus;
		public RobotManualWindow( ModbusService modbus )
		{
			this._modbus = modbus;
			InitializeComponent();
		}

		private async void RobotPickUpMoveButton_Click( object sender, RoutedEventArgs e )
		{
			_modbus.HoldingRegister32[ 0x05 ] = 1;
			await Task.Delay( 1000 );
			_modbus.HoldingRegister32[ 0x05 ] = 0;
		}

		private async void RobotPlaceMoveButton_Click( object sender, RoutedEventArgs e )
		{

			_modbus.HoldingRegister32[ 0x05 ] = 2;
			await Task.Delay( 1000 );
			_modbus.HoldingRegister32[ 0x05 ] = 0;
		}

		private void RobotXAxisPositionTextBox_LostFocus( object sender, RoutedEventArgs e )
		{
			if (int.TryParse( RobotXAxisPositionTextBox.Text, out int value ))
			{
				if (value < 0 || value > 120000)
				{
					MessageBox.Show( "1에서 120000 사이의 숫자만 입력 가능합니다.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Error );
					RobotXAxisPositionTextBox.Clear();
				}
				else
				{
					_modbus.HoldingRegister32[ 0x00 ] = value;
				}
			}
			else
			{
				MessageBox.Show( "유효한 숫자를 입력하세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Error );
				RobotXAxisPositionTextBox.Clear();
			}
		}

		private void RobotYAxisPositionTextBox_LostFocus( object sender, RoutedEventArgs e )
		{
			if (int.TryParse( RobotYAxisPositionTextBox.Text, out int value ))
			{
				if (value < 0 || value > 120000)
				{
					MessageBox.Show( "1에서 120000 사이의 숫자만 입력 가능합니다.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Error );
					RobotYAxisPositionTextBox.Clear();
				}
				else
				{
					_modbus.HoldingRegister32[ 0x01 ] = value;
				}
			}
			else
			{
				MessageBox.Show( "유효한 숫자를 입력하세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Error );
				RobotYAxisPositionTextBox.Clear();
			}
		}

		private void RobotZAxisPositionTextBox_LostFocus( object sender, RoutedEventArgs e )
		{
			if (int.TryParse( RobotZAxisPositionTextBox.Text, out int value ))
			{
				if (value < 0 || value > 120000)
				{
					MessageBox.Show( "1에서 120000 사이의 숫자만 입력 가능합니다.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Error );
					RobotZAxisPositionTextBox.Clear();
				}
				else
				{
					_modbus.HoldingRegister32[ 0x02 ] = value;
				}
			}
			else
			{
				MessageBox.Show( "유효한 숫자를 입력하세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Error );
				RobotZAxisPositionTextBox.Clear();
			}
		}

	}
}
