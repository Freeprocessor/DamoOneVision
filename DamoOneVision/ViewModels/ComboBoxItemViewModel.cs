using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;

namespace DamoOneVision.ViewModels
{
	public class ComboBoxItemViewModel : INotifyPropertyChanged
	{
		private string _selectedOption;
		private int _number; // 번호 속성 추가

		// HSV 슬라이더 값
		private int _hMinValue;
		private int _hMaxValue;
		private int _sMinValue;
		private int _sMaxValue;
		private int _vMinValue;
		private int _vMaxValue;

		public event PropertyChangedEventHandler PropertyChanged;

		// Number 속성
		public int Number
		{
			get => _number;
			set
			{
				if (_number != value)
				{
					_number = value;
					OnPropertyChanged();
				}
			}
		}

		public string SelectedOption
		{
			get => _selectedOption;
			set
			{
				if (_selectedOption != value)
				{
					_selectedOption = value;
					OnPropertyChanged();
					// CanExecute 상태 업데이트
					(LoadTempleteFileCommand as RelayCommand)?.RaiseCanExecuteChanged();
					(LoadFileCommand as RelayCommand)?.RaiseCanExecuteChanged();
				}
			}
		}

		public ObservableCollection<string> Options { get; set; }

		public ICommand LoadTempleteFileCommand { get; set; }

		public ICommand LoadFileCommand { get; set; }

		public ComboBoxItemViewModel( )
		{
			Options = new ObservableCollection<string>
			{
				"선택해주세요",
				"HSV",
				"Template Matching",
				"File Loading"
			};
			SelectedOption = "선택해주세요"; // 기본 선택값 설정

			// 슬라이더 초기값 설정
			HMinValue = 0;
			HMaxValue = 0;
			SMinValue = 0;
			SMaxValue = 0;
			VMinValue = 0;
			VMaxValue = 0;

			LoadTempleteFileCommand = new RelayCommand( LoadTempleteFile, CanLoadTempleteFile );
			LoadFileCommand = new RelayCommand( LoadFile, CanLoadFile );
		}

		// HValue 속성
		public int HMinValue
		{
			get => _hMinValue;
			set
			{
				if (_hMinValue != value)
				{
					_hMinValue = value;
					OnPropertyChanged();
				}
			}
		}
		public int HMaxValue
		{
			get => _hMaxValue;
			set
			{
				if (_hMaxValue != value)
				{
					_hMaxValue = value;
					OnPropertyChanged();
				}
			}
		}

		public int SMinValue
		{
			get => _sMinValue;
			set
			{
				if (_sMinValue != value)
				{
					_sMinValue = value;
					OnPropertyChanged();
				}
			}
		}
		public int SMaxValue
		{
			get => _sMaxValue;
			set
			{
				if (_sMaxValue != value)
				{
					_sMaxValue = value;
					OnPropertyChanged();
				}
			}
		}

		public int VMinValue
		{
			get => _vMinValue;
			set
			{
				if (_vMinValue != value)
				{
					_vMinValue = value;
					OnPropertyChanged();
				}
			}
		}

		public int VMaxValue
		{
			get => _vMaxValue;
			set
			{
				if (_vMaxValue != value)
				{
					_vMaxValue = value;
					OnPropertyChanged();
				}
			}
		}



		private bool CanLoadTempleteFile( object parameter )
		{
			return SelectedOption == "Template Matching";
		}

		private void LoadTempleteFile( object parameter )
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == true)
			{
				// 파일 선택 후 처리 로직을 여기에 구현
				MessageBox.Show( $"선택된 파일: {openFileDialog.FileName}" );
			}
		}

		private bool CanLoadFile( object parameter )
		{
			return SelectedOption == "File Loading";
		}

		private void LoadFile( object parameter )
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == true)
			{
				// 파일 선택 후 처리 로직을 여기에 구현
				MessageBox.Show( $"선택된 파일: {openFileDialog.FileName}" );
			}
		}

		protected void OnPropertyChanged( [CallerMemberName] string name = null )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
