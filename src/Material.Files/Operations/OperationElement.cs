using System;

namespace Material.Files.Operations
{
    public abstract class OperationElementBase : ViewModelBase
    {
        public abstract void Invoke(FileExistsAnswer fileExistsArgument);
        
        private double _percentage;
        public double Percentage
        {
            get => _percentage;
            protected set
            {
                _percentage = value;
                OnPropertyChanged();
            }
        }
        
        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            protected set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }
        
        private string _name;
        public string Name
        {
            get => _name;
            protected set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }
}