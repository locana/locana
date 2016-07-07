using Locana.Playback;
using Locana.Utility;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace Locana.DataModel
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
            DebugUtil.Log(() => "Creating remaining contents holder: " + startsFrom + " - " + count + " - " + containerId);
            StartsFrom = startsFrom;
            RemainingCount = count;
            CdsContainerId = containerId;
            IsPlayable = false;
        }

        public override bool Equals(object obj)
        {
            var thumb = obj as RemainingContentsHolder;
            return Equals(thumb);
        }

        public bool Equals(RemainingContentsHolder thumb)
        {
            if (thumb == null)
            {
                return false;
            }

            if (AlbumGroup != null)
            {
                return AlbumGroup.Title == thumb?.AlbumGroup?.Title && AlbumGroup.Uri == thumb?.AlbumGroup?.Uri;
            }

            if (CdsContainerId != null)
            {
                return CdsContainerId == thumb.CdsContainerId && GroupTitle == thumb.GroupTitle;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Source.GetHashCode();
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
            IsLocal = false;
        }

        public Thumbnail(ContentInfo content, StorageFile localfile)
        {
            GroupTitle = content.GroupName;
            CacheFile = localfile;
            Source = content;
            DeviceUuid = "localhost";
            IsRecent = false;
            IsLocal = true;
        }

        public override bool Equals(object obj)
        {
            var thumb = obj as Thumbnail;
            return Equals(thumb);
        }

        public bool Equals(Thumbnail thumb)
        {
            if (thumb == null)
            {
                return false;
            }

            if (CacheFile != null && CacheFile.Equals(thumb.CacheFile))
            {
                return true;
            }

            return DeviceUuid == thumb.DeviceUuid && Source.Equals(thumb.Source);
        }

        public override int GetHashCode()
        {
            return Source.GetHashCode();
        }

        public bool IsLocal { private set; get; }

        public bool IsBroken { get; set; } = false;

        public bool ThumbnailExists { get { return _ThumbnailImage != null || IsBroken; } }

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
                NotifyChanged(nameof(IsSelectable));
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
                    case SelectivityFactor.Download:
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
                return !IsLocal && (Source.MimeType.StartsWith(MimeType.Image) || Source.MimeType.StartsWith(MimeType.Video));
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
                DebugUtil.Log(() => "CacheFile is null");
                return;
            }

            try
            {
                using (var stream = await file.GetThumbnailAsync(ThumbnailMode.ListView, 90)) // 90 pix seems to be the best balance between speed and quality
                {
                    var bmp = new BitmapImage();
                    bmp.CreateOptions = BitmapCreateOptions.None;
                    await bmp.SetSourceAsync(stream);

                    if (ImageMode.Image == mode) { ThumbnailImage = bmp; }
                    else { LargeImage = bmp; }
                    IsBroken = false;
                    NotifyChanged(nameof(ThumbnailExists));
                    NotifyChanged(nameof(IsBroken));
                }
                return;
            }
            catch { }

            if (0 < RemainingRetryCount)
            {
                var delay = 1000 * (3 - RemainingRetryCount--);
                DebugUtil.Log(() => "Failed to load thumbnail from file. Retry " + delay + " msec later.");

                await Task.Delay(delay);
                NotifyChanged(nameof(ThumbnailImage));
                NotifyChanged(nameof(LargeImage));
            }
            else
            {
                DebugUtil.Log(() => "Failed to load thumbnail from file. Retry count exhausted.");
                IsBroken = true;
                NotifyChanged(nameof(ThumbnailExists));
                NotifyChanged(nameof(IsBroken));
            }
        }

        private int RemainingRetryCount = 2;

        private BitmapImage _ThumbnailImage = null;

        public virtual BitmapImage ThumbnailImage
        {
            private set
            {
                _ThumbnailImage = value;
                NotifyChanged(nameof(ThumbnailImage));
                NotifyChanged(nameof(LargeImage));
                NotifyChanged(nameof(ThumbnailExists));
            }
            get { return GetImage(ImageMode.Image); }
        }

        private BitmapImage _LargeImage = null;

        public virtual BitmapImage LargeImage
        {
            private set
            {
                _LargeImage = value;
                NotifyChanged(nameof(LargeImage));
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
                DebugUtil.Log(() => e.StackTrace);
                DebugUtil.Log(() => "Failed to fetch thumbnail image: " + Source.ThumbnailUrl);
                IsBroken = true;
                NotifyChangedOnUI(nameof(ThumbnailExists));
                NotifyChangedOnUI(nameof(IsBroken));
            }
        }
    }

    public enum SelectivityFactor
    {
        None,
        Download,
        Delete,
    }
}
