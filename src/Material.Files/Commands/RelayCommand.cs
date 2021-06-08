using System; 
using System.Windows.Input;
using Avalonia.Logging;
using Avalonia.Threading;

namespace Material.Files.Commands
{
    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;
        private event EventHandler _canExecuteChanged;
        
        private bool binded = false;
        
        public event EventHandler CanExecuteChanged
        {
            add 
            {
                _canExecuteChanged += value;
                binded = true;
                value?.Invoke(this, new EventArgs());
            }
            remove
            {
                _canExecuteChanged -= value;
                binded = false;
            }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (!binded)
                return true;
            
            var result = this.canExecute == null || this.canExecute(parameter);
            return result;
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }

        // Call this method to tell AvaloniaUI about this command can be executed at this moment.
        public void RaiseCanExecute()
        {
            // Call CanExecute via Dispatcher.UIThread.Post to prevent CanExecute can't be called from other thread.
            Dispatcher.UIThread.Post(delegate
            {
                _canExecuteChanged?.Invoke(this, new EventArgs());
            });
        } 
    }
}
