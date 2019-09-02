using System;
using System.Windows.Input;

namespace Ajuro.WPF.Base.Commands
	{
		class SaveContent : ICommand
		{
			Action<object> executeMethod;
			Func<object, bool> canExecuteMethod;


			public SaveContent(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
			{
				this.executeMethod = executeMethod;
				this.canExecuteMethod = canExecuteMethod;
			}

			// public event EventHandler CanExecuteChanged;
			public event EventHandler CanExecuteChanged
			{
				add { CommandManager.RequerySuggested += value; }
				remove { CommandManager.RequerySuggested -= value; }
			}

			public bool CanExecute(object parameter)
			{
				return true;
			}

			public void Execute(object parameter)
			{
				executeMethod(parameter);
			}
		}
	}
