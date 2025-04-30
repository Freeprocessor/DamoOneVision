using System.IO;
using System.Text.Json;

namespace DamoOneVision.Services.Repository;

public sealed class JsonModelRepository : IModelRepository
{
	readonly string _file = Path.Combine(AppContext.BaseDirectory, "models.json");
	Dictionary<string, object?> _cache = new();

	public JsonModelRepository( )
	{
		if (File.Exists( _file ))
			_cache = JsonSerializer.Deserialize<Dictionary<string, object?>>( File.ReadAllText( _file ) ) ?? new();
	}

	public T? Get<T>( string key ) =>
		_cache.TryGetValue( key, out var v ) ? (T?) v : default;

	public void Save<T>( string key, T value )
	{
		_cache[ key ] = value;
		File.WriteAllText( _file, JsonSerializer.Serialize( _cache, new JsonSerializerOptions { WriteIndented = true } ) );
		ModelChanged?.Invoke( this, new ModelChangedEventArgs( key, value ) );
	}

	public event EventHandler<ModelChangedEventArgs>? ModelChanged;
}
