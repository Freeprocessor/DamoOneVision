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

		private AdornerLayer adornerLayer;
		private SelectionAdorner selectionAdorner;
		private System.Windows.Point startPoint;
		private bool isDragging = false;


		public TeachingWindow( byte[ ] LocalPixelData, int width, int height, PixelFormat pixelFormat )
		{
			InitializeComponent();
			_viewModel = DataContext as TeachingViewModel;

			_viewModel.RawPixelData = (byte[ ]) LocalPixelData.Clone();

			this.pixelData = (byte[ ])LocalPixelData.Clone();

			Conversion.ImageProcessed += Conversion_ImageProcessed;
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

		private void ConversionImage_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			if (ConversionImage.Source != null)
			{
				// 마우스 캡처
				ConversionImage.CaptureMouse();

				// 시작 점 저장
				startPoint = e.GetPosition( ConversionImage );

				// AdornerLayer 가져오기
				if (adornerLayer == null)
				{
					adornerLayer = AdornerLayer.GetAdornerLayer( ConversionImage );
				}

				// 기존의 선택 영역 제거
				if (selectionAdorner != null)
				{
					adornerLayer.Remove( selectionAdorner );
					selectionAdorner = null;
				}

				// SelectionAdorner 생성 및 추가
				selectionAdorner = new SelectionAdorner( ConversionImage );
				selectionAdorner.SelectionRectangle = new Rect( startPoint, new System.Windows.Size( 0, 0 ) );
				adornerLayer.Add( selectionAdorner );

				isDragging = true;
			}
		}

		private void ConversionImage_MouseMove( object sender, MouseEventArgs e )
		{
			if (isDragging && selectionAdorner != null)
			{
				// 현재 위치 가져오기
				var pos = e.GetPosition(ConversionImage);

				// 선택 영역 계산
				double x = Math.Min(pos.X, startPoint.X);
				double y = Math.Min(pos.Y, startPoint.Y);
				double width = Math.Abs(pos.X - startPoint.X);
				double height = Math.Abs(pos.Y - startPoint.Y);

				// 선택 영역 설정
				selectionAdorner.SelectionRectangle = new Rect( x, y, width, height );

				// Adorner 업데이트
				selectionAdorner.InvalidateVisual();
			}
		}

		private void ConversionImage_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
			if (isDragging)
			{
				isDragging = false;
				ConversionImage.ReleaseMouseCapture();

				// 드래그 종료 후 추가 동작 없음 (이미지 저장은 별도의 버튼에서 처리)
			}
		}

		// 키보드 이벤트 핸들러
		private void Window_KeyDown( object sender, KeyEventArgs e )
		{
			if (selectionAdorner != null)
			{
				double moveStep = 1; // 이동 또는 크기 조정 단위 픽셀 수

				if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
				{
					// Shift 키가 눌린 상태 - 선택 영역 크기 조정
					switch (e.Key)
					{
						case Key.Left:
							// 오른쪽 가장자리 조정 (너비 감소)
							selectionAdorner.Resize( -moveStep, 0 );
							break;
						case Key.Right:
							// 오른쪽 가장자리 조정 (너비 증가)
							selectionAdorner.Resize( moveStep, 0 );
							break;
						case Key.Up:
							// 아래쪽 가장자리 조정 (높이 감소)
							selectionAdorner.Resize( 0, -moveStep );
							break;
						case Key.Down:
							// 아래쪽 가장자리 조정 (높이 증가)
							selectionAdorner.Resize( 0, moveStep );
							break;
					}
				}
				else
				{
					// Shift 키가 눌리지 않은 상태 - 선택 영역 이동
					switch (e.Key)
					{
						case Key.Left:
							selectionAdorner.Move( -moveStep, 0 );
							break;
						case Key.Right:
							selectionAdorner.Move( moveStep, 0 );
							break;
						case Key.Up:
							selectionAdorner.Move( 0, -moveStep );
							break;
						case Key.Down:
							selectionAdorner.Move( 0, moveStep );
							break;
					}
				}

				// 선택 영역이 이미지 경계를 벗어나지 않도록 제한
				EnsureSelectionWithinBounds();

				// 선택 영역 업데이트
				selectionAdorner.InvalidateArrange();
				selectionAdorner.InvalidateVisual();
			}
		}



		private void EnsureSelectionWithinBounds( )
		{
			if (selectionAdorner != null)
			{
				var rect = selectionAdorner.SelectionRectangle;

				// 이미지의 실제 크기
				double maxX = ConversionImage.ActualWidth;
				double maxY = ConversionImage.ActualHeight;

				// 너비와 높이가 이미지 경계를 넘지 않도록 조정
				if (rect.X + rect.Width > maxX)
				{
					rect.Width = maxX - rect.X;
				}
				if (rect.Y + rect.Height > maxY)
				{
					rect.Height = maxY - rect.Y;
				}

				// 너비와 높이가 최소 크기 이상인지 확인
				if (rect.Width < 10)
				{
					rect.Width = 10;
				}
				if (rect.Height < 10)
				{
					rect.Height = 10;
				}

				selectionAdorner.SelectionRectangle = rect;

				// 시각적 업데이트
				selectionAdorner.InvalidateArrange();
				selectionAdorner.InvalidateVisual();
			}
		}



		private void CropAndSaveImage( )
		{
			var imageSource = ConversionImage.Source as BitmapSource;
			if (imageSource != null)
			{
				// 이미지의 실제 크기와 컨트롤의 크기 비율 계산
				var scaleX = imageSource.PixelWidth / ConversionImage.ActualWidth;
				var scaleY = imageSource.PixelHeight / ConversionImage.ActualHeight;

				// 선택 영역의 좌표를 이미지의 픽셀 좌표로 변환
				var rect = selectionAdorner.SelectionRectangle;

				int pixelX = (int)(rect.X * scaleX);
				int pixelY = (int)(rect.Y * scaleY);
				int pixelWidth = (int)(rect.Width * scaleX);
				int pixelHeight = (int)(rect.Height * scaleY);

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




	}
}
