using DamoOneVision.Models;
using DamoOneVision.ViewModels;
using System.Windows;
using System.Windows.Input;

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
		private void Tile_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
			if (sender is FrameworkElement fe && fe.DataContext is FolderItem item)
			{
				SelectedFolderPath = item.Path;   // 선택된 폴더 경로 저장
				DialogResult = true;              // 창 닫기 & 결과 반환
			}
		}
		private void CloseButton_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = false;   // 필요 없으면 그냥 Close()만 호출해도 됩니다.
			Close();
		}
	}
}