
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WMS.Desktop
{
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _executeAsync;
        private readonly Func<object, Task> _executeAsyncParam;
        private readonly Action _execute;
        private readonly Action<object> _executeParam;
        private readonly Func<bool> _canExecute;

        // Синхронная команда без параметра
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // Синхронная команда с параметром
        public RelayCommand(Action<object> executeParam, Func<bool> canExecute = null)
        {
            _executeParam = executeParam;
            _canExecute = canExecute;
        }

        // Асинхронная без параметра
        public RelayCommand(Func<Task> executeAsync, Func<bool> canExecute = null)
        {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        // Асинхронная с параметром
        public RelayCommand(Func<object, Task> executeAsyncParam, Func<bool> canExecute = null)
        {
            _executeAsyncParam = executeAsyncParam;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public async void Execute(object parameter)
        {
            if (_execute != null) _execute();
            else if (_executeParam != null) _executeParam(parameter);
            else if (_executeAsync != null) await _executeAsync();
            else if (_executeAsyncParam != null) await _executeAsyncParam(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }

}
