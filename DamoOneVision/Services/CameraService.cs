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

		private MIL_ID _infraredCameraImage;
		private MIL_ID _infraredCameraConversionImage;

		private MIL_ID _sideCamera1Image;
		private MIL_ID _sideCamera1ConversionImage;

		private MIL_ID _sideCamera2Image;
		private MIL_ID _sideCamera2ConversionImage;

		private MIL_ID _sideCamera3Image;
		private MIL_ID _sideCamera3ConversionImage;


		private MIL_ID _mainInfraredCameraDisplay;
		private MIL_ID _mainSideCamera1Display;
		private MIL_ID _mainSideCamera2Display;
		private MIL_ID _mainSideCamera3Display;

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
			MIL_ID infraredCameraImage, MIL_ID sideCamera1Image, MIL_ID sideCamera2Image, MIL_ID sideCamera3Image, 
			MIL_ID infraredCameraConversionImage, MIL_ID sideCamera1ConversionImage, MIL_ID sideCamera2ConversionImage, MIL_ID sideCamera3ConversionImage,
			MIL_ID mainInfraredCameraDisplay, MIL_ID mainSideCamera1Display, MIL_ID mainSideCamera2Display, MIL_ID mainSideCamera3Display )
		{
			MilSystem = MILContext.Instance.MilSystem;

			_infraredCameraImage = infraredCameraImage;
			_sideCamera1Image = sideCamera1Image;
			_sideCamera2Image = sideCamera2Image;
			_sideCamera3Image = sideCamera3Image;
			_infraredCameraConversionImage = infraredCameraConversionImage;
			_sideCamera1ConversionImage = sideCamera1ConversionImage;
			_sideCamera2ConversionImage = sideCamera2ConversionImage;
			_sideCamera3ConversionImage = sideCamera3ConversionImage;

			_mainInfraredCameraDisplay = mainInfraredCameraDisplay;
			_mainSideCamera1Display = mainSideCamera1Display;
			_mainSideCamera2Display = mainSideCamera2Display;
			_mainSideCamera3Display = mainSideCamera3Display;

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
					_sideCamera1.ConnectAsync( ),
					_sideCamera2.ConnectAsync( ),
					_sideCamera3.ConnectAsync( )
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
					_sideCamera1.DisconnectAsync(),
					_sideCamera2.DisconnectAsync(),
					_sideCamera3.DisconnectAsync()
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

			_infraredCameraImage = _infraredCamera.ReciveImage();
			_sideCamera1Image = _sideCamera1.ReciveImage();
			_sideCamera2Image = _sideCamera2.ReciveImage();
			_sideCamera3Image = _sideCamera3.ReciveImage();

			var tasks = new[]
				{
					_infraredCamera.DisconnectAsync(),
					_sideCamera1.DisconnectAsync(),
					_sideCamera2.DisconnectAsync(),
					_sideCamera3.DisconnectAsync()
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

			if ((!_infraredCamera.IsConnected && !_sideCamera1.IsConnected && !_sideCamera2.IsConnected && !_sideCamera3.IsConnected) &&
				(_infraredCameraImage == MIL.M_NULL && _sideCamera1Image == MIL.M_NULL && _sideCamera2Image == MIL.M_NULL && _sideCamera3Image == MIL.M_NULL))
			{
				Logger.WriteLine( "카메라가 연결되어 있지 않고, 로드된 이미지도 없습니다." );
				MessageBox.Show( "카메라가 연결되어 있지 않고, 로드된 이미지도 없습니다." );

				return;
			}
			Logger.WriteLine( "Vision Trigger Detected" );

			//if (isContinuous)
			//{
			//	MessageBox.Show( "Continuous 모드에서는 Trigger 기능을 사용할 수 없습니다." );
			//	return;
			//}
			await Task.Run( async ( ) =>
			{
				if (!_isCameraCapturing)
				{
					_isCameraCapturing = true;


					if (_infraredCamera.IsConnected && _sideCamera1.IsConnected && _sideCamera2.IsConnected && _sideCamera3.IsConnected)
					{
						try
						{
							// 카메라에서 이미지 캡처
							var tasks = new[]
							{
								_infraredCamera.CaptureSingleImageAsync(),
								_sideCamera1.CaptureSingleImageAsync(),
								_sideCamera2.CaptureSingleImageAsync(),
								_sideCamera3.CaptureSingleImageAsync()
							};
							await Task.WhenAll( tasks );

							Logger.WriteLine( "카메라 이미지 캡처 완료" );

							_infraredCameraImage = _infraredCamera.ReciveImage();
							_sideCamera1Image = _sideCamera1.ReciveImage();
							_sideCamera2Image = _sideCamera2.ReciveImage();
							_sideCamera3Image = _sideCamera3.ReciveImage();

							Logger.WriteLine( "카메라 이미지 수신 완료" );
						}
						catch (Exception ex)
						{
							Logger.WriteLine( $"이미지 캡쳐 중 오류 발생: {ex.Message}" );
							MessageBox.Show( $"이미지 캡쳐 중 오류 발생: {ex.Message}" );

						}


						try
						{

							/// 이미지를 Property에 저장하고 화면에 표시하는 로직을 구성해야함
							/// 

							MIL.MdispSelect( _mainInfraredCameraDisplay, _infraredCameraImage );
							MIL.MdispSelect( _mainSideCamera1Display, _sideCamera1Image );
							MIL.MdispSelect( _mainSideCamera2Display, _sideCamera2Image );
							MIL.MdispSelect( _mainSideCamera3Display, _sideCamera3Image );

							//MIL.MdispSelect( InfraredCameraDisplay, InfraredCameraImage );
							//MIL.MdispSelect( SideCamera1Display, SideCamera1Image );
							//MIL.MdispSelect( SideCamera2Display, SideCamera2Image );
							//MIL.MdispSelect( SideCamera3Display, SideCamera3Image );


							Logger.WriteLine( "카메라 이미지 디스플레이 완료" );
						}
						catch (Exception ex)
						{
							Logger.WriteLine( $"이미지 디스플레이 중 오류 발생: {ex.Message}" );
							MessageBox.Show( $"이미지 디스플레이 중 오류 발생: {ex.Message}" );

						}
					}
					else
					{
						// 로드된 이미지가 있다면 그 이미지를 사용
					}


					try
					{
						if (_infraredCameraImage != MIL.M_NULL && _sideCamera1Image != MIL.M_NULL && _sideCamera2Image != MIL.M_NULL && _sideCamera3Image != MIL.M_NULL)
						{
							// 여기서 pixelData에 대한 추가 처리(예: HSLThreshold 등) 호출 가능
							// 예: Conversion.RunHSLThreshold(hMin, hMax, sMin, sMax, lMin, lMax, pixelData);
							// 처리 후 다시 DisplayImage(pixelData)로 화면에 갱신할 수 있음
							bool isGood = true;

							if (_infraredCameraConversionImage == MIL.M_NULL) MIL.MbufFree( _infraredCameraConversionImage );
							_infraredCameraConversionImage = MIL.M_NULL;
							if (_sideCamera1ConversionImage == MIL.M_NULL) MIL.MbufFree( _sideCamera1ConversionImage );
							_sideCamera1ConversionImage = MIL.M_NULL;
							if (_sideCamera2ConversionImage == MIL.M_NULL) MIL.MbufFree( _sideCamera2ConversionImage );
							_sideCamera2ConversionImage = MIL.M_NULL;
							if (_sideCamera3ConversionImage == MIL.M_NULL) MIL.MbufFree( _sideCamera3ConversionImage );
							_sideCamera3ConversionImage = MIL.M_NULL;



							//InfraredCameraConversionImage = Conversion.InfraredCameraModel( InfraredCameraImage, ref isGood, currentInfraredCameraModel );
							//await Task.Run( ( ) => Conversion.SideCameraModel( SideCamera1Image, MainSideCamera1Display ) );

							//var tasks = new[]
							//{
							//	///Overlay Display  Image 어떻게 처리할 것인지?
							//	///디스플레이 선언과 동시에 Overlay Image의 버퍼를 받아서 저장후에 
							//	///필요할때마다 가져다가 쓰는 방식으로 해야할 것 같음
							//	///
							//	Conversion.SideCameraModel( _sideCamera1Image, _mainSideCamera1Display ),
							//	Conversion.SideCameraModel( _sideCamera2Image, _mainSideCamera2Display ),
							//	Conversion.SideCameraModel( _sideCamera3Image, _mainSideCamera3Display )
							//};
							//bool[] result = await Task.WhenAll( tasks );

							//isGood = result[ 0 ] && result[ 1 ] && result[ 2 ];
							//MIL.MdispSelect( InfraredCameraConversionDisplay, InfraredCameraConversionImage );
							Logger.WriteLine( "이미지 처리 완료" );

							///GOOD REJECT LAMP 바인딩
							//if (!Dispatcher.CheckAccess())
							//{
							//	// UI 스레드에서 실행되도록 Dispatcher를 사용하여 호출
							//	Dispatcher.Invoke( ( ) => GoodLamp( isGood ) );
							//}
							//if (!isGood) EjectAction();

							//DisplayConversionImage( ConversionpixelData );
						}
					}
					catch (Exception ex)
					{
						Logger.WriteLine( $"이미지 처리 중 오류 발생: {ex.Message}" );
						MessageBox.Show( $"이미지 처리 중 오류 발생: {ex.Message}" );

					}

					finally
					{
						_isCameraCapturing = false;
					}

				}

			} );
			TectTime.Stop();
			Logger.WriteLine( $"이미지 처리 시간: {TectTime.ElapsedMilliseconds}ms" );


		}
	}
}
