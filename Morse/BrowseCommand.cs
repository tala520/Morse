using System;
using System.Windows.Input;

namespace Morse
{
    public class BrowseCommand : ICommand
    {
        private readonly ViewModel _vm;

        public BrowseCommand(ViewModel vm)
        {
            _vm = vm;
        }
        
        public bool CanExecute(object parameter)
        {
            return _vm.CanBrowseFile();
        }

        public void Execute(object parameter)
        {
            _vm.BrowseFile();
        }

        public event EventHandler CanExecuteChanged;
    }
}