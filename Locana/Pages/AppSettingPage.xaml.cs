using System;
using Locana.Controls;
using Locana.DataModel;
using Locana.Playback;
using Locana.Utility;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Geolocation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Locana.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppSettingPage : Page
    {
        public AppSettingPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeItems();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        void InitializeItems()
        {
            AppSettings.Children.Add(BuildShootingSection());
            AppSettings.Children.Add(BuildDisplaySection());
            AppSettings.Children.Add(BuildGallerySection());
        }

        private static SettingSection BuildShootingSection()
        {
            var section = new SettingSection(SystemUtil.GetStringResource("SettingSection_Image"));

            section.Add(new ToggleSetting(
                new AppSettingData<bool>(SystemUtil.GetStringResource("PostviewTransferSetting"), SystemUtil.GetStringResource("Guide_ReceiveCapturedImage"),
                () => { return ApplicationSettings.GetInstance().IsPostviewTransferEnabled; },
                enabled => { ApplicationSettings.GetInstance().IsPostviewTransferEnabled = enabled; })));

            var limited = (Application.Current as App).IsFunctionLimited;

            var geoGuide = limited ? "TrialMessage" : "AddGeotag_guide";
            var geoSetting = new AppSettingData<bool>(SystemUtil.GetStringResource("AddGeotag"), SystemUtil.GetStringResource(geoGuide),
                () =>
                {
                    if (limited) { return false; }
                    else { return ApplicationSettings.GetInstance().GeotagEnabled; }
                },
                enabled =>
                {
                    ApplicationSettings.GetInstance().GeotagEnabled = enabled;
                    if (enabled) { RequestPermission(); }
                });
            var geoToggle = new ToggleSetting(geoSetting);

            if (limited)
            {
                ApplicationSettings.GetInstance().GeotagEnabled = false;
                geoSetting.IsActive = false;
            }
            section.Add(geoToggle);

            return section;
        }

        private static async void RequestPermission()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    // ok.
                    break;
                case GeolocationAccessStatus.Denied:
                    // todo: show error message;
                    break;
                case GeolocationAccessStatus.Unspecified:
                    // todo: show error message;
                    break;
            }
        }

        private static SettingSection BuildDisplaySection()
        {
            var section = new SettingSection(SystemUtil.GetStringResource("SettingSection_Display"));

            section.Add(new ToggleSetting(
                new AppSettingData<bool>(SystemUtil.GetStringResource("DisplayTakeImageButtonSetting"), SystemUtil.GetStringResource("Guide_DisplayTakeImageButtonSetting"),
                () => { return ApplicationSettings.GetInstance().IsShootButtonDisplayed; },
                enabled => { ApplicationSettings.GetInstance().IsShootButtonDisplayed = enabled; })));

            section.Add(new ToggleSetting(
                new AppSettingData<bool>(SystemUtil.GetStringResource("DisplayHistogram"), SystemUtil.GetStringResource("Guide_Histogram"),
                () => { return ApplicationSettings.GetInstance().IsHistogramDisplayed; },
                enabled => { ApplicationSettings.GetInstance().IsHistogramDisplayed = enabled; })));

            var FocusFrameSetting = new AppSettingData<bool>(SystemUtil.GetStringResource("FocusFrameDisplay"), SystemUtil.GetStringResource("Guide_FocusFrameDisplay"),
                () => { return ApplicationSettings.GetInstance().RequestFocusFrameInfo; },
                enabled =>
                {
                    ApplicationSettings.GetInstance().RequestFocusFrameInfo = enabled;
                    // todo: support to show focus frames
                    //await SetupFocusFrame(enabled);
                    //if (!enabled) { _FocusFrameSurface.ClearFrames(); }
                });
            section.Add(new ToggleSetting(FocusFrameSetting));

            section.Add(new ToggleSetting(
                new AppSettingData<bool>(SystemUtil.GetStringResource("LiveviewRotation"), SystemUtil.GetStringResource("LiveviewRotation_guide"),
                    () => { return ApplicationSettings.GetInstance().LiveviewRotationEnabled; },
                    enabled =>
                    {
                        ApplicationSettings.GetInstance().LiveviewRotationEnabled = enabled;
                        // todo: support to rotate liveview image
                        //if (enabled && target != null && target.Status != null)
                        //{
                        //    RotateLiveviewImage(target.Status.LiveviewOrientationAsDouble);
                        //}
                        //else
                        //{
                        //    RotateLiveviewImage(0);
                        //}
                    })));

            section.Add(new ToggleSetting(
                new AppSettingData<bool>(SystemUtil.GetStringResource("FramingGrids"), SystemUtil.GetStringResource("Guide_FramingGrids"),
                    () => { return ApplicationSettings.GetInstance().FramingGridEnabled; },
                    enabled =>
                    {
                        ApplicationSettings.GetInstance().FramingGridEnabled = enabled;
                        // screen_view_data.FramingGridDisplayed = enabled;
                    })));

            var gridTypePanel = new ComboBoxSetting(
                new AppSettingData<int>(SystemUtil.GetStringResource("AssistPattern"), null,
                    () => { return (int)ApplicationSettings.GetInstance().GridType - 1; },
                    setting =>
                    {
                        if (setting < 0) { return; }
                        ApplicationSettings.GetInstance().GridType = (FramingGridTypes)(setting + 1);
                    },
                    SettingValueConverter.FromFramingGrid(EnumUtil<FramingGridTypes>.GetValueEnumerable())));
            gridTypePanel.SetBinding(VisibilityProperty, new Binding
            {
                Source = ApplicationSettings.GetInstance(),
                Path = new PropertyPath(nameof(ApplicationSettings.FramingGridEnabled)),
                Mode = BindingMode.OneWay,
                Converter = new BoolToVisibilityConverter(),
            });
            section.Add(gridTypePanel);

            var gridColorPanel = new ComboBoxSetting(new AppSettingData<int>(SystemUtil.GetStringResource("FramingGridColor"), null,
                    () => { return (int)ApplicationSettings.GetInstance().GridColor; },
                    setting =>
                    {
                        if (setting < 0) { return; }
                        ApplicationSettings.GetInstance().GridColor = (FramingGridColors)setting;
                    },
                    SettingValueConverter.FromFramingGridColor(EnumUtil<FramingGridColors>.GetValueEnumerable())));
            gridColorPanel.SetBinding(VisibilityProperty, new Binding
            {
                Source = ApplicationSettings.GetInstance(),
                Path = new PropertyPath(nameof(ApplicationSettings.FramingGridEnabled)),
                Mode = BindingMode.OneWay,
                Converter = new BoolToVisibilityConverter(),
            });
            section.Add(gridColorPanel);

            var fibonacciOriginPanel = new ComboBoxSetting(new AppSettingData<int>(SystemUtil.GetStringResource("FibonacciSpiralOrigin"), null,
                () => { return (int)ApplicationSettings.GetInstance().FibonacciLineOrigin; },
                setting =>
                {
                    if (setting < 0) { return; }
                    ApplicationSettings.GetInstance().FibonacciLineOrigin = (FibonacciLineOrigins)setting;
                },
                SettingValueConverter.FromFibonacciLineOrigin(EnumUtil<FibonacciLineOrigins>.GetValueEnumerable())));
            fibonacciOriginPanel.SetBinding(VisibilityProperty, new Binding
            {
                Source = ApplicationSettings.GetInstance(),
                Path = new PropertyPath(nameof(ApplicationSettings.IsFibonacciSpiralEnabled)),
                Mode = BindingMode.OneWay,
                Converter = new BoolToVisibilityConverter(),
            });
            section.Add(fibonacciOriginPanel);

            return section;
        }

        private static SettingSection BuildGallerySection()
        {
            var section = new SettingSection(SystemUtil.GetStringResource("SettingSection_ContentsSync"));

            section.Add(new ToggleSetting(
                new AppSettingData<bool>(SystemUtil.GetStringResource("Setting_PrioritizeOriginalSize"), SystemUtil.GetStringResource("Guide_PrioritizeOriginalSize"),
                    () => { return ApplicationSettings.GetInstance().PrioritizeOriginalSizeContents; },
                    enabled => { ApplicationSettings.GetInstance().PrioritizeOriginalSizeContents = enabled; })));

            section.Add(new ComboBoxSetting(
                new AppSettingData<int>(SystemUtil.GetStringResource("ContentTypes"), SystemUtil.GetStringResource("ContentTypesGuide"),
                    () => { return (int)ApplicationSettings.GetInstance().RemoteContentsSet; },
                    newValue =>
                    {
                        if (newValue != -1)
                        {
                            ApplicationSettings.GetInstance().RemoteContentsSet = (ContentsSet)newValue;
                        }
                    },
                    SettingValueConverter.FromContentsSet(EnumUtil<ContentsSet>.GetValueEnumerable()))));

            return section;
        }
    }
}
