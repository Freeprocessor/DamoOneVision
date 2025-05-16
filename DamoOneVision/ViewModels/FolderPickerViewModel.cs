using CommunityToolkit.Mvvm.ComponentModel;
using DamoOneVision.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace DamoOneVision.ViewModels
{
	public partial class FolderPickerViewModel : ObservableObject
	{
		public ObservableCollection<FolderItem> FolderItems { get; } = new();

		public FolderPickerViewModel( string rootPath )
		{
			if (!Directory.Exists( rootPath )) return;

			foreach (var dir in Directory.EnumerateDirectories( rootPath ))
				FolderItems.Add( MakeFolderItem( dir ) );
		}

		private FolderItem MakeFolderItem( string dir )
		{
			// 대표 썸네일: 첫 번째 bmp | jpg
			var firstImg = Directory.EnumerateFiles(dir, "*.*")
									.FirstOrDefault(f =>
										f.EndsWith(".bmp", true, null) ||
										f.EndsWith(".jpg", true, null) ||
										f.EndsWith(".png", true, null));

			BitmapImage thumb = null;
			if (firstImg is not null)
			{
				thumb = new BitmapImage();
				thumb.BeginInit();
				thumb.UriSource = new( firstImg );
				thumb.DecodePixelWidth = 200;      // 성능 최적화
				thumb.CacheOption = BitmapCacheOption.OnLoad;
				thumb.EndInit();
			}

			return new FolderItem( Path.GetFileName( dir ), dir, thumb );
		}
	}
}
