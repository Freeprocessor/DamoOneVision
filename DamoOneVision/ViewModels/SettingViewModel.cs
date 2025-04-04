using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;  // CommunityToolkit.Mvvm 또는 다른 RelayCommand 구현 사용
using DamoOneVision.Data;
using DamoOneVision.Models;

namespace DamoOneVision.ViewModels
{
	public class SettingViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged( string propertyName ) =>
			 PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );

		public ObservableCollection<InfraredCameraModel> InfraredCameraModels { get; set; } = new ObservableCollection<InfraredCameraModel>();

		private InfraredCameraModel? _selectedModel;
		public InfraredCameraModel? SelectedModel
		{
			get => _selectedModel;
			set
			{
				if (_selectedModel != value)
				{
					_selectedModel = value;
					OnPropertyChanged( nameof( SelectedModel ) );
				}
			}
		}

		public ICommand SaveCommand { get; }

		private readonly SettingManager _settingManager;

		public SettingViewModel( SettingManager settingManager )
		{
			_settingManager = settingManager;
			LoadModels();
			SaveCommand = new RelayCommand( SaveModels );
		}

		private void LoadModels( )
		{
			// Load the model data using SettingManager.
			ModelData data = _settingManager.LoadModelData();
			InfraredCameraModels.Clear();
			foreach (var model in data.InfraredCameraModels)
			{
				InfraredCameraModels.Add( model );
			}
			if (InfraredCameraModels.Any())
			{
				SelectedModel = InfraredCameraModels.First();
			}
		}

		private void SaveModels( )
		{
			// 1. 기존 파일 읽어오기
			string modelFilePath = _settingManager.GetModelFilePath();
			ModelData data = new ModelData();

			if (File.Exists( modelFilePath ))
			{
				string existingJson = File.ReadAllText(modelFilePath);
				data = JsonSerializer.Deserialize<ModelData>( existingJson );
				if (data == null)
					data = new ModelData();
			}

			// 2. 기존 데이터 중 MotionModels는 그대로 두고,
			//    InfraredCameraModels만 현재 ViewModel 값으로 업데이트
			data.InfraredCameraModels = InfraredCameraModels.ToList();

			// 3. (옵션) LastOpenedModel을 설정하거나 Settings 업데이트 등
			// _settingManager.Settings.LastOpenedModel = SelectedModel?.Name ?? "Default";

			// 4. 저장
			var options = new JsonSerializerOptions { WriteIndented = true };
			string json = JsonSerializer.Serialize(data, options);
			File.WriteAllText( modelFilePath, json );

			_settingManager.LoadSettings();
		}
	}
}
