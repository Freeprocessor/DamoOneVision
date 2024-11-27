﻿using System;
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
	public partial class InputDialog : Window
	{
		public string ResponseText { get; set; }
		public string Message { get; set; }

		public InputDialog( string message )
		{
			InitializeComponent();
			Message = message;
			DataContext = this;
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = true;
		}
	}
}