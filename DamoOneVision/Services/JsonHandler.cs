using DamoOneVision.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamoOneVision.Services
{
	public class JsonHandler
	{
		private readonly string _filePath;

		public JsonHandler( string filePath )
		{
			_filePath = filePath;
		}

		public async Task SaveInfraredModelsAsync( InfraredCameraModelData data )
		{
			string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
			using (StreamWriter writer = new StreamWriter( _filePath, false ))
			{
				await writer.WriteAsync( jsonString );
			}
		}

		public async Task<InfraredCameraModelData> LoadInfraredModelsAsync( )
		{
			if (!File.Exists( _filePath ))
				return new InfraredCameraModelData();

			string jsonString;
			using (StreamReader reader = new StreamReader( _filePath ))
			{
				jsonString = await reader.ReadToEndAsync();
			}

			return JsonConvert.DeserializeObject<InfraredCameraModelData>( jsonString ) ?? new InfraredCameraModelData();
		}

		public async Task SaveSideModelsAsync( SideCameraModelData data )
		{
			string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
			using (StreamWriter writer = new StreamWriter( _filePath, false ))
			{
				await writer.WriteAsync( jsonString );
			}
		}

		public async Task<SideCameraModelData> LoadSideModelsAsync( )
		{
			if (!File.Exists( _filePath ))
				return new SideCameraModelData();

			string jsonString;
			using (StreamReader reader = new StreamReader( _filePath ))
			{
				jsonString = await reader.ReadToEndAsync();
			}

			return JsonConvert.DeserializeObject<SideCameraModelData>( jsonString ) ?? new SideCameraModelData();
		}
	}
}
