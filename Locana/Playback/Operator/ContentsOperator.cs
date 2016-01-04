using Locana.DataModel;
using Locana.Pages;
using Naotaco.ImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Locana.Playback
{
    public class ContentsOperatorFactory
    {
        public static ContentsOperator CreateNew(ContentsGridPage page)
        {
            switch (page.TargetStorageType)
            {
                case StorageType.Local:
                    return new LocalContentsOperator(page.MoviePlayerScreen);
                default:
                    return null;
            }
        }
    }

    public abstract class ContentsOperator : IDisposable
    {
        public AlbumGroupCollection ContentsCollection { protected set; get; }
        public CancellationTokenSource Canceller { set; get; }

        public event Action<string> ErrorMessageRaised;
        protected void OnErrorMessage(string message)
        {
            ErrorMessageRaised?.Invoke(message);
        }

        public event EventHandler<ContentsLoadedEventArgs> PartLoaded;
        protected void OnPartLoaded(IList<Thumbnail> contents)
        {
            PartLoaded?.Invoke(this, new ContentsLoadedEventArgs(contents));
        }

        public event EventHandler<SingleContentEventArgs> SingleContentLoaded;
        protected void OnSingleContentLoaded(Thumbnail file)
        {
            SingleContentLoaded?.Invoke(this, new SingleContentEventArgs { File = file });
        }

        public event Action MovieStreamError;
        protected void OnMovieStreamError()
        {
            MovieStreamError?.Invoke();
        }

        public abstract Task LoadContents();
        public abstract Task DeleteSelectedFiles(IEnumerable<Thumbnail> items);
        public abstract Task DeleteSelectedFile(Thumbnail item);
        public abstract Task<Tuple<BitmapImage, JpegMetaData>> PlaybackStillImage(Thumbnail item);
        public abstract Task PlaybackMovie(Thumbnail item);
        public abstract void FinishMoviePlayback();
        public abstract void Dispose();
    }
}
