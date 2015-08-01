using Kazyx.RemoteApi;
using Kazyx.RemoteApi.Camera;
using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;

namespace Kazyx.Uwpmm.DataModel
{
    public class CameraStatus : ObservableBase
    {
        private Capability<string> _ExposureMode;
        public Capability<string> ExposureMode
        {
            set
            {
                _ExposureMode = value;
                NotifyChangedOnUI("ExposureMode");
            }
            get { return _ExposureMode; }
        }

        private Capability<string> _ShootMode;
        public Capability<string> ShootMode
        {
            set
            {
                _ShootMode = value;
                NotifyChangedOnUI("ShootMode");
            }
            get { return _ShootMode; }
        }

        private Capability<string> _PostviewSize;
        public Capability<string> PostviewSize
        {
            set
            {
                _PostviewSize = value;
                NotifyChangedOnUI("PostviewSize");
            }
            get { return _PostviewSize; }
        }

        private Capability<string> _BeepMode;
        public Capability<string> BeepMode
        {
            set
            {
                _BeepMode = value;
                NotifyChangedOnUI("BeepMode");
            }
            get { return _BeepMode; }
        }

        private Capability<int> _SelfTimer;
        public Capability<int> SelfTimer
        {
            set
            {
                _SelfTimer = value;
                NotifyChangedOnUI("SelfTimer");
            }
            get { return _SelfTimer; }
        }

        private Capability<StillImageSize> _StillImageSize;
        public Capability<StillImageSize> StillImageSize
        {
            set
            {
                _StillImageSize = value;
                NotifyChangedOnUI("StillImageSize");
            }
            get { return _StillImageSize; }
        }

        private Capability<string> _WhiteBalance;
        public Capability<string> WhiteBalance
        {
            set
            {
                _WhiteBalance = value;
                NotifyChangedOnUI("WhiteBalance");
                NotifyChangedOnUI("ColorTemperture");
            }
            get { return _WhiteBalance; }
        }

        private int _ColorTemperture = -1;
        public int ColorTemperture
        {
            set
            {
                _ColorTemperture = value;
                NotifyChangedOnUI("ColorTemperture");
            }
            get { return _ColorTemperture; }
        }

        private Dictionary<string, int[]> _ColorTempertureCandidates;
        public Dictionary<string, int[]> ColorTempertureCandidates
        {
            set
            {
                _ColorTempertureCandidates = value;
                NotifyChangedOnUI("ColorTemperture");
            }
            get { return _ColorTempertureCandidates; }
        }

        private Capability<string> _ShutterSpeed;
        public Capability<string> ShutterSpeed
        {
            set
            {
                _ShutterSpeed = value;
                NotifyChangedOnUI("ShutterSpeed");
            }
            get { return _ShutterSpeed; }
        }

        private Capability<string> _ISOSpeedRate;
        public Capability<string> ISOSpeedRate
        {
            set
            {
                _ISOSpeedRate = value;
                NotifyChangedOnUI("ISOSpeedRate");
            }
            get { return _ISOSpeedRate; }
        }

        private Capability<string> _FNumber;
        public Capability<string> FNumber
        {
            set
            {
                _FNumber = value;
                NotifyChangedOnUI("FNumber");
            }
            get { return _FNumber; }
        }

        private string _Status = EventParam.NotReady;
        public string Status
        {
            set
            {
                if (value != _Status)
                {
                    _Status = value;
                    NotifyChangedOnUI("Status");
                }
            }
            get { return _Status; }
        }

        private ZoomInfo _ZoomInfo = null;
        public ZoomInfo ZoomInfo
        {
            set
            {
                _ZoomInfo = value;
                NotifyChangedOnUI("ZoomInfo");
            }
            get { return _ZoomInfo; }
        }


        private Capability<string> _FocusMode;
        public Capability<string> FocusMode
        {
            set
            {
                _FocusMode = value;
                NotifyChangedOnUI("FocusMode");
            }
            get { return _FocusMode; }
        }

        private Capability<string> _MovieQuality;
        public Capability<string> MovieQuality
        {
            set
            {
                _MovieQuality = value;
                NotifyChangedOnUI("MovieQuality");
            }
            get { return _MovieQuality; }
        }

        private Capability<string> _SteadyMode;
        public Capability<string> SteadyMode
        {
            set
            {
                _SteadyMode = value;
                NotifyChangedOnUI("SteadyMode");
            }
            get { return _SteadyMode; }
        }

        private Capability<int> _ViewAngle;
        public Capability<int> ViewAngle
        {
            set
            {
                _ViewAngle = value;
                NotifyChangedOnUI("ViewAngle");
            }
            get { return _ViewAngle; }
        }

        private Capability<string> _FlashMode;
        public Capability<string> FlashMode
        {
            set
            {
                _FlashMode = value;
                NotifyChangedOnUI("FlashMode");
            }
            get { return _FlashMode; }
        }

        private TouchFocusStatus _TouchFocusStatus;
        public TouchFocusStatus TouchFocusStatus
        {
            set
            {
                _TouchFocusStatus = value;
                NotifyChangedOnUI("TouchFocusStatus");
            }
            get
            {
                return _TouchFocusStatus;
            }
        }

        private EvCapability _EvInfo;
        public EvCapability EvInfo
        {
            set
            {
                _EvInfo = value;
                NotifyChangedOnUI("EvInfo");
            }
            get { return _EvInfo; }
        }

        private List<StorageInfo> _Storages;
        public List<StorageInfo> Storages
        {
            set
            {
                _Storages = value;
                NotifyChangedOnUI("Storages");
            }
            get { return _Storages; }
        }

#if DEBUG
        public void TestRotate()
        {
            switch (_LiveviewOrientation)
            {
                case RemoteApi.Camera.Orientation.Straight:
                    LiveviewOrientation = "90";
                    break;
                case RemoteApi.Camera.Orientation.Right:
                    LiveviewOrientation = "180";
                    break;
                case RemoteApi.Camera.Orientation.Left:
                    LiveviewOrientation = "0";
                    break;
                case RemoteApi.Camera.Orientation.Opposite:
                    LiveviewOrientation = "270";
                    break;
            }
        }
#endif

        private string _LiveviewOrientation = "";
        public string LiveviewOrientation
        {
            set
            {
                if (_LiveviewOrientation != value)
                {
                    _LiveviewOrientation = value;
                    NotifyChangedOnUI("LiveviewOrientation");
                }
            }
            get { return _LiveviewOrientation == null ? Orientation.Straight : _LiveviewOrientation; }
        }

        public double LiveviewOrientationAsDouble
        {
            get
            {
               switch (_LiveviewOrientation)
                {
                    case RemoteApi.Camera.Orientation.Straight:
                        return 0;
                    case RemoteApi.Camera.Orientation.Right:
                        return 90;
                    case RemoteApi.Camera.Orientation.Left:
                        return 270;
                    case RemoteApi.Camera.Orientation.Opposite:
                        return 180;
                }
                return 0;
            }
        }

        private List<string> _PictureUrls;
        public List<string> PictureUrls
        {
            set
            {
                _PictureUrls = value;
                // NotifyChangedOnUI("PictureUrls");
                // This logic should not be implemented here in DataModel... 
                if (value != null && ApplicationSettings.GetInstance().IsPostviewTransferEnabled)
                {
                    foreach (var url in value)
                    {
                        try
                        {
                            MediaDownloader.Instance.EnqueuePostViewImage(new Uri(url, UriKind.Absolute), GeopositionManager.INSTANCE.LatestPosition);
                        }
                        catch (Exception e)
                        {
                            DebugUtil.Log(e.StackTrace);
                        }
                    }
                }
            }
        }

        private bool _ProgramShiftActivated = false;
        public bool ProgramShiftActivated
        {
            set
            {
                if (_ProgramShiftActivated != value)
                {
                    _ProgramShiftActivated = value;
                    NotifyChangedOnUI("ProgramShiftActivated");
                }
            }
            get { return _ProgramShiftActivated; }
        }

        private ProgramShiftRange _ProgramShiftRange;
        public ProgramShiftRange ProgramShiftRange
        {
            set
            {
                _ProgramShiftRange = value;
                NotifyChangedOnUI("ProgramShiftRange");
            }
            get
            {
                return _ProgramShiftRange;
            }
        }

        private string _FocusStatus;
        public string FocusStatus
        {
            set
            {
                if (_FocusStatus != value)
                {
                    _FocusStatus = value;
                    NotifyChangedOnUI("FocusStatus");
                }
            }
            get { return _FocusStatus; }
        }

        private bool _IsLiveviewAvailable = false;
        public bool IsLiveviewAvailable
        {
            set
            {
                if (_IsLiveviewAvailable != value)
                {
                    _IsLiveviewAvailable = value;
                    NotifyChangedOnUI("IsLiveviewAvailable");
                }
            }
            get
            {
                return _IsLiveviewAvailable;
            }
        }

        private bool _IsFocusFrameInfoAvailable = false;
        public bool IsLiveviewFrameInfoAvailable
        {
            set
            {
                if (_IsFocusFrameInfoAvailable != value)
                {
                    _IsFocusFrameInfoAvailable = value;
                    NotifyChangedOnUI("IsFocusFrameInfoAvailable");
                }
            }
            get
            {
                return _IsFocusFrameInfoAvailable;
            }
        }

        public AutoFocusType AfType { get; set; }

        private Capability<string> _ZoomSetting;
        public Capability<string> ZoomSetting
        {
            set
            {
                _ZoomSetting = value;
                NotifyChangedOnUI("ZoomSetting");
            }
            get { return _ZoomSetting; }
        }

        private Capability<string> _StillQuality;
        public Capability<string> StillQuality
        {
            set
            {
                _StillQuality = value;
                NotifyChangedOnUI("StillQuality");
            }
            get { return _StillQuality; }
        }

        private Capability<string> _ContShootingMode;
        public Capability<string> ContShootingMode
        {
            set
            {
                _ContShootingMode = value;
                NotifyChangedOnUI("ContShootingMode");
            }
            get { return _ContShootingMode; }
        }

        private Capability<string> _ContShootingSpeed;
        public Capability<string> ContShootingSpeed
        {
            set
            {
                _ContShootingSpeed = value;
                NotifyChangedOnUI("ContShootingSpeed");
            }
            get { return _ContShootingSpeed; }
        }

        private List<ContShootingResult> _ContShootingResult;
        public List<ContShootingResult> ContShootingResult
        {
            set
            {
                _ContShootingResult = value;
                NotifyChangedOnUI("ContShootingResult");
            }
            get { return _ContShootingResult; }
        }

        private Capability<string> _FlipMode;
        public Capability<string> FlipMode
        {
            set
            {
                _FlipMode = value;
                NotifyChangedOnUI("FlipMode");
            }
            get { return _FlipMode; }
        }

        private Capability<string> _SceneSelection;
        public Capability<string> SceneSelection
        {
            set
            {
                _SceneSelection = value;
                NotifyChangedOnUI("SceneSelection");
            }
            get { return _SceneSelection; }
        }

        private Capability<string> _IntervalTime;
        public Capability<string> IntervalTime
        {
            set
            {
                _IntervalTime = value;
                NotifyChangedOnUI("IntervalTime");
            }
            get { return _IntervalTime; }
        }

        private Capability<string> _ColorSetting;
        public Capability<string> ColorSetting
        {
            set
            {
                _ColorSetting = value;
                NotifyChangedOnUI("ColorSetting");
            }
            get { return _ColorSetting; }
        }

        private Capability<string> _MovieFileFormat;
        public Capability<string> MovieFileFormat
        {
            set
            {
                _MovieFileFormat = value;
                NotifyChangedOnUI("MovieFileFormat");
            }
            get { return _MovieFileFormat; }
        }

        private Capability<string> _InfraredRemoteControl;
        public Capability<string> InfraredRemoteControl
        {
            set
            {
                _InfraredRemoteControl = value;
                NotifyChangedOnUI("InfraredRemoteControl");
            }
            get { return _InfraredRemoteControl; }
        }

        private Capability<string> _TvColorSystem;
        public Capability<string> TvColorSystem
        {
            set
            {
                _TvColorSystem = value;
                NotifyChangedOnUI("TvColorSystem");
            }
            get { return _TvColorSystem; }
        }

        private string _TrackingFocusStatus;
        public string TrackingFocusStatus
        {
            set
            {
                if (_TrackingFocusStatus != value)
                {
                    _TrackingFocusStatus = value;
                    NotifyChangedOnUI("TrackingFocusStatus");
                }
            }
            get { return _TrackingFocusStatus; }
        }

        private Capability<string> _TrackingFocus;
        public Capability<string> TrackingFocus
        {
            set
            {
                _TrackingFocus = value;
                NotifyChangedOnUI("TrackingFocus");
            }
            get { return _TrackingFocus; }
        }

        private List<BatteryInfo> _BatteryInfo;
        public List<BatteryInfo> BatteryInfo
        {
            set
            {
                _BatteryInfo = value;
                NotifyChangedOnUI("BatteryInfo");
            }
            get { return _BatteryInfo; }
        }

        private int _RecordingTimeSec = -1;
        public int RecordingTimeSec
        {
            set
            {
                if (_RecordingTimeSec != value)
                {
                    _RecordingTimeSec = value;
                    NotifyChangedOnUI("RecordingTimeSec");
                }
            }
            get { return _RecordingTimeSec; }
        }

        private int _NumberOfShots = -1;
        public int NumberOfShots
        {
            set
            {
                if (_NumberOfShots != value)
                {
                    _NumberOfShots = value;
                    NotifyChangedOnUI("NumberOfShots");
                }
            }
            get { return _NumberOfShots; }
        }

        private Capability<int> _AutoPowerOff;
        public Capability<int> AutoPowerOff
        {
            set
            {
                _AutoPowerOff = value;
                NotifyChangedOnUI("AutoPowerOff");
            }
            get { return _AutoPowerOff; }
        }

        private Capability<string> _LoopRecTime;
        public Capability<string> LoopRecTime
        {
            set
            {
                _LoopRecTime = value;
                NotifyChangedOnUI("LoopRecTime");
            }
            get { return _LoopRecTime; }
        }

        private Capability<string> _WindNoiseReduction;
        public Capability<string> WindNoiseReduction
        {
            set
            {
                _WindNoiseReduction = value;
                NotifyChangedOnUI("WindNoiseReduction");
            }
            get { return _WindNoiseReduction; }
        }

        private Capability<string> _AudioRecording;
        public Capability<string> AudioRecording
        {
            set
            {
                _AudioRecording = value;
                NotifyChangedOnUI("AudioRecording");
            }
            get { return _AudioRecording; }
        }
    }

    public enum AutoFocusType
    {
        None,
        HalfPress,
        Touch,
    }
}
