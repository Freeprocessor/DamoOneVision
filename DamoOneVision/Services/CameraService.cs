using DamoOneVision.Camera;
using DamoOneVision.ViewModels;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DamoOneVision.Services
{
	internal class CameraService
	{
		/// <summary>
		/// 열화상 Camera
		/// </summary>
		private readonly CameraManager _infraredCamera;
		/// <summary>
		/// 측면 1 Camera
		/// </summary>
		private readonly CameraManager _sideCamera1;
		/// <summary>
		/// 측면 2 Camera
		/// </summary>
		private readonly CameraManager _sideCamera2;
		/// <summary>
		/// 측면	3 Camera	
		/// </summary>
		private readonly CameraManager _sideCamera3;

		private MIL_ID MilSystem = MIL.M_NULL;

		private MIL_ID _infraredCameraDisplay;
		private MIL_ID _sideCamera1Display;
		private MIL_ID _sideCamera2Display;
		private MIL_ID _sideCamera3Display;

		/// <summary>
		/// 카메라 연속 촬영 모드
		/// </summary>
		private bool _isCameraContinuous = false; // Continuous 모드 상태


		/// <summary>
		/// 이미지 캡처 중인지 여부
		/// </summary>
		private bool _isCameraCapturing = false;

		/// <summary>
		/// 카메라 연결 상태
		/// </summary>
		private bool _isCameraConnected = false;
		public bool IsVisionConnected => _isCameraConnected;

		/// <summary>
		/// Camera 연결 중인지 여부
		/// </summary>
		private bool _isBusy;
		public bool IsBusy => _isBusy;

		// 상태 변경을 알리기 위한 이벤트들
		public event Action<bool> CameraConnectedChanged;
		public event Action<bool> BusyStateChanged;


		public CameraService(CameraManager infraredCamera, CameraManager sideCamera1, CameraManager sideCamera2, CameraManager sideCamera3,
			MIL_ID infraredCameraDisplay, MIL_ID sideCamera1Display, MIL_ID sideCamera2Display, MIL_ID sideCamera3Display )
		{
			MilSystem = MILContext.Instance.MilSystem;



			_infraredCameraDisplay = infraredCameraDisplay;
			_sideCamera1Display = sideCamera1Display;
			_sideCamera2Display = sideCamera2Display;
			_sideCamera3Display = sideCamera3Display;

			_infraredCamera = infraredCamera;
			_sideCamera1 = sideCamera1;
			_sideCamera2 = sideCamera2;
			_sideCamera3 = sideCamera3;
		}

		public async Task ConnectAction( )
		{
			if (IsVisionConnected)
			{
				Logger.WriteLine( "이미 카메라가 연결되어 있습니다." );
				return;
			}
			SetBusy(true);
			try
			{
				var tasks = new[]
				{
					_infraredCamera.ConnectAsync( ),
					//_sideCamera1.ConnectAsync( ),
					//_sideCamera2.ConnectAsync( ),
					//_sideCamera3.ConnectAsync( )
				};

				await Task.WhenAll( tasks );

				SetVisionConnected(true);

			}
			catch (Exception ex)
			{
				MessageBox.Show( $"카메라 연결 오류\n{ex.Message}" );
				Logger.WriteLine( $"카메라 연결 오류\n{ex.Message}" );
				var tasks = new[]
				{
					_infraredCamera.DisconnectAsync(),
					//_sideCamera1.DisconnectAsync(),
					//_sideCamera2.DisconnectAsync(),
					//_sideCamera3.DisconnectAsync()
				};

				await Task.WhenAll( tasks );

			}
			finally
			{
				SetBusy(false);
			}
		}

		public async Task DisconnectAction( )
		{
			if (!_isCameraConnected)
			{
				Logger.WriteLine( "카메라가 연결되어 있지 않습니다." );
				return;
			}
			SetBusy(true);

			var tasks = new[]
				{
					_infraredCamera.DisconnectAsync(),
					//_sideCamera1.DisconnectAsync(),
					//_sideCamera2.DisconnectAsync(),
					//_sideCamera3.DisconnectAsync()
				};

			await Task.WhenAll( tasks );

			SetBusy(false);
			SetVisionConnected(false);
		}

		private void SetVisionConnected( bool connected )
		{
			if (_isCameraConnected != connected)
			{
				_isCameraConnected = connected;
				CameraConnectedChanged?.Invoke( _isCameraConnected );
			}
		}
		private void SetBusy( bool busy )
		{
			if (_isBusy != busy)
			{
				_isBusy = busy;
				BusyStateChanged?.Invoke( _isBusy );
			}
		}


		public async Task VisionTrigger( )
		{
			Stopwatch TectTime = new Stopwatch();
			TectTime.Start();

			await Task.Run( async ( ) =>
			{
				if (!_isCameraCapturing)
				{
					_isCameraCapturing = true;

					// 카메라에서 이미지 캡처
					try
					{
						var tasks = new []
						{
							CaptureImage(_infraredCamera),
							CaptureImage(_sideCamera1),
							CaptureImage(_sideCamera2),
							CaptureImage(_sideCamera3)
						};
						await Task.WhenAll( tasks );
						Logger.WriteLine( "카메라 이미지 캡처 완료" );
					}
					catch (Exception ex)
					{
						Logger.WriteLine( $"이미지 캡쳐 중 오류 발생: {ex.Message}" );
						MessageBox.Show( $"이미지 캡쳐 중 오류 발생: {ex.Message}" );
					}


					MIL.MdispSelect( _infraredCameraDisplay, _infraredCamera.ReciveImage() );
					MIL.MdispSelect( _sideCamera1Display, _sideCamera1.ReciveImage() );
					MIL.MdispSelect( _sideCamera2Display, _sideCamera2.ReciveImage() );
					MIL.MdispSelect( _sideCamera3Display, _sideCamera3.ReciveImage() );


					//try
					//{
					//	if (_infraredCameraImage != MIL.M_NULL && _sideCamera1Image != MIL.M_NULL && _sideCamera2Image != MIL.M_NULL && _sideCamera3Image != MIL.M_NULL)
					//	{
					//		// 여기서 pixelData에 대한 추가 처리(예: HSLThreshold 등) 호출 가능
					//		// 예: Conversion.RunHSLThreshold(hMin, hMax, sMin, sMax, lMin, lMax, pixelData);
					//		// 처리 후 다시 DisplayImage(pixelData)로 화면에 갱신할 수 있음
					//		bool isGood = true;

					//		if (_infraredCameraConversionImage == MIL.M_NULL) MIL.MbufFree( _infraredCameraConversionImage );
					//		_infraredCameraConversionImage = MIL.M_NULL;
					//		if (_sideCamera1ConversionImage == MIL.M_NULL) MIL.MbufFree( _sideCamera1ConversionImage );
					//		_sideCamera1ConversionImage = MIL.M_NULL;
					//		if (_sideCamera2ConversionImage == MIL.M_NULL) MIL.MbufFree( _sideCamera2ConversionImage );
					//		_sideCamera2ConversionImage = MIL.M_NULL;
					//		if (_sideCamera3ConversionImage == MIL.M_NULL) MIL.MbufFree( _sideCamera3ConversionImage );
					//		_sideCamera3ConversionImage = MIL.M_NULL;



					//		//InfraredCameraConversionImage = Conversion.InfraredCameraModel( InfraredCameraImage, ref isGood, currentInfraredCameraModel );
					//		//await Task.Run( ( ) => Conversion.SideCameraModel( SideCamera1Image, MainSideCamera1Display ) );

					//		//var tasks = new[]
					//		//{
					//		//	///Overlay Display  Image 어떻게 처리할 것인지?
					//		//	///디스플레이 선언과 동시에 Overlay Image의 버퍼를 받아서 저장후에 
					//		//	///필요할때마다 가져다가 쓰는 방식으로 해야할 것 같음
					//		//	///
					//		//	Conversion.SideCameraModel( _sideCamera1Image, _mainSideCamera1Display ),
					//		//	Conversion.SideCameraModel( _sideCamera2Image, _mainSideCamera2Display ),
					//		//	Conversion.SideCameraModel( _sideCamera3Image, _mainSideCamera3Display )
					//		//};
					//		//bool[] result = await Task.WhenAll( tasks );

					//		//isGood = result[ 0 ] && result[ 1 ] && result[ 2 ];
					//		//MIL.MdispSelect( InfraredCameraConversionDisplay, InfraredCameraConversionImage );
					//		Logger.WriteLine( "이미지 처리 완료" );

					//		///GOOD REJECT LAMP 바인딩
					//		//if (!Dispatcher.CheckAccess())
					//		//{
					//		//	// UI 스레드에서 실행되도록 Dispatcher를 사용하여 호출
					//		//	Dispatcher.Invoke( ( ) => GoodLamp( isGood ) );
					//		//}
					//		//if (!isGood) EjectAction();

					//		//DisplayConversionImage( ConversionpixelData );
					//	}
					//}
					//catch (Exception ex)
					//{
					//	Logger.WriteLine( $"이미지 처리 중 오류 발생: {ex.Message}" );
					//	MessageBox.Show( $"이미지 처리 중 오류 발생: {ex.Message}" );

					//}


					_isCameraCapturing = false;

				}

			} );
			TectTime.Stop();
			Logger.WriteLine( $"이미지 처리 시간: {TectTime.ElapsedMilliseconds}ms" );


		}

		private async Task<MIL_ID> CaptureImage( CameraManager camera )
		{
			MIL_ID MilImage = MIL.M_NULL;
			if (camera.IsConnected)
			{
				MilImage = await _infraredCamera.CaptureSingleImageAsync();
				Logger.WriteLine( $"{camera._cameraName} : 카메라 이미지 캡처 완료" );
			}
			else if (camera.ReciveImage() != MIL.M_NULL)
			{
				Logger.WriteLine( $"{camera._cameraName} : 로드된 이미지를 사용합니다." );
				MilImage = camera.ReciveImage();
				
			}
			else
			{
				Logger.WriteLine( $"{camera._cameraName} : 카메라가 연결되어 있지 않고, 로드된 이미지도 없습니다." );
				//MessageBox.Show( "카메라가 연결되어 있지 않고, 로드된 이미지도 없습니다." );
				return MIL.M_NULL;
			}
			return MilImage;


		}
	}
}
