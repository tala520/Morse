using System;
using System.Windows.Input;

namespace Morse
{
    public class ClickCommand : ICommand
    {
        private readonly ViewModel _vm;

        public ClickCommand(ViewModel vm)
        {
            _vm = vm;
        }
        
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
           _vm.ReceiveConfirm();
        }

        public event EventHandler CanExecuteChanged;
    }
}