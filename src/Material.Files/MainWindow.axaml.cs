using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Material.Files.Model;
using Material.Files.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Data.Core;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Material.Files.Gestures;

namespace Material.Files
{
    public class MainWindow : Window
    {
        private static MainWindow _instance;
        public static MainWindow Instance => _instance;

        private MainWindowViewModel Context;
        private ScrollViewer? BreadcrumbsViewer;
        private ContextMenu _contextMenu;

        public MainWindow()
        {
            _instance = this;
            PointerReleased += delegate(object? sender, PointerReleasedEventArgs args)
            {
                if (args.InitialPressMouseButton == MouseButton.XButton1)
                    OnMouseBackButtonPressed(sender, args);
            };

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.DataContext = Context = new MainWindowViewModel(this);

            Context.UpdateDrives();
            //Context.UpdateCollection(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            BreadcrumbsViewer = this.Get<ScrollViewer>(nameof(BreadcrumbsViewer));
        }

        private void DriveList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
                DriveSelectionChanged(sender, null);
        }

        private async void DriveSelectionChanged(object sender, PointerReleasedEventArgs args)
        {
            var listBox = sender as ListBox;
            if (!listBox?.IsKeyboardFocusWithin ?? false)
                return;
            FocusLastBreadcrumb();
        }

        private void FocusLastBreadcrumb()
        {
            var max = BreadcrumbsViewer?.GetValue(ScrollViewer.HorizontalScrollBarMaximumProperty) ?? 0.0;
            BreadcrumbsViewer?.SetValue(ScrollViewer.HorizontalScrollBarValueProperty, max);
        }

        private async void BlockViewItemButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (!(Context.SelectedSession is SessionWindowViewModel session))
                return;

            if (sender is Button b)
            {
                if (session.SelectedItem == b.DataContext)
                {
                    await session?.Open(session.SelectedItem as FileSystemItemModel);
                }

                session.AppendSingleSelect(b.DataContext as FileSystemItemModel);
            }
        }

        private async void OnMouseBackButtonPressed(object sender, RoutedEventArgs e)
        {
            if (!(Context.SelectedSession is SessionWindowViewModel session))
                return;

            var targetPath = session?.GetPreviousPath();
            if (targetPath != null)
            {
                await session?.OpenDirectoryWithoutRecordHistory(targetPath);
            }
        }

        private void RecyclingItemTemplateBlockView_OnSelectTemplateKey(object? sender, SelectTemplateEventArgs e)
        {
            e.TemplateKey = "ItemTemplateBlockView";
        }

        private void CreateSessionButton_OnClick(object? sender, RoutedEventArgs e)
        {
            Context.CreateSession();
        }

        private void DriveButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button b)
                Context.OpenDriveCommand.Execute(b.DataContext);
        }

        private async void PART_ItemButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!(Context.SelectedSession is SessionWindowViewModel session))
                return;

            var prop = e.GetCurrentPoint(sender as IVisual).Properties;
            if (sender is Border b && (prop.IsRightButtonPressed || prop.IsLeftButtonPressed))
            {
                session.AppendSingleSelect(b.DataContext as FileSystemItemModel);
                
                if (session.SelectedItem == b.DataContext)
                {
                    if (prop.IsLeftButtonPressed)
                        session.PointerClickSelectCount++;
                }
            }
        }

        private async void PART_ItemButton_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!(Context.SelectedSession is SessionWindowViewModel session))
                return;

            if (!(sender is Visual visual))
                return;

            if (visual.GetVisualsAt(e.GetPosition(visual)).Any(delegate(IVisual c)
            {
                var result = visual == c || visual.IsVisualAncestorOf(c);
                return result;
            }))
            {
                var pointer = e.GetCurrentPoint(visual);
                var prop = pointer.Properties;

                if (session.SelectedItem == visual.DataContext)
                {
                    if (prop.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
                        if (session.PointerClickSelectCount >= 2)
                        {
                            session.PointerClickSelectCount = 0;
                            await session?.Open(session.SelectedItem as FileSystemItemModel);
                        }
                }
            }
        }

        private void CloseSessionButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (!(sender is Visual visual))
                return;

            if (!(visual.DataContext is SessionWindowViewModel session))
                return;

            Context.SessionWindows.Remove(session);
        }

        private async void PART_ItemButton_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (!(sender is Border v))
                return;
        }
    }
}