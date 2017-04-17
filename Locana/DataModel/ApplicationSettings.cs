﻿using Locana.Controls;
using Locana.Playback;
using Locana.Utility;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Locana.DataModel
{
    public class ApplicationSettings : ObservableBase
    {
        private static ApplicationSettings sSettings = new ApplicationSettings();

        private ApplicationSettings()
        {
            IsPostviewTransferEnabled = Preference.PostviewSyncEnabled;
            IntervalTime = Preference.IntervalTime;
            IsShootButtonDisplayed = Preference.ShootButtonVisible;
            IsHistogramDisplayed = Preference.HistogramVisible;
            GeotagEnabled = Preference.GeoTaggingEnabled;
            FramingGridEnabled = Preference.FramingGridEnabled;
            GridType = Preference.FramingGridType;
            GridColor = Preference.FramingGridColor;
            FibonacciLineOrigin = Preference.FibonacciOrigin;
            RequestFocusFrameInfo = Preference.FocusFrameEnabled;
            PrioritizeOriginalSizeContents = Preference.OriginalSizeContentsPrioritized;
            RemoteContentsSet = Preference.RemoteContentsSet;
            LiveviewRotationEnabled = Preference.LiveviewRotationEnabled;
            ForcePhoneView = Preference.ForcePhoneView;
            ShowKeyCheatSheet = Preference.ShowKeyCheatSheet;
            LocalDirectoryPath = Preference.LocalDirectoryPath;
        }

        public static ApplicationSettings GetInstance()
        {
            return sSettings;
        }

        private bool _IsPostviewTransferEnabled = true;

        public bool IsPostviewTransferEnabled
        {
            set
            {
                if (_IsPostviewTransferEnabled != value)
                {
                    Preference.PostviewSyncEnabled = value;
                    _IsPostviewTransferEnabled = value;
                    NotifyChangedOnUI(nameof(IsPostviewTransferEnabled));
                }
            }
            get
            {
                return _IsPostviewTransferEnabled;
            }
        }

        private bool _PrioritizeOriginalSizeContents = false;
        public bool PrioritizeOriginalSizeContents
        {
            set
            {
                if (_PrioritizeOriginalSizeContents != value)
                {
                    Preference.OriginalSizeContentsPrioritized = value;
                    _PrioritizeOriginalSizeContents = value;
                    NotifyChangedOnUI(nameof(PrioritizeOriginalSizeContents));
                }
            }
            get { return _PrioritizeOriginalSizeContents; }
        }

        private bool _IsIntervalShootingEnabled = false;
        public bool IsIntervalShootingEnabled
        {
            set
            {
                if (_IsIntervalShootingEnabled != value)
                {
                    _IsIntervalShootingEnabled = value;

                    NotifyChangedOnUI(nameof(IsIntervalShootingEnabled));
                    NotifyChangedOnUI(nameof(IntervalTimeDisplayString));
                }
            }
            get
            {
                return _IsIntervalShootingEnabled;
            }
        }

        private int _IntervalTime = 10;

        public int IntervalTime
        {
            set
            {
                if (_IntervalTime != value)
                {
                    Preference.IntervalTime = value;
                    _IntervalTime = value;
                    // DebugUtil.Log(() => "IntervalTime changed: " + value);
                    NotifyChangedOnUI(nameof(IntervalTime));
                    NotifyChangedOnUI(nameof(IntervalTimeDisplayString));
                }
            }
            get
            {
                return _IntervalTime;
            }
        }

        public string IntervalTimeDisplayString
        {
            get
            {
                if (IsIntervalShootingEnabled)
                {
                    return string.Format(SystemUtil.GetStringResource("Seconds"), _IntervalTime);
                }
                else
                {
                    return SystemUtil.GetStringResource("Disabled_word");
                }
            }
        }

        private bool _IsShootButtonDisplayed = true;
        public bool IsShootButtonDisplayed
        {
            set
            {
                if (_IsShootButtonDisplayed != value)
                {
                    Preference.ShootButtonVisible = value;
                    _IsShootButtonDisplayed = value;
                    DebugUtil.Log(() => "ShootbuttonVisibility updated: " + value.ToString());
                    NotifyChangedOnUI(nameof(IsShootButtonDisplayed));
                }
            }
            get
            {
                return _IsShootButtonDisplayed;
            }
        }

        private bool _IsHistogramDisplayed = true;
        public bool IsHistogramDisplayed
        {
            set
            {
                if (_IsHistogramDisplayed != value)
                {
                    Preference.HistogramVisible = value;
                    _IsHistogramDisplayed = value;
                    NotifyChangedOnUI(nameof(IsHistogramDisplayed));
                }
            }
            get { return _IsHistogramDisplayed; }
        }

        private bool _GeotagEnabled = false;
        public bool GeotagEnabled
        {
            set
            {
                if (_GeotagEnabled != value)
                {
                    Preference.GeoTaggingEnabled = value;
                    _GeotagEnabled = value;
                    NotifyChangedOnUI(nameof(GeotagEnabled));
                }
            }
            get { return _GeotagEnabled; }
        }

        private bool _RequestFocusFrameInfo = true;
        public bool RequestFocusFrameInfo
        {
            set
            {
                if (_RequestFocusFrameInfo != value)
                {
                    Preference.FocusFrameEnabled = value;
                    _RequestFocusFrameInfo = value;
                    NotifyChangedOnUI(nameof(RequestFocusFrameInfo));
                }
            }
            get
            {
                return _RequestFocusFrameInfo;
            }
        }

        private bool _FramingGridEnabled = false;
        public bool FramingGridEnabled
        {
            set
            {
                if (_FramingGridEnabled != value)
                {
                    Preference.FramingGridEnabled = value;
                    _FramingGridEnabled = value;
                    NotifyChangedOnUI(nameof(FramingGridEnabled));
                    NotifyChangedOnUI(nameof(IsFibonacciSpiralEnabled));
                }
            }
            get { return _FramingGridEnabled; }
        }

        private FramingGridTypes _GridType = FramingGridTypes.Off;
        public FramingGridTypes GridType
        {
            set
            {
                if (_GridType != value)
                {
                    DebugUtil.Log(() => "GridType updated: " + value);
                    Preference.FramingGridType = value;
                    _GridType = value;
                    NotifyChangedOnUI(nameof(GridType));
                    NotifyChangedOnUI(nameof(IsFibonacciSpiralEnabled));
                }
            }
            get { return _GridType; }
        }

        public bool IsFibonacciSpiralEnabled
        {
            get { return FramingGridEnabled && GridType == FramingGridTypes.Fibonacci; }
        }

        private FramingGridColors _GridColor = FramingGridColors.White;
        public FramingGridColors GridColor
        {
            set
            {
                if (_GridColor != value)
                {
                    Preference.FramingGridColor = value;
                    _GridColor = value;
                    NotifyChangedOnUI(nameof(GridColor));
                    NotifyChangedOnUI(nameof(GridColorBrush));
                }
            }
            get { return _GridColor; }
        }

        public SolidColorBrush GridColorBrush
        {
            get
            {
                Color color;
                switch (this.GridColor)
                {
                    case FramingGridColors.White:
                        color = Color.FromArgb(200, 200, 200, 200);
                        break;
                    case FramingGridColors.Black:
                        color = Color.FromArgb(200, 50, 50, 50);
                        break;
                    case FramingGridColors.Red:
                        color = Color.FromArgb(200, 250, 30, 30);
                        break;
                    case FramingGridColors.Green:
                        color = Color.FromArgb(200, 30, 250, 30);
                        break;
                    case FramingGridColors.Blue:
                        color = Color.FromArgb(200, 30, 30, 250);
                        break;
                    default:
                        color = Color.FromArgb(200, 200, 200, 200);
                        break;

                }
                return new SolidColorBrush() { Color = color };
            }
        }

        private FibonacciLineOrigins _FibonacciLineOrigin = FibonacciLineOrigins.UpperLeft;
        public FibonacciLineOrigins FibonacciLineOrigin
        {
            get { return _FibonacciLineOrigin; }
            set
            {
                if (value != _FibonacciLineOrigin)
                {
                    Preference.FibonacciOrigin = value;
                    this._FibonacciLineOrigin = value;
                    NotifyChangedOnUI(nameof(FibonacciLineOrigin));
                }
            }
        }

        private ContentsSet _RemoteContentsType = ContentsSet.ImagesAndMovies;
        public ContentsSet RemoteContentsSet
        {
            get { return _RemoteContentsType; }
            set
            {
                if (value != _RemoteContentsType)
                {
                    Preference.RemoteContentsSet = value;
                    _RemoteContentsType = value;
                    NotifyChangedOnUI(nameof(RemoteContentsSet));
                }
            }
        }

        private bool _LiveviewRotationEnabled = false;
        public bool LiveviewRotationEnabled
        {
            get { return _LiveviewRotationEnabled; }
            set
            {
                if (value != _LiveviewRotationEnabled)
                {
                    _LiveviewRotationEnabled = value;
                    Preference.LiveviewRotationEnabled = value;
                    NotifyChangedOnUI(nameof(LiveviewRotationEnabled));
                }
            }
        }

        private bool _EnableDebugLogging = false;
        public bool EnableDebugLogging
        {
            get { return _EnableDebugLogging; }
            set
            {
                _EnableDebugLogging = value;
                NotifyChangedOnUI(nameof(EnableDebugLogging));
            }
        }

        private bool _ForcePhoneView = false;
        public bool ForcePhoneView
        {
            get { return _ForcePhoneView; }
            set
            {
                _ForcePhoneView = value;
                Preference.ForcePhoneView = value;
                NotifyChangedOnUI(nameof(ForcePhoneView));
            }
        }

        private bool _ShowKeyCheatSheet = true;
        public bool ShowKeyCheatSheet
        {
            get { return _ShowKeyCheatSheet; }
            set
            {
                _ShowKeyCheatSheet = value;
                Preference.ShowKeyCheatSheet = value;
                NotifyChangedOnUI(nameof(ShowKeyCheatSheet));
            }
        }

        private string _LocalDirectoryPath;
        public string LocalDirectoryPath
        {
            get { return _LocalDirectoryPath; }
            set
            {
                _LocalDirectoryPath = value;
                Preference.LocalDirectoryPath = value;
                NotifyChangedOnUI(nameof(LocalDirectoryPath));
            }
        }
    }
}
