using System.ComponentModel;

namespace DamoOneVision.Models
{
	public class InfraredCameraModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged( string propertyName )
			=> PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );

		private double _binarizedThreshold;
		public double BinarizedThreshold
		{
			get => _binarizedThreshold;
			set
			{
				if (_binarizedThreshold != value)
				{
					_binarizedThreshold = value;
					OnPropertyChanged( nameof( BinarizedThreshold ) );
				}
			}
		}

		private double _referenceBaseTemperature ;
		public double ReferenceBaseTemperature
		{
			get => _referenceBaseTemperature;
			set
			{
				if (_referenceBaseTemperature != value)
				{
					_referenceBaseTemperature = value;
					OnPropertyChanged( nameof( ReferenceBaseTemperature ) );
				}
			}
		}

		private double _circleCenterX;
		public double CircleCenterX
		{
			get => _circleCenterX;
			set
			{
				if (_circleCenterX != value)
				{
					_circleCenterX = value;
					OnPropertyChanged( nameof( CircleCenterX ) );
				}
			}
		}

		private double _circleCenterY;
		public double CircleCenterY
		{
			get => _circleCenterY;
			set
			{
				if (_circleCenterY != value)
				{
					_circleCenterY = value;
					OnPropertyChanged( nameof( CircleCenterY ) );
				}
			}
		}

		private double _circleMinRadius;
		public double CircleMinRadius
		{
			get => _circleMinRadius;
			set
			{
				if (_circleMinRadius != value)
				{
					_circleMinRadius = value;
					OnPropertyChanged( nameof( CircleMinRadius ) );
				}
			}
		}

		private double _circleMaxRadius;
		public double CircleMaxRadius
		{
			get => _circleMaxRadius;
			set
			{
				if (_circleMaxRadius != value)
				{
					_circleMaxRadius = value;
					OnPropertyChanged( nameof( CircleMaxRadius ) );
				}
			}
		}

		private double _circleInnerRadius;
		public double CircleInnerRadius
		{
			get => _circleInnerRadius;
			set
			{
				if (_circleInnerRadius != value)
				{
					_circleInnerRadius = value;
					OnPropertyChanged( nameof( CircleInnerRadius ) );
				}
			}
		}

		private double _circleMinAreaRatio;
		public double CircleMinAreaRatio
		{
			get => _circleMinAreaRatio;
			set
			{
				if (_circleMinAreaRatio != value)
				{
					_circleMinAreaRatio = value;
					OnPropertyChanged( nameof( CircleMinAreaRatio ) );
				}
			}
		}

		private double _circleMaxAreaRatio;
		public double CircleMaxAreaRatio
		{
			get => _circleMaxAreaRatio;
			set
			{
				if (_circleMaxAreaRatio != value)
				{
					_circleMaxAreaRatio = value;
					OnPropertyChanged( nameof( CircleMaxAreaRatio ) );
				}
			}
		}

		private double _circleAreaMinLength;
		public double CircleAreaMinLength
		{
			get => _circleAreaMinLength;
			set
			{
				if (_circleAreaMinLength != value)
				{
					_circleAreaMinLength = value;
					OnPropertyChanged( nameof( CircleAreaMinLength ) );
				}
			}
		}

		private double _avgTemperatureMin;
		public double AvgTemperatureMin
		{
			get => _avgTemperatureMin;
			set
			{
				if (_avgTemperatureMin != value)
				{
					_avgTemperatureMin = value;
					OnPropertyChanged( nameof( AvgTemperatureMin ) );
				}
			}
		}

		private double _avgTemperatureMax;
		public double AvgTemperatureMax
		{
			get => _avgTemperatureMax;
			set
			{
				if (_avgTemperatureMax != value)
				{
					_avgTemperatureMax = value;
					OnPropertyChanged( nameof( AvgTemperatureMax ) );
				}
			}
		}

		private double _underHeatCountLim;
		public double UnderHeatCountLim
		{
			get => _underHeatCountLim;
			set
			{
				if (_underHeatCountLim != value)
				{
					_underHeatCountLim = value;
					OnPropertyChanged( nameof( UnderHeatCountLim ) );
				}
			}
		}

		private double _overHeatCountLim;
		public double OverHeatCountLim
		{
			get => _overHeatCountLim;
			set
			{
				if (_overHeatCountLim != value)
				{
					_overHeatCountLim = value;
					OnPropertyChanged( nameof( OverHeatCountLim ) );
				}
			}
		}

		private double _tempDivLim;
		public double TempDivLim
		{
			get => _tempDivLim;
			set
			{
				if (_tempDivLim != value)
				{
					_tempDivLim = value;
					OnPropertyChanged( nameof( TempDivLim ) );
				}
			}
		}

		private double _neighborDiffLim ;
		public double NeighborDiffLim
		{
			get => _neighborDiffLim;
			set
			{
				if (_neighborDiffLim != value)
				{
					_neighborDiffLim = value;
					OnPropertyChanged( nameof( NeighborDiffLim ) );
				}
			}
		}

		private double _cameraFocusValue;
		public double CameraFocusValue
		{
			get => _cameraFocusValue;
			set
			{
				if (_cameraFocusValue != value)
				{
					_cameraFocusValue = value;
					OnPropertyChanged( nameof( CameraFocusValue ) );
				}
			}
		}

		private double _productHeight;
		public double ProductHeight
		{
			get => _productHeight;
			set
			{
				if (_productHeight != value)
				{
					_productHeight = value;
					OnPropertyChanged( nameof( ProductHeight ) );
				}
			}
		}
	}
}
