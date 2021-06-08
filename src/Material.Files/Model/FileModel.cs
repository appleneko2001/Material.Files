using Material.Files.Interfaces;
using Material.Styles;
using System.IO;
using System.Text;
using Material.Icons;

namespace Material.Files.Model
{
    public class FileModel : FileSystemItemModel, IHeaderIcon
    {
        private ulong m_FileSize;
        public ulong FileSize { get => m_FileSize; set { m_FileSize = value; OnPropertyChanged(); } }

        private string? m_FileSizeString;
        public string? FileSizeString { get => m_FileSizeString; set => m_FileSizeString = value; }

        private MaterialIconKind _iconKind;
        public MaterialIconKind IconControl => _iconKind;

        private bool _isExecutable;
        public bool IsExecutable => _isExecutable;

        private bool _isScript;
        public bool IsScript => _isScript;

        private bool _isShortcutLink;
        public bool IsShortcutLink => _isShortcutLink;

        private ImageThumbnailModel _imageThumbnail;
        public ImageThumbnailModel ImageThumbnail => _imageThumbnail;

        public string ToolTip
        {
            get
            {
                var builder = new StringBuilder();

                builder.AppendLine($"Filename: {Name}");
                builder.AppendLine($"Creation date: {CreationTime.ToString("f")}");
                builder.AppendLine($"Modified date: {LastWriteTime.ToString("f")}");
                builder.AppendLine($"Size: {FileSizeString} ({FileSize} bytes)");
                builder.AppendLine($"Attributes: {Attributes}");

                return builder.ToString();
            }
        }
        
        public FileModel(FileInfo info) : base(info)
        {
            m_FileSize = (ulong) info.Length;
            FileSizeString = m_FileSize.ToHumanReadableSizeString();

            var alt = info.Extension.ToLower();
            if (alt.StartsWith('.'))
                alt = alt.Remove(0, 1);
            _iconKind = GetIconKind(alt);
            _isExecutable = GetIsExecutable(alt);
            _isScript = GetIsScript(alt);
            _isShortcutLink = alt == "lnk";

            if (TryCreateImageThumbnailModel(this, out var thumbnail))
            {
                _imageThumbnail = thumbnail;
                //thumbnail.GetThumbnailAsync();
            }
        }

        private bool TryCreateImageThumbnailModel(FileModel file, out ImageThumbnailModel o)
        {
            o = null;
            try
            {
                if (file.IconControl == MaterialIconKind.FileImage)
                {
                    o = new ImageThumbnailModel(file.FullPath, delegate
                    {
                        return File.Open(FullPath, FileMode.Open);
                    });
                    return true;
                }
            }
            catch
            {
                
            }
            return false;
        }

        private static bool GetIsScript(string ext)
        {
            switch (ext)
            {
                case "sh":
                case "bat":
                case "cmd":
                    return true;

                default:
                    return false;
            }
        }
        
        private static bool GetIsExecutable(string ext)
        {
            switch (ext)
            {
                case "sh":
                case "bat":
                case "cmd":
                case "exe":
                case "com":
                    return true;

                default:
                    return false;
            }
        }
        
        private static MaterialIconKind GetIconKind(string ext)
        {
            switch (ext)
            {
                case "cfg":
                case "ini":
                case "json":
                case "dll":
                case "sys":
                    return MaterialIconKind.FileCog;
                
                case "exe":
                    return MaterialIconKind.CogBox;
                
                case "bmp":
                case "jpg":
                case "jpeg":
                case "png":
                case "gif":
                case "ico":
                case "webp":
                    return MaterialIconKind.FileImage;
                
                case "pdf":
                    return MaterialIconKind.FilePdf;
                
                case "mp3":
                case "aac":
                case "flac":
                case "wav":
                    return MaterialIconKind.FileMusic;
                
                case "mp4":
                case "avi":
                case "mkv":
                case "flv":
                    return MaterialIconKind.FileVideo;
                
                case "zip":case "rar":case "7z":case "tar":case "gz":case "arc":
                    return MaterialIconKind.ZipBox;

                case "sh":
                case "bat":
                case "cmd":
                case "txt":
                case "log":
                    return MaterialIconKind.ScriptText;
                
                case "lnk":
                    return MaterialIconKind.FileLink;

                default:
                    return MaterialIconKind.File;
            }
        }

        public override string ToString()
        {
            return $"\"{Name}\", Size={FileSizeString}";
        }
    }
}
