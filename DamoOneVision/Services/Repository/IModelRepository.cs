namespace DamoOneVision.Services.Repository;

public interface IModelRepository
{
	T? Get<T>( string key );
	void Save<T>( string key, T value );
	event EventHandler<ModelChangedEventArgs> ModelChanged;
}

public sealed class ModelChangedEventArgs : EventArgs
{
	public string Key { get; }
	public object? NewValue { get; }
	public ModelChangedEventArgs( string key, object? val ) => (Key, NewValue) = (key, val);
}
