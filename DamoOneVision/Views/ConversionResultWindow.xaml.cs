using System.Windows;
using System.Windows.Input;

namespace DamoOneVision.Views
{
	/// <summary>
	/// InfraredAnalysisResultWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ConversionResultWindow : Window
	{
		public ConversionResultWindow( )
		{
			InitializeComponent();
		}
		private void Window_MouseDown( object sender, MouseButtonEventArgs e )
		{
			this.Close();
		}
	}
}
