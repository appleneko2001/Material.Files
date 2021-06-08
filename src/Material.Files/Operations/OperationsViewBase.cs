using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Material.Files.Commands;

namespace Material.Files.Operations
{
    /// <summary>
    /// OperationsViewBase provides some properties for show operations status, commands for control operations and etc.
    /// Integrated operations break control, and archived IDisposable interface to dispose object.
    /// </summary>
    public abstract class OperationsViewBase : ViewModelBase, IDisposable
    {
        public OperationsViewBase()
        {
            _tasks = new Dictionary<ulong, OperationElementBase>();
            _runOperationCommand = new RelayCommand(RunOperationExecuted);
            _continueOperationCommand = new RelayCommand(ContinueOperationExecuted);
            //_cancelOperationCommand = new RelayCommand()
        }

        public abstract void BuildTasks(object param);

        private Dictionary<ulong, OperationElementBase> _tasks;
        
        private ulong _taskStep;
        public ulong TaskStep
        {
            get => _taskStep;
            protected set
            {
                _taskStep = value;
                OnPropertyChanged();
            }
        }

        private OperationElementBase _currentTask;
        public OperationElementBase CurrentTask
        {
            get => _currentTask;
            protected set
            {
                _currentTask = value;
                OnPropertyChanged();
            }
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            protected set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }
        
        private double _totalPercentage;
        public double TotalPercentage
        {
            get => _totalPercentage;
            protected set
            {
                _totalPercentage = value;
                OnPropertyChanged();
            }
        }
        
        private bool _isFailed;
        public bool IsFailed
        {
            get => _isFailed;
            protected set
            {
                _isFailed = value;
                OnPropertyChanged();
            }
        }
        
        private bool _isFinished;
        public bool IsFinished
        {
            get => _isFinished;
            protected set
            {
                _isFinished = value;
                OnPropertyChanged();
            }
        }

        public bool IsRunning => _runningTask != null ? _runningTask.Status == TaskStatus.Running : false;

        private Task _runningTask;
        private CancellationTokenSource _ctx;
        protected CancellationTokenSource CancellationTokenSource => _ctx;

        private ICommand _runOperationCommand;
        public ICommand RunOperationCommand => _runOperationCommand;

        private ICommand _continueOperationCommand;
        public ICommand ContinueOperationCommand => _continueOperationCommand;

        private ICommand _cancelOperationCommand;
        public ICommand CancelOperationCommand => _cancelOperationCommand;

        public event EventHandler<OperationBreakEventArgs> OperationBreak;
        private event EventHandler<OperationBreakAnswerEnum> OperationBreakAnswer;
        private ManualResetEventSlim _operationBreakWaitSwitch;
        
        private void RunOperationExecuted(object obj)
        {
            if (_runningTask != null && _runningTask.Status == TaskStatus.Running)
                throw new InvalidOperationException("Queued operation is running already!");

            var newCtx = new CancellationTokenSource();
            var task = new Task(_taskRunOperationDelegate, newCtx.Token);
            task.ContinueWith(_runOperationCompleted);

            _ctx = newCtx;
            _runningTask = task;
            OnPropertyChanged(nameof(IsRunning));
            task.Start();
        }

        private void _taskRunOperationDelegate()
        {
            var ctx = _ctx;

            while (!ctx.IsCancellationRequested)
            {
                var step = _tasks[_taskStep];
                try
                {
                    CurrentTask = step;
                    
                    //step.Invoke();
                    TaskStep++;
                }
                catch (Exception e)
                {
                    // Enter the queues break mode. Awaiting answer from user.
                    using (var mre = new ManualResetEventSlim(false))
                    {
                        _operationBreakWaitSwitch = mre;
                        
                        OperationBreakAnswerEnum answer = OperationBreakAnswerEnum.Abort;
                        EventHandler<OperationBreakAnswerEnum> handler = delegate(object? sender, OperationBreakAnswerEnum s)
                        {
                            answer = s;
                        };

                        OperationBreakAnswer += handler;
                        
                        OperationBreak?.Invoke(this, new OperationBreakEventArgs()
                        {
                            ErrorException = e
                        });
                        
                        mre.Wait();
                        _operationBreakWaitSwitch = null;
                        OperationBreakAnswer -= handler;
                        
                        if (answer == OperationBreakAnswerEnum.Abort)
                        {
                            StatusText = "Task has aborted.";
                            ctx.Cancel();
                            throw e;
                        }
                        else if (answer == OperationBreakAnswerEnum.Retry)
                        {
                            continue;
                        }
                        else if (answer == OperationBreakAnswerEnum.Continue)
                        {
                            TaskStep++;
                            continue;
                        }
                    }
                }
            }
        }

        private void _runOperationCompleted(Task task)
        {
            if (task.IsFaulted)
            {
                IsFailed = true;
            }

            IsFinished = true;
            _ctx.Dispose();
            _ctx = null;

            _runningTask = null;
            OnPropertyChanged(nameof(IsRunning));
        }

        private void ContinueOperationExecuted(object obj)
        {
            if (_operationBreakWaitSwitch == null)
                throw new InvalidOperationException("Queues is not in break mode.");
            
            if (obj is string argument)
            {
                var a = argument.ToLower();

                switch (a)
                {
                    case "continue":
                        OperationBreakAnswer?.Invoke(this, OperationBreakAnswerEnum.Continue);
                        break;
                    case "retry":
                        OperationBreakAnswer?.Invoke(this, OperationBreakAnswerEnum.Retry);
                        break;
                    case "abort":
                        OperationBreakAnswer?.Invoke(this, OperationBreakAnswerEnum.Abort);
                        break;
                    default:
                        throw new ArgumentException("Argument of continue operation should be one of them: continue, retry and abort.");
                }
                
                _operationBreakWaitSwitch.Set();
            }
        }
        
        public void Dispose()
        {
            while (_tasks.Count > 0)
            {
                var item = _tasks.Last();
                _tasks.Remove(item.Key);
            }
            
            if (_runningTask != null)
            {
                _runningTask.Dispose();
                _runningTask = null;
            }

            if (_ctx != null)
            {
                _ctx.Dispose();
                _ctx = null;
            }

            if (_operationBreakWaitSwitch != null)
            {
                _operationBreakWaitSwitch.Dispose();
                _operationBreakWaitSwitch = null;
            }
        }
    }
}