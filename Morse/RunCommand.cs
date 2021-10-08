using System;
using System.Windows.Input;

namespace Morse
{
    public class RunCommand : ICommand
    {
        private readonly ViewModel _vm;

        public RunCommand(ViewModel vm)
        {
            _vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
        
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _vm.Run();
        }
    }
}