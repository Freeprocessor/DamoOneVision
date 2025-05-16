namespace DamoOneVision.Models
{
	public class SideCameraModel
	{
		public double CircleCenterX { get; set; }
		public double CircleCenterY { get; set; }
		public double CircleMinRadius { get; set; }
		public double CircleMaxRadius { get; set; }
		public double BinarizedThreshold { get; set; }
	}

	public class SideCameraModelData
	{
		public List<SideCameraModel> SideCameraModels { get; set; } = new List<SideCameraModel>();
	}
}
