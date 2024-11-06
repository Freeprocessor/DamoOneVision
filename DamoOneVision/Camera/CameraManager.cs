using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DamoOneVision.Camera
{
	internal class CameraManager
	{
		private ICamera camera;
		private CancellationTokenSource cts;
		private Task captureTask;

		// 이벤트를 통해 이미지를 UI로 전달
		public event Action<byte[]> ImageCaptured;

		public bool IsConnected { get; private set; } = false;

		public void Connect( string cameraModel )
		{
			if (cameraModel == "Matrox")
			{
				camera = new MatroxCamera();
			}
			else if (cameraModel == "Spinnaker")
			{
				camera = new SpinnakerCamera();
			}
			else
			{
				throw new Exception( "지원되지 않는 카메라 모델입니다." );
			}

			if (camera.Connect())
			{
				cts = new CancellationTokenSource();
				captureTask = Task.Run( ( ) => CaptureImages( cts.Token ), cts.Token );
				IsConnected = true;
			}
			else
			{
				throw new Exception( "카메라 연결 실패" );
			}
		}

		private async Task CaptureImages( CancellationToken token )
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					byte[] pixelData = camera.CaptureImage();

					if (pixelData != null)
					{
						// 이벤트를 통해 이미지 전달
						ImageCaptured?.Invoke( pixelData );
					}

					token.ThrowIfCancellationRequested();
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (Exception ex)
				{
					// 예외 처리
					Console.WriteLine( $"이미지 획득 오류: {ex.Message}" );
				}

				// 적절한 프레임 속도를 위해 지연 시간 추가 (필요한 경우)
				await Task.Delay( 1 );
			}
		}

		public async Task DisconnectAsync( )
		{
			if (cts != null)
			{
				cts.Cancel();
				cts = null;
			}

			if (captureTask != null)
			{
				try
				{
					await captureTask;
				}
				catch (OperationCanceledException)
				{
					// 작업이 취소되었음을 무시
				}
				captureTask = null;
			}

			if (camera != null)
			{
				camera.Disconnect();
				camera = null;
			}

			IsConnected = false;
		}

		public int GetWidth( )
		{
			return camera?.GetWidth() ?? 0;
		}

		public int GetHeight( )
		{
			return camera?.GetHeight() ?? 0;
		}

	}
}
