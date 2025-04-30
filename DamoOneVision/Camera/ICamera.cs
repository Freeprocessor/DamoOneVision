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

		MIL_ID ReciveImage( );

		MIL_ID ReciveScaleImage( );

		MIL_ID ReciveLoadImage( );

		MIL_ID ReciveLoadScaleImage( );

		MIL_ID ReciveBinarizedImage( );

		MIL_ID ReciveLoadBinarizedImage( );

		ushort[ ] LoadImageData();

		ushort[ ] CaptureImageData( );

		Task<double> AutoFocus( );

		void ManualFocus( double focusValue );

		Task SaveImage( MIL_ID MilImage, string name );

		MIL_INT Width { get; set; }

		MIL_INT Height { get; set; }

		MIL_INT NbBands { get; set; }

		MIL_INT DataType { get; set; }
	}
}
