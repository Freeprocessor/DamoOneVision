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


		private System.Windows.Point startPoint;
		private bool isDragging = false;


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

		private void ConversionImage_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			if (ConversionImage.Source != null)
			{
				startPoint = e.GetPosition( ConversionImage );

				// 선택 영역 초기화
				SelectionRectangle.Visibility = Visibility.Visible;
				SelectionRectangle.Width = 0;
				SelectionRectangle.Height = 0;

				isDragging = true;
				ConversionImage.CaptureMouse();
			}
		}

		private void ConversionImage_MouseMove( object sender, MouseEventArgs e )
		{
			if (isDragging)
			{
				var pos = e.GetPosition(ConversionImage);

				double x = Math.Min(pos.X, startPoint.X);
				double y = Math.Min(pos.Y, startPoint.Y);
				double width = Math.Abs(pos.X - startPoint.X);
				double height = Math.Abs(pos.Y - startPoint.Y);

				// 선택 영역 업데이트
				SelectionRectangle.Margin = new Thickness( x, y, 0, 0 );
				SelectionRectangle.Width = width;
				SelectionRectangle.Height = height;
			}
		}

		private void ConversionImage_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
			if (isDragging)
			{
				isDragging = false;
				ConversionImage.ReleaseMouseCapture();

				var endPoint = e.GetPosition(ConversionImage);

				var imageSource = ConversionImage.Source as BitmapSource;
				if (imageSource != null)
				{
					// 이미지의 실제 렌더링된 크기 얻기
					var bitmapWidth = imageSource.PixelWidth;
					var bitmapHeight = imageSource.PixelHeight;

					// 이미지 컨트롤의 크기 얻기
					var controlWidth = ConversionImage.ActualWidth;
					var controlHeight = ConversionImage.ActualHeight;

					// 이미지의 렌더링된 크기 계산
					var scale = Math.Min(controlWidth / bitmapWidth, controlHeight / bitmapHeight);
					var renderWidth = bitmapWidth * scale;
					var renderHeight = bitmapHeight * scale;

					// 이미지가 컨트롤 내에서 렌더링되는 위치 계산
					var offsetX = (controlWidth - renderWidth) / 2;
					var offsetY = (controlHeight - renderHeight) / 2;

					// 마우스 좌표를 이미지 좌표로 변환
					var x1 = startPoint.X - offsetX;
					var y1 = startPoint.Y - offsetY;
					var x2 = endPoint.X - offsetX;
					var y2 = endPoint.Y - offsetY;

					// 스케일을 고려하여 이미지 픽셀 좌표로 변환
					int pixelX1 = (int)(x1 / scale);
					int pixelY1 = (int)(y1 / scale);
					int pixelX2 = (int)(x2 / scale);
					int pixelY2 = (int)(y2 / scale);

					// 좌표 정렬
					int pixelX = Math.Min(pixelX1, pixelX2);
					int pixelY = Math.Min(pixelY1, pixelY2);
					int pixelWidth = Math.Abs(pixelX1 - pixelX2);
					int pixelHeight = Math.Abs(pixelY1 - pixelY2);

					// 이미지 경계를 벗어나지 않도록 좌표 조정
					pixelX = Math.Max( 0, Math.Min( imageSource.PixelWidth - 1, pixelX ) );
					pixelY = Math.Max( 0, Math.Min( imageSource.PixelHeight - 1, pixelY ) );
					if (pixelX + pixelWidth > imageSource.PixelWidth)
						pixelWidth = imageSource.PixelWidth - pixelX;
					if (pixelY + pixelHeight > imageSource.PixelHeight)
						pixelHeight = imageSource.PixelHeight - pixelY;

					// 크기가 유효한지 확인
					if (pixelWidth > 0 && pixelHeight > 0)
					{
						// 크롭 영역 정의
						Int32Rect cropRect = new Int32Rect(pixelX, pixelY, pixelWidth, pixelHeight);

						// 크롭된 이미지 생성
						CroppedBitmap croppedBitmap = new CroppedBitmap(imageSource, cropRect);

						// 크롭된 이미지 저장
						SaveCroppedImage( croppedBitmap );
					}
					else
					{
						MessageBox.Show( "유효한 영역을 선택하세요." );
					}
				}

				// 선택 영역 숨기기
				SelectionRectangle.Visibility = Visibility.Collapsed;
			}
		}


		private void SaveCroppedImage( BitmapSource croppedBitmap )
		{
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.FileName = "CroppedImage";
			dlg.DefaultExt = ".png";
			dlg.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpg)|*.jpg|All Files (*.*)|*.*";

			Nullable<bool> result = dlg.ShowDialog();

			if (result == true)
			{
				string filename = dlg.FileName;

				BitmapEncoder encoder = null;

				if (filename.EndsWith( ".png" ))
				{
					encoder = new PngBitmapEncoder();
				}
				else if (filename.EndsWith( ".jpg" ) || filename.EndsWith( ".jpeg" ))
				{
					encoder = new JpegBitmapEncoder();
				}
				else
				{
					encoder = new BmpBitmapEncoder();
				}

				encoder.Frames.Add( BitmapFrame.Create( croppedBitmap ) );

				using (var stream = new System.IO.FileStream( filename, System.IO.FileMode.Create ))
				{
					encoder.Save( stream );
				}
			}
		}


		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e );
			Conversion.ImageProcessed -= Conversion_ImageProcessed;
		}

	}
}
