using System.IO;
using JetBrains.Annotations;
using Material.Files.Model;

namespace Material.Files.Operations
{
    public class MoveOperationElement : OperationElementBase
    {
        public MoveOperationElement(string name, FileSystemItemModel from, FileSystemItemModel to)
        {
            this._from = from;
            this.to = to;
        }

        private FileSystemItemModel _from;
        private FileSystemItemModel to;
        
        public override void Invoke(FileExistsAnswer fileExistsArgument)
        {
            StatusText = $"Moving \"{_from.Name}\" to \"{to.Name}\"";

            if (_from is FileModel fromFile)
            {
                if (to is DirectoryModel toDir)
                {
                    if(fileExistsArgument == FileExistsAnswer.None)
                        File.Move(fromFile.FullPath, toDir.FullPath);
                    else if(fileExistsArgument == FileExistsAnswer.Overwrite)
                        File.Move(fromFile.FullPath, toDir.FullPath, true);
                }
            }

            if (_from is DirectoryModel fromDir)
            {
                if (to is DirectoryModel toDir)
                {
                    if(fileExistsArgument == FileExistsAnswer.None)
                        Directory.Move(fromDir.FullPath, toDir.FullPath);
                    // Directory can be just merged into destination folder.
                    // So there are no any "Overwrite" parameter for use.
                }
            }
        }
    }
}