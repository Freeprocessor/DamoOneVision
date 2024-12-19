using DamoOneVision.Camera;
using DamoOneVision.Data;
using DamoOneVision.ViewModels;
using Matrox.MatroxImagingLibrary;
using Spinnaker;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Newtonsoft.Json;
using System.IO;


namespace DamoOneVision.ViewModels
{

	public class TeachingViewModel
	{
		//Bscking Field
		private byte[ ] _processingPixelData;
		public byte[ ] ProcessingPixelData
		{
			// Get 할 시에 _pixelData를 반환
			get => _processingPixelData;
			// Set 할 시에 _pixelData에 value를 할당하고 PropertyChanged 이벤트를 호출하여 속성값이 변경되었음을 알림
			set
			{
				if (_processingPixelData != value)
				{
					//프라이빗 필드에 새값을 할당하여 실제 데이터를 업데이트, Value는 자동으로 생성되는 값, Gettersetter를 통해 전달된 값
					_processingPixelData = value;
					//인터페이스의 PropertyChanged 이벤트를 호출하여 속성값이 변경되었음을 알림
					OnPropertyChanged( nameof( ProcessingPixelData ) );
				}
			}
		}

		public byte[ ] RawPixelData;
		//private MIL_ID MilSystem = MIL.M_NULL;
		//컬랙션의 변경사항을 알리는 기능을 제공하는 네임스페이스
		public ObservableCollection<ComboBoxItemViewModel> ComboBoxItems { get; set; }

		// MVVM 패턴에서는 ICommand를 사용하여 View에서 ViewModel의 메서드를 호출
		public ICommand AddComboBoxCommand { get; set; }
		public ICommand DeleteComboBoxCommand { get; set; }
		public ICommand ConversionProcessCommand { get; set; }


		//Model 저장을 위한 ICommand 추가

		public ICommand SaveModelCommand { get; set; }
		public ICommand LoadModelCommand { get; set; }


		public TeachingViewModel( )
		{
			// MILContext에서 MilSystem 가져오기
			//MilSystem = MILContext.Instance.MilSystem;

			ComboBoxItems = new ObservableCollection<ComboBoxItemViewModel>();
			AddComboBoxCommand = new RelayCommand( AddComboBox );
			DeleteComboBoxCommand = new RelayCommand( DeleteComboBox );
			ConversionProcessCommand = new AsyncRelayCommand( ConversionProcessAsync, CanExecuteConversionProcess );

			//Model 저장을 위한 ICommand 추가
			//SaveModelCommand = new RelayCommand( SaveModel );
			//LoadModelCommand = new RelayCommand( LoadModel );

			// CollectionChanged 이벤트 핸들러 등록
			ComboBoxItems.CollectionChanged += ComboBoxItems_CollectionChanged;

			string databasePath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"DamoOneVision",
				"models.db"
			);
			Directory.CreateDirectory( Path.GetDirectoryName( databasePath ) );
		}

		//CanExcute 메서드가 지정되지 않았으므로 기본적으로 이 명령은 항상 실행이 가능함
		private void AddComboBox( object parameter )
		{
			ComboBoxItemViewModel newItem = new ComboBoxItemViewModel();
			ComboBoxItems.Add( newItem );
			// 번호 할당은 CollectionChanged 이벤트에서 처리
		}

		private void DeleteComboBox( object parameter )
		{
			if (parameter is ComboBoxItemViewModel item && ComboBoxItems.Contains( item ))
			{
				ComboBoxItems.Remove( item );
				// 번호 재할당은 CollectionChanged 이벤트에서 처리
			}
		}

		private void ComboBoxItems_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			// 모든 아이템에 대해 번호를 재할당
			for (int i = 0; i < ComboBoxItems.Count; i++)
			{
				ComboBoxItems[ i ].Number = i + 1;
			}
		}

		private bool CanExecuteConversionProcess( object parameter )
		{
			// 변환 가능 여부 결정 로직 (예: ComboBoxItems가 비어있지 않은 경우)
			return ComboBoxItems != null && ComboBoxItems.Count > 0;
		}

		private async Task ConversionProcessAsync( object parameter )
		{
			if (parameter is int targetNumber)
			{
				try
				{
					ProcessingPixelData = (byte[])RawPixelData.Clone();
					foreach (var item in ComboBoxItems)
					{
						if (item.Number > targetNumber)
							break;

						switch (item.SelectedProcessingOption)
						{ 
							//case "HSV":
							//	Conversion.RunHSLThreshold( item.HMinValue ?? 0, item.HMaxValue ?? 0, 
							//		item.SMinValue ?? 0, item.SMaxValue ?? 0, item.VMinValue ?? 0, 
							//		item.VMaxValue ?? 0, ProcessingPixelData ) ;

							//	break;
							//case "Template Matching":
							//	TemplateMatcher templateMatcher = new TemplateMatcher();
							//	//await Task.Run( ( ) => templateMatcher.FindTemplate( item.HMinValue, item.HMaxValue, item.SMinValue, item.SMaxValue, item.VMinValue, item.VMaxValue, PixelData ) );
							//	break;

							//case "Clip":
							//	// 문자열을 상수로 매핑
							//	if (ClipOptionMapping.TryGetValue( item.SelectedClipOption, out MIL_ID clipOption ))
							//	{
							//		 Conversion.RunClip(
							//			clipOption,
							//			item.LowerLimit ?? 0, item.UpperLimit ?? 0,
							//			item.WriteLow ?? 0, item.WriteHigh ?? 0,
							//			ProcessingPixelData ) ;
							//	}
							//	else
							//	{
							//		// 매핑 실패 시 예외 처리 또는 기본값 설정
							//		throw new Exception( "유효하지 않은 Clip 옵션입니다." );
							//	}
							//	break;

						}

						// 비동기 이미지 변환 함수 호출
					}
					// TODO : 처리 완료된 PixelData를 출력
					Conversion.OnImageProcessed( ProcessingPixelData );
					// 처리 완료 후 사용자에게 알림
					//MessageBox.Show( $"1~{targetNumber}번까지의 이미지 변환이 완료되었습니다.", "변환 완료", MessageBoxButton.OK, MessageBoxImage.Information );
				}
				catch (Exception ex)
				{
					// 예외 발생 시 사용자에게 알림
					MessageBox.Show( $"이미지 변환 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error );
				}
			}
		}

		//private void SaveModel( object parameter )
		//{
		//	// 모델 이름을 입력받기 위한 다이얼로그 표시
		//	string modelName = ShowInputDialog("모델 이름을 입력하세요:");
		//	if (string.IsNullOrEmpty( modelName ))
		//	{
		//		MessageBox.Show( "모델 이름이 유효하지 않습니다." );
		//		return;
		//	}

		//	// 모든 ComboBoxItems를 직렬화하여 저장
		//	string serializedData = JsonConvert.SerializeObject(ComboBoxItems.ToList());

		//	// SQLiteHelper를 사용하여 데이터베이스에 저장
		//	try
		//	{
		//		//_dbHelper.SaveModel( modelName, serializedData );
		//		MessageBox.Show( "모델이 성공적으로 저장되었습니다." );
		//	}
		//	catch (Exception ex)
		//	{
		//		MessageBox.Show( $"모델 저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error );
		//	}
		//}
		//private void LoadModel( object parameter )
		//{
		//	// 데이터베이스에서 모델 목록 가져오기
		//	List<ModelItem> modelList;
		//	try
		//	{
		//		modelList = _dbHelper.GetModelList();
		//	}
		//	catch (Exception ex)
		//	{
		//		MessageBox.Show( $"모델 목록을 가져오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error );
		//		return;
		//	}

		//	if (modelList.Count == 0)
		//	{
		//		MessageBox.Show( "저장된 모델이 없습니다." );
		//		return;
		//	}

		//	// 모델 선택을 위한 다이얼로그 표시
		//	int selectedModelId = ShowModelSelectionDialog(modelList);
		//	if (selectedModelId == -1)
		//	{
		//		// 선택 취소 또는 오류
		//		return;
		//	}

		//	// 선택된 모델의 데이터를 불러오기
		//	string serializedData;
		//	try
		//	{
		//		serializedData = _dbHelper.LoadModelData( selectedModelId );
		//	}
		//	catch (Exception ex)
		//	{
		//		MessageBox.Show( $"모델 데이터를 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error );
		//		return;
		//	}

		//	if (string.IsNullOrEmpty( serializedData ))
		//	{
		//		MessageBox.Show( "모델 데이터를 불러오지 못했습니다." );
		//		return;
		//	}

		//	// 직렬화된 데이터를 역직렬화하여 ComboBoxItems에 반영
		//	try
		//	{
		//		DeserializeModelData( serializedData );
		//		MessageBox.Show( "모델이 성공적으로 불러와졌습니다." );
		//	}
		//	catch (Exception ex)
		//	{
		//		MessageBox.Show( $"모델 데이터를 역직렬화하는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error );
		//	}
		//}

		// 직렬화된 데이터를 역직렬화하여 ComboBoxItems에 반영
		private void DeserializeModelData( string serializedData )
		{
			var items = JsonConvert.DeserializeObject<List<ComboBoxItemViewModel>>(serializedData);
			if (items != null)
			{
				// 기존 항목을 지우고 불러온 항목으로 대체
				ComboBoxItems.Clear();
				foreach (var item in items)
				{
					// 로드된 항목의 선택 상태를 유지하거나 초기화
					// item.IsSelected = false; // 선택 상태 초기화 원할 경우
					ComboBoxItems.Add( item );
				}
				// 번호 재할당
				for (int i = 0; i < ComboBoxItems.Count; i++)
				{
					ComboBoxItems[ i ].Number = i + 1;
				}
			}
		}

		// 사용자 입력 다이얼로그 표시 메서드
		//private string ShowInputDialog( string message )
		//{
		//	InputDialog inputDialog = new InputDialog(message);
		//	if (inputDialog.ShowDialog() == true)
		//	{
		//		return inputDialog.ResponseText;
		//	}
		//	return null;
		//}

		// 모델 선택 다이얼로그 표시 메서드
		//private int ShowModelSelectionDialog( List<ModelItem> modelList )
		//{
		//	ModelSelectionDialog selectionDialog = new ModelSelectionDialog(modelList);
		//	if (selectionDialog.ShowDialog() == true)
		//	{
		//		return selectionDialog.SelectedModelId;
		//	}
		//	return -1;
		//}


		private static readonly Dictionary<string, MIL_ID> ClipOptionMapping = new Dictionary<string, MIL_ID>
		{
			{ "MIL.M_IN_RANGE", MIL.M_IN_RANGE },
			{ "MIL.M_OUT_RANGE", MIL.M_OUT_RANGE },
		};

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
