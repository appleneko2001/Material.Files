using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Threading;

namespace Material.Files
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Dispatcher.UIThread.Post(delegate
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }
}