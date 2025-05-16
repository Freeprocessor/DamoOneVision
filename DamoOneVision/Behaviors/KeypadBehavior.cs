using DamoOneVision.Controls.Keypad;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DamoOneVision.Behaviors;

public static class KeypadBehavior
{
	public static readonly DependencyProperty EnableProperty =
		DependencyProperty.RegisterAttached("Enable", typeof(bool),
			typeof(KeypadBehavior), new PropertyMetadata(false, OnChanged));

	public static bool GetEnable( DependencyObject d ) => (bool) d.GetValue( EnableProperty );
	public static void SetEnable( DependencyObject d, bool v ) => d.SetValue( EnableProperty, v );

	static Popup? _popup;
	static UnifiedKeypad? _pad;

	static void OnChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
	{
		if (d is not TextBox tb) return;

		if ((bool) e.NewValue)
		{
			tb.GotKeyboardFocus += OpenPopup;
			tb.PreviewMouseDown += OpenPopup;      // 재클릭 감지
			tb.LostKeyboardFocus += ClosePopup;     // 밖을 눌렀을 때 닫기
		}
		else
		{
			tb.GotKeyboardFocus -= OpenPopup;
			tb.PreviewMouseDown -= OpenPopup;
			tb.LostKeyboardFocus -= ClosePopup;
		}
	}

	private static void ClosePopup( object? _, KeyboardFocusChangedEventArgs __ )
	{
		if (_popup != null) _popup.IsOpen = false;   // null 체크 후 닫기
	}

	/* ---------- 팝업 띄우기 ---------- */
	static void OpenPopup( object sender, RoutedEventArgs e )
	{
		var tb = (TextBox)sender;

		/* Popup + Keypad 싱글턴 초기화 */
		if (_popup == null)
		{
			_popup = new Popup
			{
				AllowsTransparency = true,
				StaysOpen = false,
				Placement = PlacementMode.Bottom
			};
			_pad = new UnifiedKeypad();  // 한글 오토마타 DI 필요하면 생성자변경
			_popup.Child = _pad;
		}

		/* 목표 TextBox, 모드 갱신 */
		_pad!.Target = tb;
		_pad.SetMode( tb.Tag?.ToString() == "Numeric" ? PadMode.Numeric : PadMode.English );

		/* 위치 지정 & 오픈 */
		_popup.PlacementTarget = tb;
		_popup.IsOpen = true;
	}
}
