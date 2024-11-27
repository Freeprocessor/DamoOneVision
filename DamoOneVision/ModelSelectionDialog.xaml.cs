using DamoOneVision.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	public partial class ModelSelectionDialog : Window
	{
		public List<ModelItem> ModelList { get; set; }
		public int SelectedModelId { get; set; } = -1;

		public ModelSelectionDialog( List<ModelItem> modelList )
		{
			InitializeComponent();
			ModelList = modelList;
			DataContext = this;
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			if (SelectedModelId == -1)
			{
				MessageBox.Show( "모델을 선택하세요." );
				DialogResult = true;
				return;
			}

		}

	}
}