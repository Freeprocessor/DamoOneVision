using CommunityToolkit.Mvvm.Input;
using DamoOneVision.Camera;
using DamoOneVision.Data;
using DamoOneVision.Models;
using DamoOneVision.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DamoOneVision.ViewModels
{
	public class AdvancedViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged( string propertyName ) =>
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );

		private readonly CameraService _cameraService;

		private readonly JsonHandler _jsonHandler;
		private string appFolder = "";
		private string imageFolder = "";

		public ObservableCollection<string> ImagePaths { get; set; } = new();

		public InfraredInspectionResult InspectionResult
		{
			get => _inspectionResult;
			set
			{
				_inspectionResult = value;
				OnPropertyChanged( nameof( InspectionResult ) );
			}
		}
		private InfraredInspectionResult _inspectionResult = new();

		private string _selectedImage;
		public string SelectedImage
		{
			get => _selectedImage;
			set
			{
				_selectedImage = value;
				OnPropertyChanged( nameof( SelectedImage ) );
			}
		}

		private StringBuilder _logBuilder = new();
		public string LogContents
		{
			get => _logBuilder.ToString();
			private set
			{
				_logBuilder = new StringBuilder( value );
				OnPropertyChanged( nameof( LogContents ) );
			}
		}

		public ICommand LoadImagesCommand { get; }
		public ICommand ImageSelectedCommand { get; }

		public AdvancedViewModel( CameraService cameraService )
		{
			_cameraService = cameraService;

			InitLocalAppFolder();

			Logger.OnLogReceived += ( s, msg ) =>
			{
				Application.Current?.Dispatcher?.Invoke( ( ) =>
				{
					_logBuilder.AppendLine( msg );
					OnPropertyChanged( nameof( LogContents ) );
				} );
			};

			LoadImagesCommand = new AsyncRelayCommand( _ => LoadAllImages() );
			ImageSelectedCommand = new RelayCommand<string>( OnImageSelected );
		}

		private void InitLocalAppFolder( )
		{
			string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			appFolder = Path.Combine( localAppData, "DamoOneVision" );
			imageFolder = Path.Combine( appFolder, "Images", "InfraredCamera", "RAWInfraredCamera" );

			Directory.CreateDirectory( appFolder );
			Directory.CreateDirectory( imageFolder );
		}

		public async Task LoadAllImages( )
		{
			var dialog = new CommonOpenFileDialog
			{
				IsFolderPicker = true,
				Title = "이미지가 있는 폴더를 선택하세요.",
				InitialDirectory = imageFolder
			};

			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				string selectedFolder = dialog.FileName;
				ImagePaths.Clear();
				var files = Directory.GetFiles(selectedFolder, "*.bmp");
				foreach (var file in files)
					ImagePaths.Add( file );

				MessageBox.Show( $"{files.Length}개의 이미지가 로드되었습니다." );
				Logger.WriteLine( $"{files.Length}개의 이미지가 로드되었습니다." );
			}
		}

		private void OnImageSelected( string imagePath )
		{
			if (!string.IsNullOrEmpty( imagePath ) && File.Exists( imagePath ))
				_cameraService.InfraredCameraLoadImage( imagePath );
		}
	}
}
