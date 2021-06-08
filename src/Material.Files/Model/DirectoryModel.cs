using Material.Files.Interfaces;
using Material.Styles;
using System.IO;
using System.Text;
using Material.Icons;

namespace Material.Files.Model
{
    public class DirectoryModel : FileSystemItemModel, IHeaderIcon
    {
        public DirectoryModel(DirectoryInfo info) : base(info)
        {
        }
        
        public string ToolTip
        {
            get
            {
                var builder = new StringBuilder();

                builder.AppendLine($"Folder name: {Name}");
                builder.AppendLine($"Creation date: {CreationTime.ToString("f")}");
                builder.AppendLine($"Modified date: {LastWriteTime.ToString("f")}");
                builder.AppendLine($"Attributes: {Attributes}");

                return builder.ToString();
            }
        }

        public MaterialIconKind IconControl => MaterialIconKind.Folder;

        public override string ToString()
        {
            return $"\"{Name}\"";
        }
    }
}
