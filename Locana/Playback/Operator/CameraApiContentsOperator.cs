using Kazyx.RemoteApi.AvContent;
using Locana.CameraControl;
using Locana.Controls;
using Locana.DataModel;
using Locana.Utility;
using Naotaco.ImageProcessor.MetaData;
using Naotaco.ImageProcessor.MetaData.Misc;
using Naotaco.ImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace Locana.Playback.Operator
{
    public class CameraApiContentsOperator : ContentsOperator
    {
        private TargetDevice TargetDevice;
        private readonly MoviePlaybackScreen MovieScreen;
        private HttpClient HttpClient = new HttpClient();

        public CameraApiContentsOperator(TargetDevice device, MoviePlaybackScreen movieScreen)
        {
            TargetDevice = device;
            MovieScreen = movieScreen;

            MovieStreamHelper.INSTANCE.MoviePlaybackData.SeekAvailable = TargetDevice.Api.Capability.IsSupported("seekStreamingPosition");

            MovieStreamHelper.INSTANCE.StreamClosed += StreamHelper_StreamClosed;
            MovieStreamHelper.INSTANCE.StatusChanged += StreamHelper_StatusChanged;
        }

        private void StreamHelper_StatusChanged(object sender, StreamingStatusEventArgs e)
        {
            DebugUtil.Log("StreamStatusChanged: " + e.Status.Status + " - " + e.Status.Factor);
            switch (e.Status.Factor)
            {
                case StreamStatusChangeFactor.FileError:
                case StreamStatusChangeFactor.MediaError:
                case StreamStatusChangeFactor.OtherError:
                    OnErrorMessage(SystemUtil.GetStringResource("Viewer_StreamClosedByExternalCause"));
                    OnMovieStreamError();
                    break;
                default:
                    break;
            }

            MovieStreamHelper.INSTANCE.MoviePlaybackData.StreamingStatus = e.Status.Status;
            MovieStreamHelper.INSTANCE.MoviePlaybackData.StreamingStatusTransitionFactor = e.Status.Factor;
        }

        private void StreamHelper_StreamClosed(object sender, EventArgs e)
        {
            OnMovieStreamError();
        }

        public override async Task DeleteSelectedFile(Thumbnail item)
        {
            var data = item.Source as RemoteApiContentInfo;
            var contents = new TargetContents();
            contents.ContentUris = new List<string>();
            contents.ContentUris.Add(data.Uri);

            await DeleteRemoteApiContents(contents);

            ContentsCollection.Remove(item);
        }

        public override async Task DeleteSelectedFiles(IEnumerable<Thumbnail> items)
        {
            var uris = items
                .Select(item => item.Source as RemoteApiContentInfo)
                .Where(info => info != null)
                .Select(info => info.Uri).ToList();

            await DeleteRemoteApiContents(new TargetContents { ContentUris = uris });

            foreach (var item in items)
            {
                ContentsCollection.Remove(item);
            }
        }

        private async Task DeleteRemoteApiContents(TargetContents contents)
        {
            var av = TargetDevice.Api.AvContent;
            if (av != null && contents != null)
            {
                await av.DeleteContentAsync(contents).ConfigureAwait(false);
                DebugUtil.Log("Delete contents completed");
            }
            else
            {
                DebugUtil.Log("Not ready to delete contents");
            }
        }

        public override void Dispose()
        {
            HttpClient.Dispose();

            MovieStreamHelper.INSTANCE.StreamClosed -= StreamHelper_StreamClosed;
            MovieStreamHelper.INSTANCE.StatusChanged -= StreamHelper_StatusChanged;
        }

        public override void FinishMoviePlayback()
        {
            MovieStreamHelper.INSTANCE.Finish();
        }

        public override async Task LoadContents()
        {
            var loader = new RemoteApiContentsLoader(TargetDevice);
            try
            {
                OnProgressMessage(SystemUtil.GetStringResource("Progress_ChangingCameraState"));

                var StateChangeCanceller = new CancellationTokenSource(15000);
                try
                {
                    if (!await PlaybackModeHelper.MoveToContentTransferModeAsync(TargetDevice, StateChangeCanceller).ConfigureAwait(false))
                    {
                        DebugUtil.Log("ModeTransition failed");
                        throw new Exception();
                    }
                }
                finally
                {
                    StateChangeCanceller = null;
                }
                DebugUtil.Log("ModeTransition successfully finished");

                OnProgressMessage(SystemUtil.GetStringResource("Progress_FetchingContents"));
                loader.PartLoaded += RemoteContentsLoader_PartLoaded;
                await loader.Load(ApplicationSettings.GetInstance().RemoteContentsSet, Canceller).ConfigureAwait(false);
                DebugUtil.Log("RemoteApiContentsLoader completed");
            }
            catch (StorageNotSupportedException)
            {
                // This will never happen on camera devices.
                DebugUtil.Log("storage scheme is not supported");
                OnErrorMessage(SystemUtil.GetStringResource("Viewer_StorageAccessNotSupported"));
            }
            catch (NoStorageException)
            {
                DebugUtil.Log("No storages");
                OnErrorMessage(SystemUtil.GetStringResource("Viewer_NoStorage"));
            }
            catch (Exception e)
            {
                DebugUtil.Log(e.StackTrace);
                OnErrorMessage(SystemUtil.GetStringResource("Viewer_FailedToRefreshContents"));
            }
            finally
            {
                loader.PartLoaded -= RemoteContentsLoader_PartLoaded;
            }
        }

        private void RemoteContentsLoader_PartLoaded(object sender, ContentsLoadedEventArgs e)
        {
            OnPartLoaded(e);
        }

        public override async Task PlaybackMovie(Thumbnail content)
        {
            if (MovieStreamHelper.INSTANCE.IsProcessing)
            {
                MovieStreamHelper.INSTANCE.Finish();
            }

            if (TargetDevice.Api.AvContent == null)
            {
                OnErrorMessage(SystemUtil.GetStringResource("Viewer_NoAvContentApi"));
                throw new IOException();
            }

            var item = content.Source as ContentInfo;

            if (!item.RemotePlaybackAvailable)
            {
                OnErrorMessage(SystemUtil.GetStringResource("Viewer_UnplayableContent"));
                throw new IOException();
            }
            var started = await MovieStreamHelper.INSTANCE.Start(TargetDevice.Api.AvContent, new PlaybackContent
            {
                Uri = (item as RemoteApiContentInfo).Uri,
                RemotePlayType = RemotePlayMode.SimpleStreaming
            }, content.Source.Name);
            if (!started)
            {
                OnErrorMessage(SystemUtil.GetStringResource("Viewer_FailedPlaybackMovie"));
                throw new IOException();
            }

            MovieScreen.NotifyStartingStreamingMoviePlayback();
        }

        public override async Task<Tuple<BitmapImage, JpegMetaData>> PlaybackStillImage(Thumbnail content)
        {
            using (var res = await HttpClient.GetAsync(new Uri(content.Source.LargeUrl)))
            {
                if (!res.IsSuccessStatusCode)
                {
                    OnMovieStreamError();
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
