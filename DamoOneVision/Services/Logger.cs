using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace DamoOneVision.Services
{
	internal static class Logger
	{
		/// <summary>
		/// 싱글톤 Logger
		/// </summary>

		public static event EventHandler<string> OnLogReceived;
		// 로그 쓰기용 백그라운드 태스크를 중단시키기 위한 토큰
		private static readonly CancellationTokenSource _cts = new CancellationTokenSource();
		// 로그 메시지를 담아둘 BlockingCollection(큐 역할)
		private static readonly BlockingCollection<string> _logQueue = new BlockingCollection<string>(boundedCapacity: 1000);

		// 로거 초기화: 정적 생성자에서 백그라운드 작업 시작
		static Logger( )
		{
			Task.Factory.StartNew( ProcessLogQueue, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default );
		}
		private static int _classWidth = 0;   // 가장 긴 classification 길이
		private static int _funcWidth  = 0;   // 가장 긴 function 길이
		private static readonly object _lock = new();

		public static void WriteLine( string classification,
									 string function,
									 string message,
									 int type = 1 )
		{
			string functiontap = "";
			if (function.Length <= 9)
			{
				functiontap = "\t";
			}
			string logMessage =
		$"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\t" +   // 날짜 뒤 탭
        $"[{classification}]\t" +                  // 토큰 사이 탭
		$"[{function}]\t" +
		functiontap+
		message;

			Debug.WriteLine( logMessage );
			if (type != 0) _logQueue.Add( logMessage );
		}

		/// <summary>
		/// 로그 큐를 소비(Consume)하여 파일에 실제로 쓰는 함수
		/// </summary>
		private static void ProcessLogQueue( )
		{
			try
			{
				while (true)
				{
					// 3) 큐에서 메시지를 꺼냄(없으면 대기)
					string message = _logQueue.Take(_cts.Token);

					// 4) 파일에 쓰기
					WriteLogToFile( message );

					// 5) (옵션) WPF UI에 로그를 표시해야 하면 Dispatcher를 통해 실행
					//    필요 없다면 이 부분은 제거하세요.
					OnLogReceived?.Invoke( null, message );
				}
			}
			catch (OperationCanceledException)
			{
				// _cts.Cancel() 시 예외 발생 -> while문 탈출
			}
			catch (Exception ex)
			{
				// 로거 내부 오류 발생 시 처리
				Debug.WriteLine( "[Logger] Background thread exception: " + ex.Message );
			}
		}

		/// <summary>
		/// 실제 로그 파일에 한 줄 쓰는 로직
		/// </summary>
		/// <param name="message"></param>
		private static void WriteLogToFile( string message )
		{
			try
			{
				string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				string appFolder = Path.Combine(localAppData, "DamoOneVision");
				string logFolder = Path.Combine(appFolder, "Log");

				string logFileName = $"{DateTime.Today:yyyy-MM-dd}.log";
				string logFilePath = Path.Combine(logFolder, logFileName);

				// 폴더/파일 없으면 생성
				if (!Directory.Exists( logFolder ))
				{
					Directory.CreateDirectory( logFolder );
				}
				if (!File.Exists( logFilePath ))
				{
					File.Create( logFilePath ).Close();
				}

				// 파일에 실제 쓰기 (AppendAllTextAsync -> Task.Wait 또는 await 가능)
				// 여기서는 동기식 예시(File.AppendAllText)로도 충분.
				File.AppendAllText( logFilePath, $"{message}{Environment.NewLine}" );
			}
			catch (Exception ex)
			{
				// 파일 쓰기 중 오류 발생 -> Debug에 기록
				Debug.WriteLine( "[Logger] WriteLogToFile Exception: " + ex.Message );
			}
		}

		/// <summary>
		/// 프로그램 종료 시나 적절한 타이밍에 Logger 정리
		/// </summary>
		public static void Shutdown( )
		{
			// BlockingCollection에 더 이상 Add 불가
			_logQueue.CompleteAdding();

			// 백그라운드 작업 취소
			_cts.Cancel();

			// 큐에 남은 메시지들은 Take 시 OperationCanceledException 발생
			// Thread가 종료되길 기다릴 수도 있음
			// (대기하려면 Task.Wait(...) 사용)
		}

	}
}
