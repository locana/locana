using Kazyx.RemoteApi.AvContent;
using Kazyx.Uwpmm.CameraControl;
using Kazyx.Uwpmm.DataModel;
using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kazyx.Uwpmm.Playback
{
    public class RemoteApiContentsLoader : ContentsLoader
    {
        private readonly AvContentApiClient AvContentApi;

        private readonly string Udn;

        public const int CONTENT_LOOP_STEP = 50;

        public const int MAX_AUTO_LOAD_THUMBNAILS = 15;

        public RemoteApiContentsLoader(TargetDevice device)
        {
            AvContentApi = device.Api.AvContent;
            Udn = device.Udn;
        }

        public override async Task Load(ContentsSet contentsSet, CancellationTokenSource cancel)
        {
            if (!await IsStorageSupportedAsync().ConfigureAwait(false))
            {
                DebugUtil.Log("Storage scheme is not available on this device");
                throw new StorageNotSupportedException();
            }

            var storages = await GetStoragesUriAsync().ConfigureAwait(false);
            if (cancel != null && cancel.IsCancellationRequested)
            {
                DebugUtil.Log("Loading task cancelled");
                OnCancelled();
                return;
            }

            if (storages.Count == 0)
            {
                DebugUtil.Log("No storage is available on this device");
                throw new NoStorageException();
            }

            await GetContentsByDateSeparatelyAsync(storages[0], contentsSet, cancel).ConfigureAwait(false);
        }

        /// <summary>
        /// Camera devices should support "storage" scheme.
        /// </summary>
        /// <param name="av"></param>
        /// <returns></returns>
        private async Task<bool> IsStorageSupportedAsync()
        {
            var schemes = await AvContentApi.GetSchemeListAsync().ConfigureAwait(false);
            return schemes.Any(scheme => scheme.Scheme == Scheme.Storage);
        }

        private async Task<IList<string>> GetStoragesUriAsync()
        {
            var sources = await AvContentApi.GetSourceListAsync(new UriScheme { Scheme = Scheme.Storage }).ConfigureAwait(false);
            return sources.Select(source => source.Source).ToList();
        }

        private async Task GetContentsByDateSeparatelyAsync(string uri, ContentsSet contentsSet, CancellationTokenSource cancel)
        {
            DebugUtil.Log("Loading number of Dates");

            var count = await AvContentApi.GetContentCountAsync(new CountingTarget
                {
                    Grouping = ContentGroupingMode.Date,
                    Uri = uri,
                }).ConfigureAwait(false);

            DebugUtil.Log(count.NumOfContents + " dates exist.");

            if (cancel != null && cancel.IsCancellationRequested)
            {
                DebugUtil.Log("Loading task cancelled");
                OnCancelled();
                return;
            }

            var loops = count.NumOfContents / CONTENT_LOOP_STEP + (count.NumOfContents % CONTENT_LOOP_STEP == 0 ? 0 : 1);

            for (var i = 0; i < loops; i++)
            {
                var dates = await GetDateListAsync(uri, i * CONTENT_LOOP_STEP, CONTENT_LOOP_STEP).ConfigureAwait(false);
                if (cancel != null && cancel.IsCancellationRequested)
                {
                    DebugUtil.Log("Loading task cancelled");
                    OnCancelled();
                    break;
                }

                var loaded = 0;
                foreach (var date in dates)
                {
                    loaded += await GetContentsOfDaySeparatelyAsync(date, contentsSet, cancel, loaded).ConfigureAwait(false);
                }
            }
        }

        private async Task<IList<DateInfo>> GetDateListAsync(string uri, int startFrom, int count)
        {
            DebugUtil.Log("Loading DateList: " + uri + " from " + startFrom);

            var contents = await AvContentApi.GetContentListAsync(new ContentListTarget
                {
                    Sorting = SortMode.Descending,
                    Grouping = ContentGroupingMode.Date,
                    Uri = uri,
                    StartIndex = startFrom,
                    MaxContents = count
                }).ConfigureAwait(false);

            return contents.Where(content => content.IsFolder == TextBoolean.True)
                .Select(content => new DateInfo { Title = content.Title, Uri = content.Uri })
                .ToList();
        }

        private async Task<int> GetContentsOfDaySeparatelyAsync(DateInfo date, ContentsSet contentsSet, CancellationTokenSource cancel, int sum)
        {
            DebugUtil.Log("Loading: " + date.Title);

            var count = await AvContentApi.GetContentCountAsync(new CountingTarget
                {
                    Grouping = ContentGroupingMode.Date,
                    Uri = date.Uri,
                    Types = ContentsSetToTypes(contentsSet),
                }).ConfigureAwait(false);

            DebugUtil.Log(count.NumOfContents + " contents exist.");

            var loops = count.NumOfContents / CONTENT_LOOP_STEP + (count.NumOfContents % CONTENT_LOOP_STEP == 0 ? 0 : 1);
            var loaded = 0;

            for (var i = 0; i < loops; i++)
            {
                if (loaded + sum > MAX_AUTO_LOAD_THUMBNAILS)
                {
                    break;
                }

                var contents = await GetContentsOfDayAsync(date, i * CONTENT_LOOP_STEP, CONTENT_LOOP_STEP, contentsSet).ConfigureAwait(false);
                if (cancel != null && cancel.IsCancellationRequested)
                {
                    DebugUtil.Log("Loading task cancelled");
                    OnCancelled();
                    break;
                }

                loaded += contents.Count;
                DebugUtil.Log(contents.Count + " contents fetched");

                OnPartLoaded(contents.Select(content => new Thumbnail(content, Udn)).ToList());
            }

            if (loaded < count.NumOfContents)
            {
                var remaining = new RemainingContentsHolder(date, Udn, loaded, count.NumOfContents - loaded);
                var list = new List<Thumbnail>();
                list.Add(remaining);
                OnPartLoaded(list);
            }

            return loaded;
        }

        public override async Task LoadRemainingAsync(RemainingContentsHolder holder, ContentsSet contentsSet, CancellationTokenSource cancel)
        {
            var loops = holder.RemainingCount / CONTENT_LOOP_STEP + (holder.RemainingCount % CONTENT_LOOP_STEP == 0 ? 0 : 1);

            for (var i = 0; i < loops; i++)
            {
                var contents = await GetContentsOfDayAsync(holder.AlbumGroup, i * CONTENT_LOOP_STEP, CONTENT_LOOP_STEP, contentsSet).ConfigureAwait(false);
                if (cancel != null && cancel.IsCancellationRequested)
                {
                    DebugUtil.Log("Loading task cancelled");
                    OnCancelled();
                    break;
                }

                DebugUtil.Log(contents.Count + " contents fetched");

                OnPartLoaded(contents.Select(content => new Thumbnail(content, Udn)).ToList());
            }
        }

        private async Task<IList<ContentInfo>> GetContentsOfDayAsync(DateInfo date, int startFrom, int count, ContentsSet contentsSet)
        {
            DebugUtil.Log("Loading ContentsOfDay: " + date.Title + " from " + startFrom);

            var contents = await AvContentApi.GetContentListAsync(new ContentListTarget
                {
                    Sorting = SortMode.Ascending,
                    Grouping = ContentGroupingMode.Date,
                    Uri = date.Uri,
                    Types = ContentsSetToTypes(contentsSet),
                    StartIndex = startFrom,
                    MaxContents = count
                }).ConfigureAwait(false);

            return contents.Where(content => content.ImageContent != null
                        && content.ImageContent.OriginalContents != null
                        && content.ImageContent.OriginalContents.Count > 0)
                    .Select(content =>
                    {
                        var contentInfo = new RemoteApiContentInfo
                        {
                            Name = RemoveExtension(content.ImageContent.OriginalContents[0].FileName),
                            LargeUrl = content.ImageContent.LargeImageUrl,
                            ThumbnailUrl = content.ImageContent.ThumbnailUrl,
                            MimeType = ContentKindToMimeType(content.ContentKind),
                            Uri = content.Uri,
                            CreatedTime = content.CreatedTime,
                            Protected = content.IsProtected == TextBoolean.True,
                            RemotePlaybackAvailable = (content.RemotePlayTypes != null && content.RemotePlayTypes.Contains(RemotePlayMode.SimpleStreaming)),
                            GroupName = date.Title,
                        };

                        if (content.ContentKind == ContentKind.StillImage)
                        {
                            foreach (var original in content.ImageContent.OriginalContents)
                            {
                                if (original.Type == ImageType.Jpeg)
                                {
                                    contentInfo.OriginalUrl = original.Url;
                                    break;
                                }
                            }
                        }
                        else if (content.ContentKind == ContentKind.MovieMp4 || content.ContentKind == ContentKind.MovieXavcS)
                        {
                            foreach (var original in content.ImageContent.OriginalContents)
                            {
                                contentInfo.OriginalUrl = original.Url;
                                break;
                            }
                        }
                        return contentInfo;
                    })
                    .ToList<ContentInfo>();
        }

        private static string ContentKindToMimeType(string contentKind)
        {
            switch (contentKind)
            {
                case ContentKind.StillImage:
                    return MimeType.Jpeg;
                case ContentKind.MovieMp4:
                    return MimeType.Mp4;
                case ContentKind.MovieXavcS:
                    return MimeType.XavcS;
                default:
                    return MimeType.Unknown;
            }
        }

        private static string RemoveExtension(string name)
        {
            if (name == null)
            {
                return "";
            }
            if (!name.Contains("."))
            {
                return name;
            }
            else
            {
                var index = name.LastIndexOf(".");
                if (index == 0)
                {
                    return "";
                }
                else
                {
                    return name.Substring(0, index);
                }
            }
        }

        private static List<string> ContentsSetToTypes(ContentsSet contentsSet)
        {
            var types = new List<string>();

            switch (contentsSet)
            {
                case ContentsSet.ImagesAndMovies:
                    types.Add(ContentKind.StillImage);
                    types.Add(ContentKind.MovieMp4);
                    types.Add(ContentKind.MovieXavcS);
                    break;
                case ContentsSet.Images:
                    types.Add(ContentKind.StillImage);
                    break;
                case ContentsSet.Movies:
                    types.Add(ContentKind.MovieMp4);
                    types.Add(ContentKind.MovieXavcS);
                    break;
            }

            return types;
        }
    }

    public class NoStorageException : Exception
    {
    }

    public class StorageNotSupportedException : Exception
    {
    }

    public class DateInfo
    {
        public string Title { set; get; }
        public string Uri { set; get; }
    }
}
