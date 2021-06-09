using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Material.Files.Collections;

namespace Material.Files.Model
{
    public abstract class CachedThumbnailModel : ViewModelBase
    {
        private static LruPersistCachePool _diskCachePool;

        private static ObservableQueue<Task> _thumbnailRequestTasks = 
            new ObservableQueue<Task>();

        private static ManualResetEventSlim _manualResetEvent = new ManualResetEventSlim();
        
        static CachedThumbnailModel()
        {
            var target = App.ThumbnailsCacheFolder;
            
            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);

            _diskCachePool = new LruPersistCachePool(target, Utils.GIB_SIZE * 1);
            
            _thumbnailRequestTasks.CollectionChanged += ThumbnailRequestTasksOnCollectionChanged;
        }

        public CachedThumbnailModel(string cacheId)
        {
            this.cacheId = cacheId;
        }
        
        private static void ThumbnailRequestTasksOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (_manualResetEvent.IsSet)
                    return;

                ThreadPool.QueueUserWorkItem(delegate(object? state)
                {
                    _manualResetEvent.Set();

                    while (_thumbnailRequestTasks.TryDequeue(out var task))
                    {
                        try
                        {
                            task.Start();
                            task.Wait();
                        }
                        catch(Exception exception)
                        {
                            Logger.Sink.Log(LogEventLevel.Error, "CachedThumbnailModel", sender, 
                                $"Failed to initiate thumbnail, skipping... {exception.Message}");
                        }
                    }
                    _manualResetEvent.Reset();
                });
            }
        }

        protected void QueueTask(Task task)
        {
            _thumbnailRequestTasks.Enqueue(task);
        }

        protected void CacheResource(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            _diskCachePool.PushOrReplace(cacheId, stream);
        }

        protected bool IsCachedResource() => _diskCachePool.IsCacheExist(cacheId);

        public async Task GetCachedThumbnailAsync(CancellationToken cancellationToken = default)
        {
            var task = new Task(async delegate
            {
                using (var stream = _diskCachePool.GetValue(cacheId))
                {
                    Thumbnail = new Bitmap(stream);
                    IsLoaded = true;
                }
            });
            task.ContinueWith(delegate(Task task1)
            {
                if (task1.IsFaulted)
                {
                    Logger.Sink.Log(LogEventLevel.Error, "CachedThumbnailModel", this,
                        $"Failed to load cached thumbnail. {task1.Exception.InnerException.Message}");
                }
            });
            task.Start();
        }
        
        private string cacheId;

        private bool _isLoaded;

        public bool IsLoaded
        {
            get => _isLoaded;
            protected set
            {
                _isLoaded = value;
                OnPropertyChanged();
            }
        }

        private IImage _thumbnail;

        public IImage Thumbnail
        {
            get
            {
                if (_thumbnail == null)
                    CreateThumbnailAsync();

                return _thumbnail;
            }
            protected set
            {
                _thumbnail = value;
                OnPropertyChanged();
            }
        }

        protected abstract void CreateThumbnailAsync();
    }
}