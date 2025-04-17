using DamoOneVision.Camera;
using DamoOneVision.Services;
using DamoOneVision.ViewModels;
using Matrox.MatroxImagingLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace DamoOneVision.Views
{
	/// <summary>
	/// ImageCompositionWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ImageCompositionWindow : Window
	{
		private MIL_ID MilSystem = MIL.M_NULL;
		private MIL_ID MilImage1 = MIL.M_NULL;
		private MIL_ID MilImage2 = MIL.M_NULL;
		private MIL_ID MilImage = MIL.M_NULL;
		private MIL_ID MilDisplay = MIL.M_NULL;

		private ushort[ ] image1;
		private ushort[ ] image2;
		private ushort[ ] image;

		private MIL_INT width1 = 0;
		private MIL_INT height1 = 0;
		private MIL_INT nbBands1 = 0;
		private MIL_INT dataType1 = 0;

		private MIL_INT width2 = 0;
		private MIL_INT height2 = 0;
		private MIL_INT nbBands2 = 0;
		private MIL_INT dataType2 = 0;

		public ImageCompositionWindow( )
		{
			MilSystem = MILContext.Instance.MilSystem;
			InitializeComponent();
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref MilDisplay );
			CameraDisplay.DisplayId = MilDisplay;
			CameraDisplay.MouseMove += CameraDisplay_MouseMove;
		}

		private void SelectImage1_Click( object sender, RoutedEventArgs e )
		{
			if (MilImage1 != MIL.M_NULL)
			{
				MIL.MbufFree( MilImage1 );
				Logger.WriteLine( "MilImage1 해제" );
			}

			string? path = OpenImageFile();
			if (path != null)
				MIL.MbufImport( path, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MilSystem, ref MilImage1 );

			MIL.MbufInquire( MilImage1, MIL.M_SIZE_X, ref width1 );
			MIL.MbufInquire( MilImage1, MIL.M_SIZE_Y, ref height1 );
			MIL.MbufInquire( MilImage1, MIL.M_SIZE_BAND, ref nbBands1 );
			MIL.MbufInquire( MilImage1, MIL.M_TYPE, ref dataType1 );

			FileName1Text.Text = System.IO.Path.GetFileName( path ); // SelectImage1_Click 안에서

		}

		private void SelectImage2_Click( object sender, RoutedEventArgs e )
		{
			if (MilImage2 != MIL.M_NULL)
			{
				MIL.MbufFree( MilImage2 );
				Logger.WriteLine( "MilImage1 해제" );
			}

			string? path = OpenImageFile();
			if (path != null)
				MIL.MbufImport( path, MIL.M_DEFAULT, MIL.M_RESTORE + MIL.M_NO_GRAB + MIL.M_NO_COMPRESS, MilSystem, ref MilImage2 );

			MIL.MbufInquire( MilImage2, MIL.M_SIZE_X, ref width2 );
			MIL.MbufInquire( MilImage2, MIL.M_SIZE_Y, ref height2 );
			MIL.MbufInquire( MilImage2, MIL.M_SIZE_BAND, ref nbBands2 );
			MIL.MbufInquire( MilImage2, MIL.M_TYPE, ref dataType2 );

			FileName2Text.Text = System.IO.Path.GetFileName( path ); // SelectImage2_Click 안에서

		}
		private string? OpenImageFile( )
		{
			OpenFileDialog dlg = new OpenFileDialog
			{
				Filter = "Image files (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp"
			};
			return dlg.ShowDialog() == true ? dlg.FileName : null;
		}

		private void CompareImages_Click( object sender, RoutedEventArgs e )
		{

			if (MilImage != MIL.M_NULL)
			{
				MIL.MbufFree( MilImage );
				Logger.WriteLine( "MilImage 해제" );
			}

			MIL.MbufClone( MilImage1, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilImage );


			if (MilImage1 == MIL.M_NULL || MilImage2 == MIL.M_NULL)
			{
				MessageBox.Show( "두 이미지를 먼저 선택하세요." );
				return;
			}

			if (width1 != width2 || height1 != height2)
			{
				MessageBox.Show( "이미지 크기가 다릅니다." );
				return;
			}
			image1 = new ushort[ width1 * height1 ];
			image2 = new ushort[ width2 * height2 ];
			image = new ushort[ width1 * height1 ];

			MIL.MbufGet( MilImage1, image1 );
			MIL.MbufGet( MilImage2, image2 );

			int tmp;
			for (int i = 0; i < image1.Length; i++)
			{
				tmp = image1[ i ] - image2[ i ];

				image[ i ] = (ushort)Math.Abs( tmp );

			}
			MIL.MbufPut( MilImage, image );

			// 이미지 표시
			MIL.MdispSelect( MilDisplay, MilImage );
			MIL.MdispControl( MilDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( MilDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( MilDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );
			MIL.MdispControl( MilDisplay, MIL.M_MOUSE_USE, MIL.M_DISABLE );

		}

		private void SaveImage_Click( object sender, RoutedEventArgs e )
		{
			if (MilImage == MIL.M_NULL)
			{
				MessageBox.Show( "저장할 이미지가 없습니다." );
				return;
			}

			SaveFileDialog dlg = new SaveFileDialog
			{
				Filter = "BMP 파일 (*.bmp)|*.bmp",
				FileName = "diff_result.bmp"
			};

			if (dlg.ShowDialog() == true)
			{
				MIL.MbufExport( dlg.FileName, MIL.M_BMP, MilImage );
				MessageBox.Show( "이미지가 저장되었습니다." );
			}
		}

		private void CameraDisplay_MouseMove( object sender, MouseEventArgs e )
		{
			const int imageWidth = 640;
			const int imageHeight = 480;
			// 컨트롤 내 마우스 위치 가져오기
			var pos = e.GetPosition(CameraDisplay);
			double mouseX = pos.X;
			double mouseY = pos.Y;

			// 디스플레이 크기
			double displayWidth = CameraDisplay.ActualWidth;
			double displayHeight = CameraDisplay.ActualHeight;

			// 종횡비 계산
			double imageAspect = (double)imageWidth / imageHeight;
			double displayAspect = displayWidth / displayHeight;

			double scale, offsetX = 0, offsetY = 0;
			if (displayAspect > imageAspect)
			{
				scale = displayHeight / imageHeight;
				double scaledImageWidth = imageWidth * scale;
				offsetX = (displayWidth - scaledImageWidth) / 2.0;
			}
			else
			{
				scale = displayWidth / imageWidth;
				double scaledImageHeight = imageHeight * scale;
				offsetY = (displayHeight - scaledImageHeight) / 2.0;
			}

			// 실제 이미지 좌표 계산 (반올림 적용)
			int ix = (int)Math.Round((mouseX - offsetX) / scale);
			int iy = (int)Math.Round((mouseY - offsetY) / scale);

			// 경계 체크 (0 <= ix < imageWidth, 0 <= iy < imageHeight)
			if (ix < 0) ix = 0;
			else if (ix >= imageWidth) ix = imageWidth - 1;
			if (iy < 0) iy = 0;
			else if (iy >= imageHeight) iy = imageHeight - 1;

			//Logger.WriteLine( $"마우스: ({mouseX:F1}, {mouseY:F1}) => 이미지 좌표: ({ix}, {iy})" );

			// 이미지 데이터가 ushort[] 배열이라고 가정
			if (image != null && image.Length >= imageWidth * imageHeight)
			{
				int index = iy * imageWidth + ix;
				ushort pixelValue = image[index];
				//Logger.WriteLine( $"온도 값: {(double)(pixelValue-27315)/100}" );
				PixelValueText.Text = $"({ix}, {iy}) = {image[ index ]}";

			}
			else
			{
				//Logger.WriteLine( "ImageData가 null이거나 크기가 올바르지 않습니다." );
			}
		}

		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e );

			MIL.MbufFree( MilImage );
			MIL.MbufFree( MilImage1 );
			MIL.MbufFree( MilImage2 );
			MIL.MdispFree( MilDisplay );
		}


	}
}
