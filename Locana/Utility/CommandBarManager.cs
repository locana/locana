using Locana.DataModel;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Locana.Utility
{
    public class CommandBarManager
    {
        public CommandBarManager()
        {
            EnabledItems.Add(AppBarItemType.Command, new SortedSet<AppBarItem>());
            EnabledItems.Add(AppBarItemType.Hidden, new SortedSet<AppBarItem>());
            EnabledItems.Add(AppBarItemType.Content, new SortedSet<AppBarItem>());
        }

        private CommandBar bar = new CommandBar();

        private LiveviewScreenViewData _ContenteViewData = null;
        public LiveviewScreenViewData ContentViewData
        {
            get { return _ContenteViewData; }
            set
            {
                _ContenteViewData = value;
                this.bar.Content = BuildContentPanel(value, EnabledItems[AppBarItemType.Content]);
            }
        }

        StackPanel BuildContentPanel(LiveviewScreenViewData data, SortedSet<AppBarItem> items)
        {
            var panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Height = 40,
                Margin = new Thickness(24, 0, 0, 24),
            };

            if (items.Contains(AppBarItem.FNumberSlider))
            {
                var FnumberButton = NewButtonWithHandler(AppBarItem.FNumberSlider);
                FnumberButton.SetBinding(Button.VisibilityProperty, new Binding()
                {
                    Source = data,
                    Path = new PropertyPath("IsSetFNumberAvailable"),
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter(),
                    FallbackValue = false,
                });
                panel.Children.Add(FnumberButton);
            }

            if (items.Contains(AppBarItem.ShutterSpeedSlider))
            {
                var SSButton = NewButtonWithHandler(AppBarItem.ShutterSpeedSlider);
                SSButton.SetBinding(Button.VisibilityProperty, new Binding()
                {
                    Source = data,
                    Path = new PropertyPath("IsSetShutterSpeedAvailable"),
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter()
                });
                panel.Children.Add(SSButton);
            }

            if (items.Contains(AppBarItem.IsoSlider))
            {
                var IsoButton = NewButtonWithHandler(AppBarItem.IsoSlider);
                IsoButton.SetBinding(Button.VisibilityProperty, new Binding()
                {
                    Source = data,
                    Path = new PropertyPath("IsSetIsoSpeedRateAvailable"),
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter()
                });
                panel.Children.Add(IsoButton);
            }

            if (items.Contains(AppBarItem.EvSlider))
            {
                var EvButton = NewButtonWithHandler(AppBarItem.EvSlider);
                EvButton.SetBinding(Button.VisibilityProperty, new Binding()
                {
                    Source = data,
                    Path = new PropertyPath("IsSetEVAvailable"),
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter()
                });
                panel.Children.Add(EvButton);
            }

            if (items.Contains(AppBarItem.ProgramShiftSlider))
            {
                var ProgramShiftButton = NewButtonWithHandler(AppBarItem.ProgramShiftSlider);
                ProgramShiftButton.SetBinding(Button.VisibilityProperty, new Binding()
                {
                    Source = data,
                    Path = new PropertyPath("IsProgramShiftAvailable"),
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter()
                });
                panel.Children.Add(ProgramShiftButton);
            }

            return panel;
        }



        private static AppBarButton NewButton(AppBarItem item)
        {
            switch (item)
            {
                case AppBarItem.ControlPanel:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/appbar_cameraSetting.png", UriKind.Absolute) }, Label = SystemUtil.GetStringResource("AppBar_ControlPanel") };
                case AppBarItem.AppSetting:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/feature.settings.png", UriKind.Absolute) }, Label = SystemUtil.GetStringResource("AppBar_AppSetting") };
                case AppBarItem.CancelTouchAF:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/appbar_cancel.png", UriKind.Absolute) }, Label = SystemUtil.GetStringResource("AppBar_CancelTouchAf") };
                case AppBarItem.AboutPage:
                    return new AppBarButton() { Label = SystemUtil.GetStringResource("About") };
                case AppBarItem.LoggerPage:
                    return new AppBarButton() { Label = "Logger" };
                case AppBarItem.PlaybackPage:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/appbar_playback.png", UriKind.Absolute) }, Label = SystemUtil.GetStringResource("AppBar_CameraRoll") };
                case AppBarItem.DeleteMultiple:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/appbar_delete.png") }, Label = SystemUtil.GetStringResource("AppBar_Delete") };
                case AppBarItem.DownloadMultiple:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/appbar_download.png") }, Label = SystemUtil.GetStringResource("AppBar_Download") };
                case AppBarItem.ShowDetailInfo:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/appbar_display_info.png") }, Label = SystemUtil.GetStringResource("ShowDetailInfo") };
                case AppBarItem.HideDetailInfo:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/appbar_close_display.png") }, Label = SystemUtil.GetStringResource("HideDetailInfo") };
                case AppBarItem.Ok:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/appbar_ok.png") }, Label = SystemUtil.GetStringResource("AppBar_Exit") };
                case AppBarItem.WifiSetting:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/appbar_wifi.png") }, Label = SystemUtil.GetStringResource("WifiSettingLauncherButtonText") };
                case AppBarItem.Donation:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/appbar_Dollar.png") }, Label = SystemUtil.GetStringResource("Donation") };
                case AppBarItem.RotateLeft:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/appbar_rotateLeft.png") }, Label = SystemUtil.GetStringResource("RotateLeft") };
                case AppBarItem.RotateRight:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/appbar_rotateRight.png") }, Label = SystemUtil.GetStringResource("RotateRight") };
                case AppBarItem.Refresh:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/AppBar/sync.png") }, Label = SystemUtil.GetStringResource("AppBar_Refresh") };
                case AppBarItem.FNumberSlider:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/LiveViewScreen/aperture.png") }, Label = "F number" };
                case AppBarItem.ShutterSpeedSlider:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/LiveViewScreen/ShutterSpeed.png") }, Label = "Shutter speed" };
                case AppBarItem.IsoSlider:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/LiveViewScreen/ISO.png") }, Label = "ISO" };
                case AppBarItem.ProgramShiftSlider:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/LiveViewScreen/ProgramShift.png") }, Label = "ProgramShift" };
                case AppBarItem.EvSlider:
                    return new AppBarButton() { Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/LiveViewScreen/EVComp.png") }, Label = "EV" };
                case AppBarItem.Tmp_WfdPage:
                    return new AppBarButton() { Label = "WFD page" };
                default:
                    throw new NotImplementedException();
            }
        }

        private AppBarButton NewButtonWithHandler(AppBarItem item)
        {
            var button = NewButton(item);
            if (EventHolder.ContainsKey(item))
            {
                button.Click += EventHolder[item];
            }
            return button;
        }

        private readonly Dictionary<AppBarItemType, SortedSet<AppBarItem>> EnabledItems = new Dictionary<AppBarItemType, SortedSet<AppBarItem>>();

        private readonly Dictionary<AppBarItem, RoutedEventHandler> EventHolder = new Dictionary<AppBarItem, RoutedEventHandler>();

        public CommandBarManager SetEvent(AppBarItem item, RoutedEventHandler handler)
        {
            EventHolder.Add(item, handler);
            return this;
        }

        public CommandBarManager ClearEvents()
        {
            EventHolder.Clear();
            return this;
        }

        public CommandBarManager Clear()
        {
            foreach (var items in EnabledItems)
            {
                items.Value.Clear();
            }
            return this;
        }

        /// <summary>
        /// Add an item to show right end of the bar.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public CommandBarManager Command(AppBarItem item)
        {
            return Enable(AppBarItemType.Command, item);
        }

        public CommandBarManager HiddenItem(AppBarItem item)
        {
            return Enable(AppBarItemType.Hidden, item);
        }

        /// <summary>
        /// Add an item to show left end of command bar.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public CommandBarManager Content(AppBarItem item)
        {
            return Enable(AppBarItemType.Content, item);
        }

        private CommandBarManager Enable(AppBarItemType type, AppBarItem item)
        {
            if (!EnabledItems[type].Contains(item))
            {
                EnabledItems[type].Add(item);
            }
            return this;
        }

        public CommandBarManager Disable(AppBarItemType type, AppBarItem item)
        {
            if (EnabledItems[type].Contains(item))
            {
                EnabledItems[type].Remove(item);
            }
            return this;
        }

        public CommandBar CreateNew(double opacity)
        {
            bar = new CommandBar()
            {
                Opacity = opacity,
                // ClosedDisplayMode = AppBarClosedDisplayMode.Minimal,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                Background = null,
            };
            foreach (AppBarItem item in EnabledItems[AppBarItemType.Command])
            {
                var button = NewButton(item);
                if (EventHolder.ContainsKey(item))
                {
                    button.Click += EventHolder[item];
                }
                bar.PrimaryCommands.Add(button);
            }
            foreach (AppBarItem item in EnabledItems[AppBarItemType.Hidden])
            {
                var button = NewButton(item);
                if (EventHolder.ContainsKey(item))
                {
                    button.Click += EventHolder[item];
                }
                bar.SecondaryCommands.Add(button);
            }

            bar.Content = BuildContentPanel(ContentViewData, EnabledItems[AppBarItemType.Content]);
            return bar;
        }

        public bool IsEnabled(AppBarItemType type, AppBarItem item)
        {
            return EnabledItems[type].Contains(item);
        }
    }

    public enum AppBarItemType
    {
        Command,
        Content,
        Hidden,
    }

    public enum AppBarItem
    {
        PlaybackPage,
        AboutPage,
        LoggerPage,
        CancelTouchAF,
        DownloadMultiple,
        DeleteMultiple,
        Ok,
        WifiSetting,
        Donation,
        RotateRight,
        ShowDetailInfo,
        HideDetailInfo,
        RotateLeft,
        Refresh,
        AppSetting,
        ControlPanel,
        FNumberSlider,
        ShutterSpeedSlider,
        IsoSlider,
        EvSlider,
        ProgramShiftSlider,
        Tmp_WfdPage,
    }
}
