using DamoOneVision.Camera;
using DamoOneVision.ViewModels;
using DamoOneVision.ImageProcessing;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using DamoOneVision.Models;


namespace DamoOneVision.Services
{
	public class CameraService : IDisposable
	{

		public event Func<Task> VisionResult;
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

		private readonly Lazy<MainViewModel> _mainViewModel;

		private InfraredCameraModel _infraredCameraModel;

		private MIL_ID MilSystem = MIL.M_NULL;


		public MIL_ID _infraredCameraDisplay;
		public MIL_ID _sideCamera1Display;
		public MIL_ID _sideCamera2Display;
		public MIL_ID _sideCamera3Display;
		public MIL_ID _infraredCameraConversionDisplay;
		public MIL_ID _sideCamera1ConversionDisplay;
		public MIL_ID _sideCamera2ConversionDisplay;
		public MIL_ID _sideCamera3ConversionDisplay;

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

		public bool LoadImageUsed { get; set; } = false; // LoadImage 사용 여부

		// 상태 변경을 알리기 위한 이벤트들
		public event Action<bool> CameraConnectedChanged;
		public event Action<bool> BusyStateChanged;


		public CameraService(CameraManager infraredCamera, CameraManager sideCamera1, CameraManager sideCamera2, CameraManager sideCamera3,
			MilSystemService milSystemService, Lazy<MainViewModel> mainViewModel )
		{
			MilSystem = MILContext.Instance.MilSystem;

			_mainViewModel = mainViewModel;
			_infraredCameraDisplay = milSystemService.InfraredDisplay;
			_sideCamera1Display = milSystemService.SideCam1Display;
			_sideCamera2Display = milSystemService.SideCam2Display;
			_sideCamera3Display = milSystemService.SideCam3Display;

			_infraredCameraConversionDisplay = milSystemService.InfraredConversionDisplay;
			_sideCamera1ConversionDisplay = milSystemService.SideCam1ConversionDisplay;
			_sideCamera2ConversionDisplay = milSystemService.SideCam2ConversionDisplay;
			_sideCamera3ConversionDisplay = milSystemService.SideCam3ConversionDisplay;

			_infraredCamera = infraredCamera;
			_sideCamera1 = sideCamera1;
			_sideCamera2 = sideCamera2;
			_sideCamera3 = sideCamera3;
		}

		public async void SetModel( InfraredCameraModel infraredCameraModel )
		{
			_infraredCameraModel = infraredCameraModel;
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
				//_infraredCamera.AutoFocus();
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

		public void InfraredCameraAutoFocus( )
		{
			_infraredCamera.AutoFocus();
		}

		public void InfraredCameraManualFocus( )
		{
			_infraredCamera.ManualFocus();
		}

		public async void InfraredCameraLoadImage( string filePath )
		{
			LoadImageUsed = true;
			var vm = _mainViewModel.Value;
			_infraredCamera.LoadImage( MilSystem, filePath );

			MIL.MdispSelect( _infraredCameraDisplay, _infraredCamera.ReciveLoadScaleImage() );
			if (await Conversion.InfraredCameraModel( false, false, GetBinarizedImage(), GetScaleImage(), GetImage(), _infraredCameraDisplay, _infraredCameraModel, ImageData()))
			{
				vm.IsGoodColor = "Green";
				vm.IsGoodStatus = "Good";
			}
			else
			{
				vm.IsGoodColor = "Red";
				vm.IsGoodStatus = "Reject";
			}
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

		public MIL_ID GetImage( )
		{
			if (LoadImageUsed)
			{
				return _infraredCamera.ReciveLoadImage();
			}
			else
			{
				return _infraredCamera.ReciveImage();
			}
		}

		public MIL_ID GetBinarizedImage( )
		{
			return _infraredCamera.ReciveBinarizedImage();
		}

		public MIL_ID GetScaleImage( )
		{
			if (LoadImageUsed)
			{
				return _infraredCamera.ReciveLoadScaleImage();
			}
			else
			{
				return _infraredCamera.ReciveScaleImage();
			}
		}

		public ushort[ ] ImageData( )
		{
			if (LoadImageUsed)
			{
				return _infraredCamera.LoadImageData();
			}
			else
			{
				return _infraredCamera.CaptureImageData();
			}
		}


		public async Task<bool> VisionTrigger( )
		{
			LoadImageUsed = false;
			bool isGood = true;

			Stopwatch TectTime = new Stopwatch();
			TectTime.Start();

			await Task.Run( async ( ) =>
			{
				if (!_isCameraCapturing)
				{
					_isCameraCapturing = true;

					// 카메라에서 이미지 캡처
					/// SideCamera 제거 후 InfraredCamera만 사용
					try
					{
						var tasks = new []
						{
							_infraredCamera.CaptureSingleImageAsync(),
							//_sideCamera1.CaptureSingleImageAsync(),
							//_sideCamera2.CaptureSingleImageAsync(),
							//_sideCamera3.CaptureSingleImageAsync()
						};
						await Task.WhenAll( tasks );
						Logger.WriteLine( "카메라 이미지 캡처 완료" );
						//Logger.WriteLine( $"Test: {TectTime.ElapsedMilliseconds}ms" );
					}
					catch (Exception ex)
					{
						Logger.WriteLine( $"이미지 캡쳐 중 오류 발생: {ex.Message}" );
						MessageBox.Show( $"이미지 캡쳐 중 오류 발생: {ex.Message}" );
					}


					MIL.MdispSelect( _infraredCameraDisplay, _infraredCamera.ReciveScaleImage() );
					MIL.MdispSelect( _sideCamera1Display, _sideCamera1.ReciveImage() );
					MIL.MdispSelect( _sideCamera2Display, _sideCamera2.ReciveImage() );
					MIL.MdispSelect( _sideCamera3Display, _sideCamera3.ReciveImage() );
					//Logger.WriteLine( $"Test2: {TectTime.ElapsedMilliseconds}ms" );


					try
					{
						if (_infraredCamera.ReciveImage() != MIL.M_NULL)
						{
							// 여기서 pixelData에 대한 추가 처리(예: HSLThreshold 등) 호출 가능
							// 예: Conversion.RunHSLThreshold(hMin, hMax, sMin, sMax, lMin, lMax, pixelData);
							// 처리 후 다시 DisplayImage(pixelData)로 화면에 갱신할 수 있음
							

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
							isGood = await Conversion.InfraredCameraModel( false, false, GetBinarizedImage(), GetScaleImage(), GetImage(), _infraredCameraDisplay, _infraredCameraModel, ImageData() );

							Logger.WriteLine( "이미지 처리 완료" );

							///GOOD REJECT LAMP 바인딩
							///
							var vm = _mainViewModel.Value;
							if (!isGood)
							{
								vm.IsGoodColor = "Red";
								vm.IsGoodStatus = "Reject";
							}
							else
							{
								vm.IsGoodColor = "Green";
								vm.IsGoodStatus = "Good";
							}
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
						//MessageBox.Show( $"이미지 처리 중 오류 발생: {ex.Message}" );

					}


					_isCameraCapturing = false;

				}

			} );
			TectTime.Stop();
			Logger.WriteLine( $"이미지 처리 시간: {TectTime.ElapsedMilliseconds}ms" );
			return isGood;

		}

		public void Dispose( )
		{
			// 관리하는 모든 CameraManager의 Dispose를 호출합니다.
			_infraredCamera?.Dispose();
			_sideCamera1?.Dispose();
			_sideCamera2?.Dispose();
			_sideCamera3?.Dispose();

			// 가비지 컬렉터가 파이널라이저를 호출하지 않도록 합니다.
			GC.SuppressFinalize( this );
		}

	}
}
