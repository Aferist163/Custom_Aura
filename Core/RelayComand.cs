using System;
using System.Windows.Input;

namespace Custom_Aura.Core
{
    class RelayComand : ICommand
    {
        private Action<object> _execute;
        private Func<object, bool> _CanExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayComand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _CanExecute = canExecute;
        }

        public bool CanExecute(object parameter) 
        {
            return _CanExecute == null || _CanExecute(parameter);
        }

        public void Execute(object parameter) 
        {
            _execute(parameter);
        }
    }
}
