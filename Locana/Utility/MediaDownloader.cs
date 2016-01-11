using Naotaco.Jpeg.MetaData;
using Naotaco.Jpeg.MetaData.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
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

        public Action<StorageFolder, StorageFile, GeotaggingResult> Fetched;

        public Action<DownloaderError, GeotaggingResult> Failed;

        protected void OnFetched(StorageFolder folder, StorageFile file, GeotaggingResult geotaggingResult)
        {
            DebugUtil.Log("PictureSyncManager: OnFetched");
            Fetched?.Invoke(folder, file, geotaggingResult);
        }

        protected void OnFailed(DownloaderError error, GeotaggingResult geotaggingResult)
        {
            DebugUtil.Log("PictureSyncManager: OnFailed" + error);
            Failed?.Invoke(error, geotaggingResult);
        }

        public void EnqueueVideo(Uri uri, string nameBase, string extension = null)
        {
            Enqueue(uri, nameBase, Mediatype.Video, null, extension);
        }

        public void EnqueueImage(Uri uri, string nameBase, string extension, Geoposition position = null)
        {
            Enqueue(uri, nameBase, Mediatype.Image, position, extension);
        }

        public void EnqueuePostViewImage(Uri uri, Geoposition position = null)
        {
            Enqueue(uri, DIRECTORY_NAME, Mediatype.Image, position, ".jpg");
        }

        private async void Enqueue(Uri uri, string namebase, Mediatype type, Geoposition position, string extension = null)
        {
            DebugUtil.Log("ContentsDownloader: Enqueue " + uri.AbsolutePath);

            if (extension == null)
            {
                var split = uri.AbsolutePath.Split('.');
                if (split.Length > 0)
                {
                    extension = "." + split[split.Length - 1].ToLower();
                    DebugUtil.Log("detected file extension: " + extension);
                }
            }

            await SystemUtil.GetCurrentDispatcher().RunAsync(CoreDispatcherPriority.Low, () =>
            {
                var req = new DownloadRequest
                {
                    Uri = uri,
                    NameBase = namebase,
                    Completed = OnFetched,
                    Error = OnFailed,
                    GeoPosition = position,
                    Mediatype = type,
                    extension = extension
                };
                DownloadQueue.Enqueue(req);
                QueueStatusUpdated?.Invoke(DownloadQueue.Count);
                ProcessQueueSequentially();
            });
        }

        private Task task;

        private readonly Queue<DownloadRequest> DownloadQueue = new Queue<DownloadRequest>();
        public Action<int> QueueStatusUpdated;

        private void ProcessQueueSequentially()
        {
            if (task == null)
            {
                DebugUtil.Log("Create new task");
                task = Task.Factory.StartNew(async () =>
                {
                    while (DownloadQueue.Count != 0)
                    {
                        DebugUtil.Log("Dequeue - remaining " + DownloadQueue.Count);
                        await DownloadToSave(DownloadQueue.Dequeue());

                        QueueStatusUpdated?.Invoke(DownloadQueue.Count);
                    }
                    DebugUtil.Log("Queue end. Kill task");
                    task = null;
                });
            }
        }

        private async Task DownloadToSave(DownloadRequest req)
        {
            DebugUtil.Log("Download picture: " + req.Uri.OriginalString);
            try
            {
                var geoResult = GeotaggingResult.NotRequested;

                var task = HttpClient.GetAsync(req.Uri, HttpCompletionOption.ResponseContentRead);

                HttpResponseMessage res;
                try
                {
                    res = await task;
                }
                catch (Exception)
                {
                    req.Error?.Invoke(DownloaderError.Network, geoResult);
                    return;
                }

                var imageStream = (await res.Content.ReadAsInputStreamAsync()).AsStreamForRead();

                if (req.Mediatype == Mediatype.Image && req.GeoPosition != null)
                {
                    try
                    {
                        imageStream = await MetaDataOperator.AddGeopositionAsync(imageStream, req.GeoPosition, false);
                        geoResult = GeotaggingResult.OK;
                    }
                    catch (GpsInformationAlreadyExistsException)
                    {
                        geoResult = GeotaggingResult.GeotagAlreadyExists;
                    }
                    catch
                    {
                        geoResult = GeotaggingResult.UnExpectedError;
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
                            return;
                    }

                    var folder = await rootFolder.CreateFolderAsync(DIRECTORY_NAME, CreationCollisionOption.OpenIfExists);
                    var filename = string.Format(req.NameBase + "_{0:yyyyMMdd_HHmmss}" + req.extension, DateTime.Now);
                    var file = await folder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var buffer = new byte[BUFFER_SIZE];
                        using (var os = stream.GetOutputStreamAt(0))
                        {
                            int read = 0;
                            while ((read = imageStream.Read(buffer, 0, BUFFER_SIZE)) != 0)
                            {
                                await os.WriteAsync(buffer.AsBuffer(0, read));
                            }
                        }
                    }
                    req.Completed?.Invoke(folder, file, geoResult);
                    return;
                }
            }
            catch (Exception e)
            {
                DebugUtil.Log(e.Message);
                DebugUtil.Log(e.StackTrace);
                req.Error?.Invoke(DownloaderError.Unknown, GeotaggingResult.NotRequested); // TODO
            }
        }
    }

    public class DownloadRequest
    {
        public Uri Uri;
        public string NameBase;
        public Mediatype Mediatype;
        public string extension;
        public Geoposition GeoPosition;
        public Action<StorageFolder, StorageFile, GeotaggingResult> Completed;
        public Action<DownloaderError, GeotaggingResult> Error;
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

    public enum GeotaggingResult
    {
        OK,
        GeotagAlreadyExists,
        UnExpectedError,
        NotRequested,
    }
}
