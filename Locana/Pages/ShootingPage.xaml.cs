using Kazyx.ImageStream;
using Kazyx.RemoteApi;
using Kazyx.RemoteApi.Camera;
using Locana.CameraControl;
using Locana.Controls;
using Locana.DataModel;
using Locana.Settings;
using Locana.Utility;
using Naotaco.Histogram.Win2d;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Locana.Pages
{
    public sealed partial class ShootingPage : Page
    {
        public ShootingPage()
        {
            InitializeComponent();

            InitializeCommandBar();
            InitializeUI();
        }

        private void InitializeUI()
        {
            HistogramControl.Init(Histogram.ColorType.White, 800);

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
                .DeviceDependent(AppBarItem.EvSlider)
                .DeviceDependent(AppBarItem.IsoSlider)
                .DeviceDependent(AppBarItem.ProgramShiftSlider);
        }

        private DisplayRequest displayRequest = new DisplayRequest();

        private CommandBarManager _CommandBarManager = new CommandBarManager();

        private void InitializeCommandBar()
        {
            _CommandBarManager.SetEvent(AppBarItem.FNumberSlider, (s, args) => { ToggleVisibility(FnumberSlider); });
            _CommandBarManager.SetEvent(AppBarItem.ShutterSpeedSlider, (s, args) => { ToggleVisibility(SSSlider); });
            _CommandBarManager.SetEvent(AppBarItem.IsoSlider, (s, args) => { ToggleVisibility(ISOSlider); });
            _CommandBarManager.SetEvent(AppBarItem.EvSlider, (s, args) => { ToggleVisibility(EvSlider); });
            _CommandBarManager.SetEvent(AppBarItem.ProgramShiftSlider, (s, args) => { ToggleVisibility(ProgramShiftSlider); });
            _CommandBarManager.SetEvent(AppBarItem.Zoom, (s, args) => { ToggleVisibility(ZoomElements); });
            _CommandBarManager.SetEvent(AppBarItem.CancelTouchAF, (s, args) => { target?.Api?.Camera?.CancelTouchAFAsync().IgnoreExceptions(); });
        }

        private void ToggleVisibility(FrameworkElement element)
        {
            if (element.Visibility.IsVisible())
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

        private void HideAllSliders()
        {
            foreach (var s in Sliders.Children)
            {
                if (s.Visibility.IsVisible())
                {
                    AnimationHelper.CreateFadeAnimation(new FadeAnimationRequest()
                    {
                        RequestFadeType = FadeType.FadeOut,
                        Target = s as FrameworkElement,
                        Duration = TimeSpan.FromMilliseconds(150),
                        Completed = (sender, arg) => { s.Visibility = Visibility.Collapsed; }
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

            LiveviewUnit.RotateLiveviewImage(0);
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
            DebugUtil.Log(() => "orientation: " + info.CurrentOrientation);
            DebugUtil.Log(() => LayoutRoot.ActualWidth + " x " + LayoutRoot.ActualHeight);
        }

        private async void HardwareButtons_CameraPressed(object sender, CameraEventArgs e)
        {
            if (CameraStatusUtility.IsContinuousShootingMode(target)) { await StartContShooting(); }
            else { ShutterButtonPressed(); }
        }

        private async void HardwareButtons_CameraReleased(object sender, CameraEventArgs e)
        {
            if (target == null || target.Api == null) { return; }
            if (target.Api.Capability.IsAvailable("cancelHalfPressShutter"))
            {
                try
                {
                    await target?.Api?.Camera?.CancelHalfPressShutterAsync();
                }
                catch (RemoteApiException) { }
            }
            await StopContShooting();
        }

        private void HardwareButtons_CameraHalfPressed(object sender, CameraEventArgs e)
        {
            if (!target?.Api?.Capability?.IsAvailable("actHalfPressShutter") ?? true) { return; }
            target?.Api?.Camera?.ActHalfPressShutterAsync().IgnoreExceptions();
        }

        private const string WIDE_STATE = "WideState";
        private const string NARROW_STATE = "NarrowState";

        private void InitializeVisualStates()
        {
            var groups = VisualStateManager.GetVisualStateGroups(LayoutRoot);

            foreach (var g in groups)
            {
                if (g != null)
                {
                    if (g.CurrentState != null)
                    {
                        DebugUtil.Log(() => "CurrentState: " + g.CurrentState.Name);
                    }else
                    {
                        DebugUtil.Log(() => "current state is null.");
                    }
                }else
                {
                    DebugUtil.Log(() => "g is null/");
                }
            }

            groups[0].CurrentStateChanged += (sender, e) =>
            {
                DebugUtil.Log(() => "Width state changed: " + e.OldState?.Name + " -> " + e.NewState?.Name);
                switch (e.NewState?.Name)
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

            //// initialize UI according to current state
            switch (groups[0].CurrentState?.Name)
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

        private LiveviewScreenViewData ScreenViewData;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var target = e.Parameter as TargetDevice;
            await SetupScreen(target);

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

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (target != null)
            {
                target.Status.PropertyChanged -= Status_PropertyChanged;
                target.Api.AvailiableApisUpdated -= Api_AvailiableApisUpdated;
            }

            if (ScreenViewData != null)
            {
                ScreenViewData.PropertyChanged -= ScreenViewData_PropertyChanged;
            }

            liveview.JpegRetrieved -= liveview_JpegRetrieved;
            liveview.FocusFrameRetrieved -= Liveview_FocusFrameRetrieved;
            liveview.Closed -= liveview_Closed;
            HistogramCreator.Stop();
            LiveviewUnit.FpsTimer.Stop();

            MediaDownloader.Instance.Fetched -= OnFetchdImage;

            var task = SequentialOperation.TearDown(target, liveview);

            TearDownCurrentTarget();
            GeolocatorManager.INSTANCE.Stop();

            base.OnNavigatingFrom(e);
        }

        private async Task SetupScreen(TargetDevice target)
        {
            this.target = target;
            ScreenViewData = new LiveviewScreenViewData(target);
            ScreenViewData.PropertyChanged += ScreenViewData_PropertyChanged;
            LiveviewContext = new LiveviewContext(target, HistogramCreator);
            LiveviewUnit.Context = LiveviewContext;
            LayoutRoot.DataContext = ScreenViewData;

            try
            {
                await SequentialOperation.SetUp(target, liveview);
            }
            catch (Exception ex)
            {
                DebugUtil.Log(() => "Failed setup: " + ex.Message);
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("ErrorMessage_CameraSetupFailure") });
                    AppShell.Current.AppFrame.GoBack();
                });
                return;
            }

            ScreenViewData.ConnectionEstablished = true;

            target.Status.PropertyChanged += Status_PropertyChanged;
            target.Api.AvailiableApisUpdated += Api_AvailiableApisUpdated;

            liveview.JpegRetrieved += liveview_JpegRetrieved;
            liveview.FocusFrameRetrieved += Liveview_FocusFrameRetrieved;
            liveview.Closed += liveview_Closed;
            LiveviewUnit.FpsTimer.Start();

            BatteryStatusDisplay.BatteryInfo = target.Status.BatteryInfo;
            var panels = SettingPanelBuilder.CreateNew(target);
            var pn = panels.GetPanelsToShow();
            foreach (var panel in pn)
            {
                ControlPanel.Children.Add(panel);
            }

            setShootModeEnabled = target.Api.Capability.IsAvailable(API_SET_SHOOT_MODE);
            ControlPanel.SetChildrenControlHitTest(!target.Status.IsRecording());
            ControlPanel.SetChildrenControlTabStop(!target.Status.IsRecording());

            _CommandBarManager.ShootingScreenBarData = ScreenViewData;
            _CommandBarManager.ApplyShootingScreenCommands(AppBarUnit);

            LiveviewUnit.FramingGuideDataContext = ApplicationSettings.GetInstance();
            UpdateShutterButton(target.Status);

            OnCameraStatusChanged(target.Status);

            LiveviewUnit.SetupFocusFrame(ApplicationSettings.GetInstance().RequestFocusFrameInfo).IgnoreExceptions();

            SetUIHandlers();

            if (target.Status.ShootMode?.Current == ShootModeParam.Audio)
            {
                liveviewDisabledByAudioMode = true;
            }
        }

        private void ScreenViewData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var data = sender as LiveviewScreenViewData;
            switch (e.PropertyName)
            {
                case nameof(LiveviewScreenViewData.IsSetEVAvailable):
                    if (EvSlider.Visibility.IsVisible() && !data.IsSetEVAvailable) { ToggleVisibility(EvSlider); }
                    break;
                case nameof(LiveviewScreenViewData.IsSetFNumberAvailable):
                    if (FnumberSlider.Visibility.IsVisible() && !data.IsSetFNumberAvailable) { ToggleVisibility(FnumberSlider); }
                    break;
                case nameof(LiveviewScreenViewData.IsSetIsoSpeedRateAvailable):
                    if (ISOSlider.Visibility.IsVisible() && !data.IsSetIsoSpeedRateAvailable) { ToggleVisibility(ISOSlider); }
                    break;
                case nameof(LiveviewScreenViewData.IsSetShutterSpeedAvailable):
                    if (SSSlider.Visibility.IsVisible() && !data.IsSetShutterSpeedAvailable) { ToggleVisibility(SSSlider); }
                    break;
            }
        }

        private void SetUIHandlers()
        {
            FnumberSlider.SliderOperated += (s, arg) => { target?.Api?.Camera?.SetFNumberAsync(arg.Selected).IgnoreExceptions(); };
            SSSlider.SliderOperated += (s, arg) => { target?.Api?.Camera?.SetShutterSpeedAsync(arg.Selected).IgnoreExceptions(); };
            ISOSlider.SliderOperated += (s, arg) => { target?.Api?.Camera?.SetISOSpeedAsync(arg.Selected).IgnoreExceptions(); };
            EvSlider.SliderOperated += (s, arg) => { target?.Api?.Camera?.SetEvIndexAsync(arg.Selected).IgnoreExceptions(); };
            ProgramShiftSlider.SliderOperated += (s, arg) => { target?.Api?.Camera?.SetProgramShiftAsync(arg.OperatedStep).IgnoreExceptions(); };
        }

        private void Status_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
                    OnCameraStatusChanged(status);
                    break;
                case nameof(CameraStatus.ShootMode):
                    UpdateShutterButton(status);
                    RevaluateLiveviewState(status);
                    break;
                case nameof(CameraStatus.FocusStatus):
                    UpdateFocusStatus(status.FocusStatus);
                    break;
                case nameof(CameraStatus.TouchFocusStatus):
                    UpdateTouchFocus(status.TouchFocusStatus);
                    break;
                case nameof(CameraStatus.LiveviewOrientation):
                    if (ApplicationSettings.GetInstance().LiveviewRotationEnabled)
                    {
                        LiveviewUnit.RotateLiveviewImage(status.LiveviewOrientationAsDouble);
                    }
                    break;
                default:
                    break;
            }
        }

        private const string API_SET_SHOOT_MODE = "setShootMode";
        private bool recording = false;
        private bool setShootModeEnabled;

        private void Api_AvailiableApisUpdated(object sender, AvailableApiEventArgs e)
        {
            if (e.AvailableApis.Contains(API_SET_SHOOT_MODE) ^ setShootModeEnabled)
            {
                setShootModeEnabled = !setShootModeEnabled;
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    UpdateShutterButton(target.Status);
                });
            }
        }
        private Storyboard heartbeatStory;

        private void OnCameraStatusChanged(CameraStatus status)
        {
            if (status.Status == EventParam.Idle)
            {
                // When recording is stopped, clear recording time.
                status.RecordingTimeSec = 0;
            }
            if (status.IsRecording() ^ recording)
            {
                recording = !recording;
                UpdateShutterButton(status);
                ControlPanel.SetChildrenControlHitTest(!status.IsRecording());
                ControlPanel.SetChildrenControlTabStop(!status.IsRecording());

                if ((ScreenViewData?.IsAudioMode ?? false) && recording)
                {
                    CenterMicIcon.ContentTemplate = (DataTemplate)Application.Current.Resources["RecordingMicIcon"];
                    heartbeatStory = AnimationHelper.CreateHeartBeatAnimation(new AnimationRequest
                    {
                        Target = CenterMicIcon
                    }, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(500));
                    heartbeatStory.Begin();
                }
                else
                {
                    CenterMicIcon.ContentTemplate = (DataTemplate)Application.Current.Resources["MicIcon"];
                    heartbeatStory?.Stop();
                }
            }
        }

        private bool liveviewDisabledByAudioMode = false;

        private void RevaluateLiveviewState(CameraStatus status)
        {
            if (liveviewDisabledByAudioMode && status.ShootMode?.Current != ShootModeParam.Audio && liveview?.ConnectionState == ConnectionState.Closed)
            {
                SequentialOperation.OpenLiveviewStream(target.Api, liveview).IgnoreExceptions();
                liveviewDisabledByAudioMode = false;
            }
            else if (!liveviewDisabledByAudioMode && status.ShootMode?.Current == ShootModeParam.Audio)
            {
                SequentialOperation.CloseLiveviewStream(target.Api, liveview).IgnoreExceptions();
                liveviewDisabledByAudioMode = true;
            }
        }

        private void UpdateFocusStatus(string FocusStatus)
        {
            DebugUtil.Log(() => "Focus status changed: " + FocusStatus);
            UpdateFocusStatus(FocusStatus == Kazyx.RemoteApi.Camera.FocusState.Focused);
        }

        private void UpdateTouchFocus(TouchFocusStatus status)
        {
            if (status == null) { return; }
            DebugUtil.Log(() => "TouchFocusStatus changed: " + status.Focused);
            UpdateFocusStatus(status.Focused);
        }

        private void UpdateFocusStatus(bool focused)
        {
            LiveviewUnit.SetFocusedMark(focused);

            if (focused) { ShowCancelTouchAFButton(); }
            else { HideCancelTouchAFButton(); }
        }

        private void HideCancelTouchAFButton()
        {
            _CommandBarManager.Disable(AppBarItemType.Command, AppBarItem.CancelTouchAF)
                .ApplyShootingScreenCommands(AppBarUnit);
        }

        private void ShowCancelTouchAFButton()
        {
            _CommandBarManager.Command(AppBarItem.CancelTouchAF)
                .ApplyShootingScreenCommands(AppBarUnit);
        }

        private void UpdateShutterButton(CameraStatus status)
        {
            if (status == null || status.ShootMode == null || status.ShootMode.Candidates.Count == 0) { return; }

            var icons = new Dictionary<string, DataTemplate>();
            Capability<string> capa;
            if (target?.Api?.Capability?.IsAvailable(API_SET_SHOOT_MODE) ?? false && !ScreenViewData.IsRecording)
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
                icons.Add(m, LiveviewScreenViewData.GetShootModeIcon(m));
                var list = new List<string>();
                list.Add(m);
                capa = new Capability<string> { Current = m, Candidates = list };
            }

            MultiShutterButton.ModeInfo = new ShootModeInfo()
            {
                ShootModeCapability = capa,
                ModeSelected = (mode) => { target?.Api?.Camera?.SetShootModeAsync(mode).IgnoreExceptions(); },
                ButtonPressed = () => { ShutterButtonPressed(); },
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

        private LiveviewContext LiveviewContext;

        private void liveview_JpegRetrieved(object sender, JpegEventArgs e)
        {
            LiveviewContext.JpegPacket = e.Packet;
        }

        private void liveview_Closed(object sender, EventArgs e)
        {
            DebugUtil.Log(() => "Liveview connection closed");
        }

        private void Liveview_FocusFrameRetrieved(object sender, FocusFrameEventArgs e)
        {
            LiveviewContext.FocusPacket = e.Packet;
        }

        private void TearDownCurrentTarget()
        {
            LayoutRoot.DataContext = null;
            PeriodicalShootingTask?.Stop();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            target?.Api?.Camera?.ActZoomAsync(ZoomParam.DirectionOut, ZoomParam.ActionStop).IgnoreExceptions();
        }

        private void ZoomOutButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            target?.Api?.Camera?.ActZoomAsync(ZoomParam.DirectionOut, ZoomParam.Action1Shot).IgnoreExceptions();
        }

        private void ZoomOutButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            target?.Api?.Camera?.ActZoomAsync(ZoomParam.DirectionOut, ZoomParam.ActionStart).IgnoreExceptions();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            target?.Api?.Camera?.ActZoomAsync(ZoomParam.DirectionIn, ZoomParam.ActionStop).IgnoreExceptions();
        }

        private void ZoomInButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            target?.Api?.Camera?.ActZoomAsync(ZoomParam.DirectionIn, ZoomParam.Action1Shot).IgnoreExceptions();
        }

        private void ZoomInButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            target?.Api?.Camera?.ActZoomAsync(ZoomParam.DirectionIn, ZoomParam.ActionStart).IgnoreExceptions();
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
                    await target?.Api?.Camera?.StartContShootingAsync();
                }
                catch (RemoteApiException ex)
                {
                    DebugUtil.Log(() => ex.StackTrace);
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
                    DebugUtil.Log(() => ex.StackTrace);
                    AppShell.Current.Toast.PushToast(new ToastContent { Text = SystemUtil.GetStringResource("Error_StopContinuousShooting") });
                }
            }
        }

        private PeriodicalShootingTask PeriodicalShootingTask;

        private async void ShutterButtonPressed()
        {
            var handled = StartStopPeriodicalShooting();

            if (!handled)
            {
                ScreenViewData.Capturing = true;

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

                ScreenViewData.Capturing = false;
            }
        }

        private bool StartStopPeriodicalShooting()
        {
            if (target?.Status?.ShootMode?.Current == ShootModeParam.Still)
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
                            AppShell.Current.Toast.PushToast(new ToastContent
                            {
                                Text = string.Format(SystemUtil.GetStringResource("PeriodicalShooting_Status"),
                                                    PeriodicalShootingTask.Interval.ToString(),
                                                    PeriodicalShootingTask.Count.ToString())
                            });
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
            task.StatusUpdated += (status) =>
            {
                ScreenViewData.IsPeriodicalShootingRunning = status.IsRunning;
                ControlPanel.SetChildrenControlHitTest(!status.IsRunning);
                ControlPanel.SetChildrenControlTabStop(!status.IsRunning);
                UpdateShutterButton(target.Status);

                /*
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    DebugUtil.Log(() => "Status updated: " + status.Count);

                    PeriodicalShootingStatus.Visibility = status.IsRunning.AsVisibility();
                    if (status.IsRunning)
                    {
                        PeriodicalShootingStatusText.Text = string.Format(
                            SystemUtil.GetStringResource("PeriodicalShooting_Status"),
                            status.Interval.ToString(),
                            status.Count.ToString());
                    }
                });
                */
            };
            return task;
        }

        private DisplayState ControlPanelState = DisplayState.AlwaysVisible;

        enum DisplayState
        {
            AlwaysVisible,
            Collapsible,
        }

        private bool ControlPanelDisplayed = false;

        private void ToggleControlPanel()
        {
            if (ControlPanelState == DisplayState.AlwaysVisible) { return; }

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

        private void ControlPanelArrow_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            ToggleControlPanel();
        }

        private void ControlPanelArrow_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ToggleControlPanel();
        }
    }
}
