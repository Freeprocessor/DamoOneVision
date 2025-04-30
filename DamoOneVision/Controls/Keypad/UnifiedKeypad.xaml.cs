using System.Windows.Controls;
using System.Windows;

namespace DamoOneVision.Controls.Keypad;

public enum PadMode { Numeric, English, Korean }

public partial class UnifiedKeypad : UserControl
{
	readonly NumericKeypad _num = new();
	readonly EnglishKeypad _eng = new();
	readonly KoreanKeypad  _kor = new();

	public TextBox? Target { get; set; }

	public UnifiedKeypad( )
	{
		InitializeComponent();
		Hook( _num ); Hook( _eng ); Hook( _kor );
		Host.Content = _eng; // 기본 영문
	}

	void Hook( IKeypad pad )
	{
		pad.KeyPressed += ( s, txt ) =>
		{
			if (Target == null) return;

			if (txt == "Back")                         // ← 추가
			{                                          //    
				if (Target.Text.Length > 0)            // 한 글자 지우기
					Target.Text = Target.Text[ ..^1 ];   //
			}
			else
			{
				Target.Text += txt;                    // 기존 입력
			}
		};

		pad.Done += ( s, _ ) => RaiseEvent( new RoutedEventArgs( DoneEvent ) );
	}


	public void SetMode( PadMode m )
		=> Host.Content = m switch { PadMode.Numeric => _num, PadMode.Korean => _kor, _ => _eng };

	public static readonly RoutedEvent DoneEvent =
		EventManager.RegisterRoutedEvent("Done", RoutingStrategy.Bubble,
										 typeof(RoutedEventHandler), typeof(UnifiedKeypad));
	public event RoutedEventHandler Done
	{ add => AddHandler(DoneEvent, value); remove => RemoveHandler(DoneEvent, value); }
}
