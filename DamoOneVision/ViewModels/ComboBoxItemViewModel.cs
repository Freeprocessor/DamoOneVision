using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;
using Matrox.MatroxImagingLibrary;
using CommunityToolkit.Mvvm.Input;


namespace DamoOneVision.ViewModels
{
	public class ComboBoxItemViewModel : INotifyPropertyChanged
	{

		/// <summary>
		/// ProcessingOption 
		/// </summary>
		/// 
		private string _selectedProcessingOption;
		public ObservableCollection<string> ProcessingOptions { get; set; }

		public string SelectedProcessingOption
		{
			get => _selectedProcessingOption;
			set
			{
				if (_selectedProcessingOption != value)
				{
					_selectedProcessingOption = value;
					OnPropertyChanged();
					// CanExecute 상태 업데이트
					(LoadTempleteFileCommand as IRelayCommand)?.NotifyCanExecuteChanged();
					(LoadFileCommand as IRelayCommand)?.NotifyCanExecuteChanged();
				}
			}
		}

		private int _number; // 번호 속성 추가
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

		// HSV 슬라이더 값
		private int? _hMinValue;
		private int? _hMaxValue;
		private int? _sMinValue;
		private int? _sMaxValue;
		private int? _vMinValue;
		private int? _vMaxValue;


		// HValue 속성
		public int? HMinValue
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
		public int? HMaxValue
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

		public int? SMinValue
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
		public int? SMaxValue
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

		public int? VMinValue
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

		public int? VMaxValue
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


		/// <summary>
		/// ClipOption 
		/// </summary>
		/// 

		// Clip 옵션을 위한 ComboBox
		public ObservableCollection<string> ClipOptions { get; set; }
		private string _selectedClipOption;
		public string SelectedClipOption
		{
			get => _selectedClipOption;
			set
			{
				if (_selectedClipOption != value)
				{
					_selectedClipOption = value;
					OnPropertyChanged();
				}
			}
		}

		// Clip을 위한 숫자 입력 필드
		private ushort? _lowerLimit;
		public ushort? LowerLimit
		{
			get => _lowerLimit;
			set
			{
				if (_lowerLimit != value)
				{
					_lowerLimit = value;
					OnPropertyChanged();
				}
			}
		}

		private ushort? _upperLimit;
		public ushort? UpperLimit
		{
			get => _upperLimit;
			set
			{
				if (_upperLimit != value)
				{
					_upperLimit = value;
					OnPropertyChanged();
				}
			}
		}

		private ushort? _writeLow;
		public ushort? WriteLow
		{
			get => _writeLow;
			set
			{
				if (_writeLow != value)
				{
					_writeLow = value;
					OnPropertyChanged();
				}
			}
		}

		private ushort? _writeHigh;
		public ushort? WriteHigh
		{
			get => _writeHigh;
			set
			{
				if (_writeHigh != value)
				{
					_writeHigh = value;
					OnPropertyChanged();
				}
			}
		}

		// 선택 상태를 나타내는 속성 추가

		/// <summary>
		/// Command
		/// </summary>

		public ComboBoxItemViewModel( )
		{
			ProcessingOptions = new ObservableCollection<string>
			{
				"선택해주세요",
				"HSV",
				"Template Matching",
				"File Loading",
				"Clip"
			};

			SelectedProcessingOption = "선택해주세요"; // 기본 선택값 설정

			ClipOptions = new ObservableCollection<string>
			{
				"MIL.M_EQUAL",
				"MIL.M_GREATER",
				"MIL.M_GREATER_OR_EQUAL",
				"MIL.M_IN_RANGE",
				"MIL.M_LESS",
				"MIL.M_LESS_OR_EQUAL",
				"MIL.M_NOT_EQUAL",
				"MIL.M_OUT_RANGE",
				"MIL.M_SATURATION",

			};

			SelectedClipOption = ClipOptions.FirstOrDefault(); // 기본값 설정



			// 슬라이더 초기값 설정
			HMinValue = 0;
			HMaxValue = 0;
			SMinValue = 0;
			SMaxValue = 0;
			VMinValue = 0;
			VMaxValue = 0;
			LowerLimit = 0;
			UpperLimit = 0;
			WriteLow = 0;
			WriteHigh = 0;

			LoadTempleteFileCommand = new RelayCommand<object>( LoadTempleteFile, CanLoadTempleteFile );
			LoadFileCommand = new RelayCommand<object>( LoadFile, CanLoadFile );
		}

		public ICommand LoadTempleteFileCommand { get; set; }
		public ICommand LoadFileCommand { get; set; }

		private bool CanLoadTempleteFile( object parameter )
		{
			return SelectedProcessingOption == "Template Matching";
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
			return SelectedProcessingOption == "File Loading";
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


		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged( [CallerMemberName] string name = null )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}



	}
}
