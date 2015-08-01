using System;
using System.Collections.Generic;
using System.Text;

namespace Kazyx.Uwpmm.DataModel
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
    }

    public class RemoteApiContentInfo : ContentInfo
    {
        public string Uri { set; get; }
    }

    public class DlnaContentInfo : ContentInfo
    {
        /// <summary>
        /// Object ID of the content
        /// </summary>
        public string Id { set; get; }
    }
}
