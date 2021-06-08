using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SkiaSharp;

namespace Material.Files.Model
{
    public class ImageThumbnailModel : ViewModelBase
    {
        private static Dictionary<string, IImage> _cachePool =
            new Dictionary<string, IImage>();

        private Func<Stream> _streamCreator;
        private string cacheId;

        private bool _isLoaded;

        public bool IsLoaded
        {
            get => _isLoaded;
            private set
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
                {
                    if (_cachePool.ContainsKey(cacheId))
                        GetCachedThumbnailAsync();
                    else
                        GetThumbnailAsync();
                }

                return _thumbnail;
            }
            private set
            {
                _thumbnail = value;
                OnPropertyChanged();
            }
        }

        public ImageThumbnailModel(string cacheId, Func<Stream> stream)
        {
            this.cacheId = cacheId;
            _streamCreator = stream;
        }

        public async Task GetCachedThumbnailAsync()
        {
            var task = new Task(delegate
            {
                Thumbnail = _cachePool[cacheId];
                IsLoaded = true;
            });
            task.ContinueWith(delegate(Task task1)
            {
                if (task1.IsFaulted)
                {
                    Logger.Sink.Log(LogEventLevel.Error, "ImageThumbnail", this, "Failed to load cached thumbnail.");
                }
            });
            task.Start();
        }

        public async Task GetThumbnailAsync()
        {
            /*
SKImageInfo resizeInfo = new SKImageInfo(resizedWidth, resizedHeight);
using (SKBitmap resizedSKBitmap = srcBitmap.Resize(resizeInfo, SKBitmapResizeMethod.Lanczos3))
using (SKImage newImg = SKImage.FromPixels(resizedSKBitmap.PeekPixels()))
using (SKData data = newImg.Encode(SKEncodedImageFormat.Jpeg, jpegQuality))
using (Stream imgStream = data.AsStream())
{
	// save the stream and look at the image. e.g. "crisp.jpg"
}
             */
            Stream srcStream = null;
            var task = new Task(delegate
            {
                IsLoaded = false;

                srcStream = _streamCreator();

                using (var src = SKBitmap.Decode(srcStream))
                {
                    if (src == null)
                        throw new NullReferenceException("Could not initiate an SKBitmap instance.");

                    var srcW = src.Width;
                    var srcH = src.Height;

                    if (srcW == 0 || srcH == 0)
                        return;

                    var thumbSize = 160;

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

                    var resizeInfo = new SKImageInfo(thumbW, thumbH);

                    using (var resizedImage = src.Resize(resizeInfo, SKBitmapResizeMethod.Hamming))
                    {
                        using (var data = resizedImage.Encode(SKEncodedImageFormat.Png, 100))
                        {
                            var instance = new Bitmap(data.AsStream());
                            Thumbnail = instance;
                            IsLoaded = true;

                            // Save thumbnails to cache pool.
                            _cachePool.Add(cacheId, instance);
                        }
                    }
                }
            });
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

                if (task1.IsFaulted)
                {
                    Logger.Sink.Log(LogEventLevel.Error, "ImageThumbnail", this, "Failed to prepare thumbnail.");
                }
            }, srcStream);
            task.Start();
        }
    }
}