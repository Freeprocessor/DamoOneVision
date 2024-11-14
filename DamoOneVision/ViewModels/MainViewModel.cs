using DamoOneVision.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace DamoOneVision.ViewModels
{
	public class MainViewModel
	{
		public ObservableCollection<ComboBoxItemViewModel> ComboBoxItems { get; set; }
		public ICommand AddComboBoxCommand { get; set; }
		public ICommand DeleteComboBoxCommand { get; set; }

		public MainViewModel( )
		{
			ComboBoxItems = new ObservableCollection<ComboBoxItemViewModel>();
			AddComboBoxCommand = new RelayCommand( AddComboBox );
			DeleteComboBoxCommand = new RelayCommand( DeleteComboBox );

			// CollectionChanged 이벤트 핸들러 등록
			ComboBoxItems.CollectionChanged += ComboBoxItems_CollectionChanged;
		}

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
	}
}
