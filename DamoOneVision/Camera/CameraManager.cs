using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		private bool isContinuous = false;

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
			else if (cameraModel == "USB")
			{
				camera = new USBCamera();
			}
			else
			{
				throw new Exception( "지원되지 않는 카메라 모델입니다." );
			}

			if (camera.Connect())
			{
				//cts = new CancellationTokenSource();
				//captureTask = Task.Run( ( ) => CaptureImages( cts.Token ), cts.Token );
				IsConnected = true;
			}
			else
			{
				throw new Exception( "카메라 연결 실패" );
			}
		}

		public async Task DisconnectAsync( )
		{
			try
			{
				StopContinuousCapture();

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
			catch (Exception ex)
			{
				Debug.WriteLine( $"DisconnectAsync에서 예외 발생: {ex.Message}" );
				throw;
			}
		}


		public async Task<byte[ ]> CaptureSingleImageAsync( )
		{
			if (camera == null)
				throw new InvalidOperationException( "카메라가 연결되어 있지 않습니다." );

			return await Task.Run( ( ) =>
			{
				return camera.CaptureImage();
			} );
		}

		public void StartContinuousCapture( )
		{
			if (camera == null)
				throw new InvalidOperationException( "카메라가 연결되어 있지 않습니다." );

			if (captureTask == null || captureTask.IsCompleted)
			{
				isContinuous = true;
				cts = new CancellationTokenSource();
				captureTask = Task.Run( ( ) => CaptureImages( cts.Token ), cts.Token );
			}
		}

		public void StopContinuousCapture( )
		{
			if (cts != null)
			{
				isContinuous = false;
				cts.Cancel();
				cts = null;
			}
		}

		private async Task CaptureImages( CancellationToken token )
		{
			try
			{
				while (!token.IsCancellationRequested && isContinuous)
				{
					try
					{
						byte[] pixelData = camera.CaptureImage();

						if (pixelData != null)
						{
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
						Debug.WriteLine( $"이미지 캡처 중 예외 발생: {ex.Message}" );
					}

					// 필요에 따라 지연 시간 추가
					await Task.Delay( 1 );
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine( $"CaptureImages에서 예외 발생: {ex.Message}" );
			}
		}
	}
}
