using DamoOneVision.Services.Input;
namespace KeypadDemo.Services;

public class SimpleHangulAutomata : IHangulAutomata
{
	// ---- 유니코드 테이블 ----
	static readonly string[] Cho = { "ㄱ","ㄲ","ㄴ","ㄷ","ㄸ","ㄹ","ㅁ","ㅂ",
									 "ㅃ","ㅅ","ㅆ","ㅇ","ㅈ","ㅉ","ㅊ",
									 "ㅋ","ㅌ","ㅍ","ㅎ" };
	static readonly string[] Jung = { "ㅏ","ㅐ","ㅑ","ㅒ","ㅓ","ㅔ","ㅕ","ㅖ",
									  "ㅗ","ㅘ","ㅙ","ㅚ","ㅛ","ㅜ","ㅝ","ㅞ",
									  "ㅟ","ㅠ","ㅡ","ㅢ","ㅣ" };
	static readonly string[] Jong = { "","ㄱ","ㄲ","ㄳ","ㄴ","ㄵ","ㄶ","ㄷ","ㄹ",
									  "ㄺ","ㄻ","ㄼ","ㄽ","ㄾ","ㄿ","ㅀ","ㅁ",
									  "ㅂ","ㅄ","ㅅ","ㅆ","ㅇ","ㅈ","ㅊ","ㅋ",
									  "ㅌ","ㅍ","ㅎ" };

	string _cho="", _jung="", _jong="";

	public string ProcessKey( string key, string cur )
	{
		if (IsCon( key )) AddConsonant( key );
		else AddVowel( key );
		return ReplaceLast( cur, Compose() );
	}
	public string Backspace( string cur )
	{
		if (_jong != "") _jong = "";
		else if (_jung != "") _jung = "";
		else if (_cho != "") _cho = "";
		else return cur.Length > 0 ? cur[ ..^1 ] : cur;
		return ReplaceLast( cur, Compose() );
	}
	public string Commit( string cur )
	{
		string txt = ReplaceLast(cur, Compose());
		_cho = _jung = _jong = "";
		return txt;
	}

	// ----- 내부 -----
	bool IsCon( string k ) => "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ".Contains( k );
	void AddConsonant( string c )
	{
		if (_cho == "" || (_cho != "" && _jung == "" && _jong == "")) _cho = c;
		else if (_jung != "" && _jong == "") _jong = c;
		else { _cho = c; _jung = _jong = ""; }
	}
	void AddVowel( string v )
	{
		if (_jung == "") _jung = v;
		else if (_jong == "") _jung = v;     // 복모음 생략
		else { _cho = _jong; _jung = v; _jong = ""; }
	}
	string Compose( )
	{
		if (_cho == "" && _jung == "") return "";
		int cho=Array.IndexOf(Cho,_cho);
		int jung=Array.IndexOf(Jung,_jung);
		int jong=Array.IndexOf(Jong,_jong);
		int code=0xAC00+(cho*21+jung)*28+jong;
		return char.ConvertFromUtf32( code );
	}
	static string ReplaceLast( string cur, string syl )
	{
		// 마지막 “조합 중 글자”를 떼고 새로 붙인다
		if (cur.Length > 0 && IsHangulSyllable( cur[ ^1 ] )) cur = cur[ ..^1 ];
		return cur + syl;
	}
	static bool IsHangulSyllable( char c ) => c >= 0xAC00 && c <= 0xD7A3;
}
