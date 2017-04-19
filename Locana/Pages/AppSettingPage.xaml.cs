using Locana.Controls;
using Locana.DataModel;
using Locana.Playback;
using Locana.Resources;
using Locana.Utility;
using System;
using Windows.Devices.Geolocation;
using Windows.Globalization;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

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

            section.Add(new ToggleSetting
            {
                SettingData = new AppSettingData<bool>()
                {
                    Title = SystemUtil.GetStringResource("PostviewTransferSetting"),
                    Guide = SystemUtil.GetStringResource("Guide_ReceiveCapturedImage"),
                    StateProvider = () => ApplicationSettings.GetInstance().IsPostviewTransferEnabled,
                    StateObserver = enabled => ApplicationSettings.GetInstance().IsPostviewTransferEnabled = enabled
                }
            });

            var limited = (Application.Current as App).IsFunctionLimited;

            var geoGuide = limited ? "TrialMessage" : "AddGeotag_guide";
            AppSettingData<bool> geoSetting = null;
            geoSetting = new AppSettingData<bool>()
            {
                Title = SystemUtil.GetStringResource("AddGeotag"),
                Guide = SystemUtil.GetStringResource(geoGuide),
                StateProvider = () =>
                {
                    if (limited) { return false; }
                    else { return ApplicationSettings.GetInstance().GeotagEnabled; }
                },
                StateObserver = enabled =>
                {
                    ApplicationSettings.GetInstance().GeotagEnabled = enabled;
                    if (enabled) { RequestPermission(geoSetting); }
                }
            };
            var geoToggle = new ToggleSetting { SettingData = geoSetting };

            if (ApplicationSettings.GetInstance().GeotagEnabled)
            {
                RequestPermission(geoSetting);
            }

            if (limited)
            {
                ApplicationSettings.GetInstance().GeotagEnabled = false;
                geoSetting.IsActive = false;
            }
            section.Add(geoToggle);

            return section;
        }

        private static async void RequestPermission(AppSettingData<bool> geoSetting)
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    return;
                case GeolocationAccessStatus.Denied:
                    AppShell.Current.Toast.PushToast(new ToastContent
                    {
                        Text = SystemUtil.GetStringResource("UsingLocationDeclined"),
                        Duration = TimeSpan.FromSeconds(5),
                        OnTapped = async () => { await Launcher.LaunchUriAsync(new Uri("ms-settings-location:")); }
                    });
                    break;
                case GeolocationAccessStatus.Unspecified:
                    AppShell.Current.Toast.PushToast(new ToastContent
                    {
                        Text = SystemUtil.GetStringResource("UsingLocationUnspecified"),
                        Duration = TimeSpan.FromSeconds(5),
                        OnTapped = async () => { await Launcher.LaunchUriAsync(new Uri("ms-settings-location:")); }
                    });
                    break;
            }
            ApplicationSettings.GetInstance().GeotagEnabled = false;
            geoSetting.CurrentSetting = false;
        }

        private static SettingSection BuildDisplaySection()
        {
            var section = new SettingSection(SystemUtil.GetStringResource("SettingSection_Display"));

            section.Add(new ToggleSetting
            {
                SettingData = new AppSettingData<bool>()
                {
                    Title = SystemUtil.GetStringResource("DisplayTakeImageButtonSetting"),
                    Guide = SystemUtil.GetStringResource("Guide_DisplayTakeImageButtonSetting"),
                    StateProvider = () => ApplicationSettings.GetInstance().IsShootButtonDisplayed,
                    StateObserver = enabled => ApplicationSettings.GetInstance().IsShootButtonDisplayed = enabled
                }
            });

            section.Add(new ToggleSetting
            {
                SettingData = new AppSettingData<bool>()
                {
                    Title = SystemUtil.GetStringResource("DisplayHistogram"),
                    Guide = SystemUtil.GetStringResource("Guide_Histogram"),
                    StateProvider = () => ApplicationSettings.GetInstance().IsHistogramDisplayed,
                    StateObserver = enabled => ApplicationSettings.GetInstance().IsHistogramDisplayed = enabled
                }
            });

            var FocusFrameSetting = new AppSettingData<bool>()
            {
                Title = SystemUtil.GetStringResource("FocusFrameDisplay"),
                Guide = SystemUtil.GetStringResource("Guide_FocusFrameDisplay"),
                StateProvider = () => ApplicationSettings.GetInstance().RequestFocusFrameInfo,
                StateObserver = enabled =>
                {
                    ApplicationSettings.GetInstance().RequestFocusFrameInfo = enabled;
                    // todo: support to show focus frames
                    //await SetupFocusFrame(enabled);
                    //if (!enabled) { _FocusFrameSurface.ClearFrames(); }
                }
            };
            section.Add(new ToggleSetting { SettingData = FocusFrameSetting });

            section.Add(new ToggleSetting
            {
                SettingData = new AppSettingData<bool>()
                {
                    Title = SystemUtil.GetStringResource("LiveviewRotation"),
                    Guide = SystemUtil.GetStringResource("LiveviewRotation_guide"),
                    StateProvider = () => ApplicationSettings.GetInstance().LiveviewRotationEnabled,
                    StateObserver = enabled =>
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
                    }
                }
            });

            section.Add(new ToggleSetting
            {
                SettingData = new AppSettingData<bool>()
                {
                    Title = SystemUtil.GetStringResource("FramingGrids"),
                    Guide = SystemUtil.GetStringResource("Guide_FramingGrids"),
                    StateProvider = () => ApplicationSettings.GetInstance().FramingGridEnabled,
                    StateObserver = enabled =>
                    {
                        ApplicationSettings.GetInstance().FramingGridEnabled = enabled;
                        // screen_view_data.FramingGridDisplayed = enabled;
                    }
                }
            });

            var gridTypePanel = new ComboBoxSetting(new AppSettingData<int>()
            {
                Title = SystemUtil.GetStringResource("AssistPattern"),
                StateProvider = () => (int)ApplicationSettings.GetInstance().GridType - 1,
                StateObserver = setting =>
                {
                    if (setting < 0) { return; }
                    ApplicationSettings.GetInstance().GridType = (FramingGridTypes)(setting + 1);
                },
                Candidates = SettingValueConverter.FromFramingGrid(EnumUtil<FramingGridTypes>.GetValueEnumerable())
            });
            gridTypePanel.SetBinding(VisibilityProperty, new Binding
            {
                Source = ApplicationSettings.GetInstance(),
                Path = new PropertyPath(nameof(ApplicationSettings.FramingGridEnabled)),
                Mode = BindingMode.OneWay,
                Converter = new BoolToVisibilityConverter(),
            });
            section.Add(gridTypePanel);

            var gridColorPanel = new ComboBoxSetting(new AppSettingData<int>()
            {
                Title = SystemUtil.GetStringResource("FramingGridColor"),
                StateProvider = () => (int)ApplicationSettings.GetInstance().GridColor,
                StateObserver = setting =>
                 {
                     if (setting < 0) { return; }
                     ApplicationSettings.GetInstance().GridColor = (FramingGridColors)setting;
                 },
                Candidates = SettingValueConverter.FromFramingGridColor(EnumUtil<FramingGridColors>.GetValueEnumerable())
            });
            gridColorPanel.SetBinding(VisibilityProperty, new Binding
            {
                Source = ApplicationSettings.GetInstance(),
                Path = new PropertyPath(nameof(ApplicationSettings.FramingGridEnabled)),
                Mode = BindingMode.OneWay,
                Converter = new BoolToVisibilityConverter(),
            });
            section.Add(gridColorPanel);

            var fibonacciOriginPanel = new ComboBoxSetting(new AppSettingData<int>()
            {
                Title = SystemUtil.GetStringResource("FibonacciSpiralOrigin"),
                StateProvider = () => (int)ApplicationSettings.GetInstance().FibonacciLineOrigin,
                StateObserver = setting =>
                {
                    if (setting < 0) { return; }
                    ApplicationSettings.GetInstance().FibonacciLineOrigin = (FibonacciLineOrigins)setting;
                },
                Candidates = SettingValueConverter.FromFibonacciLineOrigin(EnumUtil<FibonacciLineOrigins>.GetValueEnumerable())
            });
            fibonacciOriginPanel.SetBinding(VisibilityProperty, new Binding
            {
                Source = ApplicationSettings.GetInstance(),
                Path = new PropertyPath(nameof(ApplicationSettings.IsFibonacciSpiralEnabled)),
                Mode = BindingMode.OneWay,
                Converter = new BoolToVisibilityConverter(),
            });
            section.Add(fibonacciOriginPanel);

            section.Add(new ToggleSetting
            {
                SettingData = new AppSettingData<bool>()
                {
                    Title = SystemUtil.GetStringResource("ForcePhoneView"),
                    Guide = SystemUtil.GetStringResource("ForcePhoneView_Guide"),
                    StateProvider = () => ApplicationSettings.GetInstance().ForcePhoneView,
                    StateObserver = enabled => ApplicationSettings.GetInstance().ForcePhoneView = enabled
                }
            });

            section.Add(new ToggleSetting
            {
                SettingData = new AppSettingData<bool>()
                {
                    Title = SystemUtil.GetStringResource("ShowKeyCheatSheet"),
                    Guide = SystemUtil.GetStringResource("ShowKeyCheatSheet_Guide"),
                    StateProvider = () => ApplicationSettings.GetInstance().ShowKeyCheatSheet,
                    StateObserver = enabled => ApplicationSettings.GetInstance().ShowKeyCheatSheet = enabled
                }
            });

            section.Add(new ComboBoxSetting(new AppSettingData<int>()
            {
                Title = "🌏 " + SystemUtil.GetStringResource("LanguageSetting"),
                StateProvider = () => (int)LocalizationExtensions.FromLang(ApplicationSettings.GetInstance().LanguageOverride),
                StateObserver = (index) =>
                {
                    if (index == -1)
                    {
                        return;
                    }
                    var lang = ((Localization)index).AsLang();
                    if (ApplicationSettings.GetInstance().LanguageOverride != lang)
                    {
                        ApplicationSettings.GetInstance().LanguageOverride = lang;
                        ApplicationLanguages.PrimaryLanguageOverride = lang;

                        // TODO Reload AppShell
                        // AppShell.Current.Frame.Navigate(typeof(AppShell)); NullReferenceException!!
                    }
                },
                Candidates = SettingValueConverter.FromLocalization(EnumUtil<Localization>.GetValueEnumerable())
            }
            ));

            return section;
        }

        private static SettingSection BuildGallerySection()
        {
            var section = new SettingSection(SystemUtil.GetStringResource("SettingSection_ContentsSync"));

            section.Add(new ToggleSetting
            {
                SettingData = new AppSettingData<bool>()
                {
                    Title = SystemUtil.GetStringResource("Setting_PrioritizeOriginalSize"),
                    Guide = SystemUtil.GetStringResource("Guide_PrioritizeOriginalSize"),
                    StateProvider = () => ApplicationSettings.GetInstance().PrioritizeOriginalSizeContents,
                    StateObserver = enabled => ApplicationSettings.GetInstance().PrioritizeOriginalSizeContents = enabled
                }
            });

            section.Add(new ComboBoxSetting(new AppSettingData<int>()
            {
                Title = SystemUtil.GetStringResource("ContentTypes"),
                Guide = SystemUtil.GetStringResource("ContentTypesGuide"),
                StateProvider = () => (int)ApplicationSettings.GetInstance().RemoteContentsSet,
                StateObserver = newValue =>
                 {
                     if (newValue != -1)
                     {
                         ApplicationSettings.GetInstance().RemoteContentsSet = (ContentsSet)newValue;
                     }
                 },
                Candidates = SettingValueConverter.FromContentsSet(EnumUtil<ContentsSet>.GetValueEnumerable())
            }));

            return section;
        }
    }
}
