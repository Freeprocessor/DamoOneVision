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

using DamoOneVision.ViewModels;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Drawing;
using System;
using DamoOneVision.Services;


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
		private CameraManager InfraredCamera;


		public ObservableCollection<string> ImagePaths { get; set; }
		private string appFolder;
		private string imageFolder;
		private string modelfolder;
		private string modelfile;



		private MIL_ID MilSystem = MIL.M_NULL;

		private MIL_ID InfraredCameraDisplay;
		private MIL_ID InfraredCameraConversionDisplay;

		private MIL_ID InfraredCameraImage;
		private MIL_ID InfraredCameraConversionImage;


		private int frameCount = 0;
		private DateTime fpsStartTime = DateTime.Now;
		private double currentFps = 0;

		private bool isContinuous = false; // Continuous 모드 상태
		private bool isCapturing = false;  // 이미지 캡처 중인지 여부

		private readonly JsonHandler _jsonHandler;
		public ObservableCollection<InfraredCameraModel> InfraredCameraModels { get; set; }

		private InfraredCameraModel currentInfraredCameraModel;

		// Setting
		SettingManager settingManager;

		public MainWindow( )
		{
			InitializeComponent();
			InitLocalAppFolder();
			InitMILSystem();
			//DATA BINDING
			this.DataContext = this;

			settingManager = new SettingManager();
			_jsonHandler = new JsonHandler( modelfile );
			InfraredCameraModels = new ObservableCollection<InfraredCameraModel>();
			LoadInfraredModelsAsync();

			// 윈도우 종료 이벤트 핸들러 추가
			this.Closing += Window_Closing;

			InfraredCamera = new CameraManager();
			//cameraManager.ImageCaptured += OnImageCaptured;

		}
		// 모델 수정 버튼 클릭 이벤트 핸들러
		private void EditModelButton_Click( object sender, RoutedEventArgs e )
		{
			if (true)
			{
				// 선택된 모델의 복사본 생성 (원본 변경을 방지)
				var modelCopy = new InfraredCameraModel
				{
					Name = currentInfraredCameraModel.Name,
					CircleCenterX = currentInfraredCameraModel.CircleCenterX,
					CircleCenterY = currentInfraredCameraModel.CircleCenterY,
					CircleMinRadius = currentInfraredCameraModel.CircleMinRadius,
					CircleMaxRadius = currentInfraredCameraModel.CircleMaxRadius,
					BinarizedThreshold = currentInfraredCameraModel.BinarizedThreshold
				};

				var saveWindow = new SettingWindow(modelCopy);
				if (saveWindow.ShowDialog() == true)
				{
					// 원본 모델 업데이트
					currentInfraredCameraModel.Name = saveWindow.Model.Name;
					currentInfraredCameraModel.CircleCenterX = saveWindow.Model.CircleCenterX;
					currentInfraredCameraModel.CircleCenterY = saveWindow.Model.CircleCenterY;
					currentInfraredCameraModel.CircleMinRadius = saveWindow.Model.CircleMinRadius;
					currentInfraredCameraModel.CircleMaxRadius = saveWindow.Model.CircleMaxRadius;
					currentInfraredCameraModel.BinarizedThreshold = saveWindow.Model.BinarizedThreshold;

					SaveInfraredModelsAsync(); // 자동 저장
				}
			}
			else
			{
				MessageBox.Show( "수정할 모델을 선택하세요.", "선택 오류", MessageBoxButton.OK, MessageBoxImage.Warning );
			}
		}

		// 모델 데이터를 JSON 파일로 저장하는 메서드
		private async Task SaveInfraredModelsAsync( )
		{
			var data = new InfraredCameraModelData { InfraredCameraModels = new List<InfraredCameraModel>(InfraredCameraModels) };
			await _jsonHandler.SaveInfraredModelsAsync( data );
		}



		// JSON 파일에서 모델 데이터를 불러오는 메서드
		private async Task LoadInfraredModelsAsync( )
		{
			var data = await _jsonHandler.LoadInfraredModelsAsync();
			InfraredCameraModels.Clear();
			foreach (var model in data.InfraredCameraModels)
			{
				InfraredCameraModels.Add( model );
			}
			currentInfraredCameraModel = InfraredCameraModels[ 0 ];
			await Task.CompletedTask;
		}


		private void InitLocalAppFolder( )
		{
			ImagePaths = new ObservableCollection<string>();
			string localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			appFolder = System.IO.Path.Combine( localAppData, "DamoOneVision" );
			imageFolder = System.IO.Path.Combine( appFolder, "Images" );
			modelfolder = System.IO.Path.Combine( appFolder, "Model" );
			modelfile = System.IO.Path.Combine( modelfolder, "Models.model" );
			if (!Directory.Exists( appFolder ))
			{
				Directory.CreateDirectory( appFolder );
			}
		}

		private void InitMILSystem()
		{
			MilSystem = MILContext.Instance.MilSystem;
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref InfraredCameraDisplay );
			MIL.MdispAlloc( MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref InfraredCameraConversionDisplay );

			MIL.MdispControl( InfraredCameraDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );
			MIL.MdispControl( InfraredCameraConversionDisplay, MIL.M_VIEW_MODE, MIL.M_DEFAULT );

			/// 컬러맵 설정은 필요에 따라 변경 가능
			MIL.MdispLut( InfraredCameraDisplay, MIL.M_COLORMAP_GRAYSCALE );

			infraredCameraDisplay.DisplayId = InfraredCameraDisplay;
			infraredCameraConversionDisplay.DisplayId = InfraredCameraConversionDisplay;

		}


		private async void ConnectButton_Click( object sender, RoutedEventArgs e )
		{
			try
			{

				InfraredCamera.Connect( "Matrox" );

				ConnectButton.IsEnabled = false;
				DisconnectButton.IsEnabled = true;

			}
			catch (Exception ex)
			{
				MessageBox.Show( $"카메라 연결 오류\n{ex.Message}" );
				await InfraredCamera.DisconnectAsync();
			}
		}

		private async void DisconnectButton_Click( object sender, RoutedEventArgs e )
		{
			await InfraredCamera.DisconnectAsync();

			if( InfraredCameraImage != MIL.M_NULL ) MIL.MbufFree( InfraredCameraImage );
			if( InfraredCameraConversionImage != MIL.M_NULL )MIL.MbufFree( InfraredCameraConversionImage );

			InfraredCameraImage = MIL.M_NULL;
			InfraredCameraConversionImage = MIL.M_NULL;

			FpsLabel.Content = "FPS: 0";

			ConnectButton.IsEnabled = true;
			DisconnectButton.IsEnabled = false;
		}
		

		
		//private void OnImageCaptured( byte[ ] pixelData )
		//{
		//	Dispatcher.Invoke( ( ) =>
		//	{
		//		DisplayImage( (byte[ ])pixelData.Clone() );

		//		// FPS 계산
		//		frameCount++;
		//		TimeSpan elapsed = DateTime.Now - fpsStartTime;

		//		if (elapsed.TotalSeconds >= 1.0)
		//		{
		//			currentFps = frameCount / elapsed.TotalSeconds;
		//			frameCount = 0;
		//			fpsStartTime = DateTime.Now;

		//			FpsLabel.Content = $"FPS: {currentFps:F2}";
		//			Debug.WriteLine( $"FPS 업데이트: {currentFps:F2}" );
		//		}
		//	} );
		//}



		private PixelFormat getPixelFormat()
		{
			PixelFormat pixelFormat;
			if (InfraredCamera.DataType() == 8 && InfraredCamera.NbBands() == 3)
			{
				pixelFormat = PixelFormats.Rgb24;
			}
			else if (InfraredCamera.DataType() == 8 && InfraredCamera.NbBands() == 1)
			{
				pixelFormat = PixelFormats.Gray8;
			}
			else if (InfraredCamera.DataType() == 16 && InfraredCamera.NbBands() == 1)
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
			await InfraredCamera.DisconnectAsync();
		}

		private void Click( object sender, RoutedEventArgs e )
		{
			MessageBox.Show( "버튼이 클릭되었습니다." );
		}

		private void ExitProgram( object sender, EventArgs e )
		{
			Application.Current.Shutdown();
		}
		//private void ContinuousMenuItem_Checked( object sender, RoutedEventArgs e )
		//{
		//	isContinuous = true;

		//	if (cameraManager.IsConnected)
		//	{
		//		cameraManager.StartContinuousCapture();
		//	}
		//}

		//private void ContinuousMenuItem_Unchecked( object sender, RoutedEventArgs e )
		//{
		//	isContinuous = false;

		//	if (cameraManager.IsConnected)
		//	{
		//		cameraManager.StopContinuousCapture();
		//	}
		//}

		private void TriggerButton_Click( object sender, RoutedEventArgs e )
		{
			if (!InfraredCamera.IsConnected && InfraredCameraImage == MIL.M_NULL)
			{
				MessageBox.Show( "카메라가 연결되어 있지 않고, 로드된 이미지도 없습니다." );
				return;
			}

			//if (isContinuous)
			//{
			//	MessageBox.Show( "Continuous 모드에서는 Trigger 기능을 사용할 수 없습니다." );
			//	return;
			//}
			if (!isCapturing)
			{
				isCapturing = true;

				try
				{
					if (InfraredCamera.IsConnected)
					{
						InfraredCameraImage = InfraredCamera.CaptureSingleImage( );
						MIL.MdispSelect( InfraredCameraDisplay, InfraredCameraImage );
					}
					else
					{
						// 로드된 이미지가 있다면 그 이미지를 사용
					}

					if (InfraredCameraImage != MIL.M_NULL)
					{
						// 여기서 pixelData에 대한 추가 처리(예: HSLThreshold 등) 호출 가능
						// 예: Conversion.RunHSLThreshold(hMin, hMax, sMin, sMax, lMin, lMax, pixelData);
						// 처리 후 다시 DisplayImage(pixelData)로 화면에 갱신할 수 있음
						bool isGood = false;

						if( InfraredCameraConversionImage == MIL.M_NULL ) MIL.MbufFree( InfraredCameraConversionImage );
						InfraredCameraConversionImage = MIL.M_NULL;


						InfraredCameraConversionImage = Conversion.InfraredCameraModel( InfraredCameraImage, ref isGood, currentInfraredCameraModel );
						MIL.MdispSelect( InfraredCameraConversionDisplay, InfraredCameraConversionImage );

						GoodLamp( isGood );
						//DisplayConversionImage( ConversionpixelData );
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
			MIL_ID TeachingImage = MIL.M_NULL;
			MIL.MbufClone( InfraredCameraImage, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref TeachingImage );
			// 템플릿 학습 윈도우 열기
			if (MIL.MbufInquire( InfraredCameraImage, MIL.M_TYPE, MIL.M_NULL ) != MIL.M_NULL )
			{
				MessageBox.Show( "이미지가 캡처되지 않았습니다." );
				return;
			}
			TeachingWindow teachingWindow = new TeachingWindow(TeachingImage, (int)InfraredCamera.Width(), (int)InfraredCamera.Height(), (int)InfraredCamera.NbBands(), (int)InfraredCamera.DataType(), getPixelFormat());
			teachingWindow.ShowDialog();

		}


		//private void LoadModelButton_Click( object sender, RoutedEventArgs e )
		//{
		//	// 데이터베이스에서 모델 리스트 가져오기
		//	List<ModelItem> modelList = dbHelper.GetModelList();

		//	// ModelSelectionDialog 생성 및 표시
		//	ModelSelectionDialog dialog = new ModelSelectionDialog(modelList);
		//	bool? result = dialog.ShowDialog();

		//	if (result == true)
		//	{
		//		int selectedModelId = dialog.SelectedModelId;
		//		if (selectedModelId != -1)
		//		{
		//			// 모델 데이터 로드
		//			string modelData = dbHelper.LoadModelData(selectedModelId);

		//			// 모델 데이터 처리
		//			LoadModel( modelData );
		//		}
		//		else
		//		{
		//			MessageBox.Show( "모델이 선택되지 않았습니다." );
		//		}
		//	}
		//}

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
						InfraredCameraImage = InfraredCamera.LoadImage( MilSystem, selectedImagePath);
						MIL.MdispSelect( InfraredCameraDisplay, InfraredCameraImage );
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
			//이미지가 있는지 확인, 없으면 만들기

			string[] files = Directory.GetFiles(imageFolder, "*.bmp");

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

			// 1. UI 요소의 DisplayId를 MIL.M_NULL로 설정하여 참조 해제
			if (infraredCameraDisplay != null)
			{
				infraredCameraDisplay.DisplayId = MIL.M_NULL;
			}

			if (infraredCameraConversionDisplay != null)
			{
				infraredCameraConversionDisplay.DisplayId = MIL.M_NULL;
			}

			// 2. disp 버퍼 해제
			if (InfraredCameraDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( InfraredCameraDisplay );
				InfraredCameraDisplay = MIL.M_NULL;
				Console.WriteLine( "InfraredCameraDisplay 해제 완료." );
			}

			if (InfraredCameraConversionDisplay != MIL.M_NULL)
			{
				MIL.MdispFree( InfraredCameraConversionDisplay );
				InfraredCameraConversionDisplay = MIL.M_NULL;
				Console.WriteLine( "InfraredCameraConversionDisplay 해제 완료." );
			}

			// 3. 이미지 버퍼 해제
			if (InfraredCameraImage != MIL.M_NULL)
			{
				MIL.MbufFree( InfraredCameraImage );
				InfraredCameraImage = MIL.M_NULL;
				Console.WriteLine( "InfraredCameraImage 해제 완료." );
			}

			if (InfraredCameraConversionImage != MIL.M_NULL)
			{
				MIL.MbufFree( InfraredCameraConversionImage );
				InfraredCameraConversionImage = MIL.M_NULL;
				Console.WriteLine( "InfraredCameraConversionImage 해제 완료." );
			}

			// 리소스 해제
			//templateMatcher?.Dispose();
			await InfraredCamera?.DisconnectAsync();

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

		//private void Show3DButton_Click( object sender, RoutedEventArgs e )
		//{
		//	// 별도의 윈도우를 띄워서 3D 표시
		//	_3DView _3dview = new _3DView( (byte[])RawPixelData.Clone());
		//	_3dview.Show();
		//}




	}
}
