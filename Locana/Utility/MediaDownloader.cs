using Locana.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.Web.Http;

namespace Locana.Utility
{
    public class MediaDownloader
    {
        private MediaDownloader() { }

        private static string DIRECTORY_NAME = SystemUtil.GetStringResource("ApplicationTitle");

        private const int BUFFER_SIZE = 8 * 1024;

        private static readonly HttpClient HttpClient = new HttpClient();

        private static readonly MediaDownloader instance = new MediaDownloader();
        public static MediaDownloader Instance
        {
            get { return instance; }
        }

        public Action<StorageFolder, StorageFile, GeotaggingResult.Result> Fetched;

        public Action<DownloaderError, GeotaggingResult.Result> Failed;

        protected void OnFetched(StorageFolder folder, StorageFile file, GeotaggingResult.Result geotaggingResult)
        {
            DebugUtil.Log(() => "PictureSyncManager: OnFetched");
            Fetched?.Invoke(folder, file, geotaggingResult);
        }

        protected void OnFailed(DownloaderError error, GeotaggingResult.Result geotaggingResult)
        {
            DebugUtil.Log(() => "PictureSyncManager: OnFailed" + error);
            Failed?.Invoke(error, geotaggingResult);
        }

        public Task EnqueueVideo(Uri uri, string nameBase, string extension = null)
        {
            return Enqueue(uri, nameBase, Mediatype.Video, extension);
        }

        public Task EnqueueImage(Uri uri, string nameBase, string extension)
        {
            return Enqueue(uri, nameBase, Mediatype.Image, extension);
        }

        public Task EnqueuePostViewImage(Uri uri)
        {
            return Enqueue(uri, DIRECTORY_NAME, Mediatype.Image, ".jpg");
        }

        private async Task Enqueue(Uri uri, string namebase, Mediatype type, string extension = null)
        {
            DebugUtil.LogSensitive(() => "ContentsDownloader: Enqueue {0}", uri.AbsolutePath);

            if (extension == null)
            {
                var split = uri.AbsolutePath.Split('.');
                if (split.Length > 0)
                {
                    extension = "." + split[split.Length - 1].ToLower();
                    DebugUtil.Log(() => "detected file extension: " + extension);
                }
            }

            var tcs = new TaskCompletionSource<bool>();

            await SystemUtil.GetCurrentDispatcher().RunAsync(CoreDispatcherPriority.Low, () =>
            {
                var req = new DownloadRequest
                {
                    Uri = uri,
                    NameBase = namebase,
                    Completed = OnFetched,
                    Error = OnFailed,
                    Mediatype = type,
                    FileExtension = extension,
                    CompletionSource = tcs,
                };
                DownloadQueue.Enqueue(req);
                QueueStatusUpdated?.Invoke(DownloadQueue.Count);
                ProcessQueueSequentially();
            });

            await tcs.Task;
        }

        public void Purge()
        {
            DownloadQueue.Clear();
            // Cancel ongoing download
        }

        private Task task;

        private readonly Queue<DownloadRequest> DownloadQueue = new Queue<DownloadRequest>();
        public Action<int> QueueStatusUpdated;

        private void ProcessQueueSequentially()
        {
            if (task == null)
            {
                DebugUtil.Log(() => "Create new task");
                task = Task.Factory.StartNew(async () =>
                {
                    while (DownloadQueue.Count != 0)
                    {
                        DebugUtil.Log(() => "Dequeue - remaining " + DownloadQueue.Count);
                        await DownloadToSave(DownloadQueue.Dequeue());

                        QueueStatusUpdated?.Invoke(DownloadQueue.Count);
                    }
                    DebugUtil.Log(() => "Queue end. Kill task");
                    task = null;
                });
            }
        }

        private async Task DownloadToSave(DownloadRequest req)
        {
            DebugUtil.LogSensitive(() => "Download picture: {0}", req.Uri.OriginalString);
            try
            {
                var geoResult = GeotaggingResult.Result.NotRequested;

                var task = HttpClient.GetAsync(req.Uri, HttpCompletionOption.ResponseContentRead);

                HttpResponseMessage res;
                try
                {
                    res = await task;
                }
                catch (Exception)
                {
                    req.Error?.Invoke(DownloaderError.Network, geoResult);
                    req.CompletionSource?.TrySetResult(false);
                    return;
                }

                var imageStream = (await res.Content.ReadAsInputStreamAsync()).AsStreamForRead();

                if (req.Mediatype == Mediatype.Image)
                {
                    if (ApplicationSettings.GetInstance().GeotagEnabled)
                    {
                        var position = await GeolocatorManager.INSTANCE.GetLatestPosition();
                        if (position == null)
                        {
                            geoResult = GeotaggingResult.Result.FailedToAcquireLocation;
                        }
                        else
                        {
                            var result = await GeopositionUtil.AddGeotag(imageStream, position);
                            imageStream = result.Image;
                            geoResult = result.OperationResult;
                        }
                    }
                }

                using (imageStream)
                {
                    StorageFolder rootFolder;
                    switch (req.Mediatype)
                    {
                        case Mediatype.Image:
                            rootFolder = KnownFolders.PicturesLibrary;
                            break;
                        case Mediatype.Video:
                            rootFolder = KnownFolders.PicturesLibrary;
                            // Use Pictures folder according to the behavior of built-in Camera apps
                            // rootFolder = KnownFolders.VideosLibrary;
                            break;
                        default:
                            req.CompletionSource?.TrySetResult(false);
                            return;
                    }

                    var folder = await rootFolder.CreateFolderAsync(DIRECTORY_NAME, CreationCollisionOption.OpenIfExists);
                    var filename = string.Format(req.NameBase + "_{0:yyyyMMdd_HHmmss}" + req.FileExtension, DateTime.Now);
                    var file = await folder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);

                    using (var outStream = await file.OpenStreamForWriteAsync())
                    {
                        await imageStream.CopyToAsync(outStream);
                    }

                    req.Completed?.Invoke(folder, file, geoResult);
                    req.CompletionSource?.TrySetResult(true);
                    return;
                }
            }
            catch (Exception e)
            {
                DebugUtil.Log(() => e.Message);
                DebugUtil.Log(() => e.StackTrace);
                req.Error?.Invoke(DownloaderError.Unknown, GeotaggingResult.Result.NotRequested); // TODO
                req.CompletionSource?.TrySetResult(false);
            }
        }
    }

    public class DownloadRequest
    {
        public Uri Uri { set; get; }
        public string NameBase { set; get; }
        public Mediatype Mediatype { set; get; }
        public string FileExtension { set; get; }
        public Action<StorageFolder, StorageFile, GeotaggingResult.Result> Completed { set; get; }
        public Action<DownloaderError, GeotaggingResult.Result> Error { set; get; }
        public TaskCompletionSource<bool> CompletionSource { set; get; }
    }

    public enum Mediatype
    {
        Image,
        Video,
    }

    public enum DownloaderError
    {
        Network,
        Saving,
        Argument,
        DeviceInternal,
        Gone,
        Unknown,
        None,
    }

    public class GeotaggingResult
    {
        public Stream Image { get; set; } = null;
        public Result OperationResult { get; set; }

        public enum Result
        {
            OK,
            GeotagAlreadyExists,
            UnExpectedError,
            NotRequested,
            FailedToAcquireLocation,
        }
    }
}
