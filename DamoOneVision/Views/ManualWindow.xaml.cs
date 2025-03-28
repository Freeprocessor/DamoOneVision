using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
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
using static OpenCvSharp.FileStorage;
using System.Text.RegularExpressions;
using DamoOneVision.Services;
using DamoOneVision.ViewModels;

namespace DamoOneVision
{
	/// <summary>
	/// ManualWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ManualWindow : Window
    {
		private ManualViewModel _viewModel;


		public ManualWindow( ManualViewModel viewModel )
        {
			InitializeComponent();
			_viewModel = viewModel;

			this.DataContext = _viewModel;
		}

    }



}
