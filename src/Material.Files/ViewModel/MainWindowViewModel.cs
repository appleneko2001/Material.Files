using System;
using IList = System.Collections.IList;
using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Material.Files.Model;
using Material.Dialog;
using Avalonia.Controls;
using Avalonia.Threading;
using Material.Dialog.Interfaces;
using Material.Files.Collections;
using Material.Files.Commands;
using Material.Icons;

namespace Material.Files.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindow MainWindow;

        public MainWindowViewModel(MainWindow mainWindow)
        {
            MainWindow = mainWindow;

            _LocalDrives = new ObservableCollection<DriveInfoModel>();
            _removableDrives = new ObservableCollection<DriveInfoModel>();
            _sessionWindows = new ObservableCollection<SessionWindowViewModel>();
            _sessionWindows.CollectionChanged += OnSessionWindowCollectionChanged;

            Dispatcher.UIThread.Post(delegate
            {
                var session = CreateSession();
                SelectedSessionIndex = 0;
                session.UpdateCollection(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            });
        }

        private void OnSessionWindowCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var r = SessionWindows.ToArray();
            foreach (var session in r)
            {
                session.CloseSessionCommand.RaiseCanExecute();
            }
        }

        private ObservableCollection<DriveInfoModel>? _LocalDrives;
        public ObservableCollection<DriveInfoModel>? LocalDrives => _LocalDrives;
        
        private ObservableCollection<DriveInfoModel>? _removableDrives;
        public ObservableCollection<DriveInfoModel>? RemovableDrives => _removableDrives;

        private ObservableCollection<SessionWindowViewModel>? _sessionWindows;
        public ObservableCollection<SessionWindowViewModel>? SessionWindows => _sessionWindows;

        private int _selectedSessionIndex;
        private object _selectedSession;

        public int SelectedSessionIndex
        {
            get => _selectedSessionIndex;
            set
            {
                _selectedSessionIndex = value;
                OnPropertyChanged();
            }
        }
        
        public object SelectedSession
        {
            get => _selectedSession;
            set
            {
                _selectedSession = value;
                OnPropertyChanged();
            }
        }
        
        public async void UpdateDrives()
        {
            try
            {
                await Dispatcher.UIThread.InvokeAsync(delegate
                {
                    LocalDrives?.Clear();
                    RemovableDrives?.Clear();
                }, DispatcherPriority.Background);
                
                DriveInfo[] drives = DriveInfo.GetDrives();
                
                foreach (var d in drives)
                {
                    Dispatcher.UIThread.InvokeAsync(delegate
                    {
                        if (d.IsReady)
                        {
                            switch (d.DriveType)
                            {
                                case DriveType.Fixed:
                                    LocalDrives?.Add(new DriveInfoModel(d));
                                    break;
                                case DriveType.Removable:
                                    RemovableDrives?.Add(new DriveInfoModel(d));
                                    break;
                            }
                        }
                    }, DispatcherPriority.Background).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                await ShowErrorDialog(e, "An exception occurred when enumerate local drives",
                    "Failed to enumerate local drives");
            }
        }

        public SessionWindowViewModel CreateSession()
        {
            var v = new SessionWindowViewModel(this);
            SessionWindows.Add(v);

            return v;
        }

        public RelayCommand OpenDriveCommand => new RelayCommand(OpenDriveCommandExecuted);

        private void OpenDriveCommandExecuted(object obj)
        {
            if (!(obj is DriveInfoModel drive))
                return;
            
            if (SelectedSession is SessionWindowViewModel session)
            {
                session.UpdateCollection(drive.Path);
            }
        }

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