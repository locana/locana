
using Kazyx.RemoteApi.Camera;
using Locana.CameraControl;
using Locana.Utility;
using System;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
namespace Locana.DataModel
{
    public class LiveviewScreenViewData : ObservableBase
    {
        public TargetDevice Device { private set; get; }

        public LiveviewScreenViewData(TargetDevice d)
        {
            Device = d;
            Device.Status.PropertyChanged += (sender, e) =>
            {
                NotifyChangedOnUI(nameof(ZoomPositionInCurrentBox));
                NotifyChangedOnUI(nameof(ZoomBoxIndex));
                NotifyChangedOnUI(nameof(ZoomBoxNum));
                NotifyChangedOnUI(nameof(ShutterButtonImage));
                NotifyChangedOnUI(nameof(ShutterButtonEnabled));
                NotifyChangedOnUI(nameof(IsRecording));
                NotifyChangedOnUI(nameof(ShootModeImage));
                NotifyChangedOnUI(nameof(ExposureModeImage));
                NotifyChangedOnUI(nameof(MemoryCardStatusImage));
                NotifyChangedOnUI(nameof(RecordbaleAmount));
                NotifyChangedOnUI(nameof(RecordingCount));
                NotifyChangedOnUI(nameof(IsRecordingCountAvailable));
                NotifyChangedOnUI(nameof(EvDisplayValue));
                NotifyChangedOnUI(nameof(FnumberDisplayValue));
                NotifyChangedOnUI(nameof(ISODisplayValue));
                NotifyChangedOnUI(nameof(ShutterSpeedDisplayValue));
                NotifyChangedOnUI(nameof(IsAudioMode));
                NotifyChangedOnUI(nameof(LiveviewImageDisplayed));
                NotifyChangedOnUI(nameof(FramingGridDisplayed));
                NotifyChangedOnUI(nameof(IsBatteryInfoAvailable));
                NotifyChangedOnUI(nameof(IsAvailableGetShutterSpeed));
                NotifyChangedOnUI(nameof(IsAvailableGetFNumber));
                NotifyChangedOnUI(nameof(IsAvailableGetIsoSpeedRate));
                NotifyChangedOnUI(nameof(IsAvailableGetEV));
                NotifyChangedOnUI(nameof(DisplayParamsArea));
                NotifyChangedOnUI(nameof(HistogramDisplayed));
            };
            Device.Api.AvailiableApisUpdated += (sender, e) =>
            {
                NotifyChangedOnUI(nameof(IsZoomAvailable));
                NotifyChangedOnUI(nameof(ShutterButtonEnabled));
                NotifyChangedOnUI(nameof(IsRecording));
                NotifyChangedOnUI(nameof(FNumberBrush));
                NotifyChangedOnUI(nameof(ShutterSpeedBrush));
                NotifyChangedOnUI(nameof(EvBrush));
                NotifyChangedOnUI(nameof(IsoBrush));
                NotifyChangedOnUI(nameof(ShutterSpeedDisplayValue));
                NotifyChangedOnUI(nameof(ISODisplayValue));
                NotifyChangedOnUI(nameof(FnumberDisplayValue));
                NotifyChangedOnUI(nameof(IsSetFNumberAvailable));
                NotifyChangedOnUI(nameof(IsSetShutterSpeedAvailable));
                NotifyChangedOnUI(nameof(IsSetIsoSpeedRateAvailable));
                NotifyChangedOnUI(nameof(IsSetEVAvailable));
                NotifyChangedOnUI(nameof(IsAvailableGetShutterSpeed));
                NotifyChangedOnUI(nameof(IsAvailableGetFNumber));
                NotifyChangedOnUI(nameof(IsAvailableGetIsoSpeedRate));
                NotifyChangedOnUI(nameof(IsAvailableGetEV));
                NotifyChangedOnUI(nameof(IsShootingParamAvailable));
                NotifyChangedOnUI(nameof(IsShootingParamSettingAvailable));
                NotifyChangedOnUI(nameof(IsProgramShiftAvailable));
                NotifyChangedOnUI(nameof(DisplayParamsArea));
                NotifyChangedOnUI(nameof(ShootModeChangingAvailable));
            };

            ApplicationSettings.GetInstance().PropertyChanged += (sender, args) =>
            {
                NotifyChangedOnUI(nameof(ShutterButtonImage));
                NotifyChangedOnUI(nameof(HistogramDisplayed));
            };
        }

        private static readonly DataTemplate StillIconTemplate = (DataTemplate)Application.Current.Resources["StillIcon"];
        private static readonly DataTemplate MovieIconTemplate = (DataTemplate)Application.Current.Resources["MovieIcon"];
        private static readonly DataTemplate AudioIconTemplate = (DataTemplate)Application.Current.Resources["AudioIcon"];
        private static readonly DataTemplate IntervalStillIconTemplate = (DataTemplate)Application.Current.Resources["IntervalStillIcon"];
        private static readonly DataTemplate ContinuousStillIconTemplate = (DataTemplate)Application.Current.Resources["ContShootingIcon"];
        private static readonly DataTemplate LoopIconTemplate = (DataTemplate)Application.Current.Resources["LoopIcon"];

        private static readonly DataTemplate StopIconTemplate = (DataTemplate)Application.Current.Resources["StopIcon"];

        private static readonly DataTemplate AModeTemplate = (DataTemplate)Application.Current.Resources["Mode_A"];
        private static readonly DataTemplate IAModeTemplate = (DataTemplate)Application.Current.Resources["Mode_IA"];
        private static readonly DataTemplate IAPlusModeTemplate = (DataTemplate)Application.Current.Resources["Mode_IAPlus"];
        private static readonly DataTemplate MModeTemplate = (DataTemplate)Application.Current.Resources["Mode_M"];
        private static readonly DataTemplate SModeTemplate = (DataTemplate)Application.Current.Resources["Mode_S"];
        private static readonly DataTemplate PModeTemplate = (DataTemplate)Application.Current.Resources["Mode_P"];
        private static readonly DataTemplate PShiftModeTemplate = (DataTemplate)Application.Current.Resources["Mode_PShift"];

        private static readonly DataTemplate MediaIconTemplate = (DataTemplate)Application.Current.Resources["MemoryCardIcon"];
        private static readonly DataTemplate NoMediaIconTemplate = (DataTemplate)Application.Current.Resources["NoMemoryCardIcon"];

        public static DataTemplate GetShootModeIcon(string mode)
        {
            switch (mode ?? "")
            {
                case ShootModeParam.Still:
                    return StillIconTemplate;
                case ShootModeParam.Movie:
                    return MovieIconTemplate;
                case ShootModeParam.Audio:
                    return AudioIconTemplate;
                case ShootModeParam.Interval:
                    return IntervalStillIconTemplate;
                case ShootModeParam.Loop:
                    return LoopIconTemplate;
                default:
                    return default(DataTemplate);
            }
        }

        public DataTemplate ShutterButtonImage
        {
            get
            {
                if (IsRecording)
                {
                    return StopIconTemplate;
                }

                var mode = Device.Status?.ShootMode?.Current ?? "";

                if (mode == ShootModeParam.Still)
                {
                    switch (Device.Status?.ContShootingMode?.Current ?? "")
                    {
                        case ContinuousShootMode.Cont:
                        case ContinuousShootMode.SpeedPriority:
                        case ContinuousShootMode.Burst:
                        case ContinuousShootMode.MotionShot:
                            return ContinuousStillIconTemplate;
                        default:
                            break;
                    }

                    return ApplicationSettings.GetInstance().IsIntervalShootingEnabled ? IntervalStillIconTemplate : StillIconTemplate;
                }
                else
                {
                    return GetShootModeIcon(mode);
                }
            }
        }

        private bool _IsPeriodicalShootingRunning = false;
        public bool IsPeriodicalShootingRunning
        {
            get { return _IsPeriodicalShootingRunning; }
            set
            {
                if (_IsPeriodicalShootingRunning != value)
                {
                    _IsPeriodicalShootingRunning = value;
                    NotifyChangedOnUI(nameof(IsRecording));
                    NotifyChangedOnUI(nameof(ShutterButtonImage));
                }
            }
        }

        public bool IsRecording { get { return Device.Status.IsRecording() || IsPeriodicalShootingRunning; } }

        public DataTemplate ShootModeImage
        {
            get
            {
                if (Device.Status.ShootMode == null) { return null; }
                switch (Device.Status.ShootMode.Current ?? "")
                {
                    case ShootModeParam.Still:
                        return StillIconTemplate;
                    case ShootModeParam.Movie:
                        return MovieIconTemplate;
                    case ShootModeParam.Interval:
                        return IntervalStillIconTemplate;
                    case ShootModeParam.Audio:
                        // Don't show audio icon on top.
                        return null;
                    case ShootModeParam.Loop:
                        return LoopIconTemplate;
                }
                return null;
            }
        }

        public DataTemplate ExposureModeImage
        {
            get
            {
                if (Device.Status.ExposureMode == null) { return null; }
                if (Device.Status?.ShootMode?.Current != ShootModeParam.Still) { return null; }

                switch (Device.Status.ExposureMode.Current ?? "")
                {
                    case ExposureMode.Intelligent:
                        return IAModeTemplate;
                    case ExposureMode.Superior:
                        return IAPlusModeTemplate;
                    case ExposureMode.Program:
                        if (Device.Status.ProgramShiftActivated) { return PShiftModeTemplate; }
                        return PModeTemplate;
                    case ExposureMode.Aperture:
                        return AModeTemplate;
                    case ExposureMode.SS:
                        return SModeTemplate;
                    case ExposureMode.Manual:
                        return MModeTemplate;
                }
                return null;
            }
        }

        public DataTemplate MemoryCardStatusImage
        {
            get
            {
                if (Device.Status.Storages == null) return null;
                foreach (var storage in Device.Status.Storages)
                {
                    if (storage.RecordTarget)
                    {
                        switch (storage.StorageID)
                        {
                            case "No Media":
                                return NoMediaIconTemplate;
                            case "Memory Card 1":
                            default:
                                return MediaIconTemplate;
                        }
                    }
                }
                return NoMediaIconTemplate;
            }
        }

        public bool ShutterButtonEnabled
        {
            get
            {
                if (Device.Api.Capability == null)
                {
                    return false;
                }
                switch (Device.Status.ShootMode?.Current ?? "")
                {
                    case ShootModeParam.Still:
                        if (Device.Status.Status == EventParam.Idle) { return true; }
                        if (ApplicationSettings.GetInstance().IsIntervalShootingEnabled) { return true; }
                        if (
                            Device.Status.Status == EventParam.StCapturing &&
                            Device.Status.ContShootingMode != null &&
                            (Device.Status.ContShootingMode.Current == ContinuousShootMode.Cont ||
                            Device.Status.ContShootingMode.Current == ContinuousShootMode.SpeedPriority))
                        {
                            return true;
                        }
                        break;
                    case ShootModeParam.Movie:
                        return Device.Status.Status == EventParam.Idle || Device.Status.Status == EventParam.MvRecording;
                    case ShootModeParam.Audio:
                        return Device.Status.Status == EventParam.Idle || Device.Status.Status == EventParam.AuRecording;
                    case ShootModeParam.Interval:
                        return Device.Status.Status == EventParam.Idle || Device.Status.Status == EventParam.ItvRecording;
                    case ShootModeParam.Loop:
                        return Device.Status.Status == EventParam.Idle || Device.Status.Status == EventParam.LoopRecording;
                }
                return false;
            }
        }

        public bool ShootModeChangingAvailable { get { return Device.Api.Capability?.IsAvailable("setShootMode") ?? false; } }

        public bool IsZoomAvailable { get { return Device.Api.Capability?.IsAvailable("actZoom") ?? false; } }

        public int ZoomPositionInCurrentBox { get { return Device.Status?.ZoomInfo?.PositionInCurrentBox ?? 0; } }

        public int ZoomBoxIndex { get { return Device.Status?.ZoomInfo?.CurrentBoxIndex ?? 0; } }

        public int ZoomBoxNum { get { return Device.Status?.ZoomInfo?.NumberOfBoxes ?? 0; } }

        public string RecordbaleAmount
        {
            get
            {
                if (Device.Status.Storages == null || Device.Status.ShootMode == null) { return ""; }
                foreach (StorageInfo storage in Device.Status.Storages)
                {
                    if (storage.RecordTarget)
                    {
                        switch (Device.Status.ShootMode.Current ?? "")
                        {
                            case ShootModeParam.Still:
                            case ShootModeParam.Interval:
                                if (storage.RecordableImages == -1) { return ""; }
                                return storage.RecordableImages.ToString();
                            case ShootModeParam.Movie:
                            case ShootModeParam.Audio:
                            case ShootModeParam.Loop:
                                if (storage.RecordableMovieLength == -1) { return ""; }
                                return string.Format(SystemUtil.GetStringResource("Minutes_NoTrans"), storage.RecordableMovieLength);
                            default:
                                break;
                        }
                    }
                }
                return "";
            }
        }

        public string RecordingCount
        {
            get
            {
                switch (Device.Status?.ShootMode?.Current ?? "")
                {
                    case ShootModeParam.Movie:
                    case ShootModeParam.Audio:
                    case ShootModeParam.Loop:
                        if (Device.Status.RecordingTimeSec < 0) { return ""; }
                        var min = Device.Status.RecordingTimeSec / 60;
                        var sec = Device.Status.RecordingTimeSec - min * 60;
                        return min.ToString("##00") + ":" + sec.ToString("00");
                    case ShootModeParam.Interval:
                        if (Device.Status.NumberOfShots < 0) { return ""; }
                        return string.Format(SystemUtil.GetStringResource("Pics_NoTrans"), Device.Status.NumberOfShots);
                }
                return "";
            }
        }

        public bool IsRecordingCountAvailable
        {
            get
            {
                switch (Device.Status?.ShootMode?.Current ?? "")
                {
                    case ShootModeParam.Movie:
                        return Device.Status.RecordingTimeSec > 0 && Device.Status.Status == EventParam.MvRecording;
                    case ShootModeParam.Audio:
                        return Device.Status.RecordingTimeSec > 0 && Device.Status.Status == EventParam.AuRecording;
                    case ShootModeParam.Loop:
                        return Device.Status.RecordingTimeSec > 0 && Device.Status.Status == EventParam.LoopRecording;
                    case ShootModeParam.Interval:
                        return Device.Status.NumberOfShots > 0 && Device.Status.Status == EventParam.ItvRecording;
                }
                return false;
            }
        }

        public bool DisplayParamsArea { get { return IsAvailableGetEV || IsAvailableGetFNumber || IsAvailableGetIsoSpeedRate || IsAvailableGetShutterSpeed; } }
        public bool IsAvailableGetShutterSpeed { get { return Device.Status?.ShutterSpeed?.Current != null && Device.Api.Capability.IsAvailable("getShutterSpeed"); } }
        public bool IsAvailableGetFNumber { get { return Device.Status?.FNumber?.Current != null && Device.Api.Capability.IsAvailable("getFNumber"); } }
        public bool IsAvailableGetIsoSpeedRate { get { return Device.Status?.ISOSpeedRate?.Current != null && Device.Api.Capability.IsAvailable("getIsoSpeedRate"); } }
        public bool IsAvailableGetEV { get { return Device.Status?.EvInfo != null && Device.Api.Capability.IsAvailable("getExposureCompensation"); } }

        public string ShutterSpeedDisplayValue { get { return Device.Status?.ShutterSpeed?.Current ?? "--"; } }

        public string ISODisplayValue
        {
            get
            {
                if (Device.Status == null || Device.Status.ISOSpeedRate == null || Device.Status.ISOSpeedRate.Current == null) { return ""; }
                else { return "ISO " + Device.Status.ISOSpeedRate.Current; }
            }
        }

        public string FnumberDisplayValue
        {
            get
            {
                if (Device.Status == null || Device.Status.FNumber == null || Device.Status.FNumber.Current == null) { return "F--"; }
                else { return "F" + Device.Status.FNumber.Current; }
            }
        }

        public string EvDisplayValue
        {
            get
            {
                if (Device.Status == null || Device.Status.EvInfo == null) { return ""; }
                else
                {
                    var value = EvConverter.GetEv(Device.Status.EvInfo.CurrentIndex, Device.Status.EvInfo.Candidate.IndexStep);
                    var strValue = Math.Round(value, 1, MidpointRounding.AwayFromZero).ToString("0.0");

                    if (value < 0) { return "EV " + strValue; }
                    else if (value == 0.0f) { return "EV " + strValue; }
                    else { return "EV +" + strValue; }
                }
            }
        }

        public Brush FNumberBrush { get { return ShootingParamBrush("setFNumber"); } }
        public Brush ShutterSpeedBrush { get { return ShootingParamBrush("setShutterSpeed"); } }
        public Brush EvBrush { get { return ShootingParamBrush("setExposureCompensation"); } }
        public Brush IsoBrush { get { return ShootingParamBrush("setIsoSpeedRate"); } }

        private Brush ShootingParamBrush(string api)
        {
            if (!Device.Api.Capability.IsAvailable(api)) { return ResourceManager.ForegroundBrush; }
            else { return ResourceManager.SystemControlForegroundAccentBrush; }
        }

        public bool IsShootingParamSettingAvailable { get { return IsSetFNumberAvailable || IsSetShutterSpeedAvailable || IsSetIsoSpeedRateAvailable || IsSetEVAvailable; } }
        public bool IsSetFNumberAvailable { get { return IsShootingParamAvailable("setFNumber"); } }
        public bool IsSetShutterSpeedAvailable { get { return IsShootingParamAvailable("setShutterSpeed"); } }
        public bool IsSetIsoSpeedRateAvailable { get { return IsShootingParamAvailable("setIsoSpeedRate"); } }
        public bool IsSetEVAvailable { get { return IsShootingParamAvailable("getExposureCompensation"); } }
        private bool IsShootingParamAvailable(string api) { return Device.Api != null && Device.Api.Capability.IsAvailable(api); }

        public bool IsProgramShiftAvailable { get { return Device.Api != null && Device.Api.Capability.IsAvailable("setProgramShift"); } }

        public bool IsBatteryInfoAvailable { get { return Device.Status.BatteryInfo != null; } }

        private static readonly BitmapImage GeoInfoStatusImage_OK = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/GeoInfoStatus_OK.png", UriKind.Absolute));
        private static readonly BitmapImage GeoInfoStatusImage_NG = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/GeoInfoStatus_NG.png", UriKind.Absolute));
        private static readonly BitmapImage GeoInfoStatusImage_Updating = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/GeoInfoStatus_Updating.png", UriKind.Absolute));

        private PositionStatus _GeopositionStatus = PositionStatus.Disabled;
        public PositionStatus GeopositionStatus
        {
            get { return _GeopositionStatus; }
            set
            {
                _GeopositionStatus = value;
                NotifyChangedOnUI(nameof(GeopositionStatusImage));
                DebugUtil.Log(() => "Geoposition status: " + value);
            }
        }

        public BitmapImage GeopositionStatusImage
        {
            get
            {
                switch (GeopositionStatus)
                {
                    case PositionStatus.Disabled:
                    case PositionStatus.NotAvailable:
                    case PositionStatus.NotInitialized:
                        return GeoInfoStatusImage_NG;
                    case PositionStatus.Initializing:
                    case PositionStatus.NoData:
                        return GeoInfoStatusImage_Updating;
                    case PositionStatus.Ready:
                        return GeoInfoStatusImage_OK;
                }
                return null;
            }
        }

        private bool _GeopositionEnabled;
        public bool GeopositionEnabled
        {
            get { return _GeopositionEnabled; }
            set
            {
                _GeopositionEnabled = value;
                NotifyChangedOnUI(nameof(GeopositionEnabled));
            }
        }

        public bool IsAudioMode { get { return Device.Status?.ShootMode?.Current == ShootModeParam.Audio; } }

        public bool LiveviewImageDisplayed { get { return !IsAudioMode; } }

        public bool HistogramDisplayed
        {
            get { return ApplicationSettings.GetInstance().IsHistogramDisplayed && LiveviewImageDisplayed; }
        }

        private bool _ConnectionEstablished;
        public bool ConnectionEstablished
        {
            get { return _ConnectionEstablished; }
            set
            {
                _ConnectionEstablished = value;
                NotifyChangedOnUI(nameof(ConnectionEstablished));
                NotifyChangedOnUI(nameof(ShowProgressCircle));
            }
        }

        public bool _Capturing;
        public bool Capturing
        {
            get { return _Capturing; }
            set
            {
                _Capturing = value;
                NotifyChangedOnUI(nameof(Capturing));
                NotifyChangedOnUI(nameof(ShowProgressCircle));
            }
        }

        public bool ShowProgressCircle { get { return !ConnectionEstablished || Capturing; } }

        private bool _FramingGridDisplayed = false;
        public bool FramingGridDisplayed
        {
            get
            {
                if (IsAudioMode) { return false; }
                return _FramingGridDisplayed;
            }
            set
            {
                if (_FramingGridDisplayed != value)
                {
                    _FramingGridDisplayed = value;
                    NotifyChangedOnUI(nameof(FramingGridDisplayed));
                }
            }
        }
    }
}
