using DamoOneVision.Data;
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
	public class MatroxCamera : ICamera
	{
		// Matrox SDK 관련 필드
		private MIL_ID MilSystem = MIL.M_NULL;
		private MIL_ID MilDigitizer = MIL.M_NULL;
		//private MIL_ID MilGrabImage = MIL.M_NULL;
		private MIL_ID MilImage = MIL.M_NULL;
		string appfolder="";
		string imagesFolder="";

		public MIL_INT Width { get; set; }

		public MIL_INT Height { get; set; }

		public MIL_INT NbBands { get; set; }

		public MIL_INT DataType { get; set; }

		private string CameraName { get; set; }

		//private string LUT_FILE = @"C:\Users\LEE\Desktop\DamoOneVision\DamoOneVision\ColorMap\JETColorMap.mim";

		public MatroxCamera( string CameraName )
		{
			InitImageSave();
			MilSystem = MILContext.Instance.MilSystem;
			this.CameraName = CameraName;
			Log.WriteLine($"{CameraName} is Created" );
			Log.WriteLine( $"System ID : {MilSystem} " );
		}


		public bool Connect(  )
		{
			// MILContext에서 MilSystem 가져오기
			//MilSystem = MILContext.Instance.MilSystem;
			//string selectionString = $" M_GC_DEVICE_NAME={CameraName}";
			int dev = 0;
			switch(CameraName)
			{
				case "InfraredCamera":
					dev = 0;
					break;
				case "SideCamera1":
					dev = 1;
					break;
				case "SideCamera2":
					dev = 2;
					break;
				case "SideCamera3":
					dev = 3;
					break;
			}
			//if (CameraName == "InfraredCamera") dev = MIL.M_DEV0;
			//else if (CameraName == "Sidecamera1") dev = MIL.M_DEV1;
			//else if (CameraName == "Sidecamera2") dev = MIL.M_DEV2;
			//else if (CameraName == "Sidecamera3") dev = MIL.M_DEV3;

			Log.WriteLine($"{CameraName},{(int)dev}");


			// 디지타이저(카메라) 할당

			MIL.MdigAlloc( MilSystem, dev, "M_DEFAULT", MIL.M_DEFAULT, ref MilDigitizer );
			//MIL.M_GC_CAMERA_ID( CameraName );
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

				// 이미지 버퍼 할당
				//Bayer 이미지일 경우 NbBand 확인
				//MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height, MILContext.DataType, MIL.M_IMAGE + MIL.M_GRAB , ref MilImage );
				MIL.MbufAllocColor( MilSystem, this.NbBands, this.Width, this.Height, this.DataType, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_DISP + MIL.M_PROC, ref MilImage );
			}


			return MilDigitizer != MIL.M_NULL;
		}

		public void Disconnect( )
		{

			if (MilDigitizer != MIL.M_NULL) MIL.MdigFree( MilDigitizer );
			MilDigitizer = MIL.M_NULL;

			Log.WriteLine($"{CameraName} is Disconnect");

			//if (MilImage != MIL.M_NULL) MIL.MbufFree( MilImage );
			//MilImage = MIL.M_NULL;

		}




		private void InitImageSave()
		{
			// 'Images' 폴더 경로 설정
			string localappdata = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData);
			appfolder = System.IO.Path.Combine( localappdata, "DamoOneVision" );
			imagesFolder = System.IO.Path.Combine( appfolder, "Images");

			// 폴더가 없으면 생성
			if (!Directory.Exists( imagesFolder ))
			{
				Directory.CreateDirectory( imagesFolder );
			}

		}

		private void SaveImage( ref MIL_ID MilImage, string name )
		{
			if (true)
			{
				// 현재 시간과 날짜 가져오기
				string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				// 파일 이름 생성
				string fileName = $"{name}_{timeStamp}.bmp";
				// 전체 파일 경로
				string filePath = System.IO.Path.Combine(imagesFolder, fileName);

				//SaveImage( imageData, filePath );
				MIL.MbufSave( filePath, MilImage );
			}

		}



		public MIL_ID CaptureImage( )
		{

			//MIL_ID MilDisplay = MIL.M_NULL;

			MIL.MdigGrab( MilDigitizer, MilImage );
			Log.WriteLine($"{CameraName} Grab Complete");

			//MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WINDOWED, ref MilDisplay );
			//MIL.MdispControl( MilDisplay, MIL.M_VIEW_MODE, MIL.M_AUTO_SCALE );
			//MIL.MdispSelect( MilDisplay, MilImage );


			//SaveImage( ref MilImage , "RAWImage" );


			if (true)
			{
				//InfraredCameraScaleImage( MilImage );
			}
			//MIL.MdispFree( MilDisplay );
			return MilImage;

		}
		public MIL_ID ReciveImage( )
		{
			return MilImage;
		}


		private void InfraredCameraScaleImage( MIL_ID MilImage ) 
		{
			MIL_INT SizeByte = 0;
			MIL_ID MilBayerImage = MIL.M_NULL;
			//버퍼에 쓴 Scale 데이터를 byte로 변환
			MIL.MbufInquire( MilImage, MIL.M_SIZE_BYTE, ref SizeByte );



			///
			// 버퍼이미지를 Scale히여 16bit 이미지로 변환
			if (true)
			{
				if (this.DataType == 16 && this.NbBands == 1)
				{
					ushort [] ushortScaleImageData = ShortMilImageShortScale(MilImage);

					//byte [] byteImageData = ShortToByte(ushortScaleImageData);
					// Scale된 이미지 데이터 Buffer에 전송
					//MIL.MbufPut( MilImage, ushortScaleImageData );
					MIL.MbufPut( MilImage, ushortScaleImageData );
				}
				else if (this.DataType == 8 && this.NbBands == 1)
				{
					byte [] byteScaleImageData = ByteMilImageShortScale(MilImage);
					// Scale된 이미지 데이터 Buffer에 전송
					MIL.MbufPut( MilImage, byteScaleImageData );
					//MIL.MbufPut( MilImage, ushortScaleImageData );
				}

			}

			int imageDataType = (int) MIL.MbufInquire(MilImage, MIL.M_TYPE);
			int nbBands = (int) MIL.MbufInquire(MilImage, MIL.M_SIZE_BAND);

			Log.WriteLine( "Image DataType: " + imageDataType );
			Log.WriteLine( "Number of Bands (NbBands): " + nbBands );


			SaveImage( ref MilImage , "Image" );
		}

		private ushort[ ] ShortMilImageShortScale( MIL_ID MilImage )
		{
			ushort [] ImageData = new ushort[ this.Width * this.Height ];

			MIL.MbufGet( MilImage, ImageData );


			// 이미지 데이터의 최대값을 2번째로 큰 값으로 변경
			// MindVision의 GF120이 받아오는 이미지의 0번째 값이 0XFF로 고정되는 현상을 방지하기 위함
			//var distinctNumbersDesc = ImageData.Distinct().OrderByDescending( x => x ).ToArray();
			//if (distinctNumbersDesc.Length > 1 )
			//{
			//	ImageData[ 0 ] = distinctNumbersDesc[ 1 ];
			//}

			ushort MinPixelValue = ImageData.Min();
			ushort MaxPixelValue = ImageData.Max();


			// 30~50도 범위로 Scale
			double dMinPixelValue = (30.0 / 190.0) * 65535.0;
			double dMaxPixelValue = (50.0 / 190.0) * 65535.0;

			MinPixelValue = (ushort) dMinPixelValue;
			MaxPixelValue = (ushort) dMaxPixelValue;


			for (int i = 0; i < ImageData.Length; i++)
			{
				ImageData[ i ] = (ushort) (((double) (ImageData[ i ] - MinPixelValue) / (double) (MaxPixelValue - MinPixelValue)) * 65535);
				if (ImageData[ i ] > 65535)
				{
					ImageData[ i ] = 65535;
				}
				else if (ImageData[ i ] < 0)
				{
					ImageData[ i ] = 0;
				}
			}

			return ImageData;
		}

		private byte[ ] ByteMilImageShortScale( MIL_ID MilImage )
		{
			byte [] ImageData = new byte[ this.Width * this.Height ];

			MIL.MbufGet( MilImage, ImageData );


			// 이미지 데이터의 최대값을 2번째로 큰 값으로 변경
			// MindVision의 GF120이 받아오는 이미지의 0번째 값이 0XFF로 고정되는 현상을 방지하기 위함
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
			MIL_ID MilImage = MIL.M_NULL;

			if (File.Exists( filePath ))
			{

				MIL.MbufImport( filePath, MIL.M_DEFAULT, MIL.M_RESTORE+MIL.M_NO_GRAB+MIL.M_NO_COMPRESS, MilSystem, ref MilImage );

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
			return MilImage;
		}
	}
}
