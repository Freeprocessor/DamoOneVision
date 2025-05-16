using DamoOneVision.Models;
using DamoOneVision.ViewModels;
using System.Windows;

namespace DamoOneVision.Views
{
	public partial class FolderPickerWindow : Window
	{
		public string SelectedFolderPath { get; private set; } = null!;

		public FolderPickerWindow( string rootPath )
		{
			InitializeComponent();
			DataContext = new FolderPickerViewModel( rootPath );
		}

		private void FolderButton_Click( object sender, RoutedEventArgs e )
		{
			if (sender is FrameworkElement fe && fe.DataContext is FolderItem item)
			{
				SelectedFolderPath = item.Path;
				DialogResult = true;
				Close();
			}
		}
	}
}