using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Matrox.MatroxImagingLibrary;
using System.Windows.Threading;
using System.Runtime.InteropServices;

using SpinnakerNET;
using SpinnakerNET.GenApi;

//using LiteDB;
using DamoOneVision.Camera;
using DamoOneVision.Data;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using DamoOneVision.Models;
using DamoOneVision.ViewModels;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Drawing;


namespace DamoOneVision
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	///
	public partial class MainWindow : Window
	{
		//private ICamera camera;
		//private TemplateMatcher templateMatcher;
		private CameraManager cameraManager;
		private MIL_ID MilSystem = MIL.M_NULL;
		public ObservableCollection<string> ImagePaths { get; set; }
		private string imagesFolder;

		private WriteableBitmap bitmapVision;
		private WriteableBitmap bitmapConversion;
		private byte[ ] RawPixelData;

		private int frameCount = 0;
		private DateTime fpsStartTime = DateTime.Now;
		private double currentFps = 0;

		private bool isContinuous = false; // Continuous 모드 상태
		private bool isCapturing = false;  // 이미지 캡처 중인지 여부

		// SQLiteHelper를 위한 필드 추가
		private SQLiteHelper dbHelper;


		public MainWindow( )
		{
			InitializeComponent();
			this.DataContext = this;
			ImagePaths = new ObservableCollection<string>();
			imagesFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
			MilSystem = MILContext.Instance.MilSystem;

			// 윈도우 종료 이벤트 핸들러 추가
			this.Closing += Window_Closing;

			cameraManager = new CameraManager();
			cameraManager.ImageCaptured += OnImageCaptured;

			string databasePath = System.IO.Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"DamoOneVision",
				"models.db"
			);
			Directory.CreateDirectory( System.IO.Path.GetDirectoryName( databasePath ) );
			dbHelper = new SQLiteHelper( databasePath );

			//templateMatcher = new TemplateMatcher();
		}
		private string DetectCameraModel( )
		{
			// 카메라 모델을 결정하는 로직을 구현
			// 예를 들어, 사용자 입력이나 설정 파일을 통해 결정
			// 또는 연결된 카메라를 검색하여 모델명 확인

			// 예시로 수동 설정
			return "Matrox"; // 또는 "Matrox"
							 //return "Spinnaker";
							 //return "USB";
		}
		private async void ConnectButton_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				string cameraModel = DetectCameraModel();

				cameraManager.Connect( cameraModel );

				ConnectButton.IsEnabled = false;
				DisconnectButton.IsEnabled = true;

			}
			catch (Exception ex)
			{
				MessageBox.Show( $"카메라 연결 오류\n{ex.Message}" );
				await cameraManager.DisconnectAsync();
			}
		}

		private async void DisconnectButton_Click( object sender, RoutedEventArgs e )
		{
			await cameraManager.DisconnectAsync();

			VisionImage.Source = null;
			ConversionImage.Source = null;
			bitmapVision = null;
			bitmapConversion = null;
			FpsLabel.Content = "FPS: 0";

			ConnectButton.IsEnabled = true;
			DisconnectButton.IsEnabled = false;
		}

		
		private void OnImageCaptured( byte[ ] pixelData )
		{
			Dispatcher.Invoke( ( ) =>
			{
				DisplayImage( (byte[ ])pixelData.Clone() );

				// FPS 계산
				frameCount++;
				TimeSpan elapsed = DateTime.Now - fpsStartTime;

				if (elapsed.TotalSeconds >= 1.0)
				{
					currentFps = frameCount / elapsed.TotalSeconds;
					frameCount = 0;
					fpsStartTime = DateTime.Now;

					FpsLabel.Content = $"FPS: {currentFps:F2}";
					Debug.WriteLine( $"FPS 업데이트: {currentFps:F2}" );
				}
			} );
		}


		private void DisplayImage( byte[ ] pixelData )
		{
			int width = (int)MILContext.Width;
			int height = (int)MILContext.Height;
			int bytesPerPixel = (int)MILContext.DataType*(int)MILContext.NbBands / 8;

			if (bitmapVision == null || bitmapVision.PixelWidth != width || bitmapVision.PixelHeight != height )
			{
				PixelFormat pixelFormat = getPixelFormat();
				//pixelFormat = PixelFormats.Rgb24;
				bitmapVision = new WriteableBitmap( width, height, 96, 96, pixelFormat, null );
				VisionImage.Source = bitmapVision;
			}

			bitmapVision.Lock();
			try
			{
				// 스트라이드 계산
				int stride = width * bytesPerPixel;

				// 픽셀 데이터를 WriteableBitmap에 쓰기
				bitmapVision.WritePixels( new Int32Rect( 0, 0, width, height ), (byte[ ]) pixelData.Clone(), stride, 0 );
				this.RawPixelData = pixelData;
			}
			finally
			{
				bitmapVision.Unlock();
			}
		}

		private void DisplayConversionImage( byte[ ] pixelData )
		{
			int width = (int)MILContext.Width;
			int height = (int)MILContext.Height;
			int bytesPerPixel = 8*(int)MILContext.NbBands / 8;

			if (bitmapConversion == null || bitmapConversion.PixelWidth != width || bitmapConversion.PixelHeight != height)
			{
				PixelFormat pixelFormat = PixelFormats.Gray8;
				//pixelFormat = PixelFormats.Rgb24;
				bitmapConversion = new WriteableBitmap( width, height, 96, 96, pixelFormat, null );
				ConversionImage.Source = bitmapConversion;
			}

			bitmapConversion.Lock();
			try
			{
				// 스트라이드 계산
				int stride = width * bytesPerPixel;

				// 픽셀 데이터를 WriteableBitmap에 쓰기
				bitmapConversion.WritePixels( new Int32Rect( 0, 0, width, height ), (byte[ ]) pixelData.Clone(), stride, 0 );
				//this.RawPixelData = pixelData;
			}
			finally
			{
				bitmapConversion.Unlock();
			}
		}


		private static PixelFormat getPixelFormat()
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

		private async void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			await cameraManager.DisconnectAsync();
		}

		private void Click( object sender, RoutedEventArgs e )
		{
			MessageBox.Show( "버튼이 클릭되었습니다." );
		}

		private void ExitProgram( object sender, EventArgs e )
		{
			Application.Current.Shutdown();
		}
		private void ContinuousMenuItem_Checked( object sender, RoutedEventArgs e )
		{
			isContinuous = true;

			if (cameraManager.IsConnected)
			{
				cameraManager.StartContinuousCapture();
			}
		}

		private void ContinuousMenuItem_Unchecked( object sender, RoutedEventArgs e )
		{
			isContinuous = false;

			if (cameraManager.IsConnected)
			{
				cameraManager.StopContinuousCapture();
			}
		}

		private async void TriggerButton_Click( object sender, RoutedEventArgs e )
		{
			if (!cameraManager.IsConnected && this.RawPixelData == null)
			{
				MessageBox.Show( "카메라가 연결되어 있지 않고, 로드된 이미지도 없습니다." );
				return;
			}

			if (isContinuous)
			{
				MessageBox.Show( "Continuous 모드에서는 Trigger 기능을 사용할 수 없습니다." );
				return;
			}
			if (!isCapturing)
			{
				isCapturing = true;

				try
				{
					byte[] pixelData = null;

					// 카메라 연결 상태에 따라 캡처 또는 로드된 이미지 사용
					if (cameraManager.IsConnected)
					{
						pixelData = await cameraManager.CaptureSingleImageAsync();
						DisplayImage( pixelData );
					}
					else
					{
						// 로드된 이미지가 있다면 그 이미지를 사용
						pixelData = this.RawPixelData;
					}

					if (pixelData != null)
					{
						// 여기서 pixelData에 대한 추가 처리(예: HSLThreshold 등) 호출 가능
						// 예: Conversion.RunHSLThreshold(hMin, hMax, sMin, sMax, lMin, lMax, pixelData);
						// 처리 후 다시 DisplayImage(pixelData)로 화면에 갱신할 수 있음
						bool isGood = false;
						byte[] ConversionpixelData = (byte[])pixelData.Clone();
						Conversion.Model1( ConversionpixelData, ref isGood );

						GoodLamp( isGood );
						DisplayConversionImage( ConversionpixelData );
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show( $"이미지 처리 중 오류 발생: {ex.Message}" );
				}
				finally
				{
					isCapturing = false;
				}
			}
		}
		
		private void TeachingButton_Click( object sender, RoutedEventArgs e )
		{
			// 템플릿 학습 윈도우 열기
			if (this.RawPixelData == null)
			{
				MessageBox.Show( "이미지가 캡처되지 않았습니다." );
				return;
			}
			TeachingWindow teachingWindow = new TeachingWindow((byte[])this.RawPixelData.Clone(), (int)MILContext.Width, (int)MILContext.Height, getPixelFormat());
			teachingWindow.ShowDialog();

		}


		private void LoadModelButton_Click( object sender, RoutedEventArgs e )
		{
			// 데이터베이스에서 모델 리스트 가져오기
			List<ModelItem> modelList = dbHelper.GetModelList();

			// ModelSelectionDialog 생성 및 표시
			ModelSelectionDialog dialog = new ModelSelectionDialog(modelList);
			bool? result = dialog.ShowDialog();

			if (result == true)
			{
				int selectedModelId = dialog.SelectedModelId;
				if (selectedModelId != -1)
				{
					// 모델 데이터 로드
					string modelData = dbHelper.LoadModelData(selectedModelId);

					// 모델 데이터 처리
					LoadModel( modelData );
				}
				else
				{
					MessageBox.Show( "모델이 선택되지 않았습니다." );
				}
			}
		}

		private void LoadModel( string modelData )
		{
			// 여기에 모델 로딩 로직을 구현하세요
			DeserializeModelData( modelData );
			MessageBox.Show( "모델이 로드되었습니다: " + modelData );

			// 예를 들어, modelData를 역직렬화하여 애플리케이션의 상태나 설정에 적용할 수 있습니다.
		}

		private void ListBox_SelectionChanged( object sender, System.Windows.Controls.SelectionChangedEventArgs e )
		{
			if (e.AddedItems.Count > 0)
			{
				string selectedImagePath = e.AddedItems[0] as string;
				if (!string.IsNullOrEmpty( selectedImagePath ) && File.Exists( selectedImagePath ))
				{
					// 선택된 이미지를 VisionImage에 표시
					try
					{
						byte [] pixelData = null;

						pixelData = MatroxCamera.LoadImage( MilSystem, selectedImagePath );

						// RawPixelData에 추출한 픽셀 데이터 저장
						this.RawPixelData = pixelData;
						DisplayImage( pixelData );
					}
					catch (Exception ex)
					{
						MessageBox.Show( $"이미지를 불러오는 중 오류 발생: {ex.Message}" );
					}
				}
			}
		}



		private void LoadAllTriggeredImagesButton_Click( object sender, RoutedEventArgs e )
		{
			// Images 폴더 내의 모든 BMP 파일 로드
			ImagePaths.Clear(); // 기존 리스트 비우기(원하는 경우 생략)
			string[] files = Directory.GetFiles(imagesFolder, "*.bmp");

			foreach (var file in files)
			{
				ImagePaths.Add( file );
			}

			MessageBox.Show( $"{files.Length}개의 이미지가 로드되었습니다." );
		}

		private void DeserializeModelData( string serializedData )
		{
			var items = JsonConvert.DeserializeObject<List<ComboBoxItemViewModel>>(serializedData);
		}

		protected async override void OnClosed( EventArgs e )
		{
			base.OnClosed( e );

			// 리소스 해제
			//templateMatcher?.Dispose();
			await cameraManager?.DisconnectAsync();

			// MILContext 해제
			MILContext.Instance.Dispose();
		}

		private void GoodLamp( bool isGood )
		{
			if (!isGood)
			{
				GoodRejectLamp.Background = System.Windows.Media.Brushes.Red;
				GoodRejectText.Content = "Reject";
			}
			else
			{
				GoodRejectLamp.Background = System.Windows.Media.Brushes.Green;
				GoodRejectText.Content = "Good";
			}
		}

		private void Show3DButton_Click( object sender, RoutedEventArgs e )
		{
			// 별도의 윈도우를 띄워서 3D 표시
			_3DView _3dview = new _3DView( (byte[])RawPixelData.Clone());
			_3dview.Show();
		}

	}
}
