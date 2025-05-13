using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DamoOneVision.Services;

namespace DamoOneVision.Camera
{
	public class CameraManager : IDisposable
	{
		private ICamera camera;
		//private CancellationTokenSource cts;
		//private Task captureTask;

		// 이벤트를 통해 이미지를 UI로 전달
		public event Action<byte[]> ImageCaptured;

		public bool IsConnected { get; private set; } = false;

		private bool isContinuous = false;

		public string _cameraName = "";
		public string _library = "";

		public CameraManager( string Library, string CameraName )
		{
			this._cameraName = CameraName;
			this._library = Library;
			CameraInit();


		}

		private void CameraInit( )
		{
			if (_library == "Matrox")
			{
				camera = new MatroxCamera( _cameraName );
			}
			else if (_library == "Spinnaker")
			{
				camera = new SpinnakerCamera( _cameraName );
			}
			else if (_library == "USB")
			{
				//camera = new USBCamera();
			}
			else
			{
				Logger.WriteLine( $"{_cameraName}은/는 지원되지 않는 카메라 모델입니다." );
				throw new Exception( $"{_cameraName}은/는 지원되지 않는 카메라 모델입니다." );
			}
		}

		public async Task ConnectAsync( )
		{

			await Task.Run( async ( ) =>
			{


				if (camera.Connect())
				{
					//cts = new CancellationTokenSource();
					//captureTask = Task.Run( ( ) => CaptureImages( cts.Token ), cts.Token );
					await Task.Delay( 1000 );

					///Camera Service로 이동
					//if(_cameraName == "InfraredCamera")
					//{
					//	camera.ManualFocus( 0.21140 );
					//}

					IsConnected = true;

				}
				else
				{
					Logger.WriteLine( "카메라 연결 실패" );
					throw new Exception( "카메라 연결 실패" );
				}
			} );
		}

		public async Task DisconnectAsync( )
		{
			Logger.WriteLine( $"{_cameraName} 연결 해제" );
			await Task.Run( ( ) =>
			{
				try
				{

					if (camera != null)
					{
						camera.Disconnect();
						camera = null;
					}

					IsConnected = false;
				}
				catch (Exception ex)
				{
					Logger.WriteLine( $"DisconnectAsync에서 예외 발생: {ex.Message}" );

					throw;
				}
			} );
		}


		public async Task<MIL_ID> CaptureSingleImageAsync( )
		{
			MIL_ID MilImage = MIL.M_NULL;
			await Task.Run( ( ) =>
			{
				try
				{
					if (IsConnected)
					{
						MilImage = camera.CaptureImage();
						Logger.WriteLine( $"{_cameraName} : 카메라 이미지 캡처 완료" );
					}
					else if (camera.ReciveImage() != MIL.M_NULL)
					{
						Logger.WriteLine( $"{_cameraName} : 로드된 이미지를 사용합니다." );
						MilImage = camera.ReciveImage();
					}
					else
					{
						Logger.WriteLine( $"{_cameraName} : 카메라가 연결되어 있지 않고, 로드된 이미지도 없습니다." );
						//MessageBox.Show( "카메라가 연결되어 있지 않고, 로드된 이미지도 없습니다." );
					}
				}
				catch (Exception ex)
				{
					Logger.WriteLine( $"CaptureSingleImageAsync에서 예외 발생: {ex.Message}" );
					MilImage = MIL.M_NULL;
				}
			} );

			return MilImage;
		}

		public MIL_ID ReciveImage( )
		{
			if (camera != null)
			{
				return camera.ReciveImage(); ;
			}
			return MIL.M_NULL;

		}

		public MIL_ID ReciveScaleImage( )
		{
			if (camera != null)
			{
				return camera.ReciveScaleImage(); ;
			}
			return MIL.M_NULL;

		}

		public MIL_ID ReciveLoadImage( )
		{
			if (camera != null)
			{
				return camera.ReciveLoadImage(); ;
			}
			return MIL.M_NULL;

		}

		public MIL_ID ReciveLoadScaleImage( )
		{
			if (camera != null)
			{
				return camera.ReciveLoadScaleImage(); ;
			}
			return MIL.M_NULL;

		}

		public MIL_ID ReciveBinarizedImage( )
		{
			if (camera != null)
			{
				return camera.ReciveBinarizedImage(); ;
			}
			return MIL.M_NULL;
		}

		public MIL_ID ReciveLoadBinarizedImage( )
		{
			if (camera != null)
			{
				return camera.ReciveLoadBinarizedImage(); ;
			}
			return MIL.M_NULL;
		}

		public MIL_INT Width( )
		{
			return camera.Width;
		}

		public MIL_INT Height( )
		{
			return camera.Height;
		}
		public MIL_INT NbBands( )
		{
			return camera.NbBands;
		}
		public MIL_INT DataType( )
		{
			return camera.DataType;
		}

		public MIL_ID LoadImage( MIL_ID MilSystem, string filePath )
		{
			return camera.LoadImage( MilSystem, filePath );
		}

		public ushort[ ] LoadImageData( )
		{
			if (camera != null)
			{
				return camera.LoadImageData();
			}
			return null;
		}

		public ushort[ ] CaptureImageData( )
		{
			if (camera != null)
			{
				return camera.CaptureImageData();
			}
			return null;
		}

		public async Task<double> AutoFocus( )
		{
			double focusValue = 0.0;
			if (this.IsConnected == true)
			{
				focusValue = await camera.AutoFocus();
			}
			return focusValue;
		}

		public void ManualFocus( double focusValue )
		{
			if (this.IsConnected == true)
			{
				camera.ManualFocus( focusValue );
			}
		}

		public async Task SaveImage( MIL_ID MilImage, string name )
		{
			camera.SaveImage( MilImage, name );
		}

		public void Dispose( )
		{
			if (camera is IDisposable disposableCamera)
			{
				disposableCamera.Dispose();
			}

			camera = null;

			GC.SuppressFinalize( this );
		}

	}
}