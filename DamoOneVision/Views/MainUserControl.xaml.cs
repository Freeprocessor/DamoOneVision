using DamoOneVision.ViewModels;
using System.Windows.Controls;

namespace DamoOneVision.Views
{
	/// <summary>
	/// MainUserControl.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainUserControl : UserControl
	{

		MainViewModel _viewModel;
		public MainUserControl( MainViewModel mainViewModel )
		{
			_viewModel = mainViewModel;
			DataContext = _viewModel;
			InitializeComponent();
		}





	}
}
