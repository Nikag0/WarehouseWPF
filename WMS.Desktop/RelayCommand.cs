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
        private readonly Func<Task> _execute;

        public RelayCommand(Func<Task> execute)
        {
            _execute = execute;
        }

        public async void Execute(object? parameter)
        {
            await _execute();
        }

        public bool CanExecute(object? parameter) => true;

        public event EventHandler? CanExecuteChanged;
    }
}
