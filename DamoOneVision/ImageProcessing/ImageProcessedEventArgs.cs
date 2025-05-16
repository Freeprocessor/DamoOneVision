namespace DamoOneVision.ImageProcessing
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
