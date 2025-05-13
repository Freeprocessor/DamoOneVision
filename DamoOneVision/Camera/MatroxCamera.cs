using DamoOneVision.Services;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace DamoOneVision.Camera
{
	public class MatroxCamera : ICamera,IDisposable
	{
		// Matrox SDK 관련 필드
		private MIL_ID MilSystem = MIL.M_NULL;
		private MIL_ID MilDigitizer = MIL.M_NULL;
		//private MIL_ID MilGrabImage = MIL.M_NULL;
		private MIL_ID MilImage = MIL.M_NULL;
		private MIL_ID MilScaleImage = MIL.M_NULL;
		private MIL_ID LoadMilImage = MIL.M_NULL;
		private MIL_ID LoadMilScaleImage = MIL.M_NULL;

		private MIL_ID MilBinarizedImage = MIL.M_NULL;
		private MIL_ID MilLoadBinarizedImage = MIL.M_NULL;

		private int[] _infraredImageFilter;

		string appfolder="";
		string imagesFolder="";
		string cameraImageFolder = "";
		string DCFFolder="";
		string DCFPath = "";

		public MIL_INT Width { get; set; }

		public MIL_INT Height { get; set; }

		public MIL_INT NbBands { get; set; }

		public MIL_INT DataType { get; set; }

		private string CameraName { get; set; }

		//private string LUT_FILE = @"C:\Users\LEE\Desktop\DamoOneVision\DamoOneVision\ColorMap\JETColorMap.mim";

		public MatroxCamera( string CameraName )
		{
			this.CameraName = CameraName;
			Logger.WriteLine( $"{CameraName} is Created" );

			InitImageSave();
			MilSystem = MILContext.Instance.MilSystem;
			//Log.WriteLine( $"System ID : {MilSystem} " );
		}

		private void InitImageSave( )
		{
			// 'Images' 폴더 경로 설정
			string localappdata = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData);
			appfolder = System.IO.Path.Combine( localappdata, "DamoOneVision" );
			imagesFolder = System.IO.Path.Combine( appfolder, "Images" );
			cameraImageFolder = System.IO.Path.Combine( imagesFolder, CameraName );
			DCFFolder = System.IO.Path.Combine( appfolder, "DCF" );
			DCFPath = System.IO.Path.Combine( DCFFolder, $"{CameraName}.dcf" );

			// 폴더가 없으면 생성
			if (!Directory.Exists( imagesFolder ))
			{
				Directory.CreateDirectory( imagesFolder );
			}

			if(!Directory.Exists( cameraImageFolder ))
			{
				Directory.CreateDirectory( cameraImageFolder );
			}

		}


		public bool Connect( )
		{

			// MILContext에서 MilSystem 가져오기
			//MilSystem = MILContext.Instance.MilSystem;
			//string selectionString = $" M_GC_DEVICE_NAME={CameraName}";
			int devNum = 0;
			MIL_INT countNum = 0;
			

			MIL.MsysControl(MilSystem, MIL.M_DISCOVER_DEVICE, MIL.M_DEFAULT );

			MIL.MsysInquire( MilSystem, MIL.M_DISCOVER_DEVICE_COUNT, ref countNum );
			Logger.WriteLine( $"Device Count Number : {countNum}" );

			string[] DeviceName = new string[countNum];

			for (int i = 0; i < countNum; i++)
			{
				StringBuilder deviceNameBuilder = new StringBuilder(256);
				MIL.MsysInquire( MilSystem, MIL.M_DISCOVER_DEVICE_USER_NAME + i, deviceNameBuilder );
				DeviceName[ i ] = deviceNameBuilder.ToString();
				//Log.WriteLine( $"DEV{i} Device Name : {DeviceName[ i ]}" );
				if (DeviceName[ i ] == CameraName)
				{
					devNum = i;
					break;
				}
				
			}

			Logger.WriteLine($"Camera Name: {CameraName}, Digitizer Num: {(int)devNum}");

			// 디지타이저(카메라) 할당
			MIL.MdigAlloc( MilSystem, devNum, DCFPath, MIL.M_DEFAULT, ref MilDigitizer );


			if (MilImage == MIL.M_NULL)
			{

				// 이미지 크기 및 속성 가져오기
				MIL_INT width = 0;
				MIL_INT height = 0;
				MIL_INT nbBands = 0;
				MIL_INT dataType = 0;

				MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_X, ref width );
				MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_Y, ref height );
				MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_BAND, ref nbBands );
				MIL.MdigInquire( MilDigitizer, MIL.M_TYPE, ref dataType );

				this.Width = width;
				this.Height = height;
				this.NbBands = nbBands;
				this.DataType = dataType;

				Logger.WriteLine( $"{CameraName} Spec -> Width: {width}, Height: {height}, NbBands: {nbBands}, DataType: {dataType}"  );

				// 이미지 버퍼 할당
				//Bayer 이미지일 경우 NbBand 확인
				//MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height, MILContext.DataType, MIL.M_IMAGE + MIL.M_GRAB , ref MilImage );
				MIL.MbufAllocColor( MilSystem, this.NbBands, this.Width, this.Height, this.DataType, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_DISP + MIL.M_PROC, ref MilImage );
				MIL.MbufAllocColor( MilSystem, this.NbBands, this.Width, this.Height, this.DataType, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_DISP + MIL.M_PROC, ref MilScaleImage );
				MIL.MbufAllocColor( MilSystem, this.NbBands, this.Width, this.Height, this.DataType, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_DISP + MIL.M_PROC, ref MilBinarizedImage );
			}

			if (CameraName == "InfraredCamera")
			{
				//InfraredCameraNoiseFilter( System.IO.Path.Combine( appfolder, "InfraredCameraNoiseFilter.bmp" ) );
			}

			Logger.WriteLine( $"Camera Name: {CameraName} Connect Success" );

			return MilDigitizer != MIL.M_NULL;
		}

		public void Disconnect( )
		{
			//MIL.MdigGrabWait( MilDigitizer, MIL.M_GRAB_END );

			if (MilDigitizer != MIL.M_NULL) MIL.MdigFree( MilDigitizer );
			MilDigitizer = MIL.M_NULL;

			Logger.WriteLine($"{CameraName} is Disconnect");

			//if (MilImage != MIL.M_NULL) MIL.MbufFree( MilImage );
			//MilImage = MIL.M_NULL;

		}


		public async Task SaveImage( MIL_ID MilImage, string name )
		{
			await Task.Run( ( ) =>
			{

				// 현재 시간과 날짜 가져오기
				string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_f");
				// 파일 이름 생성
				string fileName = $"{name}_{timeStamp}.bmp";

				string folderClassPath = System.IO.Path.Combine( cameraImageFolder, $"{name}" );

				if (!Directory.Exists( folderClassPath ))
				{
					Directory.CreateDirectory( folderClassPath );
				}

				string folderClassDatePath = System.IO.Path.Combine( folderClassPath, $"{DateTime.Today:yyyy-MM-d}" );

				if (!Directory.Exists( folderClassDatePath ))
				{
					Directory.CreateDirectory( folderClassDatePath );
				}

				// 전체 파일 경로
				string filePath = System.IO.Path.Combine( folderClassDatePath, fileName );
				//SaveImage( imageData, filePath );
				MIL.MbufSave( filePath, MilImage );
			} );

		}

		public MIL_ID CaptureImage( )
		{

			Stopwatch TectTime = new Stopwatch();
			TectTime.Start();
			//MIL_ID MilDisplay = MIL.M_NULL;
			for (int i=0;i<2;i++)
			{
				MIL.MdigGrab( MilDigitizer, MilImage );
			}

			MIL.MdigHalt( MilDigitizer );
			///
			//Logger.WriteLine($"{CameraName} Grab Complete");

			SaveImage( MilImage , $"RAW{CameraName}" );

			//if (CameraName == "InfraredCamera" && _infraredImageFilter != null)
			//{

			//}

			if (true)
			{
				InfraredCameraScaleImage( MilImage, MilScaleImage );
			}
			//MIL.MdispFree( MilDisplay );
			//Logger.WriteLine( CameraName + " CaptureImage Complete" );

			TectTime.Stop();
			Logger.WriteLine( $"{CameraName} Grab 시간: {TectTime.ElapsedMilliseconds}ms" );
			return MilImage;

		}

		public MIL_ID ReciveImage( )
		{
			return MilImage;
		}

		public MIL_ID ReciveScaleImage( )
		{
			return MilScaleImage;
		}

		public MIL_ID ReciveLoadImage( )
		{
			return LoadMilImage;
		}

		public MIL_ID ReciveLoadScaleImage( )
		{
			return LoadMilScaleImage;
		}

		public MIL_ID ReciveBinarizedImage( )
		{
			return MilBinarizedImage;
		}

		public MIL_ID ReciveLoadBinarizedImage( )
		{
			return MilLoadBinarizedImage;
		}

		private void InfraredCameraNoiseFilter( string filePath )
		{
			if (_infraredImageFilter == null)
			{
				MIL_ID MilImage = MIL.M_NULL;

				if (File.Exists( filePath ))
				{

					MIL.MbufImport( filePath, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MilSystem, ref MilImage );

					// 이미지 속성 가져오기
					MIL_INT width = 0;
					MIL_INT height = 0;
					MIL_INT nbBands = 0;
					MIL_INT dataType = 0;

					MIL.MbufInquire( MilImage, MIL.M_SIZE_X, ref width );
					MIL.MbufInquire( MilImage, MIL.M_SIZE_Y, ref height );
					MIL.MbufInquire( MilImage, MIL.M_SIZE_BAND, ref nbBands );
					MIL.MbufInquire( MilImage, MIL.M_TYPE, ref dataType );

					this.Width = width;
					this.Height = height;
					this.NbBands = nbBands;
					this.DataType = dataType;

				}
				else
				{
					return;
				}
				// 이미지 데이터 가져오기
				ushort [] ImageData = new ushort[ this.Width * this.Height ];
				MIL.MbufGet( MilImage, ImageData );

				MIL.MbufFree( MilImage );

				// 중앙값 필터링
				ushort[] sorted = (ushort[])ImageData.Clone();
				Array.Sort( sorted );

				int i = sorted.Length;
				int median = 0;
				if (i % 2 == 1) median = sorted[ i / 2 ];
				else median = (ushort) ((sorted[ i / 2 - 1 ] + sorted[ i / 2 ]) / 2);

				// 중앙값 필터링 결과
				_infraredImageFilter = ImageData.Select( x => (int) x - median ).ToArray();
			}
		}

		private void InfraredNoiseFiltering( ushort[] ImageData )
		{
			for (int i = 0; i < ImageData.Length; i++)
			{
				/// ushort -> int 변환 후 _infraredImageFilter[i] 빼기
				int diff = (int)ImageData[i] - _infraredImageFilter[i];

				/// 음수나 65535를 초과하지 않는 것이 보장된다면 바로 캐스팅 가능
				if (diff < 0) diff = 0;
				else if (diff > ushort.MaxValue) diff = ushort.MaxValue;

				ImageData[ i ] = (ushort) diff;
			}
		}

		private void InfraredCameraScaleImage( MIL_ID MilImage, MIL_ID MilScaleImage ) 
		{
			/// 버퍼이미지를 Scale히여 16bit 이미지로 변환
			if (true)
			{
				if (this.DataType == 16 && this.NbBands == 1)
				{
					ushort [] ushortScaleImageData = ShortMilImageShortScale(MilImage);

					/// Scale된 이미지 데이터 Buffer에 전송
					//MIL.MbufPut( MilImage, ushortScaleImageData );
					MIL.MbufPut( MilScaleImage, ushortScaleImageData );
				}
				else if (this.DataType == 8 && this.NbBands == 1 && false)
				{
					byte [] byteScaleImageData = ByteMilImageShortScale(MilImage);
					// Scale된 이미지 데이터 Buffer에 전송
					MIL.MbufPut( MilImage, byteScaleImageData );
					//MIL.MbufPut( MilImage, ushortScaleImageData );
				}

			}

			//int imageDataType = (int) MIL.MbufInquire(MilImage, MIL.M_TYPE);
			//int nbBands = (int) MIL.MbufInquire(MilImage, MIL.M_SIZE_BAND);

			//Log.WriteLine( "Image DataType: " + imageDataType );
			//Log.WriteLine( "Number of Bands (NbBands): " + nbBands );


			SaveImage( MilScaleImage, $"{CameraName}" );
		}


		// 사진상의 최대,최소 픽셀값을 이용하여 이미지 데이터를 0~65535로 스케일링	
		private ushort[ ] ShortMilImageShortScale( MIL_ID MilImage )
		{
			ushort [] ImageData = new ushort[ this.Width * this.Height ];

			MIL.MbufGet( MilImage, ImageData );

			///이미지 데이터 레밸링
			//InfraredNoiseFiltering( ImageData );

			/// 이미지 데이터의 최대값을 2번째로 큰 값으로 변경
			/// MindVision의 GF120이 받아오는 이미지의 0번째 값이 0XFF로 고정되는 현상을 방지하기 위함
			//var distinctNumbersDesc = ImageData.Distinct().OrderByDescending( x => x ).ToArray();
			//if (distinctNumbersDesc.Length > 1)
			//{
			//	ImageData[ 0 ] = distinctNumbersDesc[ 1 ];
			//}

			ushort MinPixelValue = 30115;//28도
			ushort MaxPixelValue = 36315;//90도

			MinPixelValue = ImageData.Min();
			MaxPixelValue = ImageData.Max();

			// 30~50도 범위로 Scale
			//double dMinPixelValue = (30.0 / 190.0) * 65535.0;
			//double dMaxPixelValue = (50.0 / 190.0) * 65535.0;

			//MinPixelValue = (ushort) dMinPixelValue;
			//MaxPixelValue = (ushort) dMaxPixelValue;
			int data=0;

			for (int i = 0; i < ImageData.Length; i++)
			{
				data = (int) (((double) (ImageData[ i ] - MinPixelValue) / (double) (MaxPixelValue - MinPixelValue)) * 65535);
				if (data > 65535)
				{
					ImageData[ i ] = 65535;
				}
				else if (data < 0)
				{
					ImageData[ i ] = 0;
				}
				else
				{
					ImageData[ i ] = (ushort)data;
				}
			}

			return ImageData;
		}

		private byte[ ] ByteMilImageShortScale( MIL_ID MilImage )
		{
			byte [] ImageData = new byte[ this.Width * this.Height ];

			MIL.MbufGet( MilImage, ImageData );


			/// 이미지 데이터의 최대값을 2번째로 큰 값으로 변경
			/// MindVision의 GF120이 받아오는 이미지의 0번째 값이 0XFF로 고정되는 현상을 방지하기 위함
			//var distinctNumbersDesc = ImageData.Distinct().OrderByDescending( x => x ).ToArray();
			//if (distinctNumbersDesc[ 1 ] != null)
			//{
			//	ImageData[ 0 ] = distinctNumbersDesc[ 1 ];
			//}
				

			byte MinPixelValue = ImageData.Min();
			byte MaxPixelValue = ImageData.Max();


			for (int i = 0; i < ImageData.Length; i++)
			{
				ImageData[ i ] = (byte) (((double) (ImageData[ i ] - MinPixelValue) / (double) (MaxPixelValue - MinPixelValue)) * 255);
			}

			return ImageData;
		}

		private byte[ ] ShortToByte( ushort[ ] ushortImageData )
		{
			byte [] ImageData = new byte[ this.Width * this.Height ];

			for (int i = 0; i < ImageData.Length; i++)
			{
				ImageData[ i ] = (byte) ((double) ((double) ushortImageData[ i ] / 65535) * 255);
			}


			return ImageData;
		}

		public MIL_ID LoadImage( MIL_ID MilSystem, string filePath )
		{
			
			if (File.Exists( filePath ))
			{
				if (LoadMilImage != MIL.M_NULL)
				{
					MIL.MbufFree( LoadMilImage );
					Logger.WriteLine( "LoadMilImage 해제" );
				}
				if (LoadMilScaleImage != MIL.M_NULL)
				{
					MIL.MbufFree( LoadMilScaleImage );
					Logger.WriteLine( "LoadMilScaleImage 해제" );
				}
				if (MilLoadBinarizedImage != MIL.M_NULL)
				{
					MIL.MbufFree( MilLoadBinarizedImage );
					Logger.WriteLine( "MilLoadBinarizedImage 해제" );
				}

				MIL.MbufImport( filePath, MIL.M_DEFAULT, MIL.M_RESTORE+MIL.M_NO_GRAB+MIL.M_NO_COMPRESS, MilSystem, ref LoadMilImage );
				MIL.MbufImport( filePath, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MilSystem, ref LoadMilScaleImage );
				MIL.MbufImport( filePath, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MilSystem, ref MilLoadBinarizedImage );

				// 이미지 속성 가져오기
				MIL_INT width = 0;
				MIL_INT height = 0;
				MIL_INT nbBands = 0;
				MIL_INT dataType = 0;

				MIL.MbufInquire( LoadMilImage, MIL.M_SIZE_X, ref width );
				MIL.MbufInquire( LoadMilImage, MIL.M_SIZE_Y, ref height );
				MIL.MbufInquire( LoadMilImage, MIL.M_SIZE_BAND, ref nbBands );
				MIL.MbufInquire( LoadMilImage, MIL.M_TYPE, ref dataType );

				this.Width = width;
				this.Height = height;
				this.NbBands = nbBands;
				this.DataType = dataType;

			}
			InfraredCameraScaleImage( LoadMilImage, LoadMilScaleImage );

			//SaveJetImage( LoadMilScaleImage, "JET" );

			return LoadMilImage;
		}

		public async Task<double> AutoFocus( )
		{
			MIL.MdigControlFeature( MilDigitizer, MIL.M_FEATURE_EXECUTE, "AutoFocus", MIL.M_DEFAULT );
			await Task.Delay( 2000 );

			double currentFocus = 0.0;
			MIL.MdigInquireFeature( MilDigitizer, MIL.M_FEATURE_VALUE, "FocusDistance",  MIL.M_TYPE_DOUBLE, ref currentFocus );
			//ManualFocus();
			Logger.WriteLine( $"{CameraName} AutoFocus : {currentFocus}" );
			return currentFocus;
		}

		public async void ManualFocus(double focusValue )
		{
			MIL.MdigControlFeature( MilDigitizer, MIL.M_FEATURE_VALUE, "FocusDistance", MIL.M_TYPE_DOUBLE, ref focusValue );

			await Task.Delay( 2000 );

			Logger.WriteLine( $"{CameraName} ManualFocus" );
		}

		public ushort[ ] LoadImageData( )
		{
			if (LoadMilImage == MIL.M_NULL)
			{
				//Logger.WriteLine( $"{CameraName} ImageData is NULL" );
				return null;
			}
			ushort[] imageData = new ushort[ this.Width * this.Height ];
			MIL.MbufGet( LoadMilImage, imageData );
			return imageData;
		}

		public ushort[ ] CaptureImageData( )
		{
			if (MilImage == MIL.M_NULL)
			{
				//Logger.WriteLine( $"{CameraName} ImageData is NULL" );
				return null;
			}
			ushort[] imageData = new ushort[ this.Width * this.Height ];
			MIL.MbufGet( MilImage, imageData );
			return imageData;
		}

		public void Dispose( )
		{
			Disconnect( );
			if(MilImage != MIL.M_NULL)
			{
				MIL.MbufFree( MilImage );
				Logger.WriteLine( "MilImage 해제" );
			}
			if (MilScaleImage != MIL.M_NULL)
			{
				MIL.MbufFree( MilScaleImage );
				Logger.WriteLine( "MilScaleImage 해제" );
			}
			if (MilBinarizedImage != MIL.M_NULL)
			{
				MIL.MbufFree( MilBinarizedImage );
				Logger.WriteLine( "MilBinarizedImage 해제" );
			}
			if (LoadMilImage != MIL.M_NULL)
			{
				MIL.MbufFree( LoadMilImage );
				Logger.WriteLine( "LoadMilImage 해제" );
			}
			if (LoadMilScaleImage != MIL.M_NULL)
			{
				MIL.MbufFree( LoadMilScaleImage );
				Logger.WriteLine( "LoadMilScaleImage 해제" );
			}
			if (MilLoadBinarizedImage != MIL.M_NULL)
			{
				MIL.MbufFree( MilLoadBinarizedImage );
				Logger.WriteLine( "MilLoadBinarizedImage 해제" );
			}
			if (MilDigitizer != MIL.M_NULL)
			{
				MIL.MdigFree( MilDigitizer );
				Logger.WriteLine( "MilDigitizer 해제" );
			}



		}
	}
}
