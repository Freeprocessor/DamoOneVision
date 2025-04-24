// Services/Input/IHangulAutomata.cs
namespace DamoOneVision.Services.Input;

public interface IHangulAutomata
{
	string ProcessKey( string key, string current );
	string Backspace( string current );
	string Commit( string current );
}
