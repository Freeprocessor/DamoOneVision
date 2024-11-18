using DamoOneVision.ViewModels;
using Matrox.MatroxImagingLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using static DamoOneVision.Camera.Conversion;


namespace DamoOneVision
{
	/// <summary>
	/// TeachingWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class TeachingWindow : Window
	{
		private byte[ ] pixelData;
		private WriteableBitmap bitmap;
		private int width;
		private int height;
		private PixelFormat pixelFormat;
		private MainViewModel _viewModel;

		private Camera.Conversion _conversion;

		public TeachingWindow( byte[ ] LocalPixelData, int width, int height, PixelFormat pixelFormat )
		{
			InitializeComponent();
			_viewModel = DataContext as MainViewModel;

			_viewModel.PixelData = LoadPixelData();

			_conversion = new Camera.Conversion();
			_conversion.ImageProcessed += Conversion_ImageProcessed;

			ConversionImageDisplay( LocalPixelData, width, height, pixelFormat );

		}

		private void Conversion_ImageProcessed( object sender, ImageProcessedEventArgs e )
		{
			// UI 스레드에서 실행되도록 Dispatcher 사용
			Dispatcher.Invoke( ( ) =>
			{
				ConversionImageDisplay( e.ProcessedPixelData, e.Width, e.Height, e.PixelFormat );
			} );
		}

		private byte[ ] LoadPixelData( )
		{
			return this.pixelData;
		}
		// 숫자만 입력할 수 있도록 하는 이벤트 핸들러
		private void NumberValidationTextBox( object sender, TextCompositionEventArgs e )
		{
			Regex regex = new Regex("[^0-9]+"); // 숫자가 아닌 문자 패턴
			e.Handled = regex.IsMatch( e.Text );
		}

		private void ConversionImageDisplay( byte[ ] LocalPixelData, int width, int height, PixelFormat pixelFormat )
		{
			this.pixelData = LocalPixelData;
			int bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;

			if (bitmap == null || bitmap.PixelWidth != width || bitmap.PixelHeight != height || bitmap.Format != pixelFormat)
			{
				bitmap = new WriteableBitmap( width, height, 96, 96, pixelFormat, null );
				ConversionImage.Source = bitmap;
			}

			bitmap.Lock();
			try
			{
				// 스트라이드 계산
				//int stride = width * bytesPerPixel;
				int stride = width * bytesPerPixel;

				// 픽셀 데이터를 WriteableBitmap에 쓰기
				bitmap.WritePixels( new Int32Rect( 0, 0, width, height ), this.pixelData, stride, 0 );
			}
			finally
			{
				bitmap.Unlock();
			}
		}

	}
}
