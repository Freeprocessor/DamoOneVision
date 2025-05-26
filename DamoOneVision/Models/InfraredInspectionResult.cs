using System.ComponentModel;

public class InfraredInspectionResult : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged( string name ) =>
		PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );

	private bool _moonCutIssue;
	public bool MoonCutIssue
	{
		get => _moonCutIssue;
		set { _moonCutIssue = value; OnPropertyChanged( nameof( MoonCutIssue ) ); }
	}

	private bool _circleIssue;
	public bool CircleIssue
	{
		get => _circleIssue;
		set { _circleIssue = value; OnPropertyChanged( nameof( CircleIssue ) ); }
	}

	private bool _overHeatIssue;
	public bool OverHeatIssue
	{
		get => _overHeatIssue;
		set { _overHeatIssue = value; OnPropertyChanged( nameof( OverHeatIssue ) ); }
	}

	private bool _underHeatIssue;
	public bool UnderHeatIssue
	{
		get => _underHeatIssue;
		set { _underHeatIssue = value; OnPropertyChanged( nameof( UnderHeatIssue ) ); }
	}

	private bool _temperatureIssue;

	public bool TemperatureIssue
	{
		get => _temperatureIssue;
		set { _temperatureIssue = value; OnPropertyChanged( nameof( TemperatureIssue ) ); }
	}

	private bool _neighborTempIssue ;

	public bool NeighborTempIssue
	{
		get => _neighborTempIssue;
		set { _neighborTempIssue = value; OnPropertyChanged( nameof( NeighborTempIssue ) ); }
	}

	public bool IsGood => !(MoonCutIssue || CircleIssue || OverHeatIssue || UnderHeatIssue || TemperatureIssue || NeighborTempIssue);

	// 추가 정보
	public double FillRatio { get; set; }
	public double AverageTemperature { get; set; }
	public double TempeDiv { get; set; }
	public double Radius { get; set; }
	public double MaxBlobLength { get; set; }

}
