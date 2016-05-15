using Locana.DataModel;
using Locana.Utility;
using System;
using System.Threading.Tasks;

namespace Locana.Playback
{
    public class DownloadHelper
    {
        public static Task EnqueueDownload(Thumbnail source)
        {
            if (source.IsMovie)
            {
                string ext;
                switch (source.Source.MimeType)
                {
                    case MimeType.Mp4:
                        ext = ".mp4";
                        break;
                    default:
                        ext = null;
                        break;
                }
                return MediaDownloader.Instance.EnqueueVideo(new Uri(source.Source.OriginalUrl), source.Source.Name, ext);
            }
            else if (ApplicationSettings.GetInstance().PrioritizeOriginalSizeContents && source.Source.OriginalUrl != null)
            {
                return MediaDownloader.Instance.EnqueueImage(new Uri(source.Source.OriginalUrl), source.Source.Name,
                    source.Source.MimeType == MimeType.Jpeg ? ".jpg" : null);
            }
            else
            {
                // Fallback to large size image
                return MediaDownloader.Instance.EnqueueImage(new Uri(source.Source.LargeUrl), source.Source.Name, ".jpg");
            }
        }
    }
}
