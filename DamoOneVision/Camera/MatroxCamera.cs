using DamoOneVision.Data;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace DamoOneVision.Camera
{
	public class MatroxCamera : ICamera
	{
		// Matrox SDK 관련 필드
		private MIL_ID MilSystem = MIL.M_NULL;
		private MIL_ID MilDigitizer = MIL.M_NULL;
		private MIL_ID MilImage = MIL.M_NULL;


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
			// 이미지 버퍼 할당
			if (MilImage == MIL.M_NULL)
			{
				// 이미지 크기 및 속성 가져오기
				MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_X, ref MILContext.Width );
				MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_Y, ref MILContext.Height );
				MIL.MdigInquire( MilDigitizer, MIL.M_SIZE_BAND, ref MILContext.NbBands );
				MIL.MdigInquire( MilDigitizer, MIL.M_TYPE, ref MILContext.DataType );

				// 이미지 버퍼 할당
				MIL.MbufAlloc2d( MilSystem, MILContext.Width, MILContext.Height, MILContext.DataType, MIL.M_IMAGE + MIL.M_GRAB , ref MilImage );
			}

			// 이미지 캡처
			MIL.MdigGrab( MilDigitizer, MilImage );

			// 버퍼이미지를 Scale히여 16bit 이미지로 변환
			ushort [] ushortScaleImageData = MilImageShortScale(MilImage);
			// Scale된 이미지 데이터 Buffer에 전송
			MIL.MbufPut( MilImage, ushortScaleImageData );


			// Scale된 이미지 데이터를 byte로 변환
			// TODO : 어떻게 사용할건지 확인해야함
			byte [] byteImageData = ShortToByte(ushortScaleImageData);


			MIL_INT SizeByte = 0;
			//버퍼에 쓴 Scale 데이터를 byte로 변환
			MIL.MbufInquire( MilImage, MIL.M_SIZE_BYTE, ref SizeByte );
			byte[] imageData = new byte[SizeByte];
			
			MIL.MbufGet( MilImage, imageData );

			return imageData;
		}

		private ushort[ ] MilImageShortScale( MIL_ID MilImage )
		{
			ushort [] ImageData = new ushort[ MILContext.Width * MILContext.Height ];

			MIL.MbufGet( MilImage, ImageData );


			// 이미지 데이터의 최대값을 2번째로 큰 값으로 변경
			// MindVision의 GF120이 받아오는 이미지의 0번째 값이 0XFF로 고정되는 현상을 방지하기 위함
			var distinctNumbersDesc = ImageData.Distinct().OrderByDescending( x => x ).ToArray();
			ImageData[ 0 ] = distinctNumbersDesc[ 1 ];

			ushort MinPixelValue = ImageData.Min();
			ushort MaxPixelValue = ImageData.Max();


			for (int i = 0; i < ImageData.Length; i++)
			{
				ImageData[ i ] = (ushort) (((double) (ImageData[ i ] - MinPixelValue) / (double) (MaxPixelValue - MinPixelValue)) * 65535);
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
	}
}
