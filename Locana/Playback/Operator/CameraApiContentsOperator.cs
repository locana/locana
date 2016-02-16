using Kazyx.RemoteApi.AvContent;
using Locana.CameraControl;
using Locana.Controls;
using Locana.DataModel;
using Locana.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Locana.Playback.Operator
{
    public class CameraApiContentsOperator : RemoteContentsOperator
    {
        private TargetDevice TargetDevice;
        private readonly MoviePlaybackScreen MovieScreen;

        public CameraApiContentsOperator(TargetDevice device, MoviePlaybackScreen movieScreen)
        {
            TargetDevice = device;
            TitleText = TargetDevice.FriendlyName;
            MovieScreen = movieScreen;
            MovieScreen.OnStreamingOperationRequested += MovieScreen_OnStreamingOperationRequested;
            movieScreen.MovieType = MovieFileType.SimpleStreamingMovie;
            ContentsCollection = new AlbumGroupCollection();

            MovieStreamHelper.INSTANCE.MoviePlaybackData.SeekAvailable = TargetDevice.Api.Capability.IsSupported("seekStreamingPosition");

            MovieStreamHelper.INSTANCE.StreamClosed += StreamHelper_StreamClosed;
            MovieStreamHelper.INSTANCE.StatusChanged += StreamHelper_StatusChanged;
        }

        private async void MovieScreen_OnStreamingOperationRequested(object sender, PlaybackRequestArgs e)
        {
            switch (e.Request)
            {
                case PlaybackRequest.Start:
                    await MovieStreamHelper.INSTANCE.Start(TargetDevice.Api.AvContent);
                    break;
                case PlaybackRequest.Pause:
                    await MovieStreamHelper.INSTANCE.Pause(TargetDevice.Api.AvContent);
                    break;
            }

        }

        private void StreamHelper_StatusChanged(object sender, StreamingStatusEventArgs e)
        {
            DebugUtil.Log("StreamStatusChanged: " + e.Status.Status + " - " + e.Status.Factor);
            switch (e.Status.Factor)
            {
                case StreamStatusChangeFactor.FileError:
                case StreamStatusChangeFactor.MediaError:
                case StreamStatusChangeFactor.OtherError:
                    OnErrorMessage("Viewer_StreamClosedByExternalCause");
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
            TargetDevice.Observer.Stop();

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
            await TargetDevice.Observer.StartAsync();
            try
            {
                OnProgressMessage("Progress_ChangingCameraState");

                var StateChangeCanceller = new CancellationTokenSource(15000);
                try
                {
                    if (!await PlaybackModeHelper.MoveToContentTransferModeAsync(TargetDevice, StateChangeCanceller))
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

                OnProgressMessage("Progress_FetchingContents");
                loader.PartLoaded += RemoteContentsLoader_PartLoaded;
                await loader.Load(ApplicationSettings.GetInstance().RemoteContentsSet, Canceller);
                DebugUtil.Log("RemoteApiContentsLoader completed");
            }
            catch (StorageNotSupportedException)
            {
                // This will never happen on camera devices.
                DebugUtil.Log("storage scheme is not supported");
                OnErrorMessage("Viewer_StorageAccessNotSupported");
            }
            catch (NoStorageException)
            {
                DebugUtil.Log("No storages");
                OnErrorMessage("Viewer_NoStorage");
            }
            catch (Exception e)
            {
                DebugUtil.Log(e.StackTrace);
                OnErrorMessage("Viewer_FailedToLoadContents");
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

            MovieScreen.DataContext = MovieStreamHelper.INSTANCE.MoviePlaybackData;

            if (TargetDevice.Api.AvContent == null)
            {
                OnErrorMessage("Viewer_NoAvContentApi");
                throw new IOException();
            }

            var item = content.Source as ContentInfo;

            if (!item.RemotePlaybackAvailable)
            {
                OnErrorMessage("Viewer_UnplayableContent");
                throw new IOException();
            }
            var started = await MovieStreamHelper.INSTANCE.SetContentAndStart(TargetDevice.Api.AvContent, new PlaybackContent
            {
                Uri = (item as RemoteApiContentInfo).Uri,
                RemotePlayType = RemotePlayMode.SimpleStreaming,
            }, content.Source.Name);
            if (!started)
            {
                OnErrorMessage("Viewer_FailedPlaybackMovie");
                throw new IOException();
            }

            MovieScreen.NotifyStartingStreamingMoviePlayback();
        }

        public override async Task LoadRemainingContents(RemainingContentsHolder holder)
        {
            var loader = new RemoteApiContentsLoader(TargetDevice);
            loader.PartLoaded += RemoteContentsLoader_PartLoaded;
            try
            {
                await loader.LoadRemainingAsync(holder, ApplicationSettings.GetInstance().RemoteContentsSet, Canceller);
            }
            catch (Exception e)
            {
                DebugUtil.Log(e.StackTrace);
                OnErrorMessage("Viewer_FailedToLoadContents");
            }
            finally
            {
                loader.PartLoaded -= RemoteContentsLoader_PartLoaded;
            }
        }
    }
}
