using Ajuro.Net.User.Model;
using System;
using System.Windows.Input;

namespace Ajuro.WPF.Base.Commands
{
	class ManageAccounts : ICommand
	{
		Action<IdentityModel> executeMethod;
		Func<object, bool> canExecuteMethod;
		IdentityModel identity;

		public ManageAccounts(Action<IdentityModel> executeMethod, Func<object, bool> canExecuteMethod, IdentityModel identity)
		{
			this.executeMethod = executeMethod;
			this.canExecuteMethod = canExecuteMethod;
			this.identity = identity;
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			executeMethod((IdentityModel)parameter);
		}
	}
}