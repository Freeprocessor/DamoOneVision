using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DamoOneVision.Data
{
	internal static class Log
	{

		public static async void WriteLine( string message )
		{

			Debug.WriteLine( message );
			await Task.Run( ( ) =>
			{
				string localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
				string appFolder = System.IO.Path.Combine( localAppData, "DamoOneVision" );
				string logFolder = System.IO.Path.Combine( appFolder, "Log" );

				string logFileName = $"{DateTime.Today.Year}-{DateTime.Today.Month}-{DateTime.Today.Day}.log";
				string logFile = System.IO.Path.Combine( logFolder, logFileName );

				Application.Current.Dispatcher.Invoke( DispatcherPriority.Normal, new Action( ( ) =>
				{
					MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

					mainWindow.LogTextBlock.Text += message + Environment.NewLine;
				} ) );

				
				// 로그 파일에 메시지를 기록
				// 로그 파일 경로
				// 로그 파일이 존재하지 않으면 생성
				if (!System.IO.Directory.Exists( logFolder ))
				{
					System.IO.Directory.CreateDirectory( logFolder );
				}
				if (!System.IO.File.Exists( logFile ))
				{
					System.IO.File.Create( logFile ).Close();
				}
				// 로그 파일에 메시지를 추가
				System.IO.File.AppendAllTextAsync( logFile, $"{DateTime.Now} : {message}\n" );
			} );

		}
	}
}
