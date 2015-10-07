#define WINDOWS_APP

using Kazyx.ImageStream;
using Kazyx.RemoteApi;
using Kazyx.RemoteApi.Camera;
using Kazyx.Uwpmm.CameraControl;
using Kazyx.Uwpmm.Control;
using Kazyx.Uwpmm.DataModel;
using Kazyx.Uwpmm.Settings;
using Kazyx.Uwpmm.Utility;
using Naotaco.ImageProcessor.Histogram;
using Naotaco.Nfc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Xml.Dom;
using Windows.Graphics.Display;
using Windows.Networking.Proximity;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Locana
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            NetworkObserver.INSTANCE.CameraDiscovered += NetworkObserver_Discovered;
            NetworkObserver.INSTANCE.ForceRestart();
            MediaDownloader.Instance.Fetched += OnFetchdImage;
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
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeVisualStates();
            InitializeProximityDevice();
            DisplayInformation.GetForCurrentView().OrientationChanged += MainPage_OrientationChanged;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            StopProximityDevice();
        }

        private void MainPage_OrientationChanged(DisplayInformation info, object args)
        {
            Debug.WriteLine("orientation: " + info.CurrentOrientation);
            Debug.WriteLine(LayoutRoot.ActualWidth + " x " + LayoutRoot.ActualHeight);
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
                        // if it's already closed, open immidiately
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
                        ShootingParamSliderState = DisplayState.AlwaysVisible;
                        StartToShowSliders();
                        break;
                    case SHORT_STATE:
                        ShootingParamSliderState = DisplayState.Collapsible;
                        AnimationHelper.CreateRotateAnimation(new AnimationRequest() { Target = OpenSliderImage, Duration = TimeSpan.FromMilliseconds(10) }, 180, 0).Begin();
                        StartToHideSliders();
                        break;
                }
            };
            switch (groups[1].CurrentState.Name)
            {
                case TALL_STATE:
                    ShootingParamSliderState = DisplayState.AlwaysVisible;
                    StartToShowSliders(0);
                    break;
                case SHORT_STATE:
                    ShootingParamSliderState = DisplayState.Collapsible;
                    StartToHideSliders(0);
                    break;
            }
        }

        private HistogramCreator HistogramCreator;

        private void OnFetchdImage(StorageFolder folder, StorageFile file, GeotaggingResult result)
        {
            ShowToast("picture saved!", file);
        }

        private TargetDevice target;
        private StreamProcessor liveview = new StreamProcessor();
        private ImageDataSource liveview_data = new ImageDataSource();
        private ImageDataSource postview_data = new ImageDataSource();

        LiveviewScreenViewData ScreenViewData;

        async void NetworkObserver_Discovered(object sender, CameraDeviceEventArgs e)
        {
            var target = e.CameraDevice;
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

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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

                ShootingParamSliders.DataContext = new ShootingParamViewData() { Status = target.Status, Liveview = ScreenViewData };

                HideFrontScreen();
            });

            SetUIHandlers();
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
                default:
                    break;
            }
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
        }

        private void LiveviewImage_Unloaded(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            image.DataContext = null;
            liveview.JpegRetrieved -= liveview_JpegRetrieved;
            liveview.Closed -= liveview_Closed;
            TearDownCurrentTarget();
        }

        private void TearDownCurrentTarget()
        {
            LayoutRoot.DataContext = null;
        }

        private ToastNotification BuildToast(string str, StorageFile file = null)
        {
            ToastTemplateType template = ToastTemplateType.ToastImageAndText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(template);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(str));

            var toastImageAttributes = toastXml.GetElementsByTagName("image");

            if (file == null)
            {
                ((XmlElement)toastImageAttributes[0]).SetAttribute("src", "ms-appx:///Assets/Toast/Locana_square_full.png");
            }
            else
            {
                ((XmlElement)toastImageAttributes[0]).SetAttribute("src", file.Path);
            }
            return new ToastNotification(toastXml);
        }

        private void ShowToast(string str, StorageFile file = null)
        {
            Debug.WriteLine("toast with image: " + str);
            var toast = BuildToast(str, file);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private void ShowError(string v)
        {
            Debug.WriteLine("error: " + v);
            ShowToast(v);
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
            if (CameraStatusUtility.IsContinuousShootingMode(target)) { ShowToast(SystemUtil.GetStringResource("Message_ContinuousShootingGuide")); }
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
                    ShowError(SystemUtil.GetStringResource("ErrorMessage_shootingFailure"));
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
                    ShowError(SystemUtil.GetStringResource("Error_StopContinuousShooting"));
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
                                    ShowToast(SystemUtil.GetStringResource("Message_ImageCapture_Succeed"));
                                }
                                break;
                            case SequentialOperation.ShootingResult.StartSucceed:
                            case SequentialOperation.ShootingResult.StopSucceed:
                                break;
                            case SequentialOperation.ShootingResult.StillFailed:
                            case SequentialOperation.ShootingResult.StartFailed:
                                ShowError(SystemUtil.GetStringResource("ErrorMessage_shootingFailure"));
                                break;
                            case SequentialOperation.ShootingResult.StopFailed:
                                ShowError(SystemUtil.GetStringResource("ErrorMessage_fatal"));
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
                            ShowToast(SystemUtil.GetStringResource("PeriodicalShooting_Skipped"));
                            break;
                        case PeriodicalShootingTask.PeriodicalShootingResult.Succeed:
                            ShowToast(SystemUtil.GetStringResource("Message_ImageCapture_Succeed"));
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
                            ShowError(SystemUtil.GetStringResource("ErrorMessage_Interval"));
                            break;
                        case PeriodicalShootingTask.StopReason.SkipLimitExceeded:
                            ShowError(SystemUtil.GetStringResource("PeriodicalShooting_SkipLimitExceed"));
                            break;
                        case PeriodicalShootingTask.StopReason.RequestedByUser:
                            ShowToast(SystemUtil.GetStringResource("PeriodicalShooting_StoppedByUser"));
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

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            OpenCloseSliders();
        }

        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            OpenCloseSliders();
        }

        private void OpenCloseSliders()
        {
            if (ShootingParamSliderState == DisplayState.AlwaysVisible)
            {
                return;
            }

            if (SlidersDisplayed)
            {
                StartToHideSliders();
                AnimationHelper.CreateRotateAnimation(new AnimationRequest() { Target = OpenSliderImage, Duration = TimeSpan.FromMilliseconds(200) }, 180, 0).Begin();
            }
            else
            {
                StartToShowSliders();
                AnimationHelper.CreateRotateAnimation(new AnimationRequest() { Target = OpenSliderImage, Duration = TimeSpan.FromMilliseconds(200) }, 0, 180).Begin();
            }
        }

        bool SlidersDisplayed = false;

        void StartToShowSliders(double duration = 150)
        {
            ShootingParamSliders.Visibility = Visibility.Visible;
            AnimationHelper.CreateSlideAnimation(new SlideAnimationRequest()
            {
                Target = Bottom,
                Duration = TimeSpan.FromMilliseconds(duration),
                RequestFadeSide = FadeSide.Bottom,
                RequestFadeType = FadeType.FadeIn,
                Distance = ShootingParamSliders.ActualHeight,
                Completed = (sender, args) =>
                {
                    SlidersDisplayed = true;
                }
            }).Begin();
        }

        void StartToHideSliders(double duration = 150)
        {
            if (ShootingParamSliders.ActualHeight == 0) { return; }

            AnimationHelper.CreateSlideAnimation(new SlideAnimationRequest()
            {
                Target = Bottom,
                Duration = TimeSpan.FromMilliseconds(duration),
                Completed = (sender, obj) =>
                {
                    SlidersDisplayed = false;
                },
                RequestFadeSide = FadeSide.Bottom,
                RequestFadeType = FadeType.FadeOut,
                Distance = ShootingParamSliders.ActualHeight,
                WithFade = false,
            }).Begin();
            AnimationHelper.CreateFadeAnimation(new FadeAnimationRequest()
            {
                Target = ShootingParamSliders,
                Duration = TimeSpan.FromMilliseconds(150),
                RequestFadeType = FadeType.FadeOut,
                Completed = (sender, obj) =>
                {
                    ShootingParamSliders.Opacity = 1.0;
                }
            }).Begin();
        }

        DisplayState ShootingParamSliderState = DisplayState.AlwaysVisible;
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

        ProximityDevice _ProximityDevice;
        long ProximitySubscribeId;

        private void InitializeProximityDevice()
        {
            StopProximityDevice();

            try
            {
                _ProximityDevice = ProximityDevice.GetDefault();
            }
            catch (FileNotFoundException)
            {
                _ProximityDevice = null;
                DebugUtil.Log("Caught ununderstandable exception. ");
                return;
            }
            catch (COMException)
            {
                _ProximityDevice = null;
                DebugUtil.Log("Caught ununderstandable exception. ");
                return;
            }

            if (_ProximityDevice == null)
            {
                DebugUtil.Log("It seems this is not NFC available device");
                return;
            }

            try
            {
                ProximitySubscribeId = _ProximityDevice.SubscribeForMessage("NDEF", ProximityMessageReceivedHandler);
            }
            catch (Exception e)
            {
                _ProximityDevice = null;
                DebugUtil.Log("Caught ununderstandable exception. " + e.Message + e.StackTrace);
                return;
            }
        }

        private void StopProximityDevice()
        {
            if (_ProximityDevice != null)
            {
                _ProximityDevice.StopSubscribingForMessage(ProximitySubscribeId);
                _ProximityDevice = null;
            }
        }

        private async void ProximityMessageReceivedHandler(ProximityDevice sender, ProximityMessage message)
        {
            var parser = new NdefParser(message);
            var ndefRecords = new List<NdefRecord>();

            var err = "";

            try { ndefRecords = parser.Parse(); }
            catch (NoSonyNdefRecordException) { err = SystemUtil.GetStringResource("ErrorMessage_CantFindSonyRecord"); }
            catch (NoNdefRecordException) { err = SystemUtil.GetStringResource("ErrorMessage_ParseNFC"); }
            catch (NdefParseException) { err = SystemUtil.GetStringResource("ErrorMessage_ParseNFC"); }
            catch (Exception) { err = SystemUtil.GetStringResource("ErrorMessage_fatal"); }

            if (err != "")
            {
                DebugUtil.Log("Failed to read NFC: " + err);
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { ShowError(err); });
                return;
            }

            foreach (NdefRecord r in ndefRecords)
            {
                if (r.SSID.Length > 0 && r.Password.Length > 0)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        var sb = new StringBuilder();
                        sb.Append(SystemUtil.GetStringResource("Message_NFC_succeed"));
                        sb.Append(System.Environment.NewLine);
                        sb.Append(System.Environment.NewLine);
                        sb.Append("SSID: ");
                        sb.Append(r.SSID);
                        sb.Append(System.Environment.NewLine);
                        sb.Append("Password: ");
                        sb.Append(r.Password);
                        sb.Append(System.Environment.NewLine);

                        PutToClipBoard(r.Password);

                        var dialog = new MessageDialog(sb.ToString());
                        try
                        {
                            await dialog.ShowAsync();
                        }
                        catch (UnauthorizedAccessException) {/* Duplicated message dialog */}
                    });
                    break;
                }
            }
        }

        void PutToClipBoard(string s)
        {
            var package = new DataPackage();
            package.RequestedOperation = DataPackageOperation.Copy;
            package.SetText(s);
            Clipboard.SetContent(package);
        }
    }

}
