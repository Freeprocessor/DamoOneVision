using DamoOneVision.Camera;
using DamoOneVision.ViewModels;
using Matrox.MatroxImagingLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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


namespace DamoOneVision
{
	/// <summary>
	/// TeachingWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class TeachingWindow : Window
	{

		private byte[ ] pixelData;
		private WriteableBitmap bitmap;
		private TeachingViewModel _viewModel;


		public TeachingWindow( byte[ ] LocalPixelData, int width, int height, PixelFormat pixelFormat )
		{
			InitializeComponent();
			_viewModel = DataContext as TeachingViewModel;

			_viewModel.RawPixelData = (byte[ ]) LocalPixelData.Clone();

			this.pixelData = (byte[ ])LocalPixelData.Clone();

			Conversion.ImageProcessed += Conversion_ImageProcessed;
			//_conversion.RunHSVThreshold( 0.0, 180.0, 0.0, 255.0, 0.0, 255.0, this.pixelData );
			ConversionImageDisplay( LocalPixelData );

		}

		private void Conversion_ImageProcessed( object sender, ImageProcessedEventArgs e )
		{
			try
			{
				// UI 스레드에서 실행되도록 Dispatcher 사용
				Dispatcher.Invoke( ( ) =>
				{
					ConversionImageDisplay( e.ProcessedPixelData );
				} );
			}
			catch (Exception ex)
			{
				// 예외 발생 시 로그 출력
				Debug.WriteLine( $"Exception in Conversion_ImageProcessed: {ex.Message}" );
			}
		}

		private void ConversionImageDisplay( byte[ ] LocalPixelData )
		{
			this.pixelData = LocalPixelData;
			int bytesPerPixel = (int)MILContext.DataType * (int)MILContext.NbBands / 8;

			if (bitmap == null || bitmap.PixelWidth != MILContext.Width || bitmap.PixelHeight != MILContext.Height )
			{
				bitmap = new WriteableBitmap( (int)MILContext.Width, (int)MILContext.Height, 96, 96, getPixelFormat(), null );
				ConversionImage.Source = bitmap;
			}

			bitmap.Lock();
			try
			{
				// 스트라이드 계산
				//int stride = width * bytesPerPixel;
				int stride = (int)MILContext.Width * bytesPerPixel ;

				// 픽셀 데이터를 WriteableBitmap에 쓰기
				bitmap.WritePixels( new Int32Rect( 0, 0, (int) MILContext.Width, (int) MILContext.Height ), this.pixelData, stride, 0 );
			}
			catch (Exception ex)
			{
				// 예외 발생 시 로그 출력
				Debug.WriteLine( $"Exception in ConversionImageDisplay: {ex.Message}" );
			}
			finally
			{
				bitmap.Unlock();
			}
		}

		private static PixelFormat getPixelFormat( )
		{
			PixelFormat pixelFormat;
			if (MILContext.DataType == 8 && MILContext.NbBands == 3)
			{
				pixelFormat = PixelFormats.Rgb24;
			}
			else if (MILContext.DataType == 8 && MILContext.NbBands == 1)
			{
				pixelFormat = PixelFormats.Gray8;
			}
			else if (MILContext.DataType == 16 && MILContext.NbBands == 1)
			{
				pixelFormat = PixelFormats.Gray16;
			}
			else
			{
				pixelFormat = PixelFormats.Default;
			}


			return pixelFormat;
		}

		//private byte[ ] LoadPixelData( )
		//{
		//	return this.RawPixelData;
		//}
		// 숫자만 입력할 수 있도록 하는 이벤트 핸들러
		private void NumberValidationTextBox( object sender, TextCompositionEventArgs e )
		{
			Regex regex = new Regex("[^0-9]+"); // 숫자가 아닌 문자 패턴
			e.Handled = regex.IsMatch( e.Text );
		}

		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e );
			Conversion.ImageProcessed -= Conversion_ImageProcessed;
		}

	}
}
