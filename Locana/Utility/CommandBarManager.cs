using Locana.DataModel;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

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

        public LiveviewScreenViewData ShootingScreenBarData { get; set; }

        StackPanel BuildContentPanel(LiveviewScreenViewData data, SortedSet<AppBarItem> items)
        {
            var panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Height = 40,
                Margin = new Thickness(24, 0, 0, 24),
            };

            if (data == null) { return panel; }

            if (items.Contains(AppBarItem.FNumberSlider))
            {
                var FnumberButton = NewButtonWithHandler(AppBarItem.FNumberSlider);
                FnumberButton.SetBinding(Button.VisibilityProperty, new Binding()
                {
                    Source = data,
                    Path = new PropertyPath("IsSetFNumberAvailable"),
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter(),
                    FallbackValue = Visibility.Collapsed
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
                    Converter = new BoolToVisibilityConverter(),
                    FallbackValue = Visibility.Collapsed
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
                    Converter = new BoolToVisibilityConverter(),
                    FallbackValue = Visibility.Collapsed
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
                    Converter = new BoolToVisibilityConverter(),
                    FallbackValue = Visibility.Collapsed
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
                    Converter = new BoolToVisibilityConverter(),
                    FallbackValue = Visibility.Collapsed
                });
                panel.Children.Add(ProgramShiftButton);
            }

            if (items.Contains(AppBarItem.Zoom))
            {
                var ZoomButton = NewButtonWithHandler(AppBarItem.Zoom);
                ZoomButton.SetBinding(Button.VisibilityProperty, new Binding()
                {
                    Source = data,
                    Path = new PropertyPath("IsZoomAvailable"),
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter(),
                    FallbackValue = Visibility.Collapsed
                });
                panel.Children.Add(ZoomButton);
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
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Setting), Label = SystemUtil.GetStringResource("AppSetting") };
                case AppBarItem.CancelTouchAF:
                    return new AppBarButton() { ContentTemplate = (DataTemplate)Application.Current.Resources["CancelIcon"], Label = "" };
                case AppBarItem.Close:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Cancel), Label = SystemUtil.GetStringResource("AppBar_Close") };
                case AppBarItem.DeleteMultiple:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Delete), Label = SystemUtil.GetStringResource("AppBar_Delete") };
                case AppBarItem.DownloadMultiple:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Download), Label = SystemUtil.GetStringResource("AppBar_Download") };
                case AppBarItem.ShowDetailInfo:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.OpenPane) { RenderTransform = new ScaleTransform { ScaleX = -1 }, RenderTransformOrigin = new Point { X = 0.5, Y = 0.5 } }, Label = SystemUtil.GetStringResource("ShowDetailInfo") };
                case AppBarItem.HideDetailInfo:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.ClosePane) { RenderTransform = new ScaleTransform { ScaleX = -1 }, RenderTransformOrigin = new Point { X = 0.5, Y = 0.5 } }, Label = SystemUtil.GetStringResource("HideDetailInfo") };
                case AppBarItem.Ok:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Accept), Label = SystemUtil.GetStringResource("AppBar_Exit") };
                case AppBarItem.WifiSetting:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.ThreeBars), Label = SystemUtil.GetStringResource("WifiSettingLauncherButtonText") };
                case AppBarItem.Donation:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Like), Label = SystemUtil.GetStringResource("Donation") };
                case AppBarItem.RotateLeft:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Rotate) { RenderTransform = new ScaleTransform { ScaleX = -1 }, RenderTransformOrigin = new Point { X = 0.5, Y = 0.5 } }, Label = SystemUtil.GetStringResource("RotateLeft") };
                case AppBarItem.RotateRight:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Rotate), Label = SystemUtil.GetStringResource("RotateRight") };
                case AppBarItem.Refresh:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Refresh), Label = SystemUtil.GetStringResource("AppBar_Refresh") };
                case AppBarItem.FNumberSlider:
                    return new AppBarButton() { ContentTemplate = (DataTemplate)Application.Current.Resources["ApertureIcon"] , Label = SystemUtil.GetStringResource("CommandBar_Fnumber") };
                case AppBarItem.ShutterSpeedSlider:
                    return new AppBarButton() { ContentTemplate = (DataTemplate)Application.Current.Resources["SSIcon"], Label = SystemUtil.GetStringResource("CommandBar_SS") };
                case AppBarItem.IsoSlider:
                    return new AppBarButton() { ContentTemplate = (DataTemplate)Application.Current.Resources["IsoIcon"], Label = SystemUtil.GetStringResource("CommandBar_ISO") };
                case AppBarItem.ProgramShiftSlider:
                    return new AppBarButton() { ContentTemplate = (DataTemplate)Application.Current.Resources["ProgramShiftIcon"], Label = SystemUtil.GetStringResource("CommandBar_PShift") };
                case AppBarItem.EvSlider:
                    return new AppBarButton() { ContentTemplate = (DataTemplate)Application.Current.Resources["EvIcon"], Label = SystemUtil.GetStringResource("CommandBar_EV") };
                case AppBarItem.Zoom:
                    return new AppBarButton() { ContentTemplate = (DataTemplate)Application.Current.Resources["ZoomInIcon"], Label = SystemUtil.GetStringResource("CommandBar_Zoom") };
                case AppBarItem.Resume:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Play), Label = SystemUtil.GetStringResource("AppBar_Play") };
                case AppBarItem.Pause:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Pause), Label = SystemUtil.GetStringResource("AppBar_Pause") };
                case AppBarItem.LocalStorage:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.CellPhone), Label = SystemUtil.GetStringResource("AppBar_LocalStorage") }; // TODO desktop or tablet devices
                case AppBarItem.RemoteStorage:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.MapDrive), Label = SystemUtil.GetStringResource("AppBar_RemoteStorage") };
                case AppBarItem.Cancel:
                    return new AppBarButton() { ContentTemplate = (DataTemplate)Application.Current.Resources["CancelIcon"], Label = SystemUtil.GetStringResource("AppBar_Cancel") };
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
            DebugUtil.Log("Creating button: " + item + " " + button.Visibility);
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

        public void ApplyCommands(CommandBar bar)
        {
            bar.PrimaryCommands.Clear();
            bar.SecondaryCommands.Clear();

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
        }

        public void ApplyShootingScreenContents(CommandBar bar)
        {
            bar.Content = BuildContentPanel(ShootingScreenBarData, EnabledItems[AppBarItemType.Content]);
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
        CancelTouchAF,
        DownloadMultiple,
        DeleteMultiple,
        Ok,
        WifiSetting,
        Donation,
        RotateRight,
        RotateLeft,
        ShowDetailInfo,
        HideDetailInfo,
        Resume,
        Pause,
        Close,
        Refresh,
        AppSetting,
        ControlPanel,
        FNumberSlider,
        ShutterSpeedSlider,
        IsoSlider,
        EvSlider,
        ProgramShiftSlider,
        Zoom,
        LocalStorage,
        RemoteStorage,
        Cancel,
    }
}
