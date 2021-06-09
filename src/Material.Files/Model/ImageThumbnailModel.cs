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
using Avalonia.Threading;
using SkiaSharp;

namespace Material.Files.Model
{
    public class ImageThumbnailModel : CachedThumbnailModel
    {
        private CancellationToken _ctx;

        private Func<Stream> _streamCreator;
        

        public ImageThumbnailModel(string cacheId, Func<Stream> stream) : base(cacheId)
        {
            _streamCreator = stream;
        }

        public async Task GetThumbnailAsync(CancellationToken cancellationToken = default)
        {
            Stream srcStream = null;
            var task = new Task(delegate
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (IsCachedResource())
                {
                    GetCachedThumbnailAsync();
                    return;
                }
                IsLoaded = false;
                
                srcStream = _streamCreator();

                using var src = SKBitmap.Decode(srcStream);
                if (src == null)
                    throw new NullReferenceException("Could not initiate an SKBitmap instance.");

                var srcW = src.Width;
                var srcH = src.Height;

                if (srcW == 0 || srcH == 0)
                    return;

                var thumbSize = 128;

                int thumbW = thumbSize, thumbH = thumbSize;
                var aspect = (double) srcW / srcH;
                if (aspect >= 1.0)
                {
                    var aspectH = (double) srcH / srcW;
                    thumbH = (int) Math.Round(thumbH * aspectH);
                }
                else
                {
                    thumbW = (int) Math.Round(thumbW * aspect);
                }

                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();
                var resizeInfo = new SKImageInfo(thumbW, thumbH);

                using (var resizedImage = src.Resize(resizeInfo, SKFilterQuality.High))
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();
                    using (var data = resizedImage.Encode(SKEncodedImageFormat.Png, 100))
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException();
                        using (var resultStream = data.AsStream())
                        {
                            var instance = new Bitmap(resultStream);
                            Thumbnail = instance;
                            IsLoaded = true;

                            // Save thumbnails to cache pool.
                            CacheResource(resultStream);
                        }
                    }
                }
            }, cancellationToken);
            task.ContinueWith(delegate(Task task1, object? arg)
            {
                if (arg is Stream stream)
                {
                    if (stream.CanRead)
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }

                if (task1.IsFaulted && !(task1.Exception.InnerException is OperationCanceledException))
                {
                    throw task1.Exception.InnerException;
                }
            }, srcStream);
            QueueTask(task);
        }

        public void SetCancellationToken(CancellationToken ctx)
        {
            _ctx = ctx;
        }

        protected override void CreateThumbnailAsync()
        {
            GetThumbnailAsync(_ctx);
        }
    }
}