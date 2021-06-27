using System;
using System.Windows.Input;

namespace WPFWrappedMenu.Commands
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _execute;

        private readonly Func<object, bool> _canExecute;

        public DelegateCommand(Action<object> execute) : this(execute, o => true)
        {
        }

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return false;
            }

            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (_execute == null)
            {
                return;
            }

            _execute(parameter);
        }

        private EventHandler _canExecuteChanged;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                _canExecuteChanged += value;
                CommandManager.RequerySuggested -= CommandManager_RequerySuggested;
                CommandManager.RequerySuggested += CommandManager_RequerySuggested;
            }
            remove
            {
                _canExecuteChanged -= value;
                CommandManager.RequerySuggested -= CommandManager_RequerySuggested;
            }
        }

        private void CommandManager_RequerySuggested(object sender, EventArgs e)
        {
            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            _canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
