using System;
using System.Windows;
using System.Windows.Controls;

namespace DamoOneVision.Controls.Keypad
{
	/// <summary>숫자 전용 키패드</summary>
	public partial class NumericKeypad : UserControl, IKeypad
	{
		public NumericKeypad( ) => InitializeComponent();

		/* ===== IKeypad ===== */
		public event EventHandler<string>? KeyPressed;
		public event EventHandler?         Done;

		/* ===== 내부 이벤트 ===== */
		private void OnButton( object sender, RoutedEventArgs e )
		{
			var btn = (Button)sender;
			string? tag = btn.Tag as string;

			if (tag == "Done")
			{
				Done?.Invoke( this, EventArgs.Empty );
				return;
			}

			KeyPressed?.Invoke( this, tag == "Back" ? "Back" : btn.Content!.ToString()! );
		}
	}
}
