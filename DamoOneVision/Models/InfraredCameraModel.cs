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






}