using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DamoOneVision.Data
{
	internal static class Logger
	{
		/// <summary>
		/// 싱글톤 Logger
		/// </summary>


		// 로그 쓰기용 백그라운드 태스크를 중단시키기 위한 토큰
		private static readonly CancellationTokenSource _cts = new CancellationTokenSource();
		// 로그 메시지를 담아둘 BlockingCollection(큐 역할)
		private static readonly BlockingCollection<string> _logQueue = new BlockingCollection<string>(boundedCapacity: 1000);

		// 로거 초기화: 정적 생성자에서 백그라운드 작업 시작
		static Logger( )
		{
			Task.Factory.StartNew( ProcessLogQueue, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default );
		}

		/// <summary>
		/// 외부에서 호출하는 실제 "로그 남기기" 메서드
		/// </summary>
		/// <param name="message"></param>
		public static void WriteLine( string message )
		{
			// 1) 디버그 출력
			Debug.WriteLine( message );

			// 2) Queue에 메시지를 추가 (BlockingCollection이 가득 차면 대기)
			_logQueue.Add( message );

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
					Application.Current?.Dispatcher?.BeginInvoke( DispatcherPriority.Background, new Action( ( ) =>
					{
						if (Application.Current.MainWindow is MainWindow mainWindow)
						{
							mainWindow.LogTextBlock.Text += message + Environment.NewLine;
						}
					} ) );
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
				File.AppendAllText( logFilePath, $"{DateTime.Now} : {message}{Environment.NewLine}" );
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
