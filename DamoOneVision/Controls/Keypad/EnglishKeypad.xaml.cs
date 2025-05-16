using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DamoOneVision.Controls.Keypad;

public partial class EnglishKeypad : UserControl, IKeypad
{
	public EnglishKeypad( ) => InitializeComponent();

	public event EventHandler<string>? KeyPressed;
	public event EventHandler?         Done;

	bool _upper = true;

	private void OnBtn( object sender, RoutedEventArgs e )
	{
		var b = (Button)sender;
		switch (b.Tag as string)
		{
			case "Shift":
				_upper = !_upper;
				UpdateLabels();
				return;

			case "Space": KeyPressed?.Invoke( this, " " ); return;
			case "Back": KeyPressed?.Invoke( this, "Back" ); return;
			case "Done": Done?.Invoke( this, EventArgs.Empty ); return;
		}
		string ch = b.Content!.ToString()!;
		KeyPressed?.Invoke( this, _upper ? ch.ToUpper() : ch.ToLower() );
	}

	void UpdateLabels( )
	{
		foreach (var btn in this.FindVisualChildren<Button>().Where( x => x.Tag == null ))
		{
			string t = btn.Content!.ToString()!;
			btn.Content = _upper ? t.ToUpper() : t.ToLower();
		}
	}
}

/* VisualTree 탐색 헬퍼 */
static class VisualTreeEx
{
	public static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>( this DependencyObject dep ) where T : DependencyObject
	{
		if (dep == null) yield break;
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount( dep ); i++)
		{
			var c = VisualTreeHelper.GetChild(dep, i);
			if (c is T t) yield return t;
			foreach (T sub in c.FindVisualChildren<T>()) yield return sub;
		}
	}
}
