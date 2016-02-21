using Kazyx.ImageStream;
using Kazyx.RemoteApi;
using Kazyx.RemoteApi.Camera;
using Locana.CameraControl;
using Locana.Controls;
using Locana.DataModel;
using Locana.Settings;
using Locana.Utility;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Naotaco.Histogram.Win2d;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Locana.Pages
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            InitializeCommandBar();
            InitializeUI();
            InitializeTimer();
        }

        private void InitializeUI()
        {
            HistogramControl.Init(Histogram.ColorType.White, 800);

            HistogramCreator = null;
            HistogramCreator = new HistogramCreator(HistogramCreator.HistogramResolution.Resolution_128);
            HistogramCreator.PixelSkipRate = 10;
            HistogramCreator.OnHistogramCreated += async (r, g, b) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    HistogramControl.SetHistogramValue(r, g, b);
                });
            };

            _CommandBarManager.Clear()
                .DeviceDependent(AppBarItem.Zoom)
                .DeviceDependent(AppBarItem.FNumberSlider)
                .DeviceDependent(AppBarItem.ShutterSpeedSlider)
                .DeviceDependent(AppBarItem.IsoSlider)
                .DeviceDependent(AppBarItem.EvSlider)
                .DeviceDependent(AppBarItem.ProgramShiftSlider);

            LiveviewGrid.SizeChanged += LiveviewGrid_SizeChanged;
        }

        bool sizeChanged = false;

        private void LiveviewGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            sizeChanged = true;
            DisposeLiveviewImageBitmap();
        }

        DisplayRequest displayRequest = new DisplayRequest();
        DispatcherTimer LiveviewFpsTimer = new DispatcherTimer();
        const int FPS_INTERVAL = 5000;
        int LiveviewFrameCount = 0;

        void InitializeTimer()
        {
            LiveviewFpsTimer.Interval = TimeSpan.FromMilliseconds(FPS_INTERVAL);
            LiveviewFpsTimer.Tick += (sender, arg) =>
            {
                var fps = (double)LiveviewFrameCount * 1000 / (double)FPS_INTERVAL;
                DebugUtil.Log(string.Format("[LV CanvasBitmap] {0} fps", fps));
                LiveviewFrameCount = 0;
            };
        }

        CommandBarManager _CommandBarManager = new CommandBarManager();

        void InitializeCommandBar()
        {
            _CommandBarManager.SetEvent(AppBarItem.AppSetting, (s, args) =>
            {
                this.Frame.Navigate(typeof(AppSettingPage));
            });
            _CommandBarManager.SetEvent(AppBarItem.FNumberSlider, (s, args) =>
            {
                ToggleVisibility(FnumberSlider);
            });
            _CommandBarManager.SetEvent(AppBarItem.ShutterSpeedSlider, (s, args) =>
            {
                ToggleVisibility(SSSlider);
            });
            _CommandBarManager.SetEvent(AppBarItem.IsoSlider, (s, args) =>
            {
                ToggleVisibility(ISOSlider);
            });
            _CommandBarManager.SetEvent(AppBarItem.EvSlider, (s, args) =>
            {
                ToggleVisibility(EvSlider);
            });
            _CommandBarManager.SetEvent(AppBarItem.ProgramShiftSlider, (s, args) =>
            {
                ToggleVisibility(ProgramShiftSlider);
            });
            _CommandBarManager.SetEvent(AppBarItem.Zoom, (s, args) =>
            {
                ToggleVisibility(ZoomElements);
            });
            _CommandBarManager.SetEvent(AppBarItem.CancelTouchAF, async (s, args) =>
            {
                if (target == null || target.Api == null) { return; }
                await target.Api.Camera.CancelTouchAFAsync();
            });

            _FocusFrameSurface.OnTouchFocusOperated += async (obj, args) =>
            {
                DebugUtil.Log("Touch AF operated: " + args.X + " " + args.Y);
                if (target == null || target.Api == null || !target.Api.Capability.IsAvailable("setTouchAFPosition")) { return; }
                try
                {
                    await target.Api.Camera.SetAFPositionAsync(args.X, args.Y);
                }
                catch (RemoteApiException ex)
                {
                    DebugUtil.Log(ex.StackTrace);
                }
            };
        }

        void ToggleVisibility(FrameworkElement element)
        {
            if (element.Visibility == Visibility.Visible)
            {
                AnimationHelper.CreateFadeAnimation(new FadeAnimationRequest()
                {
                    RequestFadeType = FadeType.FadeOut,
                    Target = element,
                    Duration = TimeSpan.FromMilliseconds(150),
                    Completed = (sender, arg) =>
                    {
                        element.Visibility = Visibility.Collapsed;
                    }
                }).Begin();
            }
            else
            {
                HideAllSliders();
                AnimationHelper.CreateFadeAnimation(new FadeAnimationRequest()
                {
                    RequestFadeType = FadeType.FadeIn,
                    Target = element,
                    Duration = TimeSpan.FromMilliseconds(100),
                    Completed = (sender, arg) =>
                    {
                        element.Visibility = Visibility.Visible;
                    }
                }).Begin();
            }
        }

        void HideAllSliders()
        {
            foreach (var s in Sliders.Children)
            {
                if (s.Visibility == Visibility.Visible)
                {
                    AnimationHelper.CreateFadeAnimation(new FadeAnimationRequest()
                    {
                        RequestFadeType = FadeType.FadeOut,
                        Target = s as FrameworkElement,
                        Duration = TimeSpan.FromMilliseconds(150),
                        Completed = (sender, arg) =>
                        {
                            s.Visibility = Visibility.Collapsed;
                        }
                    }).Begin();
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            displayRequest.RequestActive();

            InitializeVisualStates();
            DisplayInformation.GetForCurrentView().OrientationChanged += MainPage_OrientationChanged;

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.CameraHalfPressed += HardwareButtons_CameraHalfPressed;
                HardwareButtons.CameraReleased += HardwareButtons_CameraReleased;
                HardwareButtons.CameraPressed += HardwareButtons_CameraPressed;
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.CameraHalfPressed -= HardwareButtons_CameraHalfPressed;
                HardwareButtons.CameraReleased -= HardwareButtons_CameraReleased;
                HardwareButtons.CameraPressed -= HardwareButtons_CameraPressed;
            }

            displayRequest.RequestRelease();
        }

        private void MainPage_OrientationChanged(DisplayInformation info, object args)
        {
            Debug.WriteLine("orientation: " + info.CurrentOrientation);
            Debug.WriteLine(LayoutRoot.ActualWidth + " x " + LayoutRoot.ActualHeight);
        }

        async void HardwareButtons_CameraPressed(object sender, CameraEventArgs e)
        {
            if (CameraStatusUtility.IsContinuousShootingMode(target)) { await StartContShooting(); }
            else { ShutterButtonPressed(); }
        }

        async void HardwareButtons_CameraReleased(object sender, CameraEventArgs e)
        {
            if (target == null || target.Api == null) { return; }
            if (target.Api.Capability.IsAvailable("cancelHalfPressShutter"))
            {
                try
                {
                    await target.Api.Camera.CancelHalfPressShutterAsync();
                }
                catch (RemoteApiException) { }
            }
            await StopContShooting();
        }

        async void HardwareButtons_CameraHalfPressed(object sender, CameraEventArgs e)
        {
            if (target == null || target.Api == null || !target.Api.Capability.IsAvailable("actHalfPressShutter")) { return; }
            try
            {
                await target.Api.Camera.ActHalfPressShutterAsync();
            }
            catch (RemoteApiException) { }
        }

        const string WIDE_STATE = "WideState";
        const string NARROW_STATE = "NarrowState";
        const string TALL_STATE = "TallState";
        const string SHORT_STATE = "ShortState";

        private void InitializeVisualStates()
        {
            var groups = VisualStateManager.GetVisualStateGroups(LayoutRoot);

            Debug.WriteLine("CurrentState: " + groups[0].CurrentState.Name);
            groups[0].CurrentStateChanged += (sender, e) =>
            {
                Debug.WriteLine("Width state changed: " + e.OldState.Name + " -> " + e.NewState.Name);
                switch (e.NewState.Name)
                {
                    case WIDE_STATE:
                        ControlPanelState = DisplayState.AlwaysVisible;
                        // if it's already closed, open immediately
                        StartToShowControlPanel();
                        break;
                    case NARROW_STATE:
                        ControlPanelState = DisplayState.Collapsible;
                        // set to original angle forcibly
                        AnimationHelper.CreateRotateAnimation(new AnimationRequest() { Target = OpenControlPanelImage, Duration = TimeSpan.FromMilliseconds(10) }, 180, 0).Begin();
                        StartToHideControlPanel();
                        break;
                }
            };

            // initialize UI according to current state
            switch (groups[0].CurrentState.Name)
            {
                case WIDE_STATE:
                    ControlPanelState = DisplayState.AlwaysVisible;
                    StartToShowControlPanel(0);
                    break;
                case NARROW_STATE:
                    ControlPanelState = DisplayState.Collapsible;
                    StartToHideControlPanel(0);
                    break;
            }
        }

        private HistogramCreator HistogramCreator;

        private void OnFetchdImage(StorageFolder folder, StorageFile file, GeotaggingResult.Result result)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var bmp = new BitmapImage();
                using (var stream = await file.GetThumbnailAsync(ThumbnailMode.SingleItem))
                {
                    bmp.CreateOptions = BitmapCreateOptions.None;
                    await bmp.SetSourceAsync(stream);
                }

                var text = "";
                switch (result)
                {
                    case GeotaggingResult.Result.OK:
                        text = SystemUtil.GetStringResource("Message_ImageDL_Succeed_withGeotag");
                        break;
                    case GeotaggingResult.Result.GeotagAlreadyExists:
                        text = SystemUtil.GetStringResource("ErrorMessage_ImageDL_DuplicatedGeotag");
                        break;
                    case GeotaggingResult.Result.NotRequested:
                        text = SystemUtil.GetStringResource("Message_ImageDL_Succeed");
                        break;
                    case GeotaggingResult.Result.UnExpectedError:
                        text = SystemUtil.GetStringResource("ErrorMessage_ImageDL_Geotagging");
                        break;
                    case GeotaggingResult.Result.FailedToAcquireLocation:
                        text = SystemUtil.GetStringResource("ErrorMessage_FailedToGetGeoposition");
                        break;
                }

                AppShell.Current.Toast.PushToast(new ToastContent
                {
                    Text = text,
                    Icon = bmp,
                    MaxIconHeight = 64,
                });
            });
        }

        private TargetDevice target;
        private StreamProcessor liveview = new StreamProcessor();
        private ImageDataSource liveview_data = new ImageDataSource();
        private ImageDataSource postview_data = new ImageDataSource();

        LiveviewScreenViewData ScreenViewData;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var target = e.Parameter as TargetDevice;
            SetupScreen(target);

            MediaDownloader.Instance.Fetched += OnFetchdImage;

            await SetupGeolocatorManager();
        }

        private async Task SetupGeolocatorManager()
        {
            if (!ApplicationSettings.GetInstance().GeotagEnabled) { return; }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (!GeolocatorManager.INSTANCE.IsRunning)
                {
                    var status = await GeolocatorManager.INSTANCE.Start();
                    switch (status)
                    {
                        case GeolocationAccessStatus.Allowed:
                            break;
                        case GeolocationAccessStatus.Denied:
                            AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("UsingLocationDeclined") });
                            break;
                        case GeolocationAccessStatus.Unspecified:
                            AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("UsingLocationUnspecified") });
                            break;
                    }
                }
            });
        }

        private static void TearDownGeolocatorManager()
        {
            GeolocatorManager.INSTANCE.Stop();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (target != null)
            {
                target.Status.PropertyChanged += Status_PropertyChanged;
                target.Api.AvailiableApisUpdated += Api_AvailiableApisUpdated;
            }

            liveview.JpegRetrieved -= liveview_JpegRetrieved;
            liveview.FocusFrameRetrieved -= Liveview_FocusFrameRetrieved;
            liveview.Closed -= liveview_Closed;
            HistogramCreator.Stop();
            LiveviewFpsTimer.Stop();

            MediaDownloader.Instance.Fetched -= OnFetchdImage;

            var task = SequentialOperation.TearDown(target, liveview);

            TearDownCurrentTarget();
            TearDownGeolocatorManager();

            base.OnNavigatingFrom(e);

        }

        async void SetupScreen(TargetDevice target)
        {
            try
            {
                await SequentialOperation.SetUp(target, liveview);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed setup: " + ex.Message);
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("ErrorMessage_CameraSetupFailure") });
                    AppShell.Current.AppFrame.GoBack();
                });
                return;
            }

            this.target = target;
            ScreenViewData = new LiveviewScreenViewData(target);
            LayoutRoot.DataContext = ScreenViewData;

            target.Status.PropertyChanged += Status_PropertyChanged;
            target.Api.AvailiableApisUpdated += Api_AvailiableApisUpdated;

            liveview.JpegRetrieved += liveview_JpegRetrieved;
            liveview.FocusFrameRetrieved += Liveview_FocusFrameRetrieved;
            liveview.Closed += liveview_Closed;
            LiveviewFpsTimer.Start();

            BatteryStatusDisplay.BatteryInfo = target.Status.BatteryInfo;
            var panels = SettingPanelBuilder.CreateNew(target);
            var pn = panels.GetPanelsToShow();
            foreach (var panel in pn)
            {
                ControlPanel.Children.Add(panel);
            }
            ControlPanel.SetChildrenControlHitTest(!target.Status.IsRecording());
            ControlPanel.SetChildrenControlTabStop(!target.Status.IsRecording());

            Sliders.DataContext = new ShootingParamViewData() { Status = target.Status, Liveview = ScreenViewData };
            ShootingParams.DataContext = ScreenViewData;
            _CommandBarManager.ShootingScreenBarData = ScreenViewData;
            _CommandBarManager.ApplyShootingScreenCommands(AppBarUnit);

            ZoomElements.DataContext = ScreenViewData;

            FramingGuideSurface.DataContext = new OptionalElementsViewData() { AppSetting = ApplicationSettings.GetInstance() };
            UpdateShutterButton(target.Status);

            await SetupFocusFrame(ApplicationSettings.GetInstance().RequestFocusFrameInfo);
            _FocusFrameSurface.ClearFrames();

            HistogramControl.Visibility = ApplicationSettings.GetInstance().IsHistogramDisplayed.AsVisibility();

            SetUIHandlers();
        }

        private async Task<bool> SetupFocusFrame(bool RequestFocusFrameEnabled)
        {
            if (target == null)
            {
                DebugUtil.Log("No target to set up focus frame is available.");
                return false;
            }
            if (target.Api.Capability.IsAvailable("setLiveviewFrameInfo"))
            {
                await target.Api.Camera.SetLiveviewFrameInfoAsync(new FrameInfoSetting() { TransferFrameInfo = RequestFocusFrameEnabled });
            }

            if (RequestFocusFrameEnabled && !target.Api.Capability.IsSupported("setLiveviewFrameInfo") && target.Api.Capability.IsAvailable("setTouchAFPosition"))
            {
                // For devices which does not support to transfer focus frame info, draw focus frame itself.
                _FocusFrameSurface.SelfDrawTouchAFFrame = true;
            }
            else { _FocusFrameSurface.SelfDrawTouchAFFrame = false; }
            return true;
        }

        private void SetUIHandlers()
        {
            FnumberSlider.SliderOperated += async (s, arg) =>
            {
                DebugUtil.Log("Fnumber operated: " + arg.Selected);
                try { await target.Api.Camera.SetFNumberAsync(arg.Selected); }
                catch (RemoteApiException) { }
            };
            SSSlider.SliderOperated += async (s, arg) =>
            {
                DebugUtil.Log("SS operated: " + arg.Selected);
                try { await target.Api.Camera.SetShutterSpeedAsync(arg.Selected); }
                catch (RemoteApiException) { }
            };
            ISOSlider.SliderOperated += async (s, arg) =>
            {
                DebugUtil.Log("ISO operated: " + arg.Selected);
                try { await target.Api.Camera.SetISOSpeedAsync(arg.Selected); }
                catch (RemoteApiException) { }
            };
            EvSlider.SliderOperated += async (s, arg) =>
            {
                DebugUtil.Log("Ev operated: " + arg.Selected);
                try { await target.Api.Camera.SetEvIndexAsync(arg.Selected); }
                catch (RemoteApiException) { }
            };
            ProgramShiftSlider.SliderOperated += async (s, arg) =>
            {
                DebugUtil.Log("Program shift operated: " + arg.OperatedStep);
                try { await target.Api.Camera.SetProgramShiftAsync(arg.OperatedStep); }
                catch (RemoteApiException) { }
            };
        }

        void Status_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var status = sender as CameraStatus;
            switch (e.PropertyName)
            {
                case nameof(CameraStatus.BatteryInfo):
                    BatteryStatusDisplay.BatteryInfo = status.BatteryInfo;
                    break;
                case nameof(CameraStatus.ContShootingResult):
                    EnqueueContshootingResult(status.ContShootingResult);
                    break;
                case nameof(CameraStatus.Status):
                    if (status.Status == EventParam.Idle)
                    {
                        // When recording is stopped, clear recording time.
                        status.RecordingTimeSec = 0;
                    }
                    UpdateShutterButton(status);
                    ControlPanel.SetChildrenControlHitTest(!status.IsRecording());
                    ControlPanel.SetChildrenControlTabStop(!status.IsRecording());
                    break;
                case nameof(CameraStatus.ShootMode):
                    UpdateShutterButton(status);
                    break;
                case nameof(CameraStatus.FocusStatus):
                    UpdateFocusStatus(status.FocusStatus);
                    break;
                case nameof(CameraStatus.TouchFocusStatus):
                    UpdateTouchFocus(status.TouchFocusStatus);
                    break;
                default:
                    break;
            }
        }

        private bool showShootModeSelector;

        private void Api_AvailiableApisUpdated(object sender, AvailableApiEventArgs e)
        {
            if (e.AvailableApis.Contains("setShootMode") ^ showShootModeSelector)
            {
                showShootModeSelector = !showShootModeSelector;
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    UpdateShutterButton(target.Status);
                });
            }
        }

        private void UpdateFocusStatus(string FocusStatus)
        {
            DebugUtil.Log("Focus status changed: " + FocusStatus);
            if (FocusStatus == Kazyx.RemoteApi.Camera.FocusState.Focused)
            {
                ShowCancelTouchAFButton();
                _FocusFrameSurface.Focused = true;
            }
            else
            {
                HideCancelTouchAFButton();
                _FocusFrameSurface.Focused = false;
            }
        }

        private void HideCancelTouchAFButton()
        {
            _CommandBarManager.Disable(AppBarItemType.Command, AppBarItem.CancelTouchAF)
                .ApplyShootingScreenCommands(AppBarUnit);
        }

        void ShowCancelTouchAFButton()
        {
            _CommandBarManager.Command(AppBarItem.CancelTouchAF)
                .ApplyShootingScreenCommands(AppBarUnit);
        }

        private void UpdateTouchFocus(TouchFocusStatus status)
        {
            if (status == null) { return; }
            DebugUtil.Log("TouchFocusStatus changed: " + status.Focused);
            if (status.Focused)
            {
                ShowCancelTouchAFButton();
                _FocusFrameSurface.Focused = true;
            }
            else
            {
                HideCancelTouchAFButton();
                _FocusFrameSurface.Focused = false;
            }
        }

        private void UpdateShutterButton(CameraStatus status)
        {
            if (status == null || status.ShootMode == null || status.ShootMode.Candidates.Count == 0) { return; }

            var icons = new Dictionary<string, DataTemplate>();
            Capability<string> capa;
            if (target.Api.Capability.IsAvailable("setShootMode") && !status.IsRecording())
            {
                foreach (var m in status.ShootMode.Candidates)
                {
                    icons.Add(m, LiveviewScreenViewData.GetShootModeIcon(m));
                }
                capa = status.ShootMode;
            }
            else
            {
                var m = status.ShootMode.Current;
                DebugUtil.Log("Single shoot mode selector: " + m);
                icons.Add(m, LiveviewScreenViewData.GetShootModeIcon(m));
                var list = new List<string>();
                list.Add(m);
                capa = new Capability<string> { Current = m, Candidates = list };
            }

            MultiShutterButton.ModeInfo = new ShootModeInfo()
            {
                ShootModeCapability = capa,
                ModeSelected = async (mode) =>
                {
                    if (target != null)
                    {
                        try
                        {
                            await target.Api.Camera.SetShootModeAsync(mode);
                        }
                        catch (RemoteApiException) { }
                    }
                },
                ButtonPressed = () =>
                {
                    ShutterButtonPressed();
                },
                IconTemplates = icons,
            };
        }

        private void EnqueueContshootingResult(List<ContShootingResult> ContShootingResult)
        {
            if (ApplicationSettings.GetInstance().IsPostviewTransferEnabled)
            {
                foreach (var result in ContShootingResult)
                {
                    MediaDownloader.Instance.EnqueuePostViewImage(new Uri(result.PostviewUrl, UriKind.Absolute));
                }
            }
        }

        private bool IsDecoding = false;

        private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        CanvasBitmap LiveviewImageBitmap;

        private JpegPacket PendingPakcet;

        private BitmapSize OriginalLvSize;
        private double LvOffsetV, LvOffsetH;

        private const double DEFAULT_DPI = 96.0;

        private async void liveview_JpegRetrieved(object sender, JpegEventArgs e)
        {
            if (IsDecoding)
            {
                PendingPakcet = e.Packet;
                return;
            }

            IsDecoding = true;
            await DecodeLiveviewFrame(e.Packet);
            IsDecoding = false;

            if (HistogramCreator != null && ApplicationSettings.GetInstance().IsHistogramDisplayed && !HistogramCreator.IsRunning)
            {
                rwLock.EnterReadLock();
                try
                {
                    HistogramCreator.CreateHistogram(LiveviewImageBitmap);
                }
                finally
                {
                    rwLock.ExitReadLock();
                }
            }

        }

        private double dpi;

        private async Task DecodeLiveviewFrame(JpegPacket packet, bool retry = false)
        {
            Action trailingTask = null;

            if (LiveviewImageBitmap == null || sizeChanged)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var writeable = await LiveviewUtil.AsWriteableBitmap(packet.ImageData, Dispatcher);
                    OriginalLvSize = new BitmapSize { Width = (uint)writeable.PixelWidth, Height = (uint)writeable.PixelHeight };

                    var magnification = CalcLiveviewMagnification();
                    DebugUtil.Log(() => { return "Decode: mag: " + magnification; });
                    dpi = DEFAULT_DPI / magnification;

                    trailingTask = () =>
                    {
                        RefreshOverlayControlParams(magnification);
                        sizeChanged = false;
                    };
                });
            }
            else
            {
                rwLock.EnterWriteLock();
                try
                {
                    var toDelete = LiveviewImageBitmap;
                    trailingTask = () =>
                    {
                        // Dispose after it is drawn
                        toDelete?.Dispose();
                    };
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }

            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(packet.ImageData.AsBuffer());
                stream.Seek(0);

                var bmp = await CanvasBitmap.LoadAsync(LiveviewImageCanvas, stream, (float)dpi);
                var size = bmp.SizeInPixels;

                rwLock.EnterWriteLock();
                try
                {
                    LiveviewImageBitmap = bmp;
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }

                if (!OriginalLvSize.Equals(size))
                {
                    DisposeLiveviewImageBitmap();
                    if (!retry)
                    {
                        await DecodeLiveviewFrame(packet, true);
                    }
                    return;
                }
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LiveviewImageCanvas.Invalidate();
                trailingTask?.Invoke();
            });

            if (PendingPakcet != null)
            {
                var task = DecodeLiveviewFrame(PendingPakcet);
                PendingPakcet = null;
            }
        }

        double CalcLiveviewMagnification()
        {
            var mag_h = LiveviewImageCanvas.ActualWidth / OriginalLvSize.Width;
            var mag_v = LiveviewImageCanvas.ActualHeight / OriginalLvSize.Height;
            return Math.Min(mag_h, mag_v);
        }

        void DisposeLiveviewImageBitmap()
        {
            rwLock.EnterWriteLock();
            try
            {
                LiveviewImageBitmap?.Dispose();
                LiveviewImageBitmap = null;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        void liveview_Closed(object sender, EventArgs e)
        {
            Debug.WriteLine("Liveview connection closed");
        }

        private async void Liveview_FocusFrameRetrieved(object sender, FocusFrameEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _FocusFrameSurface.SetFocusFrames(e.Packet.FocusFrames);
            });
        }

        private void TearDownCurrentTarget()
        {
            LayoutRoot.DataContext = null;
        }

        private async void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            try { await target.Api.Camera.ActZoomAsync(ZoomParam.DirectionOut, ZoomParam.ActionStop); }
            catch (RemoteApiException ex) { DebugUtil.Log(ex.StackTrace); }
        }

        private async void ZoomOutButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try { await target.Api.Camera.ActZoomAsync(ZoomParam.DirectionOut, ZoomParam.Action1Shot); }
            catch (RemoteApiException ex) { DebugUtil.Log(ex.StackTrace); }
        }

        private async void ZoomOutButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try { await target.Api.Camera.ActZoomAsync(ZoomParam.DirectionOut, ZoomParam.ActionStart); }
            catch (RemoteApiException ex) { DebugUtil.Log(ex.StackTrace); }
        }

        private async void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            try { await target.Api.Camera.ActZoomAsync(ZoomParam.DirectionIn, ZoomParam.ActionStop); }
            catch (RemoteApiException ex) { DebugUtil.Log(ex.StackTrace); }
        }

        private async void ZoomInButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try { await target.Api.Camera.ActZoomAsync(ZoomParam.DirectionIn, ZoomParam.Action1Shot); }
            catch (RemoteApiException ex) { DebugUtil.Log(ex.StackTrace); }
        }

        private async void ZoomInButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try { await target.Api.Camera.ActZoomAsync(ZoomParam.DirectionIn, ZoomParam.ActionStart); }
            catch (RemoteApiException ex) { DebugUtil.Log(ex.StackTrace); }
        }

        private void ShutterButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CameraStatusUtility.IsContinuousShootingMode(target))
            {
                AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("Message_ContinuousShootingGuide") });
            }
            else { ShutterButtonPressed(); }
        }

        private async void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            if (CameraStatusUtility.IsContinuousShootingMode(target))
            {
                await StopContShooting();
            }
        }

        private async void ShutterButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (CameraStatusUtility.IsContinuousShootingMode(target))
            {
                await StartContShooting();
            }
            else { ShutterButtonPressed(); }
        }

        private async Task StartContShooting()
        {
            if (target == null) { return; }
            if ((PeriodicalShootingTask == null || !PeriodicalShootingTask.IsRunning) && CameraStatusUtility.IsContinuousShootingMode(target))
            {
                try
                {
                    await target.Api.Camera.StartContShootingAsync();
                }
                catch (RemoteApiException ex)
                {
                    DebugUtil.Log(ex.StackTrace);
                    AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("ErrorMessage_shootingFailure") });
                }
            }
        }

        private async Task StopContShooting()
        {
            if (target == null) { return; }
            if ((PeriodicalShootingTask == null || !PeriodicalShootingTask.IsRunning) && CameraStatusUtility.IsContinuousShootingMode(target))
            {
                try
                {
                    await SequentialOperation.StopContinuousShooting(target.Api);
                }
                catch (RemoteApiException ex)
                {
                    DebugUtil.Log(ex.StackTrace);
                    AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("Error_StopContinuousShooting") });
                }
            }
        }

        PeriodicalShootingTask PeriodicalShootingTask;

        async void ShutterButtonPressed()
        {
            var handled = StartStopPeriodicalShooting();

            if (!handled)
            {
                await SequentialOperation.StartStopRecording(
                    new List<TargetDevice> { target },
                    (result) =>
                    {
                        switch (result)
                        {
                            case SequentialOperation.ShootingResult.StillSucceed:
                                if (!ApplicationSettings.GetInstance().IsPostviewTransferEnabled)
                                {
                                    AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("Message_ImageCapture_Succeed") });
                                }
                                break;
                            case SequentialOperation.ShootingResult.StartSucceed:
                            case SequentialOperation.ShootingResult.StopSucceed:
                                break;
                            case SequentialOperation.ShootingResult.StillFailed:
                            case SequentialOperation.ShootingResult.StartFailed:
                                AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("ErrorMessage_shootingFailure") });
                                break;
                            case SequentialOperation.ShootingResult.StopFailed:
                                AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("ErrorMessage_fatal") });
                                break;
                            default:
                                break;
                        }
                    });
            }
        }

        private bool StartStopPeriodicalShooting()
        {
            if (target != null && target.Status != null && target.Status.ShootMode != null && target.Status.ShootMode.Current == ShootModeParam.Still)
            {
                if (PeriodicalShootingTask != null && PeriodicalShootingTask.IsRunning)
                {
                    PeriodicalShootingTask.Stop();
                    return true;
                }
                if (ApplicationSettings.GetInstance().IsIntervalShootingEnabled &&
                    (target.Status.ContShootingMode == null || (target.Status.ContShootingMode != null && target.Status.ContShootingMode.Current == ContinuousShootMode.Single)))
                {
                    PeriodicalShootingTask = SetupPeriodicalShooting();
                    PeriodicalShootingTask.Start();
                    return true;
                }
            }
            return false;
        }

        private PeriodicalShootingTask SetupPeriodicalShooting()
        {
            var task = new PeriodicalShootingTask(new List<TargetDevice>() { target }, ApplicationSettings.GetInstance().IntervalTime);
            task.Tick += async (result) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    switch (result)
                    {
                        case PeriodicalShootingTask.PeriodicalShootingResult.Skipped:
                            AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("PeriodicalShooting_Skipped") });
                            break;
                        case PeriodicalShootingTask.PeriodicalShootingResult.Succeed:
                            AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("Message_ImageCapture_Succeed") });
                            break;
                    };
                });
            };
            task.Stopped += async (reason) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    switch (reason)
                    {
                        case PeriodicalShootingTask.StopReason.ShootingFailed:
                            AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("ErrorMessage_Interval") });
                            break;
                        case PeriodicalShootingTask.StopReason.SkipLimitExceeded:
                            AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("PeriodicalShooting_SkipLimitExceed") });
                            break;
                        case PeriodicalShootingTask.StopReason.RequestedByUser:
                            AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("PeriodicalShooting_StoppedByUser") });
                            break;
                    };
                });
            };
            task.StatusUpdated += async (status) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    DebugUtil.Log("Status updated: " + status.Count);
                    //if (status.IsRunning)
                    //{
                    //    PeriodicalShootingStatus.Visibility = Visibility.Visible;
                    //    PeriodicalShootingStatusText.Text = SystemUtil.GetStringResource("PeriodicalShooting_Status")
                    //        .Replace("__INTERVAL__", status.Interval.ToString())
                    //        .Replace("__PHOTO_NUM__", status.Count.ToString());
                    //}
                    //else { PeriodicalShootingStatus.Visibility = Visibility.Collapsed; }
                });
            };
            return task;
        }

        DisplayState ControlPanelState = DisplayState.AlwaysVisible;

        enum DisplayState
        {
            AlwaysVisible,
            Collapsible,
        }

        bool ControlPanelDisplayed = false;

        private void OpenCloseControlPanel()
        {
            if (ControlPanelState == DisplayState.AlwaysVisible)
            {
                return;
            }

            if (ControlPanelDisplayed)
            {
                StartToHideControlPanel();
                AnimationHelper.CreateRotateAnimation(new AnimationRequest() { Target = OpenControlPanelImage, Duration = TimeSpan.FromMilliseconds(200) }, 180, 0).Begin();
            }
            else
            {
                StartToShowControlPanel();
                AnimationHelper.CreateRotateAnimation(new AnimationRequest() { Target = OpenControlPanelImage, Duration = TimeSpan.FromMilliseconds(200) }, 0, 180).Begin();
            }
        }

        private void StartToShowControlPanel(double duration = 150)
        {
            ControlPanelScroll.Visibility = Visibility.Visible;
            AnimationHelper.CreateSlideAnimation(new SlideAnimationRequest()
            {
                Target = ControlPanelUnit,
                Duration = TimeSpan.FromMilliseconds(duration),
                RequestFadeSide = FadeSide.Right,
                RequestFadeType = FadeType.FadeIn,
                Distance = ControlPanelScroll.ActualWidth,
                Completed = (sender, arg) =>
                {
                    ControlPanelDisplayed = true;
                }
            }).Begin();
        }

        private void StartToHideControlPanel(double duration = 150)
        {
            if (ControlPanelScroll.ActualHeight == 0) { return; }

            AnimationHelper.CreateSlideAnimation(new SlideAnimationRequest()
            {
                Target = ControlPanelUnit,
                Duration = TimeSpan.FromMilliseconds(duration),
                Completed = (sender, obj) =>
                {
                    ControlPanelDisplayed = false;
                    ControlPanelScroll.Visibility = Visibility.Collapsed;
                },
                RequestFadeSide = FadeSide.Right,
                RequestFadeType = FadeType.FadeOut,
                Distance = ControlPanelScroll.ActualWidth,
                WithFade = false,
            }).Begin();
            AnimationHelper.CreateFadeAnimation(new FadeAnimationRequest()
            {
                Target = ControlPanelScroll,
                Duration = TimeSpan.FromMilliseconds(150),
                RequestFadeType = FadeType.FadeOut,
                Completed = (sender, obj) =>
                {
                    ControlPanelScroll.Opacity = 1.0;
                }
            }).Begin();
        }

        private void Grid_ManipulationCompleted_1(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            OpenCloseControlPanel();
        }

        private void Grid_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            OpenCloseControlPanel();
        }

        void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            rwLock.EnterReadLock();
            try
            {
                if (LiveviewImageBitmap == null) { return; }

                args.DrawingSession.DrawImage(LiveviewImageBitmap, (float)LvOffsetH, (float)LvOffsetV);
            }
            finally
            {
                rwLock.ExitReadLock();
            }
            LiveviewFrameCount++;
        }

        private void LiveviewImageCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DisposeLiveviewImageBitmap();
        }

        private void RefreshOverlayControlParams(double magnification)
        {
            double imageHeight, imageWidth;

            rwLock.EnterReadLock();
            try
            {
                if (LiveviewImageBitmap == null)
                {
                    // Maybe changing window size
                    return;
                }

                imageHeight = LiveviewImageBitmap.SizeInPixels.Height * magnification;
                imageWidth = LiveviewImageBitmap.SizeInPixels.Width * magnification;
            }
            finally
            {
                rwLock.ExitReadLock();
            }

            LvOffsetV = (LiveviewImageCanvas.ActualHeight - imageHeight) / 2;
            LvOffsetH = (LiveviewImageCanvas.ActualWidth - imageWidth) / 2;

            _FocusFrameSurface.Height = imageHeight;
            _FocusFrameSurface.Width = imageWidth;
            _FocusFrameSurface.Margin = new Thickness(LvOffsetH, LvOffsetV, 0, 0);
            FramingGuideSurface.Height = imageHeight;
            FramingGuideSurface.Width = imageWidth;
            FramingGuideSurface.Margin = new Thickness(LvOffsetH, LvOffsetV, 0, 0);
        }
    }

}
