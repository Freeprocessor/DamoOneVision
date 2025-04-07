using DamoOneVision.ViewModels;
using System.Windows.Controls;

namespace DamoOneVision.Views
{
	public partial class SettingUserControl : UserControl
	{
		public event EventHandler GoMainRequested;
		SettingViewModel _viewModel;
		public SettingUserControl( SettingViewModel viewModel )
		{
			InitializeComponent();
			_viewModel = viewModel;

			this.DataContext = _viewModel;
		}

		private void GoMain_Click( object sender, System.Windows.RoutedEventArgs e )
		{
			_viewModel.UpdateCameraSettingsStop();
			GoMainRequested?.Invoke( this, EventArgs.Empty );
		}
	}
}
