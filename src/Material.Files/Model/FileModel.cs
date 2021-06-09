using System;
using Material.Files.Interfaces;
using Material.Styles;
using System.IO;
using System.Text;
using System.Threading;
using Material.Files.Resolvers;
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

        private MimeType _mimeType;

        public string ToolTip
        {
            get
            {
                var builder = new StringBuilder();

                builder.AppendLine($"Filename: {Name}");
                builder.AppendLine($"Creation date: {CreationTime.ToString("f")}");
                builder.AppendLine($"Modified date: {LastWriteTime.ToString("f")}");
                builder.AppendLine($"Size: {FileSizeString} ({FileSize} bytes)");
                builder.AppendLine($"Known MIME-type: {_mimeType.Name} ({_mimeType.ToString()})");
                builder.AppendLine($"Attributes: {Attributes}");

                return builder.ToString();
            }
        }
        
        public FileModel(FileInfo info, CancellationToken ctx = default) : base(info)
        {
            m_FileSize = (ulong) info.Length;
            FileSizeString = m_FileSize.ToHumanReadableSizeString();

            var alt = info.Extension.ToLower();
            if (alt.StartsWith('.'))
                alt = alt.Remove(0, 1);
            _isExecutable = GetIsExecutable(alt);
            _isScript = GetIsScript(alt);
            _isShortcutLink = alt == "lnk";

            _mimeType = MimeTypesDatabase.GetMime(alt);
            _iconKind = GetIconKind();

            if (TryCreateImageThumbnailModel(this, out var thumbnail))
            {
                _imageThumbnail = thumbnail;
                thumbnail.SetCancellationToken(ctx);
                //thumbnail.GetThumbnailAsync();
            }
        }

        private bool TryCreateImageThumbnailModel(FileModel file, out ImageThumbnailModel o)
        {
            o = null;
            try
            {
                if (file._mimeType.Type == "image")
                {
                    //var hashResult = App.HashCompute.ComputeHash(Encoding.UTF8.GetBytes(file.FullPath));
                    //var cacheId = hashResult.AsHexString(true);
                    var cacheId = file.FullPath;
                    o = new ImageThumbnailModel(cacheId, delegate
                    {
                        return File.Open(FullPath, FileMode.Open);
                    });
                    return true;
                }
            }
            catch(Exception e)
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
        
        private MaterialIconKind GetIconKind()
        {
            if (_mimeType.Icon is MaterialIconKind k)
                return k;
            return (MaterialIconKind)MimeTypesDatabase.OctetStreamMime.Icon;
        }

        public override string ToString()
        {
            return $"\"{Name}\", Size={FileSizeString}";
        }
    }
}
