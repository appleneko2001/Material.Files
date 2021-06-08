using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Material.Files.Commands;
using Material.Files.ViewModel;

namespace Material.Files.Model
{
    public class BreadcrumbsModel : ViewModelBase
    {
        private string? m_Text;
        public string? Text { get => m_Text; set { m_Text = value; OnPropertyChanged(); } }

        private string? m_Prefix;
        public string? Prefix { get => m_Prefix; set => m_Prefix = value;  }

        public ulong ToIndex;

        public RelayCommand JumpIntoPathCommand => new RelayCommand(JumpIntoPathCommandExecuted);

        private void JumpIntoPathCommandExecuted(object obj)
        {
            if (!(obj is SessionWindowViewModel session))
                return;

            session.ToPath(ToIndex);
        }
        
        public override string ToString()
        {
            return $"{Prefix}{Text}";
        }
    }
}
