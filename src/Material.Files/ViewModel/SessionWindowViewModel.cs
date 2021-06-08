using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Threading;
using Material.Dialog;
using Material.Files.Commands;
using Material.Files.Model;
using Material.Icons;

namespace Material.Files.ViewModel
{
    public class SessionWindowViewModel : ViewModelBase
    {
        private MainWindowViewModel _mainWindowViewModel;
        public MainWindowViewModel MainWindowViewModel => _mainWindowViewModel;
        
        public SessionWindowViewModel(MainWindowViewModel mainViewModel)
        {
            _mainWindowViewModel = mainViewModel;
            m_PathBreadcrumbs = new ObservableCollection<BreadcrumbsModel>();
            
            _contextHeaderButtons = new ContextHeaderButtonViewModel[]
            {
                new ContextHeaderButtonViewModel(MaterialIconKind.Refresh, "Refresh"),
                new ContextHeaderButtonViewModel(MaterialIconKind.SelectAll, "Select all"),
                new ContextHeaderButtonViewModel(MaterialIconKind.InformationOutline, "Properties"),
            };
            
            InitFileExplorerViewModel();
            InitBrowserHistoryStack();
            InitItemSelectionViewModel();
        }
        
        #region Common properties

        private ContextHeaderButtonViewModel[] _contextHeaderButtons;
        public ContextHeaderButtonViewModel[] ContextHeaderButton => _contextHeaderButtons;
        
        /// <summary>
        /// Current browser location
        /// </summary>
        public string? CurrentPath;

        private string? m_CurrentDirectory;

        /// <summary>
        /// Current directory name (NOT LOCATION!)
        /// </summary>
        public string? CurrentDirectory
        {
            get => m_CurrentDirectory;
            set
            {
                m_CurrentDirectory = value;
                OnPropertyChanged();
            }
        }

        private string? m_CurrentDirectorySupporting;

        /// <summary>
        /// Current directory supporting text (Text under main header)
        /// </summary>
        public string? CurrentDirectorySupporting
        {
            get => m_CurrentDirectorySupporting;
            set
            {
                m_CurrentDirectorySupporting = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Main status view model

        /// <summary>
        /// Get the value about explorer is enumerating files and folders.
        /// </summary>
        public bool IsRunningTask => RunningTask != null;

        private string _contentSupportingText;
        
        /// <summary>
        /// Get and set the inner text of explorer view. It mostly used for show errors.
        /// </summary>
        public string ContentSupportingText
        {
            get => _contentSupportingText;
            set
            {
                _contentSupportingText = value;
                OnPropertyChanged();
            }
        }
        
        #endregion
        
        #region Selection-related things

        /// <summary>
        /// Get the clicked counts of selected item.
        /// </summary>
        public int PointerClickSelectCount = 0;
        
        private ObservableCollection<object> selectedItemsCollection;
        public ObservableCollection<object> SelectedItemsCollection => selectedItemsCollection;
        
        private object? m_SelectedItem;

        public object? SelectedItem
        {
            get => m_SelectedItem;
            set
            {
                if (m_SelectedItem != value)
                {
                    m_SelectedItem = value;
                    PointerClickSelectCount = 0; 
                }
                OnPropertyChanged();
                ValidateContextMenu();
            }
        }
        
        public void InitItemSelectionViewModel()
        {
            selectedItemsCollection = new ObservableCollection<object>();
            selectedItemsCollection.CollectionChanged += SelectedItemsCollectionOnCollectionChanged;
        }
        
        private void SelectedItemsCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if(e.NewItems != null)
                    foreach (FileSystemItemModel item in e.NewItems)
                    {
                        item.IsSelected = true;
                    }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
            {
                if(e.OldItems != null)
                    foreach (FileSystemItemModel item in e.OldItems)
                    {
                        item.IsSelected = false;
                    }
            }
        }
        
        public void AppendSingleSelect(FileSystemItemModel v)
        {
            for (int i = SelectedItemsCollection.Count - 1; i >= 0; i--)
            {
                SelectedItemsCollection.RemoveAt(i);
            }
            SelectedItemsCollection.Add(v);
            SelectedItem = v;
        }

        #endregion

        #region File and folders viewer view model

        private void InitFileExplorerViewModel()
        {
            m_ContentCollections = new ObservableCollection<object>();
            m_ContentCollections.CollectionChanged += ContentCollectionsOnCollectionChanged;
        }
        
        private void ContentCollectionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                selectedItemsCollection.Clear();
                SelectedItem = null;
                PointerClickSelectCount = 0;
            }
        }
        
        private ObservableCollection<object>? m_ContentCollections;
        public ObservableCollection<object>? ContentCollections => m_ContentCollections;

        private CancellationTokenSource taskCancellationSource;
        private Task RunningTask;
        
        /// <summary>
        /// Set browser location and enumerate files and folders.
        /// </summary>
        /// <param name="path">The requested location. It should not be null.</param>
        public async Task UpdateCollection(string path)
        {
            if (!CurrentPath.IsSameLocation(path))
            {
                if (BrowserPeekIndex < BrowserHistoryStack.Count - 1 && BrowserPeekIndex >= 0)
                {
                    var target = BrowserPeekIndex;
                    for (int i = BrowserHistoryStack.Count - 1; i > target; i--)
                    {
                        BrowserHistoryStack.RemoveAt(i);
                    }
                }

                BrowserHistoryStack.Add(CurrentPath);
            }
            
            await OpenDirectoryWithoutRecordHistory(path);
        }

        /// <summary>
        /// Asynchronously enumerate files and folders, adding results to view model. This procedure will not record the history of browses.
        /// </summary>
        /// <param name="path">The requested location. It should not be null.</param>
        public async Task OpenDirectoryWithoutRecordHistory(string path)
        {
            ContentSupportingText = null;
            
            if (RunningTask != null)
            {
                if (RunningTask.Status == TaskStatus.Running)
                    taskCancellationSource.Cancel();
            }

            var ctx = new CancellationTokenSource();

            int filesCount = 0, dirsCount = 0;
            
            var task = new Task(delegate
            {
                OnPropertyChanged(nameof(IsRunningTask));
                CurrentPath = path;
                Dispatcher.UIThread.Post(delegate { UpdateBreadcrumbs(path); });
                DirectoryInfo dir = new DirectoryInfo(path);
                CurrentDirectory = dir.Name;
                CurrentDirectorySupporting = $"Loading";
                Dispatcher.UIThread.InvokeAsync(delegate()
                {
                    ContentCollections?.Clear();
                }, DispatcherPriority.Background).Wait();

                var dirs = dir.EnumerateDirectories();
                var files = dir.EnumerateFiles();

                var results = new List<FileSystemItemModel>();

                if (ctx.IsCancellationRequested)
                    throw new TaskCanceledException();
                
                foreach (var item in dirs)
                    results.Add(new DirectoryModel(item));
                foreach (var item in files)
                    results?.Add(new FileModel(item));

                filesCount = files.Count();
                dirsCount = dirs.Count();

                foreach (var models in SplitList(results))
                {
                    if (ctx.IsCancellationRequested)
                        throw new TaskCanceledException();
                    
                    Dispatcher.UIThread.InvokeAsync(delegate
                        {
                            foreach (var m in models)
                            {
                                ContentCollections.Add(m);
                            }
                        },
                        DispatcherPriority.Background).Wait(ctx.Token);
                    Task.Delay(1).Wait();
                }
            }, ctx.Token);
            task.ContinueWith(async delegate(Task task1)
            {
                RunningTask = null;
                ctx.Dispose();

                CurrentDirectorySupporting = Utils.FormatCurrentDirSupportingText(filesCount, dirsCount);
                
                if (task1.IsFaulted && !(task1.Exception.InnerException is TaskCanceledException))
                {
                    var builder = new StringBuilder();
                    builder.AppendLine("An exception occurred when enumerate filesystem:");
                    builder.AppendLine(task1.Exception.InnerException.Message + "\n");
                    //builder.AppendLine("Stacktrace: ");
                    //builder.AppendLine(task1.Exception.InnerException.StackTrace);

                    ContentSupportingText = builder.ToString();
                }
                else if (task1.IsCompleted)
                {
                    if (filesCount == 0 && dirsCount == 0)
                    {
                        var builder = new StringBuilder();
                        builder.AppendLine("This folder is empty.");
                        ContentSupportingText = builder.ToString();
                    }
                }
                OnPropertyChanged(nameof(IsRunningTask));
            });
            taskCancellationSource = ctx;
            RunningTask = task;
            task.Start();
        }
        
        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize=20)  
        {        
            for (int i = 0; i < locations.Count; i += nSize) 
            { 
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i)); 
            }  
        } 
        
        #endregion

        public string GetPath(string name)
        {
            return Path.Combine(CurrentPath ?? "", name ?? "");
        }
        
        public async Task Open(FileSystemItemModel? SelectedItem)
        {
            string fullPath = GetPath(SelectedItem.Name);
            switch (SelectedItem)
            {
                case DirectoryModel _:
                    await UpdateCollection(fullPath);
                    break;
                case FileModel _:
                    Utils.OpenByShell(fullPath);
                    break;
            }
        }
        
        public async Task OpenFolder(string fileFolderName)
        {
            await UpdateCollection(GetPath(fileFolderName));
        }

        #region Breadcrumbs-related things

        private ObservableCollection<BreadcrumbsModel> m_PathBreadcrumbs;
        public ObservableCollection<BreadcrumbsModel> PathBreadcrumbs => m_PathBreadcrumbs;
        
        public void UpdateBreadcrumbs(string path)
        {
            string[] paths = path.Split(Path.DirectorySeparatorChar);
            PathBreadcrumbs.Clear();
            ulong index = 0;
            foreach (var p in paths)
            {
                if (p == "")
                    continue;
                var item = new BreadcrumbsModel
                {
                    Text = p,
                    ToIndex = index
                };
                if (index > 0)
                    item.Prefix = Path.DirectorySeparatorChar.ToString();
                PathBreadcrumbs.Add(item);
                index++;
            }
        }

        public async Task ToPath(ulong index)
        {
            string[] paths = CurrentPath?.Split(Path.DirectorySeparatorChar) ?? Array.Empty<string>();
            ulong i = 0;
            string completedPath = "";
            foreach (var path in paths)
            {
                if (i > index)
                    break;
                completedPath = Path.Combine(completedPath, path);
                i++;
            }

            await UpdateCollection(completedPath + Path.DirectorySeparatorChar);
        }

        #endregion

        #region Browser history

        private ObservableCollection<string> _browserHistoryStack;
        public ObservableCollection<string> BrowserHistoryStack => _browserHistoryStack;

        private int BrowserPeekIndex = -1;

        public void InitBrowserHistoryStack()
        {
            _browserHistoryStack = new ObservableCollection<string>();
            _browserHistoryStack.CollectionChanged += BrowserHistoryStackCollectionChanged;
        }
        
        private void BrowserHistoryStackCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
                BrowserPeekIndex = _browserHistoryStack.Count - 1;
        }
        
        public string GetPreviousPath()
        {
            if (BrowserPeekIndex <= -1 || BrowserPeekIndex >= BrowserHistoryStack.Count)
                return null;

            var result = BrowserHistoryStack.ElementAt(BrowserPeekIndex);
            BrowserPeekIndex--;
            return result;
        }
        
        #endregion
        
        public void ValidateContextMenu()
        {
            ContextMenuCommands.ContextMenuOpen(this, null);
        }

        #region Commands
        
        public RelayCommand GotoPreviousDirectoryCommand => new RelayCommand(GotoPreviousDirectoryCommandExecuted);

        private async void GotoPreviousDirectoryCommandExecuted(object arg)
        {
            if (PathBreadcrumbs.Count <= 1)
                return;

            var index = PathBreadcrumbs.Count - 2;
            ToPath(PathBreadcrumbs[index].ToIndex);
        }

        public RelayCommand SelectItemCommand => new RelayCommand(SelectItemCommandExecuted);

        private async void SelectItemCommandExecuted(object arg)
        {
            var focusedItem = FocusManager.Instance.Current;
            if (focusedItem == null)
                return;

            if (!(focusedItem is Visual v))
                return;
            
            if (!(v.DataContext is FileSystemItemModel m))
                return;

            AppendSingleSelect(m);
        }
        
        public RelayCommand OpenCommand => new RelayCommand(OpenCommandExecuted);

        private async void OpenCommandExecuted(object arg)
        {
            var focusedItem = FocusManager.Instance.Current;
            if (focusedItem == null)
                return;

            if (!(focusedItem is Visual v))
                return;
            
            if (!(v.DataContext is FileSystemItemModel m))
                return;
            
            await Open(m);
        }
        
        public RelayCommand CloseSessionCommand => new RelayCommand(CloseSessionCommandExecuted, CloseSessionCommandCanExecute);

        private bool CloseSessionCommandCanExecute(object arg)
        {
            if (_mainWindowViewModel.SessionWindows.Count > 1)
                return true;
            
            return false;
        }
        
        private void CloseSessionCommandExecuted(object arg)
        {
            if (!CloseSessionCommandCanExecute(arg))
                return;

            var index = _mainWindowViewModel.SelectedSessionIndex;
            if (_mainWindowViewModel.SelectedSession == this)
            {
                var c = _mainWindowViewModel.SessionWindows.Count - 1;
                if (index > c)
                {
                    index = c;
                }
            }
            
            _mainWindowViewModel.SessionWindows.Remove(this);
            _mainWindowViewModel.SelectedSessionIndex = index;
        }

        #endregion
        
        
        private Task<DialogResult> ShowErrorDialog(Exception exc, string supportingText, string header)
        {
            return DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams
            {
                ContentHeader = header,
                SupportingText = $"{supportingText}: \n{exc.Message}\n{exc.StackTrace}",
                Width = 800,
                DialogButtons = DialogHelper.CreateSimpleDialogButtons(Dialog.Enums.DialogButtonsEnum.Ok)
            }).Show();
        }
    }
}