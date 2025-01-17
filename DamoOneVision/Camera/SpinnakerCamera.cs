using DamoOneVision.Data;
using Matrox.MatroxImagingLibrary;
using SpinnakerNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace DamoOneVision.Camera
{
	public class SpinnakerCamera : ICamera
	{
		private MIL_ID MilSystem = MIL.M_NULL;
		private MIL_ID MilImage = MIL.M_NULL;
		private IManagedCamera camera = null;
		private ManagedSystem system = null;

		string appfolder="";
		string imagesFolder="";
		string DCFFolder="";
		string DCFFilePath = "";

		public MIL_INT Width { get; set; }

		public MIL_INT Height { get; set; }

		public MIL_INT NbBands { get; set; }

		public MIL_INT DataType { get; set; }

		public string CameraName { get; set; }

		public SpinnakerCamera( string CameraName )
		{
			InitImageSave();
			MilSystem = MILContext.Instance.MilSystem;
			this.CameraName = CameraName;
			Logger.WriteLine( $"{CameraName} Spinnaker 카메라 연결" );
		}
		private void InitImageSave( )
		{
			// 'Images' 폴더 경로 설정
			string localappdata = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData);
			appfolder = System.IO.Path.Combine( localappdata, "DamoOneVision" );
			imagesFolder = System.IO.Path.Combine( appfolder, "Images" );

			// 폴더가 없으면 생성
			if (!Directory.Exists( imagesFolder ))
			{
				Directory.CreateDirectory( imagesFolder );
			}

		}

		public bool Connect( )
		{
			try
			{
				system = new ManagedSystem();
				IList<IManagedCamera> camList = system.GetCameras();

				if (camList.Count > 0)
				{
					camera = camList[ 0 ];
					camera.Init();


					camera.AcquisitionFrameRate.Value = 30.0;

					//return true;
				}
				else
				{
					MessageBox.Show( "Spinnaker 카메라를 찾을 수 없습니다." );
					return false;
				}

				if(MilImage == MIL.M_NULL)
				{
					MIL_INT width = (int) camera.Width.Value;
					MIL_INT height = (int) camera.Height.Value;
					MIL_INT nbBands = 1;
					MIL_INT dataType = 16;

					MIL.MbufAllocColor( MilSystem, nbBands, width, height, dataType, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP, ref MilImage );
				}

				Logger.WriteLine( $"{CameraName} Spinnaker 카메라 연결 성공" );

				return true;
			}
			catch (Exception ex)
			{
				// 예외 처리
				MessageBox.Show( $"Spinnaker 카메라 연결 오류: {ex.Message}" );
				return false;
			}
		}

		public void Disconnect( )
		{
			try
			{
				if (camera != null)
				{
					camera.DeInit();
					camera.Dispose();
					camera = null;
				}
				if (system != null)
				{
					system.Dispose();
					system = null;
				}
			}
			catch (Exception ex)
			{
				// 예외 처리
				MessageBox.Show( $"Spinnaker 카메라 연결 종료 오류: {ex.Message}" );
			}
		}

		private async void SaveImage( MIL_ID MilImage, string name )
		{
			await Task.Run( ( ) =>
			{
				// 현재 시간과 날짜 가져오기
				string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				// 파일 이름 생성
				string fileName = $"{name}_{timeStamp}.bmp";
				// 전체 파일 경로
				string filePath = System.IO.Path.Combine( imagesFolder, fileName );
				//SaveImage( imageData, filePath );
				MIL.MbufSave( filePath, MilImage );
			} );

		}

		public MIL_ID CaptureImage( )
		{
			try
			{
				camera.BeginAcquisition();

				IManagedImage rawImage = camera.GetNextImage();

				if (rawImage.IsIncomplete)
				{
					// 불완전한 이미지 처리
					MessageBox.Show( "Spinnaker 이미지가 불완전합니다." );
					rawImage.Release();
					camera.EndAcquisition();
					return MIL.M_NULL;
				}
				else
				{
					int bufferSize = (int)(rawImage.Width * rawImage.Height * 3);
					byte[] pixelData = new byte[bufferSize];

					// 이미지 데이터 복사
					Marshal.Copy( rawImage.DataPtr, pixelData, 0, bufferSize );

					rawImage.Release();
					camera.EndAcquisition();

					MIL.MbufPut( MilImage, pixelData );

					return MilImage;
				}
			}
			catch (Exception ex)
			{
				// 예외 처리
				MessageBox.Show( $"Spinnaker 이미지 획득 오류: {ex.Message}" );
				return MIL.M_NULL;
			}
		}

		public MIL_ID ReciveImage( )
		{
			return MilImage;
		}

		public MIL_ID LoadImage( MIL_ID MilSystem, string filePath )
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
			return MilImage;
		}


	}
}
