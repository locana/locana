using Kazyx.ImageStream;
using Kazyx.RemoteApi;
using Kazyx.RemoteApi.Camera;
using Kazyx.Uwpmm.DataModel;
using Kazyx.Uwpmm.Playback;
using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Kazyx.Uwpmm.CameraControl
{
    public class SequentialOperation
    {
        public static async Task SetUp(TargetDevice device, StreamProcessor liveview, CancellationTokenSource cancel = null)
        {
            DebugUtil.Log("Set up control");
            try
            {
                await device.Api.RetrieveApiList();
                var info = await device.Api.Camera.GetApplicationInfoAsync().ConfigureAwait(false);
                cancel.ThrowIfCancelled();

                device.Api.Capability.Version = new ServerVersion(info.Version);

                await device.Observer.StartAsync().ConfigureAwait(false);
                cancel.ThrowIfCancelled();

                if (device.Api.AvContent != null)
                {
                    DebugUtil.Log("This device support ContentsTransfer mode. Turn on Shooting mode at first.");
                    if (!await PlaybackModeHelper.MoveToShootingModeAsync(device, cancel).ConfigureAwait(false))
                    {
                        throw new Exception();
                    }
                    cancel.ThrowIfCancelled();
                }

                if (device.Api.Capability.IsSupported("startRecMode"))
                {
                    await device.Api.Camera.StartRecModeAsync().ConfigureAwait(false);
                    cancel.ThrowIfCancelled();
                }

                // No need to check runtime availability. We have to open stream except in audio mode.
                if (device.Status.ShootMode.Current != ShootModeParam.Audio)
                {
                    if (!await OpenLiveviewStream(device.Api, liveview).ConfigureAwait(false))
                    {
                        DebugUtil.Log("Failed to open liveview connection.");
                        throw new Exception("Failed to open liveview connection.");
                    }
                    cancel.ThrowIfCancelled();
                }

                if (device.Api.Capability.IsSupported("setCurrentTime"))
                {
                    try
                    {
                        await device.Api.System.SetCurrentTimeAsync( //
                            DateTimeOffset.UtcNow, (int)DateTimeOffset.Now.Offset.TotalMinutes).ConfigureAwait(false);
                    }
                    catch (RemoteApiException) { } // This API always fails on some models.
                    cancel.ThrowIfCancelled();
                }
            }
            catch (RemoteApiException e)
            {
                DebugUtil.Log("Failed setup: " + e.code);
                device.Observer.Stop();
                throw;
            }
            catch (OperationCanceledException e)
            {
                DebugUtil.Log("Operation cancelled");
                device.Observer.Stop();
                throw;
            }
        }

        public static async Task<bool> OpenLiveviewStream(DeviceApiHolder api, StreamProcessor liveview)
        {
            DebugUtil.Log("Open liveview stream");
            try
            {
                var url = await api.Camera.StartLiveviewAsync().ConfigureAwait(false);
                return await liveview.OpenConnection(new Uri(url)).ConfigureAwait(false);
            }
            catch (RemoteApiException e)
            {
                DebugUtil.Log("Failed to startLiveview: " + e.code);
                return false;
            }
            catch (Exception e)
            {
                DebugUtil.Log("Unknown error while opening liveview stream: " + e.StackTrace);
                return false;
            }
        }

        public static async Task<bool> CloseLiveviewStream(DeviceApiHolder api, StreamProcessor liveview)
        {
            DebugUtil.Log("Close liveview stream");
            try
            {
                liveview.CloseConnection();
                await api.Camera.StopLiveviewAsync().ConfigureAwait(false);
                return true;
            }
            catch (RemoteApiException e)
            {
                DebugUtil.Log("Failed to stopLiveview: " + e.code);
                return false;
            }
        }

        public static async Task<bool> ReOpenLiveviewStream(DeviceApiHolder api, StreamProcessor liveview)
        {
            DebugUtil.Log("Reopen liveview stream");
            liveview.CloseConnection();
            await Task.Delay(2000).ConfigureAwait(false);
            return await OpenLiveviewStream(api, liveview).ConfigureAwait(false);
        }

        public static async Task<bool> TakePicture(DeviceApiHolder api, Geoposition position)
        {
            return await TakePicture(api, position, false).ConfigureAwait(false);
        }

        private static async Task<bool> TakePicture(DeviceApiHolder api, Geoposition position, bool awaiting = false)
        {
            DebugUtil.Log("Taking picture sequence");
            try
            {
                var urls = awaiting //
                    ? await api.Camera.AwaitTakePictureAsync().ConfigureAwait(false) //
                    : await api.Camera.ActTakePictureAsync().ConfigureAwait(false);
                DebugUtil.Log("Success taking picture");

                if (ApplicationSettings.GetInstance().IsPostviewTransferEnabled)
                {
                    foreach (var url in urls)
                    {
                        try
                        {
                            var uri = new Uri(url);
                            MediaDownloader.Instance.EnqueuePostViewImage(uri, position);
                        }
                        catch (Exception e)
                        {
                            DebugUtil.Log(e.Message);
                            DebugUtil.Log(e.StackTrace);
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch (RemoteApiException e)
            {
                if (e.code != StatusCode.StillCapturingNotFinished)
                {
                    DebugUtil.Log("Failed to take picture: " + e.code);
                    throw e;
                }
            }
            DebugUtil.Log("Take picture timeout: await for completion");
            return await TakePicture(api, position, true).ConfigureAwait(false);
        }

        public static async Task StopContinuousShooting(DeviceApiHolder api)
        {
            int retry = 5;
            while (retry-- > 0)
            {
                try
                {
                    await api.Camera.StopContShootingAsync();
                    break;
                }
                catch (RemoteApiException) { }
                DebugUtil.Log("failed to stop cont shooting. retry count: " + retry);
                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
        }

        internal static async Task CleanupShootingMode(TargetDevice device)
        {
            switch (device.Status.Status)
            {
                case EventParam.MvRecording:
                    try { await device.Api.Camera.StopMovieRecAsync(); }
                    catch (RemoteApiException) { }
                    break;
                case EventParam.AuRecording:
                    try { await device.Api.Camera.StopAudioRecAsync(); }
                    catch (RemoteApiException) { }
                    break;
                case EventParam.ItvRecording:
                    try { await device.Api.Camera.StopIntervalStillRecAsync(); }
                    catch (RemoteApiException) { }
                    break;
            }
        }

        private delegate Task RemoteRequestTask();
        private static async void TryToStart(RemoteRequestTask task, Action<ShootingResult> Finished)
        {
            try
            {
                await task();
                if (Finished != null) { Finished.Invoke(ShootingResult.StartSucceed); }
            }
            catch (RemoteApiException ex)
            {
                DebugUtil.Log(ex.StackTrace);
                if (Finished != null) { Finished.Invoke(ShootingResult.StartFailed); }
            }
        }

        private static async void TryToStop(RemoteRequestTask task, Action<ShootingResult> Finished)
        {
            try
            {
                await task();
                if (Finished != null) { Finished.Invoke(ShootingResult.StopSucceed); }
            }
            catch (RemoteApiException ex)
            {
                DebugUtil.Log(ex.StackTrace);
                if (Finished != null) { Finished.Invoke(ShootingResult.StopFailed); }

            }
        }

        internal enum ShootingResult
        {
            StillSucceed,
            StillFailed,
            StartSucceed,
            StartFailed,
            StopSucceed,
            StopFailed,
        };

        internal static async Task StartStopRecording(List<TargetDevice> devices, Action<ShootingResult> Finished)
        {
            foreach (var target in devices)
            {
                if (target == null || target.Status == null || target.Status.ShootMode == null) { return; }

                switch (target.Status.ShootMode.Current)
                {
                    case ShootModeParam.Still:
                        try
                        {
                            await TakePicture(target.Api, GeopositionManager.INSTANCE.LatestPosition);
                            if (Finished != null) { Finished.Invoke(ShootingResult.StillSucceed); }
                        }
                        catch (RemoteApiException e)
                        {
                            DebugUtil.Log(e.StackTrace);
                            if (Finished != null) { Finished.Invoke(ShootingResult.StillFailed); }
                        }
                        break;
                    case ShootModeParam.Movie:
                        if (target.Status.Status == EventParam.Idle)
                        {
                            TryToStart(target.Api.Camera.StartMovieRecAsync, Finished);
                        }
                        else if (target.Status.Status == EventParam.MvRecording)
                        {
                            TryToStop(target.Api.Camera.StopMovieRecAsync, Finished);
                        }
                        break;
                    case ShootModeParam.Audio:
                        if (target.Status.Status == EventParam.Idle)
                        {
                            TryToStart(target.Api.Camera.StartAudioRecAsync, Finished);
                        }
                        else if (target.Status.Status == EventParam.AuRecording)
                        {
                            TryToStop(target.Api.Camera.StopAudioRecAsync, Finished);
                        }
                        break;
                    case ShootModeParam.Interval:
                        if (target.Status.Status == EventParam.Idle)
                        {
                            TryToStart(target.Api.Camera.StartIntervalStillRecAsync, Finished);
                        }
                        else if (target.Status.Status == EventParam.ItvRecording)
                        {
                            TryToStop(target.Api.Camera.StopIntervalStillRecAsync, Finished);
                        }
                        break;
                    case ShootModeParam.Loop:
                        if (target.Status.Status == EventParam.Idle)
                        {
                            TryToStart(target.Api.Camera.StartLoopRecAsync, Finished);
                        }
                        else if (target.Status.Status == EventParam.LoopRecording)
                        {
                            TryToStop(target.Api.Camera.StopLoopRecAsync, Finished);
                        }
                        break;
                }
            }
        }
    }
}
