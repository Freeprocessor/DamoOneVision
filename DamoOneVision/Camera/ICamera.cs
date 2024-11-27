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
		byte[ ] CaptureImage( );
	}
}
