using System;
using System.Windows;
using System.Windows.Controls;
using DamoOneVision.Services.Input; // IHangulAutomata 위치

namespace DamoOneVision.Controls.Keypad
{
	/// <summary>두벌식 한글 키패드</summary>
	public partial class KoreanKeypad : UserControl, IKeypad
	{
		private readonly IHangulAutomata _ime;
		private string _buffer = "";

		public KoreanKeypad( IHangulAutomata ime )
		{
			_ime = ime;
			InitializeComponent();
		}

		/* ===== IKeypad ===== */
		public event EventHandler<string>? KeyPressed;
		public event EventHandler?         Done;

		/* ===== 내부 이벤트 ===== */
		private void OnButton( object sender, RoutedEventArgs e )
		{
			var btn = (Button)sender;
			string? tag = btn.Tag as string;

			switch (tag)
			{
				case "Space":
					_buffer = _ime.Commit( _buffer ) + " ";
					KeyPressed?.Invoke( this, _buffer );
					break;

				case "Back":
					_buffer = _ime.Backspace( _buffer );
					KeyPressed?.Invoke( this, _buffer );
					break;

				case "Done":
					_buffer = _ime.Commit( _buffer );
					KeyPressed?.Invoke( this, _buffer );
					Done?.Invoke( this, EventArgs.Empty );
					break;

				case "Shift":
					/* 세벌식/대문자 같은 추가 기능이 있다면 여기서 처리 */
					break;

				default:
					_buffer = _ime.ProcessKey( btn.Content!.ToString()!, _buffer );
					KeyPressed?.Invoke( this, _buffer );
					break;
			}
		}
	}
}
