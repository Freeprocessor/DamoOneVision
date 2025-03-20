using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamoOneVision.Models
{

	public class InfraredCameraModel
	{
		public string Name { get; set; }
		public double CircleCenterX { get; set; }
		public double CircleCenterY { get; set; }
		public double CircleMinRadius { get; set; }
		public double CircleMaxRadius { get; set; }
		public double BinarizedThreshold { get; set; }
	}

	public class MotionModel
	{
		public string Name { get; set; }
		public double XAxisSpeed { get; set; }
		public double XAxisReturnSpeed { get; set; }
		public double XAxisAcceleration { get; set; }
		public double XAxisDeceleration { get; set; }
		public double XAxisJogSpeed { get; set; }
		public double XAxisWaitingPostion { get; set; }
		public double XAxisEndPostion { get; set; }

		public double ZAxisSpeed { get; set; }
		public double ZAxisReturnSpeed { get; set; }
		public double ZAxisAcceleration { get; set; }
		public double ZAxisDeceleration { get; set; }
		public double ZAxisJogSpeed { get; set; }
		public double ZAxisWaitingPostion { get; set; }
		public double ZAxisEndPostion { get; set; }

	}


	public class ModelData
	{
		public List<InfraredCameraModel> InfraredCameraModels { get; set; } = new List<InfraredCameraModel>();
		public List<MotionModel> MotionModels { get; set; } = new List<MotionModel>();
	}

}