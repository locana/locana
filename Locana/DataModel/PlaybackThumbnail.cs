using Kazyx.Uwpmm.Playback;
using Kazyx.Uwpmm.Utility;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace Kazyx.Uwpmm.DataModel
{
    public class RemainingContentsHolder : Thumbnail
    {
        public RemainingContentsHolder(DateInfo date, string uuid, int startsFrom, int count)
            : base(new ContentInfo { GroupName = date.Title }, uuid)
        {
            StartsFrom = startsFrom;
            RemainingCount = count;
            AlbumGroup = date;
            IsPlayable = false;
        }

        public RemainingContentsHolder(string containerId, string groupTitle, string uuid, int startsFrom, int count)
            : base(new ContentInfo { GroupName = groupTitle }, uuid)
        {
            DebugUtil.Log("Creating remaining contents holder: " + startsFrom + " - " + count + " - " + containerId);
            StartsFrom = startsFrom;
            RemainingCount = count;
            CdsContainerId = containerId;
            IsPlayable = false;
        }

        public int StartsFrom { private set; get; }
        public int RemainingCount { private set; get; }
        public DateInfo AlbumGroup { private set; get; }
        public string CdsContainerId { private set; get; }

        public override string OverlayText
        {
            get
            {
                if (RemainingCount == 0) { return null; }
                else { return "+" + RemainingCount; }
            }
        }

        public override bool IsSelectable
        {
            get
            {
                switch (SelectivityFactor)
                {
                    case SelectivityFactor.None:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public override bool IsMovie { get { return false; } }
        public override bool IsDeletable { get { return false; } }
        public override bool IsCopyable { get { return false; } }
        public override bool IsContent { get { return false; } }
        public override BitmapImage ThumbnailImage { get { return null; } }
        public override BitmapImage LargeImage { get { return null; } }
    }

    public class Thumbnail : ObservableBase
    {
        public Thumbnail(ContentInfo content, string uuid)
        {
            GroupTitle = content.GroupName;
            Source = content;
            DeviceUuid = uuid;
            IsRecent = false;
        }

        public Thumbnail(ContentInfo content, StorageFile localfile)
        {
            GroupTitle = content.GroupName;
            CacheFile = localfile;
            Source = content;
            DeviceUuid = "localhost";
            IsRecent = false;
        }

        public ContentInfo Source { private set; get; }

        public virtual string OverlayText { get { return null; } }

        public virtual bool IsMovie
        {
            get
            {
                return Source.MimeType.StartsWith(MimeType.Video, StringComparison.OrdinalIgnoreCase);
            }
        }

        private SelectivityFactor factor = SelectivityFactor.None;
        public SelectivityFactor SelectivityFactor
        {
            set
            {
                factor = value;
                NotifyChanged("IsSelectable");
            }
            get { return factor; }
        }

        public virtual bool IsSelectable
        {
            get
            {
                switch (SelectivityFactor)
                {
                    case SelectivityFactor.None:
                        return true;
                    case SelectivityFactor.CopyToPhone:
                        return IsCopyable;
                    case SelectivityFactor.Delete:
                        return !Source.Protected;
                    default:
                        throw new NotImplementedException("Unknown SelectivityFactor");
                }
            }
        }

        public virtual bool IsDeletable { get { return !Source.Protected; } }

        public virtual bool IsCopyable
        {
            get
            {
                return Source.MimeType.StartsWith(MimeType.Image) || Source.MimeType.StartsWith(MimeType.Video);
            }
        }

        public bool IsPlayable { set; get; }

        public virtual bool IsContent { get { return true; } }

        public bool IsRecent { set; get; }

        private string DeviceUuid { set; get; }

        public string GroupTitle { private set; get; }

        public StorageFile CacheFile { private set; get; }

        private async Task LoadCachedThumbnailImageAsync(ImageMode mode)
        {
            var file = CacheFile;
            if (file == null)
            {
                DebugUtil.Log("CacheFile is null");
                return;
            }

            try
            {
                using (var stream = await file.GetThumbnailAsync(ThumbnailMode.ListView))
                {
                    var bmp = new BitmapImage();
                    bmp.CreateOptions = BitmapCreateOptions.None;
                    await bmp.SetSourceAsync(stream);

                    if (ImageMode.Image == mode) { ThumbnailImage = bmp; }
                    else { LargeImage = bmp; }
                }
                return;
            }
            catch { }

            if (0 < RemainingRetryCount)
            {
                var delay = 1000 * (3 - RemainingRetryCount--);
                DebugUtil.Log("Failed to load thumbnail from file. Retry " + delay + " msec later.");

                await Task.Delay(delay);
                NotifyChanged("ThumbnailImage");
                NotifyChanged("LargeImage");
            }
            else
            {
                DebugUtil.Log("Failed to load thumbnail from file. Retry count exhausted.");
            }
        }

        private int RemainingRetryCount = 2;

        private BitmapImage _ThumbnailImage = null;

        public virtual BitmapImage ThumbnailImage
        {
            private set
            {
                _ThumbnailImage = value;
                NotifyChanged("ThumbnailImage");
                NotifyChanged("LargeImage");
            }
            get { return GetImage(ImageMode.Image); }
        }

        private BitmapImage _LargeImage = null;

        public virtual BitmapImage LargeImage
        {
            private set
            {
                _LargeImage = value;
                NotifyChanged("LargeImage");
            }
            get { return GetImage(ImageMode.Album); }
        }

        private BitmapImage GetImage(ImageMode mode)
        {
            BitmapImage tmp = null;
            if (ImageMode.Image == mode)
            {
                tmp = Interlocked.Exchange(ref _ThumbnailImage, null);
            }
            else
            {
                tmp = Interlocked.Exchange(ref _LargeImage, null);
            }
            if (tmp != null)
            {
                return tmp;
            }

            if (CacheFile == null)
            {
                var task = FetchThumbnailAsync().ConfigureAwait(false);
            }
            else
            {
                var task = LoadCachedThumbnailImageAsync(mode).ConfigureAwait(false);
            }

            return null;
        }

        private enum ImageMode
        {
            Image,
            Album,
        }

        private async Task FetchThumbnailAsync()
        {
            try
            {
                var file = await ThumbnailCacheLoader.INSTANCE.LoadCacheFileAsync(DeviceUuid, Source);
                CacheFile = file;
                await LoadCachedThumbnailImageAsync(ImageMode.Image);
            }
            catch (Exception e)
            {
                DebugUtil.Log(e.StackTrace);
                DebugUtil.Log("Failed to fetch thumbnail image: " + Source.ThumbnailUrl);
            }
        }
    }

    public enum SelectivityFactor
    {
        None,
        CopyToPhone,
        Delete,
    }
}
