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
		private readonly DeviceControlService _deviceControlService;
		private readonly MotionService _motionService;
		private ManualViewModel _viewModel;


		public ManualWindow( DeviceControlService deviceControlService, MotionService motionService )
        {
			this._deviceControlService = deviceControlService;
			this._motionService = motionService;
			InitializeComponent();
			_viewModel = new ManualViewModel( deviceControlService, motionService );

			this.DataContext = _viewModel;
		}

		private void NumericTextBox_PreviewTextInput( object sender, TextCompositionEventArgs e )
		{
			// 숫자만 허용
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch( e.Text );
		}

		private void OnPaste( object sender, DataObjectPastingEventArgs e )
		{
			if (e.DataObject.GetDataPresent( typeof( string ) ))
			{
				string text = (string)e.DataObject.GetData(typeof(string));
				if (!IsTextNumeric( text ))
				{
					e.CancelCommand();
				}
			}
			else
			{
				e.CancelCommand();
			}
		}

		private bool IsTextNumeric( string text )
		{
			Regex regex = new Regex("^[0-9]+$");
			return regex.IsMatch( text );
		}

	}



}
