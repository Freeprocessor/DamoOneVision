using System.Windows;
using System.Windows.Input;

namespace DamoOneVision.Views
{
	public partial class AboutWindow : Window
	{
		public AboutWindow( )
		{
			InitializeComponent();
		}
		protected override void OnPreviewKeyDown( KeyEventArgs e )
		{
			base.OnPreviewKeyDown( e );

			if (e.Key == Key.Escape)
			{
				this.Close();
			}
		}

		private void Close_Click( object sender, RoutedEventArgs e )
		{
			this.Close();
		}
	}
}