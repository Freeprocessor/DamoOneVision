using System;
using System.Windows.Input;
//ICommand 인터페이스를 간편하게 구현할 수 있도록 도와주는 도움 클래스
namespace DamoOneVision.ViewModels
{
	public class RelayCommand : ICommand
	{
		private readonly Action<object> _execute;
		private readonly Func<object, bool> _canExecute;

		public event EventHandler CanExecuteChanged;

		public RelayCommand( Action<object> execute, Func<object, bool> canExecute = null )
		{
			_execute = execute ?? throw new ArgumentNullException( nameof( execute ) );
			_canExecute = canExecute;
		}

		public bool CanExecute( object parameter ) => _canExecute == null || _canExecute( parameter );

		public void Execute( object parameter ) => _execute( parameter );

		public void RaiseCanExecuteChanged( ) => CanExecuteChanged?.Invoke( this, EventArgs.Empty );
	}
}
