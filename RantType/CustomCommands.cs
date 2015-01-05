using System;
using System.Windows.Input;

namespace RantType
{
	class ShowSettingsCommand : ICommand
	{
		public bool CanExecute(object parameter) => true;

		public void Execute(object parameter)
		{
			Executed?.Invoke(this, new EventArgs());
		}

		public event EventHandler CanExecuteChanged;

		public event EventHandler Executed;
	}
}
