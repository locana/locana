using Kazyx.RemoteApi;
using Kazyx.RemoteApi.Camera;
using Kazyx.Uwpmm.DataModel;
using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kazyx.Uwpmm.CameraControl
{
    public class StatusObserver
    {
        public StatusObserver(TargetDevice device)
        {
            this.api = device.Api;
            this.status = device.Status;
        }

        private readonly DeviceApiHolder api;

        private readonly CameraStatus status;

        public bool IsProcessing { private set; get; }

        private int failure_count = 0;

        private const int RETRY_LIMIT = 3;

        private const int RETRY_INTERVAL_SEC = 3;

        public event Action EndByError;

        private ApiVersion version = ApiVersion.V1_0;

        private CancellationTokenSource cancel;

        public async Task<bool> StartAsync()
        {
            DebugUtil.Log("StatusObserver: Start");
            if (IsProcessing)
            {
                DebugUtil.Log("StatusObserver: Already processing");
                return false;
            }

            if (api.Capability.IsSupported("getEvent", "1.3")) { version = ApiVersion.V1_3; }
            else if (api.Capability.IsSupported("getEvent", "1.2")) { version = ApiVersion.V1_2; }
            else if (api.Capability.IsSupported("getEvent", "1.1")) { version = ApiVersion.V1_1; }
            else { version = ApiVersion.V1_0; }

            failure_count = 0;
            if (!await Refresh().ConfigureAwait(false))
            {
                DebugUtil.Log("StatusObserver: Failed to start");
                return false;
            }

            cancel = new CancellationTokenSource();

            IsProcessing = true;
            PollingLoop();
            return true;
        }

        public void Stop()
        {
            DebugUtil.Log("StatusObserver: Stop");
            if (cancel != null)
            {
                cancel.Cancel();
            }
            IsProcessing = false;
        }

        public async Task<bool> Refresh()
        {
            DebugUtil.Log("StatusObserver: Refresh");
            try
            {
                await UpdateStatus(await api.Camera.GetEventAsync(false, version)).ConfigureAwait(false);
            }
            catch (RemoteApiException e)
            {
                DebugUtil.Log("StatusObserver: Refresh failed - " + e.code);
                return false;
            }
            return true;
        }

        private async Task UpdateStatus(Event @event)
        {
            TrySetNotNull(@event.AvailableApis, val => api.Capability.AvailableApis = val);

            status.IsLiveviewAvailable = @event.LiveviewAvailable;
            TrySetNotNull(@event.ShootModeInfo, val => status.ShootMode = val);
            TrySetNotNull(@event.ExposureMode, val => status.ExposureMode = val);
            TrySetNotNull(@event.ISOSpeedRate, val => status.ISOSpeedRate = val);
            TrySetNotNull(@event.ShutterSpeed, val => status.ShutterSpeed = val);
            TrySetNotNull(@event.FNumber, val => status.FNumber = val);
            TrySetNotNull(@event.ZoomInfo, val => status.ZoomInfo = val);
            TrySetNotNull(@event.PostviewSizeInfo, val => status.PostviewSize = val);
            TrySetNotNull(@event.SelfTimerInfo, val => status.SelfTimer = val);
            TrySetNotNull(@event.BeepMode, val => status.BeepMode = val);
            TrySetNotNull(@event.FlashMode, val => status.FlashMode = val);
            TrySetNotNull(@event.FocusMode, val => status.FocusMode = val);
            TrySetNotNull(@event.ViewAngle, val => status.ViewAngle = val);
            TrySetNotNull(@event.SteadyMode, val => status.SteadyMode = val);
            TrySetNotNull(@event.MovieQuality, val => status.MovieQuality = val);
            TrySetNotNull(@event.TouchAFStatus, val => status.TouchFocusStatus = val);
            TrySetNotNull(@event.ProgramShiftActivated, val => status.ProgramShiftActivated = val.Value);
            TrySetNotNull(@event.PictureUrls, val => status.PictureUrls = val);
            TrySetNotNull(@event.LiveviewOrientation, val => status.LiveviewOrientation = val);
            TrySetNotNull(@event.EvInfo, val => status.EvInfo = val);
            TrySetNotNull(@event.CameraStatus, val => status.Status = val);
            TrySetNotNull(@event.StorageInfo, val => status.Storages = val);
            TrySetNotNull(@event.BatteryInfo, val => status.BatteryInfo = val);
            TrySetNotNull(@event.FNumber, val => status.FNumber = val);
            TrySetNotNull(@event.ShutterSpeed, val => status.ShutterSpeed = val);
            TrySetNotNull(@event.EvInfo, val => status.EvInfo = val);
            TrySetNotNull(@event.ISOSpeedRate, val => status.ISOSpeedRate = val);
            TrySetPositiveOrZero(@event.RecordingTimeSec, val => status.RecordingTimeSec = val);
            TrySetPositiveOrZero(@event.NumberOfShots, val => status.NumberOfShots = val);
            TrySetNotNull(@event.ContShootingMode, val => status.ContShootingMode = val);
            TrySetNotNull(@event.ContShootingSpeed, val => status.ContShootingSpeed = val);
            TrySetNotNull(@event.ContShootingResult, val => status.ContShootingResult = val);
            TrySetNotNull(@event.ZoomSetting, val => status.ZoomSetting = val);
            TrySetNotNull(@event.SceneSelection, val => status.SceneSelection = val);
            TrySetNotNull(@event.TrackingFocusMode, val => status.TrackingFocus = val);
            TrySetNotNull(@event.TrackingFocusStatus, val => status.TrackingFocusStatus = val);
            TrySetNotNull(@event.MovieFormat, val => status.MovieFileFormat = val);
            TrySetNotNull(@event.FlipMode, val => status.FlipMode = val);
            TrySetNotNull(@event.IntervalTime, val => status.IntervalTime = val);
            TrySetNotNull(@event.ColorSetting, val => status.ColorSetting = val);
            TrySetNotNull(@event.IrRemoteControl, val => status.InfraredRemoteControl = val);
            TrySetNotNull(@event.TvColorSystem, val => status.TvColorSystem = val);
            TrySetNotNull(@event.AutoPowerOff, val => status.AutoPowerOff = val);
            TrySetNotNull(@event.ImageQuality, val => status.StillQuality = val);
            TrySetNotNull(@event.LoopRecTime, val => status.LoopRecTime = val);
            TrySetNotNull(@event.WindNoiseReduction, val => status.WindNoiseReduction = val);
            TrySetNotNull(@event.AudioRecording, val => status.AudioRecording = val);

            if (@event.StillImageSize != null)
            {
                if (@event.StillImageSize.CapabilityChanged)
                {
                    try
                    {
                        var size = await api.Camera.GetAvailableStillSizeAsync().ConfigureAwait(false);
                        size.Candidates.Sort(CompareStillSize);
                        status.StillImageSize = size;
                    }
                    catch (RemoteApiException)
                    {
                        DebugUtil.Log("Failed to get still image size capability");
                    }
                }
                else
                {
                    status.StillImageSize.Current = @event.StillImageSize.Current;
                    status.StillImageSize = status.StillImageSize;
                }
            }

            if (@event.WhiteBalance != null)
            {
                if (@event.WhiteBalance.CapabilityChanged)
                {
                    try
                    {
                        var wb = await api.Camera.GetAvailableWhiteBalanceAsync().ConfigureAwait(false);
                        var candidates = new List<string>();
                        var tmpCandidates = new Dictionary<string, int[]>();
                        foreach (var mode in wb.Candidates)
                        {
                            candidates.Add(mode.WhiteBalanceMode);
                            var tmpList = new List<int>();
                            if (mode.Candidates.Count == 3)
                            {
                                for (int i = mode.Candidates[1]; i <= mode.Candidates[0]; i += mode.Candidates[2])
                                {
                                    tmpList.Add(i);
                                }
                            }
                            tmpCandidates.Add(mode.WhiteBalanceMode, tmpList.ToArray());
                        }

                        status.WhiteBalance = new Capability<string> { Candidates = candidates, Current = wb.Current.Mode };
                        status.ColorTempertureCandidates = tmpCandidates;
                        status.ColorTemperture = wb.Current.ColorTemperature;
                    }
                    catch (RemoteApiException)
                    {
                        DebugUtil.Log("Failed to get white balance capability");
                    }
                }
                else
                {
                    if (@event.WhiteBalance != null)
                    {
                        status.WhiteBalance.Current = @event.WhiteBalance.Current.Mode;
                    }
                    status.ColorTemperture = @event.WhiteBalance.Current.ColorTemperature;
                }
            }
        }

        private async void PollingLoop()
        {
            if (!IsProcessing)
            {
                return;
            }

            try
            {
                OnSuccess(await api.Camera.GetEventAsync(true, version, cancel).ConfigureAwait(false));
            }
            catch (RemoteApiException e)
            {
                OnError(e.code);
            }
        }

        private async void OnSuccess(Event @event)
        {
            failure_count = 0;
            await UpdateStatus(@event).ConfigureAwait(false);
            PollingLoop();
        }

        private async void OnError(StatusCode code)
        {
            switch (code)
            {
                case StatusCode.Timeout:
                    DebugUtil.Log("GetEvent timeout without any event. Retry for the next event");
                    PollingLoop();
                    return;
                case StatusCode.NotAcceptable:
                case StatusCode.CameraNotReady:
                case StatusCode.IllegalState:
                case StatusCode.ServiceUnavailable:
                case StatusCode.Any:
                    if (failure_count++ < RETRY_LIMIT)
                    {
                        DebugUtil.Log("GetEvent failed - retry " + failure_count + ", status: " + code);
                        await Task.Delay(TimeSpan.FromSeconds(RETRY_INTERVAL_SEC)).ConfigureAwait(false);
                        PollingLoop();
                        return;
                    }
                    break;
                case StatusCode.DuplicatePolling:
                    DebugUtil.Log("GetEvent failed duplicate polling");
                    // Long polling is now cancellable. Duplicated polling should not happen.
                    break;
                case StatusCode.Cancelled:
                    DebugUtil.Log("GetEvent polling loop cancelled.");
                    return;
                default:
                    DebugUtil.Log("GetEvent failed with code: " + code);
                    break;
            }

            DebugUtil.Log("StatusObserver Error limit");

            if (IsProcessing)
            {
                Stop();
                EndByError.Raise();
            }
        }

        private bool TrySetNotNull<T>(T newValue, Action<T> setter)
        {
            if (newValue == null)
            {
                return false;
            }
            setter(newValue);
            return true;
        }

        private bool TrySetPositiveOrZero(int newValue, Action<int> setter)
        {
            if (newValue < 0)
            {
                return false;
            }
            setter(newValue);
            return true;
        }

        private static int CompareStillSize(StillImageSize x, StillImageSize y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }

            if (!x.SizeDefinition.EndsWith("M") || !y.SizeDefinition.EndsWith("M"))
            {
                var comp = x.SizeDefinition.CompareTo(y.SizeDefinition);
                if (comp == 0)
                {
                    return x.AspectRatio.CompareTo(y.AspectRatio);
                }
                else
                {
                    return comp;
                }
            }

            var xv = (int)double.Parse(x.SizeDefinition.Substring(0, x.SizeDefinition.Length - 1)) * 100;
            var yv = (int)double.Parse(y.SizeDefinition.Substring(0, y.SizeDefinition.Length - 1)) * 100;

            if (xv == yv)
            {
                return x.AspectRatio.CompareTo(y.AspectRatio);
            }
            else
            {
                return xv < yv ? 1 : -1;
            }
        }
    }
}
