using Locana.DataModel;
using Locana.Pages;
using Naotaco.ImageProcessor.MetaData;
using Naotaco.ImageProcessor.MetaData.Misc;
using Naotaco.ImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

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

        public event Action<string> ProgressMessageRaised;
        protected void OnProgressMessage(string message)
        {
            ProgressMessageRaised?.Invoke(message);
        }

        public event EventHandler<ContentsLoadedEventArgs> ChunkContentsLoaded;
        protected void OnPartLoaded(ContentsLoadedEventArgs e)
        {
            ChunkContentsLoaded?.Invoke(this, e);
        }

        public event EventHandler<SingleContentEventArgs> SingleContentLoaded;
        protected void OnSingleContentLoaded(SingleContentEventArgs e)
        {
            SingleContentLoaded?.Invoke(this, e);
        }

        public event Action MovieStreamError;
        protected void OnMovieStreamError()
        {
            MovieStreamError?.Invoke();
        }

        public abstract Task LoadContents();
        public abstract Task LoadRemainingContents(RemainingContentsHolder holder);
        public abstract Task DeleteSelectedFiles(IEnumerable<Thumbnail> items);
        public abstract Task DeleteSelectedFile(Thumbnail item);
        public abstract Task<Tuple<BitmapImage, JpegMetaData>> PlaybackStillImage(Thumbnail item);
        public abstract Task PlaybackMovie(Thumbnail item);
        public abstract void FinishMoviePlayback();
        public abstract void Dispose();
    }

    public abstract class RemoteContentsOperator : ContentsOperator
    {
        protected readonly HttpClient HttpClient = new HttpClient();

        public override async Task<Tuple<BitmapImage, JpegMetaData>> PlaybackStillImage(Thumbnail content)
        {
            using (var res = await HttpClient.GetAsync(new Uri(content.Source.LargeUrl)))
            {
                if (!res.IsSuccessStatusCode)
                {
                    throw new IOException();
                }

                var buff = await res.Content.ReadAsBufferAsync();
                using (var stream = new InMemoryRandomAccessStream())
                {
                    await stream.WriteAsync(buff); // Copy to the new stream to avoid stream crash issue.
                    if (stream.Size == 0)
                    {
                        throw new IOException();
                    }
                    stream.Seek(0);

                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    try
                    {
                        var meta = await JpegMetaDataParser.ParseImageAsync(stream.AsStream());
                        return Tuple.Create(bitmap, meta);
                    }
                    catch (UnsupportedFileFormatException)
                    {
                        return Tuple.Create<BitmapImage, JpegMetaData>(bitmap, null);
                    }
                }
            }
        }
    }
}
