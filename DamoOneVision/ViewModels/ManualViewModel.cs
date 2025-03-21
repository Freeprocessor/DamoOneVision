﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DamoOneVision.Services;

namespace DamoOneVision.ViewModels
{
	class ManualViewModel : INotifyPropertyChanged
	{


		private MotionService _motionService;
		private DeviceControlService _deviceControlService;

		private double _waitingPosition;
		private double _endPosition;
		private double _currentPosition;
		private double _targetPosition;
		private double _velocity;
		private double _acceleration = 400.0; // 기본 가속도
		private double _deceleration = 400.0; // 기본 감속도
		private bool _isMoving;

		/// <summary>
		/// PropertyChanged 이벤트 핸들러, WPF 바인딩을 위해 필요
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;



		public ManualViewModel( DeviceControlService deviceControlService, MotionService motionService )
		{
			_deviceControlService = deviceControlService;
			_motionService = motionService;

			XAxisJogPStartCommand = new RelayCommand( () => XAxisJogPStart() );
			XAxisJogNStartCommand = new RelayCommand( () => XAxisJogNStart() );
			XAxisJogStopCommand = new RelayCommand( ( ) => XAxisJogStop() );
			ZAxisJogPStartCommand = new RelayCommand( ( ) => ZAxisJogPStart() );
			ZAxisJogNStartCommand = new RelayCommand( ( ) => ZAxisJogNStart() );
			ZAxisJogStopCommand = new RelayCommand( ( ) => ZAxisJogStop() );

			MoveCommand = new AsyncRelayCommand( XAxisMoveToTargetAsync, CanMove );
			StopCommand = new RelayCommand( StopMotion );

			EjectONCommand = new RelayCommand( ( ) => EjectorManualON() );
			EjectOFFCommand = new RelayCommand( ( ) => EjectorManualOFF() );
			MainCVOnCommand = new RelayCommand( ( ) => MainCVOn() );
			MainCVOffCommand = new RelayCommand( ( ) => MainCVOff() );

			TowerLampStartCommand = new RelayCommand( ( ) => TowerLampStart() );
			TowerLampStopCommand = new RelayCommand( ( ) => TowerLampStop() );
			TowerLampErrorCommand = new RelayCommand( ( ) => TowerLampError() );


		}

		public double WaitingPosition
		{
			get => _waitingPosition;
			set
			{
				_waitingPosition = value;
				OnPropertyChanged( nameof( WaitingPosition ) );
				(MoveCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}

		public double EndPosition
		{
			get => _endPosition;
			set
			{
				_endPosition = value;
				OnPropertyChanged( nameof( EndPosition ) );
				(MoveCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}

		public double CurrentPosition
		{
			get => _currentPosition;
			set
			{
				_currentPosition = value;
				OnPropertyChanged( nameof( CurrentPosition ) );
				(MoveCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}

		public double Velocity
		{
			get => _velocity;
			set
			{
				_velocity = value;
				OnPropertyChanged( nameof( Velocity ) );
				(MoveCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}

		public double Acceleration
		{
			get => _acceleration;
			set
			{
				_acceleration = value;
				OnPropertyChanged( nameof( Acceleration ) );
			}
		}

		public double Deceleration
		{
			get => _deceleration;
			set
			{
				_deceleration = value;
				OnPropertyChanged( nameof( Deceleration ) );
			}
		}

		public ICommand EjectONCommand { get; }
		public ICommand EjectOFFCommand { get; }
		public ICommand MainCVOnCommand { get; }
		public ICommand MainCVOffCommand { get; }

		public ICommand TowerLampStartCommand { get; }
		public ICommand TowerLampStopCommand { get; }
		public ICommand TowerLampErrorCommand { get; }


		public ICommand XAxisJogPStartCommand { get; }
		public ICommand XAxisJogNStartCommand { get; }
		public ICommand XAxisJogStopCommand { get; }
		public ICommand ZAxisJogPStartCommand { get; }
		public ICommand ZAxisJogNStartCommand { get; }
		public ICommand ZAxisJogStopCommand { get; }


		public ICommand MoveCommand { get; }
		public ICommand StopCommand { get; }


		private bool CanMove( )
		{
			return !_isMoving && Velocity > 0;
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
			_motionService.ZAxisJogStop();
			// MouseUp 시 실행할 로직
			// ex) CAXM.AxmMoveSStop(axisNo);
		}

		private async Task XAxisMoveToTargetAsync( )
		{
			_isMoving = true;
			(MoveCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();

			try
			{
				await _motionService.XAxisMoveToPosition( _targetPosition, Velocity, Acceleration, Deceleration );
			}
			finally
			{
				_isMoving = false;
				(MoveCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
			}
		}

		private void StopMotion( )
		{
			_motionService.XAxisStop();
		}

		protected void OnPropertyChanged( string propertyName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}
	}
}
