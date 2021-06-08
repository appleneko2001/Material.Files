using Avalonia.Controls;
using Avalonia.Interactivity;
using Material.Files.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Material.Dialog;
using Material.Dialog.Enums;
using Material.Dialog.Icons;
using Material.Files.Model;

namespace Material.Files.Commands
{
    public static class ContextMenuCommands
    {
        public static RelayCommand DeleteCommand => new RelayCommand(DeleteCommand_Execute, DeleteCommand_CanExecute);

        private static bool DeleteCommand_CanExecute(object arg)
        {
            if(ValidateViewModel(arg, out var vm))
            {
                if(vm?.SelectedItemsCollection.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static async void DeleteCommand_Execute(object obj)
        {
            if(ValidateViewModel(obj, out var vm))
            {
                if(vm?.SelectedItemsCollection.Count == 0 )
                {
                    return;
                }
            }

            var path = vm.CurrentPath;
            var targets = vm?.SelectedItemsCollection.ToArray();

            var builder = new StringBuilder();

            var tips = targets.Length == 1 ? $"{targets.First()}" : $"{targets.Length} items";
            
            builder.Append($"Are you sure to delete {tips} ? This action can be undone!");
            
            var supporting = builder.ToString();
            var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams()
            {
                StartupLocation = WindowStartupLocation.CenterOwner,
                Borderless = false, ContentHeader = "Confirm to delete",
                DialogHeaderIcon = DialogIconKind.Warning, WindowTitle = "Confirm to delete",
                SupportingText = supporting,
                DialogButtons = DialogHelper.CreateSimpleDialogButtons(DialogButtonsEnum.YesNo)
            });

            if ((await dialog.ShowDialog(MainWindow.Instance)).GetResult == "yes")
            {
                foreach (var item in targets)
                {
                    if (item is FileModel m)
                    {
                        File.Delete(Path.Combine(path, m.Name));
                    }
                    else if (item is DirectoryModel d)
                    {
                        Directory.Delete(Path.Combine(path, d.Name));
                    }
                }
            } 
        }

        public static RelayCommand CutCommand => new RelayCommand(OpenCommand_Execute, OpenCommand_CanExecute);
        
        public static RelayCommand OpenCommand => new RelayCommand(OpenCommand_Execute, OpenCommand_CanExecute);
        
        public static RelayCommand RunCommand => new RelayCommand(OpenCommand_Execute, RunCommand_CanExecute);
        
        public static RelayCommand ModifyScriptCommand => new RelayCommand(ModifyScriptCommand_Execute, ModifyScriptCommand_CanExecute);
        
        public static RelayCommand OpenWithCommand => new RelayCommand(OpenWithCommand_Execute, OpenOpenWithCommand_CanExecute);

        private static void OpenWithCommand_Execute(object obj)
        {
            if (ValidateViewModel(obj, out var vm))
            {
                var item = vm.SelectedItem as Model.FileSystemItemModel;
                Utils.ShowOpenWithDialog(vm.GetPath(item.Name));
            }
        }

        private static bool OpenOpenWithCommand_CanExecute(object arg)
        {
            if(ValidateViewModel(arg, out var vm))
            {
                if (vm?.SelectedItem is FileModel m)
                {
                    // To prevent using "Open with" feature to break default behaviour of running executable files,
                    // We have to disable this command for this case.
                    if (m.IsExecutable || m.IsShortcutLink)
                        return false;
                        
                    return true;
                }
            }
            return false;
        }

        public static RelayCommand OpenInNewTabCommand => new RelayCommand(OpenInNewTabCommand_Execute, OpenInNewTabCommand_CanExecute);

        public static void ContextMenuOpen(object sender, RoutedEventArgs args)
        {
            OpenCommand.RaiseCanExecute();
            OpenInNewTabCommand.RaiseCanExecute();
        }

        private static bool ValidateViewModel(object param, out SessionWindowViewModel? model)
        {
            model = param as SessionWindowViewModel;
            return model != null;
        }

        private static bool ModifyScriptCommand_CanExecute(object param)
        {
            if(ValidateViewModel(param, out var vm))
            {
                if(vm?.SelectedItem is FileModel m)
                {
                    if (m.IsScript)
                        return true;
                }
            }
            return false;
        }
        private static void ModifyScriptCommand_Execute(object param)
        {
            if (ValidateViewModel(param, out var vm))
            {
                if(vm?.SelectedItem is FileModel m)
                {
                    if (m.IsScript)
                        Utils.RunProcess("notepad", m.FullPath);
                }
            }
        }
        
        private static bool RunCommand_CanExecute(object param)
        {
            if(ValidateViewModel(param, out var vm))
            {
                if(vm?.SelectedItem is FileModel m)
                {
                    if (m.IsExecutable || m.IsShortcutLink)
                        return true;
                }
            }
            return false;
        }
        
        private static bool OpenCommand_CanExecute(object param)
        {
            if(ValidateViewModel(param, out var vm))
            {
                if(vm?.SelectedItem is FileModel m)
                {
                    // To prevent using "Open with" feature to break default behaviour of running executable files,
                    // We have to disable this command for this case.
                    if (m.IsExecutable || m.IsShortcutLink)
                        return false;
                    return true;
                }
                else if (vm?.SelectedItem is DirectoryModel)
                    return true;
            }
            return false;
        }

        private static void OpenCommand_Execute(object param)
        {
            if (ValidateViewModel(param, out var vm))
            {
                vm.Open(vm.SelectedItem as Model.FileSystemItemModel);
            }
        }
        
        private static bool OpenInNewTabCommand_CanExecute(object param)
        {
            if(ValidateViewModel(param, out var vm))
            {
                if(vm?.SelectedItem != null)
                {
                    return vm?.SelectedItem is DirectoryModel;
                }
            }
            return false;
        }

        private static void OpenInNewTabCommand_Execute(object param)
        {
            if (ValidateViewModel(param, out var vm))
            {
                var session = vm.MainWindowViewModel.CreateSession();

                if (vm.SelectedItem is Model.FileSystemItemModel file)
                {
                    var path = Path.Combine(vm.CurrentPath, file.Name);
                    session.UpdateCollection(path);
                }
            }
        }
    }
}
