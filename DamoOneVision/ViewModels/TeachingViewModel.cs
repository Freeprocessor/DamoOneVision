using DamoOneVision.Camera;
using DamoOneVision.ViewModels;
using Matrox.MatroxImagingLibrary;
using Spinnaker;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace DamoOneVision.ViewModels
{

	public class TeachingViewModel
	{
		//Bscking Field
		private byte[ ] _pixelData;
		public byte[ ] PixelData
		{
			// Get 할 시에 _pixelData를 반환
			get => _pixelData;
			// Set 할 시에 _pixelData에 value를 할당하고 PropertyChanged 이벤트를 호출하여 속성값이 변경되었음을 알림
			set
			{
				if (_pixelData != value)
				{
					//프라이빗 필드에 새값을 할당하여 실제 데이터를 업데이트, Value는 자동으로 생성되는 값, Gettersetter를 통해 전달된 값
					_pixelData = value;
					//인터페이스의 PropertyChanged 이벤트를 호출하여 속성값이 변경되었음을 알림
					OnPropertyChanged( nameof( PixelData ) );
				}
			}
		}
		//private MIL_ID MilSystem = MIL.M_NULL;
		//컬랙션의 변경사항을 알리는 기능을 제공하는 네임스페이스
		public ObservableCollection<ComboBoxItemViewModel> ComboBoxItems { get; set; }

		// MVVM 패턴에서는 ICommand를 사용하여 View에서 ViewModel의 메서드를 호출
		public ICommand AddComboBoxCommand { get; set; }
		public ICommand DeleteComboBoxCommand { get; set; }
		public ICommand ConversionProcessCommand { get; set; }

		public TeachingViewModel( )
		{
			// MILContext에서 MilSystem 가져오기
			//MilSystem = MILContext.Instance.MilSystem;

			ComboBoxItems = new ObservableCollection<ComboBoxItemViewModel>();
			AddComboBoxCommand = new RelayCommand( AddComboBox );
			DeleteComboBoxCommand = new RelayCommand( DeleteComboBox );
			ConversionProcessCommand = new AsyncRelayCommand( ConversionProcessAsync, CanExecuteConversionProcess );

			// CollectionChanged 이벤트 핸들러 등록
			ComboBoxItems.CollectionChanged += ComboBoxItems_CollectionChanged;
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
					
					foreach (var item in ComboBoxItems)
					{
						if (item.Number > targetNumber)
							break;

						switch (item.SelectedOption)
						{
							case "HSV":
								await Task.Run( ( ) => Conversion.RunHSVThreshold( item.HMinValue, item.HMaxValue, item.SMinValue, item.SMaxValue, item.VMinValue, item.VMaxValue, PixelData ));

								break;
							case "Template Matching":
								TemplateMatcher templateMatcher = new TemplateMatcher();
								//await Task.Run( ( ) => templateMatcher.FindTemplate( item.HMinValue, item.HMaxValue, item.SMinValue, item.SMaxValue, item.VMinValue, item.VMaxValue, PixelData ) );
								break;

						}

						// 비동기 이미지 변환 함수 호출
						
					}

					// 처리 완료 후 사용자에게 알림
					//MessageBox.Show( $"1~{targetNumber}번까지의 이미지 변환이 완료되었습니다.", "변환 완료", MessageBoxButton.OK, MessageBoxImage.Information );
				}
				catch (Exception ex)
				{
					// 예외 발생 시 사용자에게 알림
					//MessageBox.Show( $"이미지 변환 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error );
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
