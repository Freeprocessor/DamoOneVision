﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Input;

//namespace DamoOneVision.ViewModels
//{
//	internal class AsyncRelayCommand:ICommand
//	{
//		private readonly Func<object, Task> _execute;
//		private readonly Predicate<object> _canExecute;
//		private bool _isExecuting;

//		public event EventHandler CanExecuteChanged;

//		public AsyncRelayCommand( Func<object, Task> execute, Predicate<object> canExecute = null )
//		{
//			_execute = execute ?? throw new ArgumentNullException( nameof( execute ) );
//			_canExecute = canExecute;
//		}

//		public bool CanExecute( object parameter )
//		{
//			return !_isExecuting && (_canExecute == null || _canExecute( parameter ));
//		}

//		public async void Execute( object parameter )
//		{
//			_isExecuting = true;
//			RaiseCanExecuteChanged();
//			try
//			{
//				await _execute( parameter );
//			}
//			finally
//			{
//				_isExecuting = false;
//				RaiseCanExecuteChanged();
//			}
//		}

//		public void RaiseCanExecuteChanged( )
//		{
//			CanExecuteChanged?.Invoke( this, EventArgs.Empty );
//		}
//	}
//}
