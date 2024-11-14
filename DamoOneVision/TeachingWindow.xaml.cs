using Matrox.MatroxImagingLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DamoOneVision
{
	/// <summary>
	/// TeachingWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class TeachingWindow : Window
	{


		public TeachingWindow( )
		{
			InitializeComponent();
		}
		// 숫자만 입력할 수 있도록 하는 이벤트 핸들러
		private void NumberValidationTextBox( object sender, TextCompositionEventArgs e )
		{
			Regex regex = new Regex("[^0-9]+"); // 숫자가 아닌 문자 패턴
			e.Handled = regex.IsMatch( e.Text );
		}
	}
}
