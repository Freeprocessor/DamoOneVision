using System.Windows;

namespace DamoOneVision.Views
{
	public partial class ExitConfirmationWindow : Window
	{
		public ExitConfirmationWindow( )
		{
			InitializeComponent();
		}

		private void Yes_Click( object sender, RoutedEventArgs e )
		{
			Application.Current.Shutdown();
		}

		private void No_Click( object sender, RoutedEventArgs e )
		{
			this.Close();
		}
	}
}
