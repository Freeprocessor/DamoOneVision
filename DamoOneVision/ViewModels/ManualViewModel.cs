using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DamoOneVision.Services;
using DamoOneVision.Data;
using DamoOneVision.Models;

namespace DamoOneVision.ViewModels
{
	public class ManualViewModel : INotifyPropertyChanged
	{


		private MotionService _motionService;
		private DeviceControlService _deviceControlService;
		private CameraService _cameraService;

		
		private SettingManager _settingManager;
		public InfraredCameraModel _infraredCameraModel { get; set; }
		public MotionModel motionModel { get; set; }

		private double _xAxisWaitingPosition;
		private double _xAxisEndPosition;
		private double _xAxisCommandPosition;
		private double _xAxisVelocity = 10000;
		private double _xAxisAcceleration = 0.5; // 기본 가속도
		private double _xAxisDeceleration = 0.5; // 기본 감속도
		private bool _xAxisIsMoving;

		private double _zAxisWorkPosition;
		private double _zAxisCommandPosition;
		private double _zAxisVelocity = 10000;
		private double _zAxisAcceleration = 0.5; // 기본 가속도
		private double _zAxisDeceleration = 0.5; // 기본 감속도
		private bool _zAxisIsMoving;

		private double _conveyorSpeed;



		private readonly DispatcherTimer _positionTimer;

		/// <summary>
		/// PropertyChanged 이벤트 핸들러, WPF 바인딩을 위해 필요
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;



		public ManualViewModel( DeviceControlService deviceControlService, MotionService motionService, CameraService cameraService, SettingManager settingManager )
		{
			_deviceControlService = deviceControlService;
			_motionService = motionService;
			_cameraService = cameraService;

			_settingManager = settingManager;

			XAxisJogPStartCommand = new RelayCommand( ( ) => XAxisJogPStart() );
			XAxisJogNStartCommand = new RelayCommand( ( ) => XAxisJogNStart() );
			XAxisJogStopCommand = new RelayCommand( ( ) => XAxisJogStop() );
			ZAxisJogPStartCommand = new RelayCommand( ( ) => ZAxisJogPStart() );
			ZAxisJogNStartCommand = new RelayCommand( ( ) => ZAxisJogNStart() );
			ZAxisJogStopCommand = new RelayCommand( ( ) => ZAxisJogStop() );

			XAxisMoveWaitCommand = new AsyncRelayCommand( XAxisMoveWaitAsync, XAxisCanMove );
			XAxisMoveEndCommand = new AsyncRelayCommand( XAxisMoveEndAsync, XAxisCanMove );
			XAxisStopCommand = new RelayCommand( XAxisStopMotion );

			ZAxisMoveWorkCommand = new AsyncRelayCommand( ZAxisMoveWorkAsync, ZAxisCanMove );
			ZAxisStopCommand = new RelayCommand( ZAxisStopMotion );

			XAxisServoONCommand = new RelayCommand( ( ) => XAxizServoON() );
			ZAxisServoONCommand = new RelayCommand( ( ) => ZAxizServoON() );
			XAxisServoOFFCommand = new RelayCommand( ( ) => XAxizServoOFF() );
			ZAxisServoOFFCommand = new RelayCommand( ( ) => ZAxizServoOFF() );

			XAxisHomeCommand = new RelayCommand( ( ) => XAxisHome() );
			ZAxisHomeCommand = new RelayCommand( ( ) => ZAxisHome() );

			EjectONCommand = new RelayCommand( ( ) => EjectorManualON() );
			EjectOFFCommand = new RelayCommand( ( ) => EjectorManualOFF() );
			MainCVOnCommand = new RelayCommand( ( ) => MainCVOn() );
			MainCVOffCommand = new RelayCommand( ( ) => MainCVOff() );

			SideCVOnCommand = new RelayCommand( ( ) => SideCVOn() );
			SideCVOffCommand = new RelayCommand( ( ) => SideCVOff() );

			TowerLampStartCommand = new RelayCommand( ( ) => TowerLampStart() );
			TowerLampStopCommand = new RelayCommand( ( ) => TowerLampStop() );
			TowerLampErrorCommand = new RelayCommand( ( ) => TowerLampError() );

			AutoFocusCommand = new RelayCommand( ( ) => AutoFocus() );
			ManualFocusCommand = new RelayCommand( ( ) => ManualFocus() );

			_positionTimer = new DispatcherTimer();
			_positionTimer.Interval = TimeSpan.FromMilliseconds( 200 ); // 0.2초마다 업데이트
			_positionTimer.Tick += PositionTimer_Tick;


			setModel();
		}

		private void setModel( )
		{
			var modeldata = _settingManager.LoadModelData( _settingManager.LastOpenedModel());
			_infraredCameraModel = modeldata.InfraredCameraModels.First();
			motionModel = modeldata.MotionModels.First();

		}



		public void PositionReadStart( )
		{
			_positionTimer.Start();
		}

		public void PositionReadStop( )
		{
			_positionTimer.Stop();
		}

		public double XAxisWaitingPosition
		{
			get => _xAxisWaitingPosition;
			set
			{
				_xAxisWaitingPosition = value;
				OnPropertyChanged( nameof( XAxisWaitingPosition ) );
				(XAxisMoveWaitCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
				(XAxisMoveEndCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}

		public double XAxisEndPosition
		{
			get => _xAxisEndPosition;
			set
			{
				_xAxisEndPosition = value;
				OnPropertyChanged( nameof( XAxisEndPosition ) );
				(XAxisMoveWaitCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
				(XAxisMoveEndCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}

		public double XAxisCommandPosition
		{
			get => _xAxisCommandPosition;
			set
			{
				_xAxisCommandPosition = value;
				OnPropertyChanged( nameof( XAxisCommandPosition ) );
				(XAxisMoveWaitCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
				(XAxisMoveEndCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}

		public double XAxisVelocity
		{
			get => _xAxisVelocity;
			set
			{
				_xAxisVelocity = value;
				OnPropertyChanged( nameof( XAxisVelocity ) );
				(XAxisMoveWaitCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
				(XAxisMoveEndCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}

		public double XAxisAcceleration
		{
			get => _xAxisAcceleration;
			set
			{
				_xAxisAcceleration = value;
				OnPropertyChanged( nameof( XAxisAcceleration ) );
			}
		}

		public double XAxisDeceleration
		{
			get => _xAxisDeceleration;
			set
			{
				_xAxisDeceleration = value;
				OnPropertyChanged( nameof( XAxisDeceleration ) );
			}
		}


		public double ZAxisWorkPosition
		{
			get => _zAxisWorkPosition;
			set
			{
				_zAxisWorkPosition = value;
				OnPropertyChanged( nameof( ZAxisWorkPosition ) );
				(ZAxisMoveWorkCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}


		public double ZAxisCommandPosition
		{
			get => _zAxisCommandPosition;
			set
			{
				_zAxisCommandPosition = value;
				OnPropertyChanged( nameof( ZAxisCommandPosition ) );
				(ZAxisMoveWorkCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}

		public double ZAxisVelocity
		{
			get => _zAxisVelocity;
			set
			{
				_zAxisVelocity = value;
				OnPropertyChanged( nameof( ZAxisVelocity ) );
				(ZAxisMoveWorkCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}

		public double ZAxisAcceleration
		{
			get => _zAxisAcceleration;
			set
			{
				_zAxisAcceleration = value;
				OnPropertyChanged( nameof( ZAxisAcceleration ) );
			}
		}

		public double ZAxisDeceleration
		{
			get => _zAxisDeceleration;
			set
			{
				_zAxisDeceleration = value;
				OnPropertyChanged( nameof( ZAxisDeceleration ) );
			}
		}

		public double ConveyorSpeed
		{
			get => _conveyorSpeed;
			set
			{
				_conveyorSpeed = value;
				OnPropertyChanged( nameof( ConveyorSpeed ) );
			}
		}


		public ICommand EjectONCommand { get; }
		public ICommand EjectOFFCommand { get; }
		public ICommand MainCVOnCommand { get; }
		public ICommand MainCVOffCommand { get; }
		public ICommand SideCVOnCommand { get; }
		public ICommand SideCVOffCommand { get; }
		public ICommand TowerLampStartCommand { get; }
		public ICommand TowerLampStopCommand { get; }
		public ICommand TowerLampErrorCommand { get; }


		public ICommand XAxisJogPStartCommand { get; }
		public ICommand XAxisJogNStartCommand { get; }
		public ICommand XAxisJogStopCommand { get; }
		public ICommand ZAxisJogPStartCommand { get; }
		public ICommand ZAxisJogNStartCommand { get; }
		public ICommand ZAxisJogStopCommand { get; }


		public ICommand XAxisMoveWaitCommand { get; }
		public ICommand XAxisMoveEndCommand { get; }
		public ICommand XAxisStopCommand { get; }
		public ICommand ZAxisMoveWorkCommand { get; }
		public ICommand ZAxisStopCommand { get; }

		public ICommand XAxisServoONCommand { get; }
		public ICommand ZAxisServoONCommand { get; }
		public ICommand XAxisServoOFFCommand { get; }
		public ICommand ZAxisServoOFFCommand { get; }

		public ICommand XAxisHomeCommand { get; }
		public ICommand ZAxisHomeCommand { get; }
		public ICommand AutoFocusCommand { get; }
		public ICommand ManualFocusCommand { get; }


		private bool XAxisCanMove( )
		{
			return !_xAxisIsMoving && XAxisVelocity > 0;
		}

		private bool ZAxisCanMove( )
		{
			return !_zAxisIsMoving && ZAxisVelocity > 0;
		}


		private void TowerLampStart( )
		{
			_deviceControlService.TowerLampAsync( "START" );
		}

		private void TowerLampStop( )
		{
			_deviceControlService.TowerLampAsync( "STOP" );
		}

		private void TowerLampError( )
		{
			_deviceControlService.TowerLampAsync( "ERROR" );
		}

		private void EjectorManualON( )
		{
			_deviceControlService.EjectorManualON();
		}

		private void EjectorManualOFF( )
		{
			_deviceControlService.EjectorManualOFF();
		}

		private void MainCVOn()
		{
			_deviceControlService.MainCVOn();
		}

		private void MainCVOff( )
		{
			_deviceControlService.MainCVOff();
		}

		private void SideCVOn( )
		{
			_deviceControlService.SideCVOn();
		}

		private void SideCVOff( )
		{
			_deviceControlService.SideCVOff();
		}

		private void XAxisJogPStart( )
		{
			_motionService.XAxisJogPStart();
			// MouseDown 시 실행할 로직
			// ex) CAXM.AxmMoveVel(axisNo, velocity, accel, decel);
		}

		private void XAxisJogNStart( )
		{
			_motionService.XAxisJogNStart();
			// MouseDown 시 실행할 로직
			// ex) CAXM.AxmMoveVel(axisNo, velocity, accel, decel);
		}

		private void XAxisJogStop( )
		{
			_motionService.XAxisStop();
			// MouseUp 시 실행할 로직
			// ex) CAXM.AxmMoveSStop(axisNo);
		}

		private void ZAxisJogPStart( )
		{
			_motionService.ZAxisJogPStart();
			// MouseDown 시 실행할 로직
			// ex) CAXM.AxmMoveVel(axisNo, velocity, accel, decel);
		}

		private void ZAxisJogNStart( )
		{
			_motionService.ZAxisJogNStart();
			// MouseDown 시 실행할 로직
			// ex) CAXM.AxmMoveVel(axisNo, velocity, accel, decel);
		}

		private void ZAxisJogStop( )
		{
			_motionService.ZAxisStop();
			// MouseUp 시 실행할 로직
			// ex) CAXM.AxmMoveSStop(axisNo);
		}

		private async Task XAxisMoveWaitAsync( )
		{
			//await XAxisMoveToTargetAsync( XAxisWaitingPosition );

			_xAxisIsMoving = true;
			(XAxisMoveWaitCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			(XAxisMoveEndCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();

			try
			{
				await _motionService.XAxisMoveToPosition( motionModel.XAxisWaitingPosition, motionModel.XAxisReturnSpeed, motionModel.XAxisReturnAcceleration, motionModel.XAxisReturnDeceleration );
			}
			finally
			{
				_xAxisIsMoving = false;
				(XAxisMoveWaitCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
				(XAxisMoveEndCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}

		private async Task XAxisMoveEndAsync( )
		{
			//await XAxisMoveToTargetAsync( XAxisEndPosition );
			_xAxisIsMoving = true;
			(XAxisMoveWaitCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			(XAxisMoveEndCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();

			try
			{
				await _motionService.XAxisMoveToPosition( motionModel.XAxisEndPosition, motionModel.XAxisTrackingSpeed, motionModel.XAxisMoveAcceleration, motionModel.XAxisMoveDeceleration );
			}
			finally
			{
				_xAxisIsMoving = false;
				(XAxisMoveWaitCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
				(XAxisMoveEndCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}

		private async Task ZAxisMoveWorkAsync( )
		{
			_zAxisIsMoving = true;
			(ZAxisMoveWorkCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();

			try
			{
				await _motionService.ZAxisMoveToPosition( motionModel.ZAxisWorkPosition, motionModel.ZAxisSpeed, motionModel.ZAxisAcceleration, motionModel.ZAxisDeceleration );
			}
			finally
			{
				_zAxisIsMoving = false;
				(ZAxisMoveWorkCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
			//await ZAxisMoveToTargetAsync( ZAxisWorkPosition );
		}

		//private async Task XAxisMoveToTargetAsync( double TargetPosition )
		//{
		//	_xAxisIsMoving = true;
		//	(XAxisMoveWaitCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
		//	(XAxisMoveEndCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();

		//	try
		//	{
		//		await _motionService.XAxisMoveToPosition( TargetPosition, XAxisVelocity, XAxisAcceleration, XAxisDeceleration );
		//	}
		//	finally
		//	{
		//		_xAxisIsMoving = false;
		//		(XAxisMoveWaitCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
		//		(XAxisMoveEndCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
		//	}
		//}

		//private async Task ZAxisMoveToTargetAsync( double TargetPosition )
		//{
		//	_zAxisIsMoving = true;
		//	(ZAxisMoveWorkCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();

		//	try
		//	{
		//		await _motionService.ZAxisMoveToPosition( TargetPosition, ZAxisVelocity, ZAxisAcceleration, ZAxisDeceleration );
		//	}
		//	finally
		//	{
		//		_zAxisIsMoving = false;
		//		(ZAxisMoveWorkCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
		//	}
		//}

		private void XAxizServoON( )
		{
			_motionService.XAxisServoOn( );
		}

		private void ZAxizServoON( )
		{
			_motionService.ZAxisServoOn( );
		}

		private void XAxizServoOFF( )
		{
			_motionService.XAxisServoOff();
		}

		private void ZAxizServoOFF( )
		{
			_motionService.ZAxisServoOff();
		}

		private void XAxisHome( )
		{
			_motionService.XAxisHome();
		}

		private void ZAxisHome( )
		{
			_motionService.ZAxisHome();
		}

		private void XAxisStopMotion( )
		{
			_motionService.XAxisStop();
		}

		private void ZAxisStopMotion( )
		{
			_motionService.ZAxisStop();
		}

		private void AutoFocus( )
		{
			_cameraService.InfraredCameraAutoFocus();
		}

		private void ManualFocus( )
		{
			_cameraService.InfraredCameraManualFocus();
		}


		private void PositionTimer_Tick( object? sender, EventArgs e )
		{
			XAxisCommandPosition = _motionService.XAxisGetCommandPosition();
			ZAxisCommandPosition = _motionService.ZAxisGetCommandPosition();
			ConveyorSpeed = _motionService.GetConveyorSpeed();
		}

		protected void OnPropertyChanged( string propertyName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}
	}
}
