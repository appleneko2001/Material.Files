using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Material.Icons;

namespace Material.Files.Model
{
    public class DriveInfoModel : ViewModelBase
    {
        public DriveInfoModel(System.IO.DriveInfo info)
        {
            m_Name = info.Name;
            m_Label = info.VolumeLabel;
            Path = info.RootDirectory.FullName;
            
            switch (info.DriveType)
            {
                case DriveType.Fixed:
                    _iconKind = MaterialIconKind.Hdd;
                    break;
                case DriveType.Removable:
                    _iconKind = MaterialIconKind.UsbFlashDrive;
                    break;
                case DriveType.CDRom:
                    _iconKind = MaterialIconKind.Album;
                    break;
                case DriveType.Network:
                    _iconKind = MaterialIconKind.FolderNetwork;
                    break;
                case DriveType.Ram:
                    _iconKind = MaterialIconKind.Memory;
                    break;
                default:
                    _iconKind = MaterialIconKind.Help;
                    break;
            }
        }

        public readonly string Path;

        private string? m_Name;
        public string? Name { get => m_Name; set { m_Name = value; OnPropertyChanged(); } }

        private string? m_Label;
        public string? Label { get => m_Label; set { m_Label = value; OnPropertyChanged(); } }

        private MaterialIconKind _iconKind;
        public MaterialIconKind IconKind => _iconKind;
        
        public string ToolTip
        {
            get
            {
                var info = new DriveInfo(Name);
                var builder = new StringBuilder();

                builder.AppendLine($"{info.Name}  ({(!string.IsNullOrWhiteSpace(info.VolumeLabel) ? info.VolumeLabel : "NO NAME")})");
                builder.AppendLine();
                

                var used = info.TotalSize - info.TotalFreeSpace;
                
                builder.AppendLine($"Total disk space: {info.TotalSize.ToHumanReadableSizeString()}");
                builder.AppendLine($"Used: {(used).ToHumanReadableSizeString()}, " +
                                   $"Remains: {info.TotalFreeSpace.ToHumanReadableSizeString()}");
                builder.AppendLine($"File system: {info.DriveFormat}");

                return builder.ToString();
            }
        }

        public override string ToString()
        {
            return m_Name ?? "Unknown Drive";
        }
    }
}
