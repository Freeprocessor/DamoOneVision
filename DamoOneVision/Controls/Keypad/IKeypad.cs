using System;

namespace DamoOneVision.Controls.Keypad
{
	/// <summary>통합 키패드용 공통 이벤트 인터페이스</summary>
	public interface IKeypad
	{
		/// <summary>버튼을 눌렀을 때(Back 포함) 현재 텍스트를 알림</summary>
		event EventHandler<string> KeyPressed;

		/// <summary>“OK / Done” 누른 경우 알림</summary>
		event EventHandler         Done;
	}
}
