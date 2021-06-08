using System;
using System.IO;

namespace Material.Files.Model
{
    public class FileSystemItemModel : ViewModelBase
    {
        public FileSystemItemModel(FileSystemInfo info)
        {
            m_Name = info.Name;
            _fullPath = info.FullName;
            m_CreationTime = info.CreationTime;
            m_LastAccessTime = info.LastAccessTime;
            m_LastWriteTime = info.LastWriteTime;
            m_Attributes = info.Attributes;
        }

        private string? _fullPath;
        public string FullPath => _fullPath;

        private string? m_Name;
        public string? Name { get => m_Name; set { m_Name = value; OnPropertyChanged(); } }

        private DateTime m_CreationTime;
        public DateTime CreationTime { get => m_CreationTime; set { m_CreationTime = value; OnPropertyChanged(); } }

        private DateTime m_LastAccessTime;
        public DateTime LastAccessTime { get => m_LastAccessTime; set { m_LastAccessTime = value; OnPropertyChanged(); } }

        private DateTime m_LastWriteTime;
        public DateTime LastWriteTime { get => m_LastWriteTime; set { m_LastWriteTime = value; OnPropertyChanged(); } }

        private FileAttributes m_Attributes;
        public FileAttributes Attributes { get => m_Attributes; set { m_Attributes = value; OnPropertyChanged(); } }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public bool IsHidden => Attributes.HasFlag(FileAttributes.Hidden);

        public bool IsSymbolicLink => Attributes.HasFlag(FileAttributes.ReparsePoint);
        
        public double Opacity => IsHidden ? 0.5 : 1.0;
    }
}
