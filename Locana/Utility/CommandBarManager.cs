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
            EnabledItems.Add(AppBarItemType.DeviceDependent, new SortedSet<AppBarItem>());
        }

        public LiveviewScreenViewData ShootingScreenBarData { get; set; }

        private static AppBarButton NewButton(AppBarItem item)
        {
            switch (item)
            {
                case AppBarItem.CancelTouchAF:
                    return new AppBarButton() { ContentTemplate = (DataTemplate)Application.Current.Resources["CancelIcon"], Label = SystemUtil.GetStringResource("AppBar_CancelTouchAf") };
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
                case AppBarItem.Donation:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Like), Label = SystemUtil.GetStringResource("Donation") };
                case AppBarItem.RotateLeft:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Rotate) { RenderTransform = new ScaleTransform { ScaleX = -1 }, RenderTransformOrigin = new Point { X = 0.5, Y = 0.5 } }, Label = SystemUtil.GetStringResource("RotateLeft") };
                case AppBarItem.RotateRight:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Rotate), Label = SystemUtil.GetStringResource("RotateRight") };
                case AppBarItem.FNumberSlider:
                    return new AppBarButton() { ContentTemplate = (DataTemplate)Application.Current.Resources["ApertureIcon"], Label = SystemUtil.GetStringResource("CommandBar_Fnumber") };
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
                    return new AppBarButton() { ContentTemplate = (DataTemplate)Application.Current.Resources["ic_phonelink_white"], Label = SystemUtil.GetStringResource("AppBar_LocalStorage") };
                case AppBarItem.RemoteStorage:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.MapDrive), Label = SystemUtil.GetStringResource("AppBar_RemoteStorage") };
                case AppBarItem.CancelSelection:
                    return new AppBarButton() { Icon = new SymbolIcon(Symbol.Cancel), Label = SystemUtil.GetStringResource("AppBar_Cancel") };
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
            DebugUtil.Log(() => "Creating button: " + item + " " + button.Visibility);
            return button;
        }

        private readonly Dictionary<AppBarItemType, SortedSet<AppBarItem>> EnabledItems = new Dictionary<AppBarItemType, SortedSet<AppBarItem>>();

        private readonly Dictionary<AppBarItem, RoutedEventHandler> EventHolder = new Dictionary<AppBarItem, RoutedEventHandler>();

        private readonly List<AppBarItem> HeartBeater = new List<AppBarItem>();

        private readonly List<AppBarItem> Accented = new List<AppBarItem>();

        public CommandBarManager SetEvent(AppBarItem item, RoutedEventHandler handler)
        {
            EventHolder.Add(item, handler);
            return this;
        }

        public CommandBarManager SetHeartBeat(AppBarItem item)
        {
            HeartBeater.Add(item);
            return this;
        }

        public CommandBarManager SetAccentColor(AppBarItem item)
        {
            Accented.Add(item);
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
        public CommandBarManager DeviceDependent(AppBarItem item)
        {
            return Enable(AppBarItemType.DeviceDependent, item);
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

                if (HeartBeater.Contains(item))
                {
                    ApplyHeartBeatAnimation(button);
                }
                if (Accented.Contains(item))
                {
                    button.Foreground = ResourceManager.SystemControlForegroundAccentBrush;
                }
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

        public void ApplyShootingScreenCommands(CommandBar bar)
        {
            bar.PrimaryCommands.Clear();
            ApplyCommands(bar);

            if (ShootingScreenBarData == null) { return; }

            if (EnabledItems[AppBarItemType.DeviceDependent].Contains(AppBarItem.FNumberSlider))
            {
                var FnumberButton = NewButtonWithHandler(AppBarItem.FNumberSlider);
                FnumberButton.SetBinding(UIElement.VisibilityProperty, new Binding()
                {
                    Source = ShootingScreenBarData,
                    Path = new PropertyPath(nameof(LiveviewScreenViewData.IsSetFNumberAvailable)),
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter(),
                    FallbackValue = Visibility.Collapsed
                });
                bar.PrimaryCommands.Add(FnumberButton);
            }

            if (EnabledItems[AppBarItemType.DeviceDependent].Contains(AppBarItem.ShutterSpeedSlider))
            {
                var SSButton = NewButtonWithHandler(AppBarItem.ShutterSpeedSlider);
                SSButton.SetBinding(UIElement.VisibilityProperty, new Binding()
                {
                    Source = ShootingScreenBarData,
                    Path = new PropertyPath(nameof(LiveviewScreenViewData.IsSetShutterSpeedAvailable)),
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter(),
                    FallbackValue = Visibility.Collapsed
                });
                bar.PrimaryCommands.Add(SSButton);
            }

            if (EnabledItems[AppBarItemType.DeviceDependent].Contains(AppBarItem.IsoSlider))
            {
                var IsoButton = NewButtonWithHandler(AppBarItem.IsoSlider);
                IsoButton.SetBinding(UIElement.VisibilityProperty, new Binding()
                {
                    Source = ShootingScreenBarData,
                    Path = new PropertyPath(nameof(LiveviewScreenViewData.IsSetIsoSpeedRateAvailable)),
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter(),
                    FallbackValue = Visibility.Collapsed
                });
                bar.PrimaryCommands.Add(IsoButton);
            }

            if (EnabledItems[AppBarItemType.DeviceDependent].Contains(AppBarItem.EvSlider))
            {
                var EvButton = NewButtonWithHandler(AppBarItem.EvSlider);
                EvButton.SetBinding(UIElement.VisibilityProperty, new Binding()
                {
                    Source = ShootingScreenBarData,
                    Path = new PropertyPath(nameof(LiveviewScreenViewData.IsSetEVAvailable)),
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter(),
                    FallbackValue = Visibility.Collapsed
                });
                bar.PrimaryCommands.Add(EvButton);
            }

            if (EnabledItems[AppBarItemType.DeviceDependent].Contains(AppBarItem.ProgramShiftSlider))
            {
                var ProgramShiftButton = NewButtonWithHandler(AppBarItem.ProgramShiftSlider);
                ProgramShiftButton.SetBinding(UIElement.VisibilityProperty, new Binding()
                {
                    Source = ShootingScreenBarData,
                    Path = new PropertyPath(nameof(LiveviewScreenViewData.IsProgramShiftAvailable)),
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter(),
                    FallbackValue = Visibility.Collapsed
                });
                bar.PrimaryCommands.Add(ProgramShiftButton);
            }

            if (EnabledItems[AppBarItemType.DeviceDependent].Contains(AppBarItem.Zoom))
            {
                var ZoomButton = NewButtonWithHandler(AppBarItem.Zoom);
                ZoomButton.SetBinding(UIElement.VisibilityProperty, new Binding()
                {
                    Source = ShootingScreenBarData,
                    Path = new PropertyPath(nameof(LiveviewScreenViewData.IsZoomAvailable)),
                    Mode = BindingMode.OneWay,
                    Converter = new BoolToVisibilityConverter(),
                    FallbackValue = Visibility.Collapsed
                });
                bar.PrimaryCommands.Add(ZoomButton);
            }
        }

        public bool IsEnabled(AppBarItemType type, AppBarItem item)
        {
            return EnabledItems[type].Contains(item);
        }

        private void ApplyHeartBeatAnimation(AppBarButton button)
        {
            AnimationHelper.CreateHeartBeatAnimation(new AnimationRequest
            {
                Target = button.Icon, // only icons are suppoted for now.
            }, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(500)).Begin();
        }
    }



    public enum AppBarItemType
    {
        Command,
        DeviceDependent,
        Hidden,
    }

    public enum AppBarItem
    {
        CancelTouchAF,
        DownloadMultiple,
        DeleteMultiple,
        Ok,
        Donation,
        RotateRight,
        RotateLeft,
        ShowDetailInfo,
        HideDetailInfo,
        Resume,
        Pause,
        Close,
        FNumberSlider,
        ShutterSpeedSlider,
        IsoSlider,
        EvSlider,
        ProgramShiftSlider,
        Zoom,
        LocalStorage,
        RemoteStorage,
        CancelSelection,
    }
}
