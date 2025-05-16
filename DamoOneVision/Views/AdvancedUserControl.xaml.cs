using DamoOneVision.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DamoOneVision.Views
{
	/// <summary>
	/// ManualUserControl.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class AdvancedUserControl : UserControl
	{
		public event EventHandler GoMainRequested;
		AdvancedViewModel _viewModel;
		public AdvancedUserControl( AdvancedViewModel viewModel )
		{
			InitializeComponent();
			_viewModel = viewModel;

			_viewModel.PropertyChanged += ( s, e ) =>
			{
				if (e.PropertyName == nameof( _viewModel.LogContents ))
				{
					Dispatcher.Invoke( ( ) =>
					{
						LogScroll.ScrollToEnd();
					} );
				}
			};

			this.DataContext = _viewModel;
		}

		private void GoMain_Click( object sender, RoutedEventArgs e )
		{
			GoMainRequested?.Invoke( this, EventArgs.Empty );
		}
		private void ImageListBox_PreviewMouseWheel( object sender, MouseWheelEventArgs e )
		{
			// ListBox 내부에 있는 ScrollViewer를 찾습니다.
			if (sender is DependencyObject depObj)
			{
				ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(depObj);
				if (scrollViewer != null)
				{
					// 마우스 휠 이벤트에 따른 스크롤 오프셋을 조절합니다.
					scrollViewer.ScrollToVerticalOffset( scrollViewer.VerticalOffset - e.Delta );
					e.Handled = true;
				}
			}
		}

		// Visual Tree에서 특정 타입의 자식을 찾는 헬퍼 메서드
		private static T FindVisualChild<T>( DependencyObject parent ) where T : DependencyObject
		{
			if (parent == null)
				return null;
			int childCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childCount; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);
				if (child is T typedChild)
					return typedChild;
				T descendant = FindVisualChild<T>(child);
				if (descendant != null)
					return descendant;
			}
			return null;
		}


	}
}
