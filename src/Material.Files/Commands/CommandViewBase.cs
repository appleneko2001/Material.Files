using System;

namespace Material.Files.Commands
{
    /// <summary>
    /// CommandViewModel is a view model object that archived feature of RelayCommand, and it can be used by Binding.
    /// </summary>
    public class CommandViewModel : ViewModelBase
    {
        public CommandViewModel(Action<object> execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }
        
        private Action<object> execute;
        public void Invoke(object param) => execute(param);
        
        private Func<bool> canExecute;
        public bool CanExecuted
        {
            get
            {
                var result = this.canExecute == null || this.canExecute();
                return result;
            }
        }

        public void RaiseCanExecuted() => OnPropertyChanged(nameof(CanExecuted));
    }
}