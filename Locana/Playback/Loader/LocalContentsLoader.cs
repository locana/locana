using Locana.DataModel;
using Locana.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Locana.Playback
{
    public class LocalContentsLoader : ContentsLoader
    {
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

                DebugUtil.LogSensitive(() => "Load from local picture folder: {0}", folder.Name);
                await LoadContentsAsync(folder, cancel).ConfigureAwait(false);
                if (cancel?.IsCancellationRequested ?? false)
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

                DebugUtil.LogSensitive(() => "Load from local video folder: {0}", folder.Name);
                await LoadContentsAsync(folder, cancel).ConfigureAwait(false);
                if (cancel?.IsCancellationRequested ?? false)
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

            if (cancel?.IsCancellationRequested ?? false)
            {
                return;
            }

            var thumbs = list.Select(file =>
                {
                    var thumb = StorageFileToThumbnail(folder, file);
                    OnSingleContentLoaded(thumb);
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
            }, file)
            {
                IsPlayable = true,
            };
        }

        private async Task LoadFilesRecursively(List<StorageFile> into, StorageFolder folder, CancellationTokenSource cancel)
        {
            var files = await folder.GetFilesAsync();

            if (cancel?.IsCancellationRequested ?? false)
            {
                return;
            }

            // Add any files discovered in the directory without ContentType filtering.
            // Sometimes accurate ContentType is not obtained with StorageFile.ContentType.
            into.AddRange(files);

            foreach (var child in await folder.GetFoldersAsync())
            {
                if (cancel?.IsCancellationRequested ?? false)
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
