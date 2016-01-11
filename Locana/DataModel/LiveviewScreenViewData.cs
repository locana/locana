
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
        readonly TargetDevice Device;

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
                NotifyChangedOnUI(nameof(Processing));
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
                NotifyChangedOnUI(nameof(IsShootingParamDisplayAvailable));
                NotifyChangedOnUI(nameof(ShootModeChangingAvailable));
            };

            ApplicationSettings.GetInstance().PropertyChanged += (sender, args) =>
            {
                NotifyChangedOnUI(nameof(ShutterButtonImage));
            };
        }

        private static readonly BitmapImage StillImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/Camera.png", UriKind.Absolute));
        private static readonly BitmapImage CamImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/Camcorder.png", UriKind.Absolute));
        private static readonly BitmapImage AudioImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/Music.png", UriKind.Absolute));
        private static readonly BitmapImage StopImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/Stop.png", UriKind.Absolute));
        private static readonly BitmapImage IntervalStillImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/IntervalStillRecButton.png", UriKind.Absolute));
        private static readonly BitmapImage ContShootingImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/ContShootingButton.png", UriKind.Absolute));
        private static readonly BitmapImage LoopRecImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/LoopRecButton.png", UriKind.Absolute));

        private static readonly BitmapImage StillModeImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/mode_photo.png", UriKind.Absolute));
        private static readonly BitmapImage MovieModeImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/mode_movie.png", UriKind.Absolute));
        private static readonly BitmapImage IntervalModeImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/mode_interval.png", UriKind.Absolute));
        private static readonly BitmapImage AudioModeImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/mode_audio.png", UriKind.Absolute));
        private static readonly BitmapImage LoopModeImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/mode_loop.png", UriKind.Absolute));

        private static readonly DataTemplate StillIconTemplate = (DataTemplate)Application.Current.Resources["StillIcon"];
        private static readonly DataTemplate MovieIconTemplate = (DataTemplate)Application.Current.Resources["MovieIcon"];
        private static readonly DataTemplate AudioIconTemplate = (DataTemplate)Application.Current.Resources["AudioIcon"];
        private static readonly DataTemplate IntervalStillIconTemplate = (DataTemplate)Application.Current.Resources["IntervalStillIcon"];
        private static readonly DataTemplate ContinuousStillIconTemplate = (DataTemplate)Application.Current.Resources["ContShootingIcon"];
        private static readonly DataTemplate LoopIconTemplate = (DataTemplate)Application.Current.Resources["LoopIcon"];

        private static readonly DataTemplate StopIconTemplate = (DataTemplate)Application.Current.Resources["StopIcon"];

        private static readonly BitmapImage AModeImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/ExposureMode_A.png", UriKind.Absolute));
        private static readonly BitmapImage IAModeImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/ExposureMode_iA.png", UriKind.Absolute));
        private static readonly BitmapImage IAPlusModeImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/ExposureMode_iAPlus.png", UriKind.Absolute));
        private static readonly BitmapImage MModeImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/ExposureMode_M.png", UriKind.Absolute));
        private static readonly BitmapImage SModeImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/ExposureMode_S.png", UriKind.Absolute));
        private static readonly BitmapImage PModeImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/ExposureMode_P.png", UriKind.Absolute));
        private static readonly BitmapImage PShiftModeImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/ExposureMode_P_shift.png", UriKind.Absolute));

        private static readonly BitmapImage AvailableMediaImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/memory_card.png", UriKind.Absolute));
        private static readonly BitmapImage NoMediaImage = new BitmapImage(new Uri("ms-appx:///Assets/LiveviewScreen/no_memory_card.png", UriKind.Absolute));

        private static readonly DataTemplate MediaIconTemplate = (DataTemplate)Application.Current.Resources["MemoryCardIcon"];
        private static readonly DataTemplate NoMediaIconTemplate = (DataTemplate)Application.Current.Resources["NoMemoryCardIcon"];

        public static DataTemplate GetShootModeIcon(string mode)
        {
            switch (mode)
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
                    return StillIconTemplate;
            }
        }

        public DataTemplate ShutterButtonImage
        {
            get
            {
                if (Device.Status.ShootMode == null)
                {
                    return StillIconTemplate;
                }
                return CurrentModeImage<DataTemplate>();
            }
        }

        private T CurrentModeImage<T>()
        {
            var current = Device.Status.Status;

            if (current == EventParam.MvRecording ||
                current == EventParam.AuRecording ||
                current == EventParam.LoopRecording ||
                current == EventParam.ItvRecording)
            {
                if (typeof(T) == typeof(BitmapImage))
                {
                    return (T)(object)StopImage;
                }
                else if (typeof(T) == typeof(DataTemplate))
                {
                    return (T)(object)StopIconTemplate;
                }
            }

            switch (Device.Status.ShootMode.Current)
            {
                case ShootModeParam.Still:
                    if (Device.Status.ContShootingMode != null &&
                        (Device.Status.ContShootingMode.Current == ContinuousShootMode.Cont ||
                        Device.Status.ContShootingMode.Current == ContinuousShootMode.SpeedPriority ||
                        Device.Status.ContShootingMode.Current == ContinuousShootMode.Burst ||
                        Device.Status.ContShootingMode.Current == ContinuousShootMode.MotionShot))
                    {
                        if (typeof(T) == typeof(BitmapImage))
                        {
                            return (T)(object)ContShootingImage;
                        }
                        else if (typeof(T) == typeof(DataTemplate))
                        {
                            return (T)(object)ContinuousStillIconTemplate;
                        }
                    }
                    if (ApplicationSettings.GetInstance().IsIntervalShootingEnabled)
                    {
                        if (typeof(T) == typeof(BitmapImage))
                        {
                            return (T)(object)IntervalStillImage;
                        }
                        else if (typeof(T) == typeof(DataTemplate))
                        {
                            return (T)(object)IntervalStillIconTemplate;
                        }
                    }
                    if (typeof(T) == typeof(BitmapImage))
                    {
                        return (T)(object)StillImage;
                    }
                    else if (typeof(T) == typeof(DataTemplate))
                    {
                        return (T)(object)StillIconTemplate;
                    }
                    break;
                case ShootModeParam.Movie:
                    if (typeof(T) == typeof(BitmapImage))
                    {
                        return (T)(object)CamImage;
                    }
                    else if (typeof(T) == typeof(DataTemplate))
                    {
                        return (T)(object)MovieIconTemplate;
                    }
                    break;
                case ShootModeParam.Audio:
                    if (typeof(T) == typeof(BitmapImage))
                    {
                        return (T)(object)AudioImage;
                    }
                    else if (typeof(T) == typeof(DataTemplate))
                    {
                        return (T)(object)AudioIconTemplate;
                    }
                    break;
                case ShootModeParam.Interval:
                    if (typeof(T) == typeof(BitmapImage))
                    {
                        return (T)(object)IntervalStillImage;
                    }
                    else if (typeof(T) == typeof(DataTemplate))
                    {
                        return (T)(object)IntervalStillIconTemplate;
                    }
                    break;
                case ShootModeParam.Loop:
                    if (typeof(T) == typeof(BitmapImage))
                    {
                        return (T)(object)LoopRecImage;
                    }
                    else if (typeof(T) == typeof(DataTemplate))
                    {
                        return (T)(object)LoopIconTemplate;
                    }
                    break;
                default:
                    return default(T);
            }
            return default(T);
        }

        public DataTemplate ShootModeImage
        {
            get
            {
                if (Device.Status.ShootMode == null) { return null; }
                switch (Device.Status.ShootMode.Current)
                {
                    case ShootModeParam.Still:
                        return StillIconTemplate;
                    case ShootModeParam.Movie:
                        return MovieIconTemplate;
                    case ShootModeParam.Interval:
                        return IntervalStillIconTemplate;
                    case ShootModeParam.Audio:
                        return AudioIconTemplate;
                    case ShootModeParam.Loop:
                        return LoopIconTemplate;
                }
                return null;
            }
        }

        public BitmapImage ExposureModeImage
        {
            get
            {
                if (Device.Status.ExposureMode == null) { return null; }
                switch (Device.Status.ExposureMode.Current)
                {
                    case ExposureMode.Intelligent:
                        return IAModeImage;
                    case ExposureMode.Superior:
                        return IAPlusModeImage;
                    case ExposureMode.Program:
                        if (Device.Status.ProgramShiftActivated) { return PShiftModeImage; }
                        return PModeImage;
                    case ExposureMode.Aperture:
                        return AModeImage;
                    case ExposureMode.SS:
                        return SModeImage;
                    case ExposureMode.Manual:
                        return MModeImage;
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
                switch (Device.Status.ShootMode.Current)
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

        public bool ShootModeChangingAvailable
        {
            get
            {
                if (Device?.Api?.Capability == null) { return false; }
                return Device.Api.Capability.IsAvailable("setShootMode");
            }
        }

        public bool IsZoomAvailable { get { return Device.Api.Capability != null && Device.Api.Capability.IsAvailable("actZoom"); } }

        public bool IsRecording
        {
            get
            {
                if (Device.Status == null) { return false; }
                switch (Device.Status.Status)
                {
                    case EventParam.MvRecording:
                    case EventParam.AuRecording:
                    case EventParam.ItvRecording:
                    case EventParam.LoopRecording:
                        return true;
                }
                return false;
            }
        }

        public bool Processing
        {
            get
            {
                if (Device.Status == null) { return true; }
                switch (Device.Status.Status)
                {
                    case EventParam.Idle:
                    case EventParam.MvRecording:
                    case EventParam.AuRecording:
                    case EventParam.ItvRecording:
                    case EventParam.LoopRecording:
                        return false;
                }
                return true;
            }
        }

        public int ZoomPositionInCurrentBox
        {
            get
            {
                if (Device.Status.ZoomInfo == null) { return 0; }
                DebugUtil.Log("Zoom pos " + Device.Status.ZoomInfo.PositionInCurrentBox);
                return Device.Status.ZoomInfo.PositionInCurrentBox;
            }
        }

        public int ZoomBoxIndex
        {
            get
            {
                if (Device.Status.ZoomInfo == null) { return 0; }
                return Device.Status.ZoomInfo.CurrentBoxIndex;
            }
        }

        public int ZoomBoxNum
        {
            get
            {
                if (Device.Status.ZoomInfo == null) { return 0; }
                return Device.Status.ZoomInfo.NumberOfBoxes;
            }
        }

        public string RecordbaleAmount
        {
            get
            {
                if (Device.Status.Storages == null || Device.Status.ShootMode == null) { return ""; }
                foreach (StorageInfo storage in Device.Status.Storages)
                {
                    if (storage.RecordTarget)
                    {
                        switch (Device.Status.ShootMode.Current)
                        {
                            case ShootModeParam.Still:
                            case ShootModeParam.Interval:
                                if (storage.RecordableImages == -1) { return ""; }
                                return storage.RecordableImages.ToString();
                            case ShootModeParam.Movie:
                            case ShootModeParam.Audio:
                            case ShootModeParam.Loop:
                                if (storage.RecordableMovieLength == -1) { return ""; }
                                return storage.RecordableMovieLength.ToString() + " min.";
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
                if (Device.Status.ShootMode == null) { return ""; }
                switch (Device.Status.ShootMode.Current)
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
                        return Device.Status.NumberOfShots + " pics.";
                }
                return "";
            }
        }

        public bool IsRecordingCountAvailable
        {
            get
            {
                if (Device.Status.ShootMode == null) { return false; }
                switch (Device.Status.ShootMode.Current)
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

        public bool IsShootingParamDisplayAvailable { get { return IsAvailableGetEV || IsAvailableGetFNumber || IsAvailableGetIsoSpeedRate || IsAvailableGetShutterSpeed; } }
        public bool IsAvailableGetShutterSpeed { get { return Device.Status != null && Device.Status.ShutterSpeed != null && Device.Status.ShutterSpeed.Current != null && Device.Api.Capability.IsAvailable("getShutterSpeed"); } }
        public bool IsAvailableGetFNumber { get { return Device.Status != null && Device.Status.FNumber != null && Device.Status.FNumber.Current != null && Device.Api.Capability.IsAvailable("getFNumber"); } }
        public bool IsAvailableGetIsoSpeedRate { get { return Device.Status != null && Device.Status.ISOSpeedRate != null && Device.Status.ISOSpeedRate.Current != null && Device.Api.Capability.IsAvailable("getIsoSpeedRate"); } }
        public bool IsAvailableGetEV { get { return Device.Status != null && Device.Status.EvInfo != null && Device.Api.Capability.IsAvailable("getExposureCompensation"); } }

        public string ShutterSpeedDisplayValue
        {
            get
            {
                if (Device.Status == null || Device.Status.ShutterSpeed == null || Device.Status.ShutterSpeed.Current == null)
                {
                    return "--";
                }
                else
                {
                    return Device.Status.ShutterSpeed.Current;
                }
            }
        }

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
            if (Device.Api == null || !Device.Api.Capability.IsAvailable(api)) { return ResourceManager.ForegroundBrush; }
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
                DebugUtil.Log("Geoposition status: " + value);
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

        public bool IsAudioMode
        {
            get { return Device.Status.ShootMode.Current == ShootModeParam.Audio; }
        }

        public bool LiveviewImageDisplayed
        {
            get { return !IsAudioMode; }
        }

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

        private bool _IsWaitingConnection = true;
        public bool IsWaitingConnection
        {
            get { return _IsWaitingConnection; }
            set
            {
                if (value != _IsWaitingConnection)
                {
                    _IsWaitingConnection = value;
                    NotifyChangedOnUI(nameof(IsWaitingConnection));
                }
            }
        }

        public string FriendlyName
        {
            get { return "Connected: " + Device.FriendlyName; }
        }

        public void NotifyFriendlyNameUpdated()
        {
            NotifyChangedOnUI(nameof(FriendlyName));
        }
    }
}
