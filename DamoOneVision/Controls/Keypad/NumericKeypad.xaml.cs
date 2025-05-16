using System.Windows;
using System.Windows.Controls;

namespace DamoOneVision.Controls.Keypad;

/// <summary>숫자 전용 키패드</summary>
public partial class NumericKeypad : UserControl, IKeypad
{
	public NumericKeypad( ) => InitializeComponent();

	public event EventHandler<string>? KeyPressed;
	public event EventHandler?         Done;

	private void OnBtn( object sender, RoutedEventArgs e )
	{
		var b = (Button)sender;
		if ((b.Tag as string) == "Done") { Done?.Invoke( this, EventArgs.Empty ); return; }
		KeyPressed?.Invoke( this, (b.Tag as string) == "Back" ? "Back" : b.Content!.ToString()! );
	}
}
