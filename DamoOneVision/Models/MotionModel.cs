using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace DamoOneVision.Models
{
	public class MotionModel
	{
		public double XAxisWaitingPosition { get; set; }
		public double XAxisEndPosition { get; set; }
		public double XAxisTrackingSpeed { get; set; }
		public double XAxisReturnSpeed { get; set; }
		public double XAxisMoveAcceleration { get; set; }
		public double XAxisMoveDeceleration { get; set; }
		public double XAxisReturnAcceleration { get; set; }
		public double XAxisReturnDeceleration { get; set; }
		public double XAxisJogSpeed { get; set; }
		public double XAxisJogAcceleration { get; set; }
		public double XAxisJogDeceleration { get; set; }

		public int XAxisOriginDirection { get; set; }
		public uint XAxisOriginSensor { get; set; }
		public uint XAxisOriginUseZPhase { get; set; }
		public double XAxisOriginDelay { get; set; }
		public double XAxisOriginOffset { get; set; }

		public double XAxisOriginSpeed1 { get; set; }
		public double XAxisOriginSpeed2 { get; set; }
		public double XAxisOriginCreepSpeed { get; set; }
		public double XAxisOriginZPhaseSpeed { get; set; }
		public double XAxisOriginAcceleration { get; set; }
		public double XAxisOriginDeceleration { get; set; }


		public double ZAxisWorkPosition { get; set; }
		public double ZAxisEndPosition { get; set; }
		public double ZAxisSpeed { get; set; }
		public double ZAxisAcceleration { get; set; }
		public double ZAxisDeceleration { get; set; }
		public double ZAxisJogSpeed { get; set; }
		public double ZAxisJogAcceleration { get; set; }
		public double ZAxisJogDeceleration { get; set; }


		public int ZAxisOriginDirection { get; set; }
		public uint ZAxisOriginSensor { get; set; }
		public uint ZAxisOriginUseZPhase { get; set; }
		public double ZAxisOriginDelay { get; set; }
		public double ZAxisOriginOffset { get; set; }

		public double ZAxisOriginSpeed1 { get; set; }
		public double ZAxisOriginSpeed2 { get; set; }
		public double ZAxisOriginCreepSpeed { get; set; }
		public double ZAxisOriginZPhaseSpeed { get; set; }
		public double ZAxisOriginAcceleration { get; set; }
		public double ZAxisOriginDeceleration { get; set; }

	}

	public class ModelData
	{
		public List<InfraredCameraModel> InfraredCameraModels { get; set; } = new List<InfraredCameraModel>();
		public List<MotionModel> MotionModels { get; set; } = new List<MotionModel>();
	}
}
