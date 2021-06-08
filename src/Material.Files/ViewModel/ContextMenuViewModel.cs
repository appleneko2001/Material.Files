using Material.Files.Commands;

namespace Material.Files.ViewModel
{
    public class ContextMenuViewModel : ViewModelBase
    {
        public RelayCommand OpenCommand => ContextMenuCommands.OpenCommand;
        public RelayCommand OpenInNewTabCommand => ContextMenuCommands.OpenInNewTabCommand;
    }
}