using System;

using Kazyx.Uwpmm.DataModel;
using Kazyx.Uwpmm.Utility;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using Locana.Control;

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
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    Frame.CanGoBack
                    ? AppViewBackButtonVisibility.Visible
                    : AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;
        }

        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            SystemNavigationManager.GetForCurrentView().BackRequested -= BackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        void InitializeItems()
        {
            var limited = false; // todo: (App.Current as App).IsFunctionLimited;

            var image_settings = new SettingSection(SystemUtil.GetStringResource("SettingSection_Image"));

            AppSettings.Children.Add(image_settings);

            image_settings.Add(new ToggleSetting(
                new AppSettingData<bool>(SystemUtil.GetStringResource("PostviewTransferSetting"), SystemUtil.GetStringResource("Guide_ReceiveCapturedImage"),
                () => { return ApplicationSettings.GetInstance().IsPostviewTransferEnabled; },
                enabled => { ApplicationSettings.GetInstance().IsPostviewTransferEnabled = enabled; })));

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
                    // todo: support geotagging
                    //if (enabled) { EnableGeolocator(); }
                    //else { DisableGeolocator(); }
                });
            var geoToggle = new ToggleSetting(geoSetting);

            if (limited)
            {
                ApplicationSettings.GetInstance().GeotagEnabled = false;
                geoSetting.IsActive = false;
            }
            image_settings.Add(geoToggle);

            var display_settings = new SettingSection(SystemUtil.GetStringResource("SettingSection_Display"));

            AppSettings.Children.Add(display_settings);

            display_settings.Add(new ToggleSetting(
                new AppSettingData<bool>(SystemUtil.GetStringResource("DisplayTakeImageButtonSetting"), SystemUtil.GetStringResource("Guide_DisplayTakeImageButtonSetting"),
                () => { return ApplicationSettings.GetInstance().IsShootButtonDisplayed; },
                enabled => { ApplicationSettings.GetInstance().IsShootButtonDisplayed = enabled; })));

            display_settings.Add(new ToggleSetting(
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
            display_settings.Add(new ToggleSetting(FocusFrameSetting));

            display_settings.Add(new ToggleSetting(
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

            display_settings.Add(new ToggleSetting(
                new AppSettingData<bool>(SystemUtil.GetStringResource("FramingGrids"), SystemUtil.GetStringResource("Guide_FramingGrids"),
                    () => { return ApplicationSettings.GetInstance().FramingGridEnabled; },
                    enabled =>
                    {
                        ApplicationSettings.GetInstance().FramingGridEnabled = enabled;
                        // screen_view_data.FramingGridDisplayed = enabled;
                    })));

            var gridTypePanel = new ComboBoxSetting(
                new AppSettingData<int>("Pattern", null,
                    () => { return (int)ApplicationSettings.GetInstance().GridType - 1; },
                    setting =>
                    {
                        if (setting < 0) { return; }
                        ApplicationSettings.GetInstance().GridType = (FramingGridTypes)(setting + 1);
                    },
                    SettingValueConverter.FromFramingGrid(EnumUtil<FramingGridTypes>.GetValueEnumerable())));
            gridTypePanel.SetBinding(ComboBoxSetting.VisibilityProperty, new Binding
            {
                Source = ApplicationSettings.GetInstance(),
                Path = new PropertyPath("FramingGridEnabled"),
                Mode = BindingMode.OneWay,
                Converter = new BoolToVisibilityConverter(),
            });
            display_settings.Add(gridTypePanel);

            var gridColorPanel = new ComboBoxSetting(new AppSettingData<int>(SystemUtil.GetStringResource("FramingGridColor"), null,
                    () => { return (int)ApplicationSettings.GetInstance().GridColor; },
                    setting =>
                    {
                        if (setting < 0) { return; }
                        ApplicationSettings.GetInstance().GridColor = (FramingGridColors)setting;
                    },
                    SettingValueConverter.FromFramingGridColor(EnumUtil<FramingGridColors>.GetValueEnumerable())));
            gridColorPanel.SetBinding(ComboBoxSetting.VisibilityProperty, new Binding
            {
                Source = ApplicationSettings.GetInstance(),
                Path = new PropertyPath("FramingGridEnabled"),
                Mode = BindingMode.OneWay,
                Converter = new BoolToVisibilityConverter(),
            });
            display_settings.Add(gridColorPanel);

            var fibonacciOriginPanel = new ComboBoxSetting(new AppSettingData<int>(SystemUtil.GetStringResource("FibonacciSpiralOrigin"), null,
                () => { return (int)ApplicationSettings.GetInstance().FibonacciLineOrigin; },
                setting =>
                {
                    if (setting < 0) { return; }
                    ApplicationSettings.GetInstance().FibonacciLineOrigin = (FibonacciLineOrigins)setting;
                },
                SettingValueConverter.FromFibonacciLineOrigin(EnumUtil<FibonacciLineOrigins>.GetValueEnumerable())));
            fibonacciOriginPanel.SetBinding(ComboBoxSetting.VisibilityProperty, new Binding
            {
                Source = ApplicationSettings.GetInstance(),
                Path = new PropertyPath("IsFibonacciSpiralEnabled"),
                Mode = BindingMode.OneWay,
                Converter = new BoolToVisibilityConverter(),
            });
            display_settings.Add(fibonacciOriginPanel);
        }
    }
}
