using DamoOneVision.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace DamoOneVision.Views
{
	/// <summary>
	/// ManualUserControl.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ManualUserControl : UserControl
	{
		public event EventHandler GoMainRequested;
		ManualViewModel _viewModel;
		public ManualUserControl( ManualViewModel viewModel )
		{
			InitializeComponent();
			_viewModel = viewModel;

			this.DataContext = _viewModel;
		}

		private void GoMain_Click( object sender, RoutedEventArgs e )
		{
			GoMainRequested?.Invoke( this, EventArgs.Empty );
		}


	}
}
