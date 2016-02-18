using Locana.Controls;
using Locana.Pages;
using Locana.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Locana
{
    /// <summary>
    /// The "chrome" layer of the app that provides top-level navigation with
    /// proper keyboarding navigation.
    /// </summary>
    public sealed partial class AppShell : Page
    {
        // Declare the top level nav items
        private List<NavMenuItem> navlist = new List<NavMenuItem>(
            new[]
            {
                new NavMenuItem()
                {
                    IconResId = "ic_linked_camera_white",
                    Label = SystemUtil.GetStringResource("AppShell_RemoteShooting"),
                    DestPage = typeof(EntrancePage)
                },
                new NavMenuItem()
                {
                    IconResId = "ic_collection_white",
                    Label = SystemUtil.GetStringResource("PhotoGallery"),
                    DestPage = typeof(ContentsGridPage)
                },
                /*
                new NavMenuItem()
                {
                    IconResId = "ic_network_wifi_white",
                    Label = "WifiDirectPage [TBD]",
                    DestPage = typeof(WifiDirectPage)
                },
                */
            });
        private List<NavMenuItem> navBottomlist = new List<NavMenuItem>(
            new[]
            {
                new NavMenuItem()
                {
                    IconResId = "ic_settings_white",
                    Label = SystemUtil.GetStringResource("AppSettings"),
                    DestPage = typeof(AppSettingPage)
                },
                new NavMenuItem()
                {
                    IconResId = "ic_thumb_up_white",
                    Label = SystemUtil.GetStringResource("Donation"),
                    DestPage = typeof(DonationPage)
                },
                new NavMenuItem()
                {
                    IconResId = "ic_info_outline_white",
                    Label = SystemUtil.GetStringResource("About"),
                    DestPage = typeof(AboutPage)
                }
            });

        private List<NavMenuItem> wholeList = new List<NavMenuItem>();

        public Toast Toast { get { return MessageToast; } }

        public static AppShell Current = null;

        /// <summary>
        /// Initializes a new instance of the AppShell, sets the static 'Current' reference,
        /// adds callbacks for Back requests and changes in the SplitView's DisplayMode, and
        /// provide the nav menu list with the data to display.
        /// </summary>
        public AppShell()
        {
            this.InitializeComponent();

            navlist.ForEach(item => wholeList.Add(item));
            navBottomlist.ForEach(item => wholeList.Add(item));

            this.Loaded += (sender, args) =>
            {
                Current = this;

                this.TogglePaneButton.Focus(FocusState.Programmatic);
            };

            this.RootSplitView.RegisterPropertyChangedCallback(
                SplitView.DisplayModeProperty,
                (s, a) =>
                {
                    // Ensure that we update the reported size of the TogglePaneButton when the SplitView's
                    // DisplayMode changes.
                    this.CheckTogglePaneButtonSizeChanged();
                });

            SystemNavigationManager.GetForCurrentView().BackRequested += SystemNavigationManager_BackRequested;

            NavMenuList.ItemsSource = navlist;
            NavMenuListBottom.ItemsSource = navBottomlist;
        }

        public Frame AppFrame { get { return this.frame; } }

        public EventHandler<BackRequestedEventArgs> BackRequested;

        /// <summary>
        /// Default keyboard focus movement for any unhandled keyboarding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppShell_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            FocusNavigationDirection direction = FocusNavigationDirection.None;
            switch (e.Key)
            {
                case VirtualKey.Left:
                case VirtualKey.GamepadDPadLeft:
                case VirtualKey.GamepadLeftThumbstickLeft:
                case VirtualKey.NavigationLeft:
                    direction = FocusNavigationDirection.Left;
                    break;
                case VirtualKey.Right:
                case VirtualKey.GamepadDPadRight:
                case VirtualKey.GamepadLeftThumbstickRight:
                case VirtualKey.NavigationRight:
                    direction = FocusNavigationDirection.Right;
                    break;

                case VirtualKey.Up:
                case VirtualKey.GamepadDPadUp:
                case VirtualKey.GamepadLeftThumbstickUp:
                case VirtualKey.NavigationUp:
                    direction = FocusNavigationDirection.Up;
                    break;

                case VirtualKey.Down:
                case VirtualKey.GamepadDPadDown:
                case VirtualKey.GamepadLeftThumbstickDown:
                case VirtualKey.NavigationDown:
                    direction = FocusNavigationDirection.Down;
                    break;
            }

            if (direction != FocusNavigationDirection.None)
            {
                var control = FocusManager.FindNextFocusableElement(direction) as Windows.UI.Xaml.Controls.Control;
                if (control != null)
                {
                    control.Focus(FocusState.Programmatic);
                    e.Handled = true;
                }
            }
        }

        #region BackRequested Handlers

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            BackRequested?.Invoke(sender, e);

            if (!e.Handled && this.AppFrame.CanGoBack)
            {
                e.Handled = true;
                this.AppFrame.GoBack();
            }
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Navigate to the Page for the selected <paramref name="listViewItem"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="listViewItem"></param>
        private void NavMenuList_ItemInvoked(object sender, ListViewItem listViewItem)
        {
            var item = (NavMenuItem)((NavMenuListView)sender).ItemFromContainer(listViewItem);

            if (item != null)
            {
                if (item.DestPage != null &&
                    item.DestPage != this.AppFrame.CurrentSourcePageType)
                {
                    this.AppFrame.Navigate(item.DestPage, item.Arguments);
                }
            }
        }

        /// <summary>
        /// Ensures the nav menu reflects reality when navigation is triggered outside of
        /// the nav menu buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNavigatingToPage(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                RevertSelection(NavMenuList, e);
                RevertSelection(NavMenuListBottom, e);
            }
        }

        private void RevertSelection(NavMenuListView view, NavigatingCancelEventArgs e)
        {
            var item = (from p in wholeList where p.DestPage == e.SourcePageType select p).SingleOrDefault();
            if (item == null && this.AppFrame.BackStackDepth > 0)
            {
                // In cases where a page drills into sub-pages then we'll highlight the most recent
                // navigation menu item that appears in the BackStack
                foreach (var entry in this.AppFrame.BackStack.Reverse())
                {
                    item = (from p in wholeList where p.DestPage == entry.SourcePageType select p).SingleOrDefault();
                    if (item != null)
                        break;
                }
            }

            var container = (ListViewItem)view.ContainerFromItem(item);

            // While updating the selection state of the item prevent it from taking keyboard focus.  If a
            // user is invoking the back button via the keyboard causing the selected nav menu item to change
            // then focus will remain on the back button.
            if (container != null) container.IsTabStop = false;
            view.SetSelectedItem(container);
            if (container != null) container.IsTabStop = true;
        }

        private void OnNavigatedToPage(object sender, NavigationEventArgs e)
        {
            DebugUtil.Log("Shell: OnNavigatedToPage: " + e.Content.GetType());
            // After a successful navigation set keyboard focus to the loaded page
            if (e.Content is Page && e.Content != null)
            {
                var control = (Page)e.Content;
                control.Loaded += Page_Loaded;

                UpdateSelection(NavMenuList, control);
                UpdateSelection(NavMenuListBottom, control);
            }

            // Update the Back button depending on whether we can go Back.
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppFrame.CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }

        private void UpdateSelection(NavMenuListView view, Page page)
        {
            var item = (from p in wholeList where p.DestPage == page.GetType() select p).SingleOrDefault();

            var container = (ListViewItem)view.ContainerFromItem(item);

            if (container == null) DebugUtil.Log("Nothing selected");

            if (container != null) container.IsTabStop = false;
            view.SetSelectedItem(container);
            if (container != null) container.IsTabStop = true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((Page)sender).Focus(FocusState.Programmatic);
            ((Page)sender).Loaded -= Page_Loaded;
            this.CheckTogglePaneButtonSizeChanged();
        }

        #endregion

        public Rect TogglePaneButtonRect
        {
            get;
            private set;
        }

        /// <summary>
        /// An event to notify listeners when the hamburger button may occlude other content in the app.
        /// The custom "PageHeader" user control is using this.
        /// </summary>
        public event TypedEventHandler<AppShell, Rect> TogglePaneButtonRectChanged;

        /// <summary>
        /// Callback when the SplitView's Pane is toggled open or close.  When the Pane is not visible
        /// then the floating hamburger may be occluding other content in the app unless it is aware.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TogglePaneButton_Checked(object sender, RoutedEventArgs e)
        {
            this.CheckTogglePaneButtonSizeChanged();
        }

        /// <summary>
        /// Check for the conditions where the navigation pane does not occupy the space under the floating
        /// hamburger button and trigger the event.
        /// </summary>
        private void CheckTogglePaneButtonSizeChanged()
        {
            if (this.RootSplitView.DisplayMode == SplitViewDisplayMode.Inline ||
                this.RootSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                var transform = this.TogglePaneButton.TransformToVisual(this);
                var rect = transform.TransformBounds(new Rect(0, 0, this.TogglePaneButton.ActualWidth, this.TogglePaneButton.ActualHeight));
                this.TogglePaneButtonRect = rect;
            }
            else
            {
                this.TogglePaneButtonRect = new Rect();
            }

            var handler = this.TogglePaneButtonRectChanged;
            if (handler != null)
            {
                // handler(this, this.TogglePaneButtonRect);
                handler.DynamicInvoke(this, this.TogglePaneButtonRect);
            }
        }

        /// <summary>
        /// Enable accessibility on each nav menu item by setting the AutomationProperties.Name on each container
        /// using the associated Label of each item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NavMenuItemContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (!args.InRecycleQueue && args.Item != null && args.Item is NavMenuItem)
            {
                args.ItemContainer.SetValue(AutomationProperties.NameProperty, ((NavMenuItem)args.Item).Label);
            }
            else
            {
                args.ItemContainer.ClearValue(AutomationProperties.NameProperty);
            }
        }

        private void NavMenuList_Loaded(object sender, RoutedEventArgs e)
        {
            var page = frame.Content as Page;
            if (page == null)
            {
                return;
            }
            UpdateSelection(sender as NavMenuListView, page);
        }
    }
}
