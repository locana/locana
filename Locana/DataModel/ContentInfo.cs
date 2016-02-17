namespace Locana.DataModel
{
    public class ContentInfo
    {
        /// <summary>
        /// File name without extension.
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// Only Jpeg original url will be set if it is available.
        /// </summary>
        public string OriginalUrl { set; get; }
        public string LargeUrl { set; get; }
        public string ThumbnailUrl { set; get; }
        public string MimeType { set; get; }
        public string CreatedTime { set; get; }
        public bool Protected { set; get; }

        public string GroupName { set; get; }
        public bool RemotePlaybackAvailable { set; get; }

        public override bool Equals(object obj)
        {
            var info = obj as ContentInfo;
            return Equals(info);
        }

        public bool Equals(ContentInfo info)
        {
            if (info == null)
            {
                return false;
            }
            return Name == info.Name
                && OriginalUrl == info.OriginalUrl
                && LargeUrl == info.LargeUrl
                && ThumbnailUrl == info.ThumbnailUrl
                && MimeType == info.MimeType
                && GroupName == info.GroupName;
        }

        public override int GetHashCode()
        {
            if (OriginalUrl != null)
            {
                return OriginalUrl.GetHashCode();
            }
            if (LargeUrl != null)
            {
                return LargeUrl.GetHashCode();
            }
            if (ThumbnailUrl != null)
            {
                return ThumbnailUrl.GetHashCode();
            }
            return base.GetHashCode();
        }
    }

    public class RemoteApiContentInfo : ContentInfo
    {
        public string Uri { set; get; }

        public override bool Equals(object obj)
        {
            var info = obj as RemoteApiContentInfo;
            return Equals(info);
        }

        public bool Equals(RemoteApiContentInfo info)
        {
            if (info == null)
            {
                return false;
            }
            return Uri == info.Uri;
        }

        public override int GetHashCode()
        {
            return Uri.GetHashCode();
        }
    }

    public class DlnaContentInfo : ContentInfo
    {
        /// <summary>
        /// Object ID of the content
        /// </summary>
        public string Id { set; get; }

        public override bool Equals(object obj)
        {
            var info = obj as DlnaContentInfo;
            return Equals(info);
        }

        public bool Equals(DlnaContentInfo info)
        {
            if (info == null)
            {
                return false;
            }
            return Id == info.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
