using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace DamoOneVision.Views
{
	public partial class SplashWindow : Window
	{
		public SplashWindow( )
		{
			InitializeComponent();
		}

		private void Window_Loaded( object sender, RoutedEventArgs e )
		{
			// 페이드 인 효과
			var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));
			this.BeginAnimation( Window.OpacityProperty, fadeIn );
		}

		public void FadeOutAndClose( )
		{
			// 페이드 아웃 효과
			var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(500)));
			fadeOut.Completed += ( s, e ) => this.Close();
			this.BeginAnimation( Window.OpacityProperty, fadeOut );
		}
	}
}
