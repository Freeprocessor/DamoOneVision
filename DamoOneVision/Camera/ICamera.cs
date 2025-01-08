using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamoOneVision.Camera
{
	public interface ICamera
	{
		bool Connect( );
		void Disconnect( );
		MIL_ID CaptureImage( );

		MIL_ID LoadImage( MIL_ID MilSystem, string filePath );

		MIL_INT Width { get; set; }

		MIL_INT Height { get; set; }

		MIL_INT NbBands { get; set; }

		MIL_INT DataType { get; set; }
	}
}
