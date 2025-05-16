using System.Windows;
using System.Windows.Controls;

namespace DamoOneVision.Controls.Keypad;

public partial class KoreanKeypad : UserControl, IKeypad
{
	public KoreanKeypad( ) => InitializeComponent();

	public event EventHandler<string>? KeyPressed;
	public event EventHandler?         Done;

	private void OnBtn( object sender, RoutedEventArgs e )
	{
		var b = (Button)sender;
		switch (b.Tag as string)
		{
			case "Space": KeyPressed?.Invoke( this, " " ); return;
			case "Done": Done?.Invoke( this, EventArgs.Empty ); return;
		}
		KeyPressed?.Invoke( this, b.Content!.ToString()! ); // 추후 오토마타와 결합
	}
}
