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
		private MIL_ID MilImage = MIL.M_NULL;
		private bool isIRCamera = false;
		//private string LUT_FILE = @"C:\Users\LEE\Desktop\DamoOneVision\DamoOneVision\ColorMap\JETColorMap.mim";


		public bool Connect( )
		{
			// MILContext에서 MilSystem 가져오기
			MilSystem = MILContext.Instance.MilSystem;

			// 디지타이저(카메라) 할당
			MIL.MdigAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref MilDigitizer );

			return MilDigitizer != MIL.M_NULL;
		}

		public void Disconnect( )
		{
			if (MilDigitizer != MIL.M_NULL)
			{
				MIL.MdigFree( MilDigitizer );
				MilDigitizer = MIL.M_NULL;
			}

			if (MilImage != MIL.M_NULL)
			{
				MIL.MbufFree( MilImage );
				MilImage = MIL.M_NULL;
			}
		}

		public byte[ ] CaptureImage( )
		{
			// 'Images' 폴더 경로 설정
			string imagesFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

			// 폴더가 없으면 생성
			if (!Directory.Exists( imagesFolder ))
			{
				Directory.CreateDirectory( imagesFolder );
			}

			// 이미지 버퍼 할당
			if (MilImage == MIL.M_NULL)
			{

				// 이미지 크기 및 속성 가져오기
				MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_X, ref MILContext.Width );
				MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_Y, ref MILContext.Height );
				MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_BAND, ref MILContext.NbBands );
				MIL.MdigInquire( MilDigitizer, MIL.M_TYPE, ref MILContext.DataType );

				//MILContext.NbBands = 1;

				// 이미지 버퍼 할당
				//Bayer 이미지일 경우 NbBand 확인
				//MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height, MILContext.DataType, MIL.M_IMAGE + MIL.M_GRAB , ref MilImage );
				MIL.MbufAllocColor(MilSystem, MILContext.NbBands, MILContext.Width, MILContext.Height, MILContext.DataType, MIL.M_IMAGE + MIL.M_GRAB, ref MilImage );
				isIRCamera = true;
			}

			// 이미지 캡처
			MIL.MdigGrab( MilDigitizer, MilImage );

			if (true)
			{
				// 현재 시간과 날짜 가져오기
				string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				// 파일 이름 생성
				string fileName = $"RAWImage_{timeStamp}.bmp";
				// 전체 파일 경로
				string filePath = System.IO.Path.Combine(imagesFolder, fileName);

				//SaveImage( imageData, filePath );
				MIL.MbufSave( filePath, MilImage );
			}

			MIL_INT SizeByte = 0;
			MIL_ID MilBayerImage = MIL.M_NULL;
			//버퍼에 쓴 Scale 데이터를 byte로 변환
			MIL.MbufInquire( MilImage, MIL.M_SIZE_BYTE, ref SizeByte );



			///
			// 버퍼이미지를 Scale히여 16bit 이미지로 변환
			if (isIRCamera == true)
			{
				if (MILContext.DataType == 16 && MILContext.NbBands == 1)
				{
					ushort [] ushortScaleImageData = ShortMilImageShortScale(MilImage);

					//byte [] byteImageData = ShortToByte(ushortScaleImageData);
					// Scale된 이미지 데이터 Buffer에 전송
					//MIL.MbufPut( MilImage, ushortScaleImageData );
					MIL.MbufPut( MilImage, ushortScaleImageData );
				}
				else if (MILContext.DataType == 8 && MILContext.NbBands == 1)
				{
					byte [] byteScaleImageData = ByteMilImageShortScale(MilImage);
					// Scale된 이미지 데이터 Buffer에 전송
					MIL.MbufPut( MilImage, byteScaleImageData );
					//MIL.MbufPut( MilImage, ushortScaleImageData );
				}
				//MIL_ID JETColorMap = MIL.M_NULL;
				//MIL_ID MimLut = MIL.M_NULL;
				//MIL_ID ChildImage = MIL.M_NULL;

				//MIL.MbufImport( LUT_FILE, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MilSystem, ref JETColorMap );
				//MIL.MbufAllocColor(MilSystem, 3, MILContext.Width, MILContext.Height, MILContext.DataType, MIL.M_IMAGE + MIL.M_PROC, ref MimLut);

				//MIL.MbufClear( MimLut, MIL.M_COLOR_BLACK );

				//MIL.MbufChildColor(MimLut, MIL.M_PLANAR, 0, MIL.M_ALL_BANDS, MilImage, MIL.M_ALL_BANDS, 0, 0, MIL.M_NULL, ref ChildImage );

				//MIL.MimLutMap(MilImage, MimLut, JETColorMap );
			}


			//Bayer 이미지일 경우 RGB로 변환
			///TODO : Bayer 이미지를 RGB로 변환하는 코드 추가

			SizeByte = 0;
			//버퍼에 쓴 Scale 데이터를 byte로 변환
			MIL.MbufInquire( MilImage, MIL.M_SIZE_BYTE, ref SizeByte );
			byte[] imageData = new byte[SizeByte];

			MIL.MbufGet( MilImage, imageData );

			int imageDataType = (int) MIL.MbufInquire(MilImage, MIL.M_TYPE);
			int nbBands = (int) MIL.MbufInquire(MilImage, MIL.M_SIZE_BAND);

			Debug.WriteLine( "Image DataType: " + imageDataType );
			Debug.WriteLine( "Number of Bands (NbBands): " + nbBands );

			if (true)
			{
				// 현재 시간과 날짜 가져오기
				string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				// 파일 이름 생성
				string fileName = $"Image_{timeStamp}.bmp";
				// 전체 파일 경로
				string filePath = System.IO.Path.Combine(imagesFolder, fileName);

				//SaveImage( imageData, filePath );
				MIL.MbufSave( filePath, MilImage);
			}



			return imageData;
			

			// Scale된 이미지 데이터를 byte로 변환
			// TODO : 어떻게 사용할건지 확인해야함
			//byte [] byteImageData = ShortToByte(ushortScaleImageData);
			//

		}

		private ushort[ ] ShortMilImageShortScale( MIL_ID MilImage )
		{
			ushort [] ImageData = new ushort[ MILContext.Width * MILContext.Height ];

			MIL.MbufGet( MilImage, ImageData );


			// 이미지 데이터의 최대값을 2번째로 큰 값으로 변경
			// MindVision의 GF120이 받아오는 이미지의 0번째 값이 0XFF로 고정되는 현상을 방지하기 위함
			var distinctNumbersDesc = ImageData.Distinct().OrderByDescending( x => x ).ToArray();
			if (distinctNumbersDesc.Length > 1 )
			{
				ImageData[ 0 ] = distinctNumbersDesc[ 1 ];
			}

			ushort MinPixelValue = ImageData.Min();
			ushort MaxPixelValue = ImageData.Max();


			for (int i = 0; i < ImageData.Length; i++)
			{
				ImageData[ i ] = (ushort) (((double) (ImageData[ i ] - MinPixelValue) / (double) (MaxPixelValue - MinPixelValue)) * 65535);
			}

			return ImageData;
		}

		private byte[ ] ByteMilImageShortScale( MIL_ID MilImage )
		{
			byte [] ImageData = new byte[ MILContext.Width * MILContext.Height ];

			MIL.MbufGet( MilImage, ImageData );


			// 이미지 데이터의 최대값을 2번째로 큰 값으로 변경
			// MindVision의 GF120이 받아오는 이미지의 0번째 값이 0XFF로 고정되는 현상을 방지하기 위함
			var distinctNumbersDesc = ImageData.Distinct().OrderByDescending( x => x ).ToArray();
			if (distinctNumbersDesc[ 1 ] != null)
			{
				ImageData[ 0 ] = distinctNumbersDesc[ 1 ];
			}
				

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
			byte [] ImageData = new byte[ MILContext.Width * MILContext.Height ];

			for (int i = 0; i < ImageData.Length; i++)
			{
				ImageData[ i ] = (byte) ((double) ((double) ushortImageData[ i ] / 65535) * 255);
			}


			return ImageData;
		}

		static public byte[ ] LoadImage( MIL_ID MilSystem, string filePath )
		{
			MIL_ID MilImage = MIL.M_NULL;
			MIL.MbufAllocColor( MilSystem, 1, 464, 348, 16, MIL.M_IMAGE + MIL.M_PROC, ref MilImage );
			byte[] imageData = null;
			if (File.Exists( filePath ))
			{
				int SizeByte = 0;
				MIL.MbufLoad( filePath, MilImage );

				MIL.MbufInquire( MilImage, MIL.M_SIZE_BYTE, ref SizeByte );
				imageData = new byte[ SizeByte ];

				MIL.MbufInquire( MilImage, MIL.M_SIZE_X, ref MILContext.Width );
				MIL.MbufInquire( MilImage, MIL.M_SIZE_Y, ref MILContext.Height );
				MIL.MbufInquire( MilImage, MIL.M_SIZE_BAND, ref MILContext.NbBands );
				MIL.MbufInquire( MilImage, MIL.M_TYPE, ref MILContext.DataType );


				MIL.MbufGet( MilImage, imageData );
			}
			return imageData;
		}
	}
}
