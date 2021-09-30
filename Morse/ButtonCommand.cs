using System;
using System.Windows.Input;

namespace Morse
{
    public class ButtonCommand : ICommand
    {
        private ViewModel vm;

        public ButtonCommand(ViewModel vm)
        {
            this.vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
        
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            vm.Run();
        }
    }
}