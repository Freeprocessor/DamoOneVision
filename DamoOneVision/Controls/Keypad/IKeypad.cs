namespace DamoOneVision.Controls.Keypad;

public interface IKeypad
{
	event EventHandler<string> KeyPressed; // 입력(또는 Back) 시 호출
	event EventHandler         Done;       // “OK” 버튼 누름
}
