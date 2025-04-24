using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DamoOneVision.Controls.Keypad
{
	/// <summary>영문 QWERTY 키패드</summary>
	public partial class EnglishKeypad : UserControl, IKeypad
	{
		public EnglishKeypad( ) => InitializeComponent();

		/* ===== IKeypad ===== */
		public event EventHandler<string>? KeyPressed;
		public event EventHandler?         Done;

		/* ===== 내부 상태 ===== */
		private bool _upper = true;

		private void OnButton( object sender, RoutedEventArgs e )
		{
			var btn = (Button)sender;
			string? tag = btn.Tag as string;

			switch (tag)
			{
				case "Shift":
					_upper = !_upper;
					UpdateLabels();
					break;

				case "Space":
					KeyPressed?.Invoke( this, " " );
					break;

				case "Back":
					KeyPressed?.Invoke( this, "Back" );
					break;

				case "Done":
					Done?.Invoke( this, EventArgs.Empty );
					break;

				default:        // 알파벳
					string ch = btn.Content!.ToString()!;
					KeyPressed?.Invoke( this, _upper ? ch.ToUpper() : ch.ToLower() );
					break;
			}
		}

		private void UpdateLabels( )
		{
			foreach (var b in this.FindVisualChildren<Button>()
								   .Where( b => b.Tag == null ))
			{
				string txt = b.Content!.ToString()!;
				b.Content = _upper ? txt.ToUpper() : txt.ToLower();
			}
		}
	}

	/* VisualTree 탐색용 확장 메서드 */
	internal static class VisualTreeHelperEx
	{
		public static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>( this DependencyObject dep ) where T : DependencyObject
		{
			if (dep == null) yield break;
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount( dep ); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(dep, i);
				if (child is T t) yield return t;
				foreach (T sub in child.FindVisualChildren<T>())
					yield return sub;
			}
		}
	}
}
