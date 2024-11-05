using SpinnakerNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DamoOneVision.Camera
{
	public class SpinnakerCamera : ICamera
	{
		private IManagedCamera camera = null;
		private ManagedSystem system = null;
		private int width = 0;
		private int height = 0;

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

					// 이미지 크기 가져오기
					width = (int) camera.Width.Value;
					height = (int) camera.Height.Value;

					return true;
				}
				else
				{
					MessageBox.Show( "Spinnaker 카메라를 찾을 수 없습니다." );
					return false;
				}
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

		public byte[ ] CaptureImage( )
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
					return null;
				}
				else
				{
					int bufferSize = (int)(rawImage.Width * rawImage.Height);
					byte[] pixelData = new byte[bufferSize];

					// 이미지 데이터 복사
					Marshal.Copy( rawImage.DataPtr, pixelData, 0, bufferSize );

					rawImage.Release();
					camera.EndAcquisition();

					return pixelData;
				}
			}
			catch (Exception ex)
			{
				// 예외 처리
				MessageBox.Show( $"Spinnaker 이미지 획득 오류: {ex.Message}" );
				return null;
			}
		}

		public int GetWidth( )
		{
			return width;
		}

		public int GetHeight( )
		{
			return height;
		}
	}
}
