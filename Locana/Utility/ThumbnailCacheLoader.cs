using Kazyx.Uwpmm.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;

namespace Kazyx.Uwpmm.Utility
{
    public class ThumbnailCacheLoader
    {
        private const string CACHE_ROOT = "thumb_cache";

        private readonly HttpClient HttpClient;

        private StorageFolder CacheFolder;

        private ThumbnailCacheLoader()
        {
            HttpClient = new HttpClient();
        }

        private async Task LoadCacheRoot()
        {
            if (CacheFolder == null)
            {
                var root = ApplicationData.Current.TemporaryFolder;
                CacheFolder = await root.CreateFolderAsync(CACHE_ROOT, CreationCollisionOption.OpenIfExists);
            }
        }

        private static readonly ThumbnailCacheLoader instance = new ThumbnailCacheLoader();

        public static ThumbnailCacheLoader INSTANCE
        {
            get { return instance; }
        }

        private const int THUMBNAIL_SIZE = 240;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uuid">Specify uuid to delete directory for the device, otherwise delete all of stored cache.</param>
        public async Task DeleteCache(string uuid = null)
        {
            var root = ApplicationData.Current.TemporaryFolder;
            if (uuid == null)
            {
                DebugUtil.Log("Delete all of thumbnail cache.");

                await LoadCacheRoot().ConfigureAwait(false);
                await CacheFolder.DeleteDirectoryRecursiveAsync(false).ConfigureAwait(false);
            }
            else
            {
                DebugUtil.Log("Delete thumbnail cache of " + uuid);

                await LoadCacheRoot().ConfigureAwait(false);
                var uuidRoot = await CacheFolder.GetFolderAsync(uuid.Replace(":", "-"));
                await uuidRoot.DeleteDirectoryRecursiveAsync().ConfigureAwait(false);
            }
        }

        private object LoopLock = new object();
        private Task DownloadLoop;

        private object HolderLock = new object();
        private Queue<TaskSource> Queue = new Queue<TaskSource>();
        private Stack<TaskSource> Stack = new Stack<TaskSource>();

        private const uint FIFO_THRESHOLD = 40;

        public void CleanupRemainingTasks()
        {
            lock (HolderLock)
            {
                Queue.Clear();
                Stack.Clear();
            }
        }

        /// <summary>
        /// Asynchronously download thumbnail image and return local storage file.
        /// </summary>
        /// <param name="uuid">UUID of the target device.</param>
        /// <param name="content">Source of thumbnail image.</param>
        /// <returns>Local storage file</returns>
        public async Task<StorageFile> LoadCacheFileAsync(string uuid, ContentInfo content)
        {
            var uri = new Uri(content.ThumbnailUrl);
            var directory = uuid.Replace(":", "-") + "/";
            var filename = content.CreatedTime.Replace(":", "-").Replace("/", "-") + "--" + Path.GetFileName(uri.LocalPath);
            if (!filename.Contains("."))
            {
                filename += ".jpg";
            }
            else if (filename.EndsWith(".arw", StringComparison.OrdinalIgnoreCase))
            {
                filename = filename.Substring(0, filename.Length - 3) + "jpg";
            }

            DebugUtil.Log("Loading " + content.ThumbnailUrl + " into " + directory);

            await LoadCacheRoot().ConfigureAwait(false);
            var folder = await CacheFolder.CreateFolderAsync(directory, CreationCollisionOption.OpenIfExists);

            var file = await folder.TryGetFileAsync(filename).ConfigureAwait(false);
            if (file != null)
            {
                return file;
            }

            var tcs = new TaskCompletionSource<StorageFile>();

            var taskSource = new TaskSource { uri = uri, folder = folder, filename = filename, tcs = tcs };
            lock (HolderLock)
            {
                if (Stack.Count == 0 && Queue.Count < FIFO_THRESHOLD)
                {
                    Queue.Enqueue(taskSource);
                }
                else
                {
                    Stack.Push(taskSource);
                }
            }
            TryRunTask();
            return await tcs.Task.ConfigureAwait(false);
        }

        public async Task<StorageFile> EnqueueFileDownload(StorageFolder root, Uri uri, string extension = "")
        {
            var filename = Path.GetFileName(uri.LocalPath) + extension;

            var file = await root.TryGetFileAsync(filename).ConfigureAwait(false);
            if (file != null)
            {
                return file;
            }

            var tcs = new TaskCompletionSource<StorageFile>();

            var taskSource = new TaskSource { uri = uri, folder = root, filename = filename, tcs = tcs };
            lock (HolderLock)
            {
                if (Stack.Count == 0 && Queue.Count < FIFO_THRESHOLD)
                {
                    Queue.Enqueue(taskSource);
                }
                else
                {
                    Stack.Push(taskSource);
                }
            }
            TryRunTask();
            return await tcs.Task.ConfigureAwait(false);
        }

        private void TryRunTask()
        {
            lock (LoopLock)
            {
                if (DownloadLoop != null)
                {
                    return;
                }
                DebugUtil.Log("Create new download loop.");
                DownloadLoop = Task.Factory.StartNew(async () =>
                {
                    while (Queue.Count != 0 || Stack.Count != 0)
                    {
                        TaskSource source;
                        lock (HolderLock)
                        {
                            if (Stack.Count > 0) { source = Stack.Pop(); }
                            else if (Queue.Count > 0) { source = Queue.Dequeue(); }
                            else { break; }
                        }
                        await source.DownloadAsync(HttpClient);
                    }
                    DebugUtil.Log("Download loop end.");
                    lock (LoopLock)
                    {
                        DownloadLoop = null;
                    }
                });
            }
        }

        private class TaskSource
        {
            public Uri uri;
            public StorageFolder folder;
            public string filename;
            public TaskCompletionSource<StorageFile> tcs;

            public async Task DownloadAsync(HttpClient client)
            {

                var file = await folder.TryGetFileAsync(filename).ConfigureAwait(false);
                if (file != null)
                {
                    tcs.TrySetResult(file);
                    return;
                }

                DebugUtil.Log("Start downloading: " + uri);
                try
                {
                    var res = await client.GetAsync(uri, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
                    if (res.StatusCode != HttpStatusCode.OK)
                    {
                        tcs.TrySetResult(null);
                    }
                    using (var stream = await res.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        DebugUtil.Log("Writing: " + filename);
                        var dst = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                        using (var outStream = await dst.OpenStreamForWriteAsync().ConfigureAwait(false))
                        {
                            await stream.CopyToAsync(outStream).ConfigureAwait(false);
                        }
                        tcs.TrySetResult(dst);
                    }
                }
                catch (Exception e)
                {
                    DebugUtil.Log(e.StackTrace);
                    tcs.TrySetException(e);
                }
            }
        }
    }
}
