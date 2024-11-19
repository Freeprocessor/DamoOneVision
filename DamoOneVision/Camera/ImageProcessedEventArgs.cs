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
		public int Width { get; }
		public int Height { get; }
		public PixelFormat PixelFormat { get; }

		public ImageProcessedEventArgs( byte[ ] data, int width, int height, PixelFormat format )
		{
			ProcessedPixelData = data;
			Width = (int) width;
			Height = (int) height;
			PixelFormat = format;
		}
	}
}
