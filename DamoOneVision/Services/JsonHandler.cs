using DamoOneVision.Models;
using Newtonsoft.Json;
using System.IO;

namespace DamoOneVision.Services
{
	public class JsonHandler
	{
		private readonly string _filePath;

		public JsonHandler( string filePath )
		{
			_filePath = filePath;
		}

		public async Task SaveModelsAsync( ModelData data )
		{
			string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
			using (StreamWriter writer = new StreamWriter( _filePath, false ))
			{
				await writer.WriteAsync( jsonString );
			}
		}

		public async Task<ModelData> LoadModelsAsync( )
		{
			if (!File.Exists( _filePath ))
				return new ModelData();

			string jsonString;
			using (StreamReader reader = new StreamReader( _filePath ))
			{
				jsonString = await reader.ReadToEndAsync();
			}

			return JsonConvert.DeserializeObject<ModelData>( jsonString ) ?? new ModelData();
		}

	}
}
