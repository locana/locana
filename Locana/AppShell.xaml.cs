using Locana.Controls;
using Locana.Pages;
using Locana.Utility;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Locana
{
    public sealed partial class AppShell : Page
    {
        private List<NavMenuItem> topItems = new List<NavMenuItem>(
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
        private List<NavMenuItem> bottomItems = new List<NavMenuItem>(
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

        public Toast Toast { get { return MessageToast; } }

        public void ShowProgressDialog(string text)
        {
            LayoutRoot.IsHitTestVisible = false;
            DownloadDialog.ProgressMessage = text;
            DownloadDialog.Visibility = Visibility.Visible;
        }

        public void HideProgressDialog()
        {
            DownloadDialog.Visibility = Visibility.Collapsed;
            LayoutRoot.IsHitTestVisible = true;
        }

        public Visibility ProgressDialogVisibility { get { return DownloadDialog.Visibility; } }

        public static AppShell Current = null;

        public AppShell()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                Current = this;
            };

            SystemNavigationManager.GetForCurrentView().BackRequested += SystemNavigationManager_BackRequested;

            MenuControl.ItemsSource = topItems;
            MenuControl.OptionsItemsSource = bottomItems;

            CoreWindow.GetForCurrentThread().KeyDown += Global_KeyDown;
        }

        private void OnMenuItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as NavMenuItem;

            if (item.DestPage != null &&
                item.DestPage != AppFrame.CurrentSourcePageType)
            {
                AppFrame.Navigate(item.DestPage, item.Arguments);
            }
        }

        private void Global_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.KeyStatus.RepeatCount == 1)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.Back:
                        AppFrame.GoBack();
                        args.Handled = true;
                        break;
                }
            }
        }

        public Frame AppFrame { get { return this.frame; } }

        public EventHandler<BackRequestedEventArgs> BackRequested;

        #region BackRequested Handlers

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            BackRequested?.Invoke(sender, e);

            if (!e.Handled && AppFrame.CanGoBack)
            {
                e.Handled = true;
                AppFrame.GoBack();
            }
        }

        #endregion

        #region Navigation

        private void OnNavigatedToPage(object sender, NavigationEventArgs e)
        {
            DebugUtil.Log(() => "Shell: OnNavigatedToPage: " + e.Content.GetType());
            // After a successful navigation set keyboard focus to the loaded page
            var page = e.Content as Page;
            if (page != null)
            {
                page.Loaded += Page_Loaded;
                UpdateSelectionInMenu(page);
            }

            // Update the Back button depending on whether we can go Back.
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppFrame.CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }

        private ListViewBase MenuPrivateTopListView;
        private ListViewBase MenuPrivateBottomListView;

        private void RetrivePrivateListViews(HamburgerMenu menu)
        {
            var topListField = (typeof(HamburgerMenu)).GetField("_buttonsListView", BindingFlags.Instance | BindingFlags.NonPublic);

            var topFieldValue = topListField.GetValue(menu);
            MenuPrivateTopListView = topListField.GetValue(menu) as ListViewBase;

            var bottomListField = (typeof(HamburgerMenu)).GetField("_optionsListView", BindingFlags.Instance | BindingFlags.NonPublic);
            MenuPrivateBottomListView = bottomListField.GetValue(menu) as ListViewBase;
        }

        private void MenuControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSelectionInMenu(AppFrame.Content as Page);
        }

        private void UpdateSelectionInMenu(Page page)
        {
            if (MenuPrivateTopListView == null || MenuPrivateBottomListView == null)
            {
                RetrivePrivateListViews(MenuControl);
            }
            if (MenuPrivateTopListView == null || MenuPrivateBottomListView == null)
            {
                return;
            }

            var menuItem = topItems
            .Where(p => p.DestPage == page.GetType())
            .SingleOrDefault();

            UpdateListViewSelection(MenuPrivateTopListView, menuItem);

            var optionItem = bottomItems
                .Where(p => p.DestPage == page.GetType())
                .SingleOrDefault();

            UpdateListViewSelection(MenuPrivateBottomListView, optionItem);
        }

        private void UpdateListViewSelection(ListViewBase view, NavMenuItem menuItem)
        {
            var selectedContainer = view.ContainerFromItem(menuItem) as ListViewItem;

            if (selectedContainer != null)
            {
                selectedContainer.IsTabStop = false;
            }

            var index = -1;
            if (selectedContainer != null)
            {
                index = view.IndexFromContainer(selectedContainer);
            }
            for (var i = 0; i < view.Items.Count; i++)
            {
                var container = view.ContainerFromIndex(i) as ListViewItem;
                if (container == null)
                {
                    continue;
                }
                container.IsSelected = i == index;
            }

            if (selectedContainer != null)
            {
                selectedContainer.IsTabStop = true;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((Page)sender).Focus(FocusState.Programmatic);
            ((Page)sender).Loaded -= Page_Loaded;
        }

        #endregion
    }
}
