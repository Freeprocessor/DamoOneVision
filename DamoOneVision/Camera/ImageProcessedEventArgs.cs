using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DamoOneVision.Camera
{
	public class ImageProcessedEventArgs : EventArgs
	{
		public byte[ ] ProcessedPixelData { get; }

		public ImageProcessedEventArgs( byte[ ] data )
		{
			ProcessedPixelData = data;
		}
	}
}
