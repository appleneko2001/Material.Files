using Material.Icons;

namespace Material.Files.ViewModel
{
    public class ContextHeaderButtonViewModel : ViewModelBase
    {
        public ContextHeaderButtonViewModel(MaterialIconKind icon, string tooltip)
        {
            _icon = icon;
            _tooltip = tooltip;
        }
        
        private MaterialIconKind _icon;
        public MaterialIconKind Icon => _icon;

        private string _tooltip;
        public string ToolTip => _tooltip;
    }
}