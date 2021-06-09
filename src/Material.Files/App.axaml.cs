using System;
using System.IO;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Material.Files.Commands;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Material.Files
{
    public class App : Application
    {
        public static string TargetCacheFolder { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Material.Files", "Caches");
        
        public static string ThumbnailsCacheFolder { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Material.Files", "Caches", "Thumbnails");

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            UpdateFileContextMenus();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private const string ContextMenuResKey = "FileContextMenus";

        public void UpdateFileContextMenus()
        {
            /*
        <MenuItem Header="Open" Command="{x:Static commands:ContextMenuCommands.OpenCommand}" CommandParameter="{Binding }"/>
        <MenuItem Header="Open as..." Command="{x:Static commands:ContextMenuCommands.OpenWithCommand}" CommandParameter="{Binding }"/>
        <MenuItem Header="Open in new tab" Command="{x:Static commands:ContextMenuCommands.OpenInNewTabCommand}" CommandParameter="{Binding }"/>
        <Separator/>
        <MenuItem Header="Cut" Icon="{ext:MaterialIconExt Kind=ContentCut}"/>
        <MenuItem Header="Copy" Icon="{ext:MaterialIconExt Kind=ContentCopy}"/>
        <MenuItem Header="Paste" Icon="{ext:MaterialIconExt Kind=ContentPaste}"/>
        <Separator/>
        <MenuItem Header="Delete" Icon="{ext:MaterialIconExt Kind=TrashCan}" Command="{x:Static commands:ContextMenuCommands.DeleteCommand}" CommandParameter="{Binding }"/>
        <MenuItem Header="Rename"/>
        <Separator/>
        <MenuItem Header="Properties"/>
             */
            Dispatcher.UIThread.Post(delegate
            {
                var items = new AvaloniaList<Visual>();
                var menu = new ContextMenu()
                {
                    Items = items
                };
                menu.AttachedToLogicalTree += MenuOnAttachedToLogicalTree;
                
                var binding = new Binding("");

                int AddMenuItem(ICommand command, string header, MaterialIconKind? icon = null, string tooltip = null, bool hideIfCannotExecute = false)
                {
                    var item = new MenuItem()
                    {
                        Command = command,
                        Header = header
                    };
                    
                    // Initiate delayed binding to apply binding only when ui element initialized.
                    DelayedBinding.Add(item, MenuItem.CommandParameterProperty, binding);

                    if (hideIfCannotExecute)
                    {
                        var isVisibleBinding = new Binding("$self.IsEffectivelyEnabled");
                        DelayedBinding.Add(item, MenuItem.IsVisibleProperty, isVisibleBinding);
                    }
                    
                    if (icon != null)
                    {
                        item.Icon = new MaterialIcon() {Kind = icon.Value};
                    }

                    if (tooltip != null)
                    {
                        item.SetValue(ToolTip.TipProperty, tooltip);
                    }

                    items.Add(item);

                    return items.IndexOf(item);
                }

                int AddSeparator()
                {
                    var item = new Separator();
                    items.Add(item);
                    return items.IndexOf(item);
                }

                AddMenuItem(ContextMenuCommands.RunCommand, "Run", hideIfCannotExecute:true);
                AddMenuItem(ContextMenuCommands.ModifyScriptCommand, "Edit script", hideIfCannotExecute:true);
                AddMenuItem(ContextMenuCommands.OpenCommand, "Open", hideIfCannotExecute:true);
                AddMenuItem(ContextMenuCommands.OpenWithCommand, "Open as...", hideIfCannotExecute:true);
                AddMenuItem(ContextMenuCommands.OpenInNewTabCommand, "Open in new tab", hideIfCannotExecute:true);
                AddSeparator();
                AddMenuItem(ContextMenuCommands.DeleteCommand, "Delete", MaterialIconKind.TrashCan);
                AddMenuItem(null, "Rename", MaterialIconKind.LeadPencil);
                AddSeparator();
                AddMenuItem(null, "Properties");

                if (this.Resources.ContainsKey(ContextMenuResKey))
                {
                    var visual = this.Resources[ContextMenuResKey] as Visual;
                    visual.AttachedToLogicalTree -= MenuOnAttachedToLogicalTree;
                    
                    this.Resources[ContextMenuResKey] = menu;
                }
                else
                {
                    this.Resources.Add(ContextMenuResKey, menu);
                }
            });
        }

        private void MenuOnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
        {
            if (!(sender is Visual v))
                return;
        }
    }
}