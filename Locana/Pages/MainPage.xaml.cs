#define WINDOWS_APP

using Kazyx.ImageStream;
using Kazyx.RemoteApi;
using Kazyx.RemoteApi.Camera;
using Locana.CameraControl;
using Locana.Controls;
using Locana.DataModel;
using Locana.Settings;
using Locana.Utility;
using Naotaco.ImageProcessor.Histogram;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.Storage;
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
            MediaDownloader.Instance.Fetched += OnFetchdImage;

            InitializeCommandBar();
            InitializeUI();
        }

        private void InitializeUI()
        {
            HistogramControl.Init(Histogram.ColorType.White, 800);

            HistogramCreator = null;
            HistogramCreator = new HistogramCreator(HistogramCreator.HistogramResolution.Resolution_256);
            HistogramCreator.OnHistogramCreated += async (r, g, b) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    HistogramControl.SetHistogramValue(r, g, b);
                });
            };

            var bar = _CommandBarManager.Clear(). //
                HiddenItem(AppBarItem.AppSetting). //
                Content(AppBarItem.Zoom). //
                Content(AppBarItem.FNumberSlider). //
                Content(AppBarItem.ShutterSpeedSlider). //
                Content(AppBarItem.IsoSlider). //
                Content(AppBarItem.EvSlider). //
                Content(AppBarItem.ProgramShiftSlider). //
                CreateNew(1.0);
            this.AppBarUnit.Children.Clear();
            this.AppBarUnit.Children.Add(bar);


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

            groups[1].CurrentStateChanged += (sender, e) =>
            {
                Debug.WriteLine("Height state changed: " + e.OldState.Name + " -> " + e.NewState.Name);
                switch (e.NewState.Name)
                {
                    case TALL_STATE:
                        break;
                    case SHORT_STATE:
                        break;
                }
            };
        }

        void HideCommandBar()
        {
            if (this.AppBarUnit.Children.Count > 0)
            {
                //                (this.AppBarUnit.Children[0] as CommandBar).ClosedDisplayMode = AppBarClosedDisplayMode.
            }
        }

        private HistogramCreator HistogramCreator;

        private void OnFetchdImage(StorageFolder folder, StorageFile file, GeotaggingResult result)
        {
            PageHelper.ShowToast("picture saved!", file);
        }

        private TargetDevice target;
        private StreamProcessor liveview = new StreamProcessor();
        private ImageDataSource liveview_data = new ImageDataSource();
        private ImageDataSource postview_data = new ImageDataSource();

        LiveviewScreenViewData ScreenViewData;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var target = e.Parameter as TargetDevice;
            SetupScreen(target);

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                Frame.CanGoBack
                ? AppViewBackButtonVisibility.Visible
                : AppViewBackButtonVisibility.Collapsed;

            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;
        }

        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.Navigate(typeof(EntrancePage));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            SystemNavigationManager.GetForCurrentView().BackRequested -= BackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
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
                return;
            }

            this.target = target;
            target.Status.PropertyChanged += Status_PropertyChanged;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                ScreenViewData = new LiveviewScreenViewData(target);
                ScreenViewData.NotifyFriendlyNameUpdated();
                BatteryStatusDisplay.BatteryInfo = target.Status.BatteryInfo;
                LayoutRoot.DataContext = ScreenViewData;
                var panels = SettingPanelBuilder.CreateNew(target);
                var pn = panels.GetPanelsToShow();
                foreach (var panel in pn)
                {
                    ControlPanel.Children.Add(panel);
                }

                Sliders.DataContext = new ShootingParamViewData() { Status = target.Status, Liveview = ScreenViewData };
                ShootingParams.DataContext = ScreenViewData;
                _CommandBarManager.ContentViewData = ScreenViewData;
                ZoomElements.DataContext = ScreenViewData;

                FramingGuideSurface.DataContext = new OptionalElementsViewData() { AppSetting = ApplicationSettings.GetInstance() };
                UpdateShutterButton(target.Status);

                await SetupFocusFrame(ApplicationSettings.GetInstance().RequestFocusFrameInfo);
                _FocusFrameSurface.ClearFrames();

                HideFrontScreen();
            });

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

        private void HideFrontScreen()
        {
            ScreenViewData.IsWaitingConnection = false;
        }

        void Status_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var status = sender as CameraStatus;
            switch (e.PropertyName)
            {
                case "BatteryInfo":
                    BatteryStatusDisplay.BatteryInfo = status.BatteryInfo;
                    break;
                case "ContShootingResult":
                    EnqueueContshootingResult(status.ContShootingResult);
                    break;
                case "Status":
                    if (status.Status == EventParam.Idle)
                    {
                        // When recording is stopped, clear recording time.
                        status.RecordingTimeSec = 0;
                    }
                    break;
                case "ShootMode":
                    UpdateShutterButton(status);
                    break;
                case "FocusStatus":
                    UpdateFocusStatus(status.FocusStatus);
                    break;
                case "TouchFocusStatus":
                    UpdateTouchFocus(status.TouchFocusStatus);
                    break;
                default:
                    break;
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
            var bar = _CommandBarManager.Disable(AppBarItemType.Command, AppBarItem.CancelTouchAF).CreateNew(1.0);
            this.AppBarUnit.Children.Clear();
            this.AppBarUnit.Children.Add(bar);
        }

        void ShowCancelTouchAFButton()
        {
            var bar = _CommandBarManager.Command(AppBarItem.CancelTouchAF).CreateNew(1.0);
            this.AppBarUnit.Children.Clear();
            this.AppBarUnit.Children.Add(bar);
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
            var icons = new Dictionary<string, BitmapImage>();
            foreach (var m in status.ShootMode.Candidates)
            {
                icons.Add(m, LiveviewScreenViewData.GetShootModeIcon(m));
            }
            MultiShutterButton.ModeInfo = new Controls.ShootModeInfo()
            {
                ShootModeCapability = status.ShootMode,
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
                Icons = icons,
            };
        }

        private static void EnqueueContshootingResult(List<ContShootingResult> ContShootingResult)
        {
            if (ApplicationSettings.GetInstance().IsPostviewTransferEnabled)
            {
                foreach (var result in ContShootingResult)
                {
                    MediaDownloader.Instance.EnqueuePostViewImage(new Uri(result.PostviewUrl, UriKind.Absolute), GeopositionManager.INSTANCE.LatestPosition);
                }
            }
        }

        private bool IsRendering = false;

        async void liveview_JpegRetrieved(object sender, JpegEventArgs e)
        {
            if (IsRendering) { return; }

            IsRendering = true;
            await LiveviewUtil.SetAsBitmap(e.Packet.ImageData, liveview_data, HistogramCreator, Dispatcher);
            IsRendering = false;
        }

        void liveview_Closed(object sender, EventArgs e)
        {
            Debug.WriteLine("Liveview connection closed");
        }

        private void LiveviewImage_Loaded(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            image.DataContext = liveview_data;
            liveview.JpegRetrieved += liveview_JpegRetrieved;
            liveview.Closed += liveview_Closed;
            liveview.FocusFrameRetrieved += Liveview_FocusFrameRetrieved;
        }

        private async void Liveview_FocusFrameRetrieved(object sender, FocusFrameEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _FocusFrameSurface.SetFocusFrames(e.Packet.FocusFrames);
            });
        }

        private void LiveviewImage_Unloaded(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            image.DataContext = null;
            liveview.JpegRetrieved -= liveview_JpegRetrieved;
            liveview.Closed -= liveview_Closed;
            liveview.FocusFrameRetrieved -= Liveview_FocusFrameRetrieved;
            TearDownCurrentTarget();
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
            if (CameraStatusUtility.IsContinuousShootingMode(target)) { PageHelper.ShowToast(SystemUtil.GetStringResource("Message_ContinuousShootingGuide")); }
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
                    PageHelper.ShowErrorToast(SystemUtil.GetStringResource("ErrorMessage_shootingFailure"));
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
                    PageHelper.ShowErrorToast(SystemUtil.GetStringResource("Error_StopContinuousShooting"));
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
                                    PageHelper.ShowToast(SystemUtil.GetStringResource("Message_ImageCapture_Succeed"));
                                }
                                break;
                            case SequentialOperation.ShootingResult.StartSucceed:
                            case SequentialOperation.ShootingResult.StopSucceed:
                                break;
                            case SequentialOperation.ShootingResult.StillFailed:
                            case SequentialOperation.ShootingResult.StartFailed:
                                PageHelper.ShowErrorToast(SystemUtil.GetStringResource("ErrorMessage_shootingFailure"));
                                break;
                            case SequentialOperation.ShootingResult.StopFailed:
                                PageHelper.ShowErrorToast(SystemUtil.GetStringResource("ErrorMessage_fatal"));
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
                            PageHelper.ShowToast(SystemUtil.GetStringResource("PeriodicalShooting_Skipped"));
                            break;
                        case PeriodicalShootingTask.PeriodicalShootingResult.Succeed:
                            PageHelper.ShowToast(SystemUtil.GetStringResource("Message_ImageCapture_Succeed"));
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
                            PageHelper.ShowErrorToast(SystemUtil.GetStringResource("ErrorMessage_Interval"));
                            break;
                        case PeriodicalShootingTask.StopReason.SkipLimitExceeded:
                            PageHelper.ShowErrorToast(SystemUtil.GetStringResource("PeriodicalShooting_SkipLimitExceed"));
                            break;
                        case PeriodicalShootingTask.StopReason.RequestedByUser:
                            PageHelper.ShowToast(SystemUtil.GetStringResource("PeriodicalShooting_StoppedByUser"));
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

        private void LiveviewImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            var height = (sender as Image).RenderSize.Height;
            var width = (sender as Image).RenderSize.Width;

            // To fit focus frames and grids to liveview image
            this._FocusFrameSurface.Height = height;
            this._FocusFrameSurface.Width = width;
            this.FramingGuideSurface.Height = height;
            this.FramingGuideSurface.Width = width;

            // FollowLiveviewDisplay();
        }

    }

}
