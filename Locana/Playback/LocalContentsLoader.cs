using Kazyx.Uwpmm.DataModel;
using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Kazyx.Uwpmm.Playback
{
    public class LocalContentsLoader : ContentsLoader
    {
        public event EventHandler<SingleContentEventArgs> SingleContentLoaded;

        protected void OnContentLoaded(Thumbnail file)
        {
            SingleContentLoaded.Raise(this, new SingleContentEventArgs { File = file });
        }

        public override async Task Load(ContentsSet contentsSet, CancellationTokenSource cancel)
        {
            await LoadPictures(cancel);
            await LoadVideos(cancel);

            OnCompleted();
        }

        private async Task LoadPictures(CancellationTokenSource cancel)
        {
            var library = KnownFolders.PicturesLibrary;

            var folders = await library.GetFoldersAsync();

            foreach (var folder in folders)
            {
                if (folder.Name != Album.LOCANA_DIRECTORY)
                {
                    continue;
                }

                DebugUtil.Log("Load from local picture folder: " + folder.Name);
                await LoadContentsAsync(folder, cancel).ConfigureAwait(false);
                if (cancel != null && cancel.IsCancellationRequested)
                {
                    OnCancelled();
                    break;
                }
            }

            foreach (var folder in folders)
            {
                if (folder.Name == Album.LOCANA_DIRECTORY)
                {
                    continue;
                }

                DebugUtil.Log("Load from local picture folder: " + folder.Name);
                await LoadContentsAsync(folder, cancel).ConfigureAwait(false);
                if (cancel != null && cancel.IsCancellationRequested)
                {
                    OnCancelled();
                    break;
                }
            }
        }

        private async Task LoadVideos(CancellationTokenSource cancel)
        {
            var library = KnownFolders.VideosLibrary;

            var folders = await library.GetFoldersAsync();

            foreach (var folder in folders)
            {
                if (folder.Name != Album.LOCANA_DIRECTORY)
                {
                    continue;
                }

                DebugUtil.Log("Load from local video folder: " + folder.Name);
                await LoadContentsAsync(folder, cancel).ConfigureAwait(false);
                if (cancel != null && cancel.IsCancellationRequested)
                {
                    OnCancelled();
                    break;
                }
            }

            foreach (var folder in folders)
            {
                if (folder.Name == Album.LOCANA_DIRECTORY)
                {
                    continue;
                }

                DebugUtil.Log("Load from local video folder: " + folder.Name);
                await LoadContentsAsync(folder, cancel).ConfigureAwait(false);
                if (cancel != null && cancel.IsCancellationRequested)
                {
                    OnCancelled();
                    break;
                }
            }
        }

        private async Task LoadContentsAsync(StorageFolder folder, CancellationTokenSource cancel)
        {
            var list = new List<StorageFile>();
            await LoadFilesRecursively(list, folder, cancel).ConfigureAwait(false);

            if (cancel != null && cancel.IsCancellationRequested)
            {
                return;
            }

            var thumbs = list.Select(file =>
                {
                    var thumb = StorageFileToThumbnail(folder, file);
                    SingleContentLoaded.Raise(this, new SingleContentEventArgs { File = thumb });
                    return thumb;
                }).ToList();

            OnPartLoaded(thumbs);
        }

        public static Thumbnail StorageFileToThumbnail(StorageFolder folder, StorageFile file)
        {
            return new Thumbnail(new ContentInfo
            {
                Protected = false,
                MimeType = file.ContentType,
                GroupName = folder.DisplayName,
                OriginalUrl = file.Path,
            }, file);
        }

        // readonly string[] IMAGE_MIME_TYPES = { "image/jpeg", "image/png", "image/bmp", "image/gif", "video/mp4" };

        private async Task LoadFilesRecursively(List<StorageFile> into, StorageFolder folder, CancellationTokenSource cancel)
        {
            var files = await folder.GetFilesAsync();

            if (cancel != null && cancel.IsCancellationRequested)
            {
                return;
            }

            // into.AddRange(files.Where(file => IMAGE_MIME_TYPES.Any(type => file.ContentType.Equals(type, StringComparison.OrdinalIgnoreCase))));
            into.AddRange(files.Where(file =>
                file.ContentType.StartsWith(MimeType.Image, StringComparison.OrdinalIgnoreCase)
                || file.ContentType.StartsWith(MimeType.Video, StringComparison.OrdinalIgnoreCase)));

            foreach (var child in await folder.GetFoldersAsync())
            {
                if (cancel != null && cancel.IsCancellationRequested)
                {
                    return;
                }
                await LoadFilesRecursively(into, child, cancel).ConfigureAwait(false);
            }
        }

        public override Task LoadRemainingAsync(RemainingContentsHolder holder, ContentsSet contentsSet, CancellationTokenSource cancel)
        {
            throw new NotImplementedException();
        }
    }

    public class SingleContentEventArgs : EventArgs
    {
        public Thumbnail File { set; get; }
    }
}
