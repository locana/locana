using Locana.Common;
using Locana.Controls;
using Locana.DataModel;
using Locana.Network;
using Locana.Playback;
using Locana.Playback.Operator;
using Locana.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Locana.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContentsGridPage : Page
    {
        private NavigationHelper navigationHelper;

        private ContentsOperator Operator;

        private DisplayRequest displayRequest = new DisplayRequest();

        public ContentsGridPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            CommandBarManager.SetEvent(AppBarItem.Ok, (s, args) =>
            {
                DebugUtil.Log("Ok clicked");
                switch (InnerState)
                {
                    case ViewerState.Multi:
                        switch (Operator.ContentsCollection.SelectivityFactor)
                        {
                            case SelectivityFactor.Delete:
                                DeleteSelectedFiles();
                                break;
                            case SelectivityFactor.Download:
                                FetchSelectedImages();
                                break;
                            default:
                                DebugUtil.Log("Nothing to do for current SelectivityFactor: " + Operator.ContentsCollection.SelectivityFactor);
                                break;
                        }
                        UpdateSelectionMode(SelectivityFactor.None);
                        UpdateInnerState(ViewerState.Single);
                        break;
                    default:
                        DebugUtil.Log("Nothing to do for current InnerState: " + InnerState);
                        break;
                }
            });
            CommandBarManager.SetEvent(AppBarItem.DeleteMultiple, (s, args) =>
            {
                UpdateSelectionMode(SelectivityFactor.Delete);
                UpdateInnerState(ViewerState.Multi);
            });
            CommandBarManager.SetEvent(AppBarItem.DownloadMultiple, (s, args) =>
            {
                UpdateSelectionMode(SelectivityFactor.Download);
                UpdateInnerState(ViewerState.Multi);
            });
            CommandBarManager.SetEvent(AppBarItem.RotateRight, (s, args) =>
            {
                PhotoScreen.RotateImage(Rotation.Right);
            });
            CommandBarManager.SetEvent(AppBarItem.RotateLeft, (s, args) =>
            {
                PhotoScreen.RotateImage(Rotation.Left);
            });
            CommandBarManager.SetEvent(AppBarItem.ShowDetailInfo, async (s, args) =>
            {
                PhotoScreen.DetailInfoDisplayed = true;
                await Task.Delay(500);
                UpdateAppBar();
            });
            CommandBarManager.SetEvent(AppBarItem.HideDetailInfo, async (s, args) =>
            {
                PhotoScreen.DetailInfoDisplayed = false;
                await Task.Delay(500);
                UpdateAppBar();
            });
            CommandBarManager.SetEvent(AppBarItem.Resume, (s, args) =>
            {
                MoviePlayer.Resume();
            });
            CommandBarManager.SetEvent(AppBarItem.Pause, (s, args) =>
            {
                MoviePlayer.Pause();
            });
            CommandBarManager.SetEvent(AppBarItem.Close, (s, args) =>
            {
                switch (InnerState)
                {
                    case ViewerState.StillPlayback:
                        ReleaseDetail();
                        break;
                    case ViewerState.MoviePlayback:
                        FinishMoviePlayback();
                        break;
                }
                UpdateInnerState(ViewerState.Single);
            });
            CommandBarManager.SetEvent(AppBarItem.LocalStorage, (s, args) =>
            {
                var tuple = Tuple.Create<string, string>(nameof(StorageType.Local), null);
                Frame.Navigate(typeof(ContentsGridPage), tuple);
            });
            CommandBarManager.SetEvent(AppBarItem.RemoteStorage, (s, args) =>
            {
                var menuFlyout = CreateRemoteDrivesMenuFlyout();

                switch (menuFlyout.Items.Count)
                {
                    case 0:
                        UpdateTopBar();
                        break;
                    // case 1:
                    // TODO Transit directly
                    // break;
                    default:
                        FlyoutBase.SetAttachedFlyout(s as FrameworkElement, menuFlyout);
                        FlyoutBase.ShowAttachedFlyout(s as FrameworkElement);
                        break;
                }
            });
            CommandBarManager.SetEvent(AppBarItem.Cancel, (s, args) =>
            {
                UpdateInnerState(ViewerState.Single);
            });
        }

        private MenuFlyout CreateRemoteDrivesMenuFlyout()
        {
            var menu = new MenuFlyout();

            if (TargetStorageType != StorageType.Dummy && DummyContentsFlag.Enabled)
            {
                var item = new MenuFlyoutItem
                {
                    Text = "Dummy storage",
                };
                item.Tapped += DummyStorage_Tapped;
                menu.Items.Add(item);
            }

            NetworkObserver.INSTANCE.CameraDevices.Where(device =>
            {
                return device.Api.AvContent != null && device.Udn != RemoteStorageId;
            }).ToList().ForEach(device =>
            {
                var item = new RemoteStorageMenuFlyoutItem(device.Udn, StorageType.CameraApi)
                {
                    Text = device.FriendlyName,
                };
                item.Tapped += RemoteStorage_Tapped;
                menu.Items.Add(item);
            });

            NetworkObserver.INSTANCE.CdsDevices.Where(upnp =>
            {
                return upnp.UDN != RemoteStorageId;
            }).ToList().ForEach(upnp =>
            {
                var item = new RemoteStorageMenuFlyoutItem(upnp.UDN, StorageType.Dlna)
                {
                    Text = upnp.FriendlyName,
                };
                item.Tapped += RemoteStorage_Tapped;
                menu.Items.Add(item);
            });

            return menu;
        }

        private void RemoteStorage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var item = sender as RemoteStorageMenuFlyoutItem;
            var tuple = Tuple.Create(item.StorageType.ToString(), item.Id);
            Frame.Navigate(typeof(ContentsGridPage), tuple);
        }

        private void DummyStorage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var tuple = Tuple.Create<string, string>(nameof(StorageType.Dummy), null);
            Frame.Navigate(typeof(ContentsGridPage), tuple);
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        public StorageType TargetStorageType { private set; get; } = StorageType.Local;

        private string RemoteStorageId { set; get; }

        public MoviePlaybackScreen MoviePlayerScreen { get { return MoviePlayer; } }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);

            displayRequest.RequestActive();

            if (e.NavigationMode != NavigationMode.New)
            {
                navigationHelper.GoBack();
                return;
            }

            var tuple = e.Parameter as Tuple<string, string>;
            switch (tuple?.Item1 ?? "")
            {
                case nameof(StorageType.Local):
                case "":
                    TargetStorageType = StorageType.Local;
                    break;
                case nameof(StorageType.CameraApi):
                    TargetStorageType = StorageType.CameraApi;
                    break;
                case nameof(StorageType.Dlna):
                    TargetStorageType = StorageType.Dlna;
                    break;
                case nameof(StorageType.Dummy):
                    TargetStorageType = StorageType.Dummy;
                    break;
            }

            RemoteStorageId = tuple?.Item2;

            DebugUtil.Log("OnNavigatedTo: " + tuple);

            UpdateInnerState(ViewerState.Single);

            Operator = ContentsOperatorFactory.CreateNew(this, RemoteStorageId);

            if (Operator == null)
            {
                DebugUtil.Log("Specified device is invalidated");
                navigationHelper.GoBack();
                return;
            }

            TitleBarText.Text = Operator.TitleText;

            Operator.SingleContentLoaded += Operator_SingleContentLoaded;
            Operator.ChunkContentsLoaded += Operator_ChunkContentsLoaded;
            Operator.ErrorMessageRaised += Operator_ErrorMessageRaised;
            Operator.MovieStreamError += Operator_MovieStreamError;
            Operator.Canceller = new CancellationTokenSource();

            FinishMoviePlayback();

            PhotoScreen.DataContext = PhotoData;
            SetStillDetailVisibility(false);

            LoadContents();

            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;

            UpdateTopBar();

            NetworkObserver.INSTANCE.CdsDiscovered += NetworkObserver_CdsDiscovered;
            NetworkObserver.INSTANCE.CameraDiscovered += NetworkObserver_CameraDiscovered;
            NetworkObserver.INSTANCE.DevicesCleared += NetworkObserver_DevicesCleared;
            NetworkObserver.INSTANCE.Start();
        }

        private void NetworkObserver_DevicesCleared(object sender, EventArgs e)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (TargetStorageType != StorageType.Local)
                {
                    var tuple = Tuple.Create<string, string>(nameof(StorageType.Local), null);
                    Frame.Navigate(typeof(ContentsGridPage), tuple);
                }
                else {
                    UpdateTopBar();
                }
            });
        }

        private void NetworkObserver_CameraDiscovered(object sender, CameraDeviceEventArgs e)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateTopBar();
            });
        }

        private void NetworkObserver_CdsDiscovered(object sender, CdServiceEventArgs e)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateTopBar();
            });
        }

        private void UpdateTopBar()
        {
            CommandBarManager.Clear();

            if (TargetStorageType != StorageType.Local)
            {
                CommandBarManager.Command(AppBarItem.LocalStorage);
            }
            if (CountRemoteStorages() != 0)
            {
                CommandBarManager.Command(AppBarItem.RemoteStorage);
            }

            CommandBarManager.ApplyCommands(TitleBar);
        }

        private int CountRemoteStorages()
        {
            var count = 0;

            count += NetworkObserver.INSTANCE.CameraDevices.Where(device =>
            {
                return device.Api.AvContent != null && device.Udn != RemoteStorageId;
            }).Count();

            count += NetworkObserver.INSTANCE.CdsDevices.Where(upnp =>
            {
                return upnp.UDN != RemoteStorageId;
            }).Count();

            if (TargetStorageType != StorageType.Dummy && DummyContentsFlag.Enabled)
            {
                count++;
            }

            return count;
        }

        private void Operator_MovieStreamError()
        {
            UpdateInnerState(ViewerState.Single);

            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MoviePlayerWrapper.Visibility = Visibility.Collapsed;
                HideProgress();
            });
        }

        private void Operator_ErrorMessageRaised(string obj)
        {
            ShowToast(obj);
        }

        private async void Operator_ChunkContentsLoaded(object sender, ContentsLoadedEventArgs e)
        {
            if (InnerState == ViewerState.OutOfPage) return;

            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                DebugUtil.Log("Adding " + e.Contents.Count + " contents to RemoteGrid");
                AddContentsToCollection(e.Contents);
            });
        }

        private async void Operator_SingleContentLoaded(object sender, SingleContentEventArgs e)
        {
            if (InnerState == ViewerState.OutOfPage) return;

            var list = new List<Thumbnail>();
            list.Add(e.File);

            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                AddContentsToCollection(list);
            });
        }

        private void AddContentsToCollection(IList<Thumbnail> contents)
        {
            bool updateAppBarAfterAdded = false;
            if (Operator.ContentsCollection.Count == 0)
            {
                updateAppBarAfterAdded = true;
            }
            foreach (var content in contents)
            {
                switch (ApplicationSettings.GetInstance().RemoteContentsSet)
                {
                    case ContentsSet.Images:
                        if (content.IsMovie) continue;
                        break;
                    case ContentsSet.Movies:
                        if (!content.IsMovie) continue;
                        break;
                }

                Operator.ContentsCollection.Add(content);
                if (updateAppBarAfterAdded)
                {
                    UpdateAppBar();
                    updateAppBarAfterAdded = false;
                }
            }
        }

        private void UpdateSelectionMode(SelectivityFactor factor)
        {
            Operator.ContentsCollection.SelectivityFactor = factor;
            switch (factor)
            {
                case SelectivityFactor.None:
                    ContentsGrid.SelectionMode = ListViewSelectionMode.None;
                    break;
                case SelectivityFactor.Delete:
                case SelectivityFactor.Download:
                    ContentsGrid.SelectionMode = ListViewSelectionMode.Multiple;
                    break;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            NetworkObserver.INSTANCE.Stop();
            NetworkObserver.INSTANCE.CdsDiscovered -= NetworkObserver_CdsDiscovered;
            NetworkObserver.INSTANCE.CameraDiscovered -= NetworkObserver_CameraDiscovered;
            NetworkObserver.INSTANCE.DevicesCleared -= NetworkObserver_DevicesCleared;

            ThumbnailCacheLoader.INSTANCE.CleanupRemainingTasks();

            SystemNavigationManager.GetForCurrentView().BackRequested -= BackRequested;

            FinishMoviePlayback();
            ReleaseDetail();
            PhotoScreen.DataContext = null;

            if (Operator != null)
            {
                Operator.SingleContentLoaded -= Operator_SingleContentLoaded;
                Operator.ChunkContentsLoaded -= Operator_ChunkContentsLoaded;
                Operator.ErrorMessageRaised -= Operator_ErrorMessageRaised;
                Operator.MovieStreamError -= Operator_MovieStreamError;
                Operator.Canceller.Cancel();
                Operator.ContentsCollection.Clear();
                Operator.Dispose();
            }

            HideProgress();

            UpdateInnerState(ViewerState.OutOfPage);

            displayRequest.RequestRelease();

            navigationHelper.OnNavigatedFrom(e);
        }

        CommandBarManager CommandBarManager = new CommandBarManager();

        private ViewerState InnerState = ViewerState.Single;

        private void UpdateInnerState(ViewerState state)
        {
            InnerState = state;
            UpdateAppBar();
        }

        private void UpdateAppBar()
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (InnerState)
                {
                    case ViewerState.Multi:
                        CommandBarManager.Clear()
                            .Command(AppBarItem.Cancel);
                        if (ContentsGrid.SelectedItems.Count != 0)
                        {
                            CommandBarManager.Command(AppBarItem.Ok);
                        }
                        break;
                    case ViewerState.Single:
                        UpdateSelectionMode(SelectivityFactor.None);
                        CommandBarManager.Clear();
                        if (Operator.ContentsCollection.Count != 0)
                        {
                            if (TargetStorageType != StorageType.Local)
                            {
                                CommandBarManager.Command(AppBarItem.DownloadMultiple);
                            }
                            CommandBarManager.Command(AppBarItem.DeleteMultiple);
                        }
                        break;
                    case ViewerState.StillPlayback:
                        if (PhotoScreen.DetailInfoDisplayed)
                        {
                            CommandBarManager.Clear()
                                .Command(AppBarItem.RotateRight)
                                .Command(AppBarItem.RotateLeft)
                                .Command(AppBarItem.HideDetailInfo)
                                .Command(AppBarItem.Close);
                        }
                        else
                        {
                            CommandBarManager.Clear()
                                .Command(AppBarItem.RotateRight)
                                .Command(AppBarItem.RotateLeft)
                                .Command(AppBarItem.ShowDetailInfo)
                                .Command(AppBarItem.Close);
                        }
                        break;
                    case ViewerState.MoviePlayback:
                        CommandBarManager.Clear()
                            .Command(AppBarItem.Resume)
                            .Command(AppBarItem.Pause)
                            .Command(AppBarItem.Close);
                        break;
                    default:
                        CommandBarManager.Clear();
                        break;
                }
                CommandBarManager.ApplyAll(AppBarUnit);

                if (AppBarUnit.PrimaryCommands.Count == 0)
                {
                    AppBarUnit.Visibility = Visibility.Collapsed;
                }
                else
                {
                    AppBarUnit.Visibility = Visibility.Visible;
                }
            });
        }

        internal PhotoPlaybackData PhotoData = new PhotoPlaybackData();

        private async void LoadContents()
        {
            ChangeProgressText(SystemUtil.GetStringResource("Progress_LoadingLocalContents"));

            await Operator.LoadContents();
            HideProgress();
        }

        private async void InitializeContentsGridContents()
        {
            DebugUtil.Log("InitializeContentsGridContents");
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                Operator.ContentsCollection.Clear();
            });
        }

        private async void HideProgress()
        {
            DebugUtil.Log("Hide Progress");
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    // Maybe mobile devices
                    await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
                }
                ProgressCircle.IsActive = false;
            });
        }

        private async void ChangeProgressText(string text)
        {
            DebugUtil.Log("Show Progress: " + text);
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    // Maybe mobile devices
                    var bar = StatusBar.GetForCurrentView();
                    bar.ProgressIndicator.ProgressValue = null;
                    bar.ProgressIndicator.Text = text;
                    await bar.ProgressIndicator.ShowAsync();
                }
                ProgressCircle.IsActive = true;
            });
        }

        private void SetStillDetailVisibility(bool visible)
        {
            if (visible)
            {
                IsViewingDetail = true;
                PhotoScreen.Visibility = Visibility.Visible;
                ContentsGrid.IsEnabled = false;
                UpdateInnerState(ViewerState.StillPlayback);
            }
            else
            {
                IsViewingDetail = false;
                PhotoScreen.Visibility = Visibility.Collapsed;
                ContentsGrid.IsEnabled = true;
                UpdateInnerState(ViewerState.Single);
            }
        }

        private void ReleaseDetail()
        {
            PhotoScreen.ReleaseImage();
            SetStillDetailVisibility(false);
        }

        private bool IsViewingDetail = false;

        private async void DeleteSelectedFiles()
        {
            DebugUtil.Log("DeleteSelectedFiles: " + ContentsGrid.SelectedItems.Count);
            var items = ContentsGrid.SelectedItems;
            if (items.Count == 0)
            {
                HideProgress();
                return;
            }

            await Operator.DeleteSelectedFiles(items.Select(item => item as Thumbnail));

            UpdateAppBar();
        }

        private async void ShowToast(string message)
        {
            DebugUtil.Log(message);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Toast.PushToast(new ToastContent() { Text = message });
            });
        }

        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            DebugUtil.Log("Backkey pressed.");
            if (IsViewingDetail)
            {
                DebugUtil.Log("Release detail.");
                ReleaseDetail();
                e.Handled = true;
            }

            if (MoviePlayerWrapper.Visibility == Visibility.Visible)
            {
                DebugUtil.Log("Close local movie stream.");
                FinishMoviePlayback();
                e.Handled = true;
            }

            if (ContentsGrid.SelectionMode == ListViewSelectionMode.Multiple)
            {
                DebugUtil.Log("Set selection mode none.");
                UpdateInnerState(ViewerState.Single);
                e.Handled = true;
            }

            // Frame.Navigate(typeof(MainPage));
        }

        private void ContentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GridSelectionChanged(sender, e);
        }

        private void GridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selector = sender as GridView;
            if (selector.SelectionMode == ListViewSelectionMode.Multiple)
            {
                DebugUtil.Log("SelectionChanged in multi mode");
                var contents = selector.SelectedItems;
                DebugUtil.Log("Selected Items: " + contents.Count);

                UpdateInnerState(ViewerState.Multi);
            }
        }

        private void ContentsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            GridSources.Source = Operator.ContentsCollection;
            (GridHolder.ZoomedOutView as ListViewBase).ItemsSource = GridSources.View.CollectionGroups;
        }

        private void ContentsGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            GridSources.Source = null;
        }

        private void ContentsGrid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void ContentsGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void Playback_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var data = item.DataContext as Thumbnail;
            PlaybackContent(data);
        }

        private void PlaybackContent(Thumbnail content)
        {
            if (IsViewingDetail)
            {
                return;
            }

            if (content.IsMovie)
            {
                PlaybackMovie(content);
            }
            else
            {
                PlaybackStillImage(content);
            }
        }

        private async void PlaybackStillImage(Thumbnail content)
        {
            ChangeProgressText(SystemUtil.GetStringResource("Progress_OpeningDetailImage"));

            try
            {
                var res = await Operator.PlaybackStillImage(content);

                PhotoScreen.SourceBitmap = res.Item1;
                PhotoScreen.Init();
                PhotoScreen.SetBitmap();

                PhotoData.MetaData = res.Item2;
                if (res.Item2 == null)
                {
                    PhotoScreen.DetailInfoDisplayed = false;
                }
                SetStillDetailVisibility(true);
            }
            catch
            {
                ShowToast(SystemUtil.GetStringResource("Viewer_FailedToOpenDetail"));
            }
            finally
            {
                HideProgress();
            }
        }

        private async void PlaybackMovie(Thumbnail content)
        {
            ChangeProgressText(SystemUtil.GetStringResource("Progress_OpeningMovieStream"));
            UpdateInnerState(ViewerState.MoviePlayback);

            try
            {
                MoviePlayerWrapper.Visibility = Visibility.Visible;
                await Operator.PlaybackMovie(content);
            }
            catch (Exception e)
            {
                DebugUtil.Log(e.StackTrace);
                MoviePlayerWrapper.Visibility = Visibility.Collapsed;
                UpdateInnerState(ViewerState.Single);
            }
            finally
            {
                HideProgress();
            }
        }

        private void FinishMoviePlayback()
        {
            UpdateInnerState(ViewerState.Single);

            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Operator.FinishMoviePlayback();
                MoviePlayerWrapper.Visibility = Visibility.Collapsed;
            });
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            try
            {
                EnqueueDownload(item.DataContext as Thumbnail);
            }
            catch (Exception ex)
            {
                DebugUtil.Log(ex.StackTrace);
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var data = item.DataContext as Thumbnail;
            await Operator.DeleteSelectedFile(data);
        }

        private async void ContentsGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ContentsGrid.SelectionMode == ListViewSelectionMode.Multiple)
            {
                return;
            }

            var image = sender as Grid;
            var content = image.DataContext as Thumbnail;

            if (content.IsContent)
            {
                PlaybackContent(content);
            }
            else
            {
                var holder = image.DataContext as RemainingContentsHolder;
                Operator.ContentsCollection.Remove(holder, false);
                await Operator.LoadRemainingContents(holder);
            }
        }

        private void FetchSelectedImages()
        {
            var items = ContentsGrid.SelectedItems;
            if (items.Count == 0)
            {
                HideProgress();
                return;
            }

            foreach (var item in new List<object>(items))
            {
                try
                {
                    EnqueueDownload(item as Thumbnail);
                }
                catch (Exception e)
                {
                    DebugUtil.Log(e.StackTrace);
                }
            }
        }

        private void EnqueueDownload(Thumbnail source)
        {
            if (source.IsMovie)
            {
                string ext;
                switch (source.Source.MimeType)
                {
                    case MimeType.Mp4:
                        ext = ".mp4";
                        break;
                    default:
                        ext = null;
                        break;
                }
                MediaDownloader.Instance.EnqueueVideo(new Uri(source.Source.OriginalUrl), source.Source.Name, ext);
            }
            else if (ApplicationSettings.GetInstance().PrioritizeOriginalSizeContents && source.Source.OriginalUrl != null)
            {
                MediaDownloader.Instance.EnqueueImage(new Uri(source.Source.OriginalUrl), source.Source.Name,
                    source.Source.MimeType == MimeType.Jpeg ? ".jpg" : null);
            }
            else
            {
                // Fallback to large size image
                MediaDownloader.Instance.EnqueueImage(new Uri(source.Source.LargeUrl), source.Source.Name, ".jpg");
            }
        }
    }

    public enum ViewerState
    {
        Single,
        StillPlayback,
        MoviePlayback,
        Multi,
        OutOfPage,
    }

    public enum StorageType
    {
        Local,
        Dlna,
        CameraApi,
        Dummy,
    }

    public class RemoteStorageMenuFlyoutItem : MenuFlyoutItem
    {
        public RemoteStorageMenuFlyoutItem(string id, StorageType type)
        {
            Id = id;
            StorageType = type;
        }

        public string Id { private set; get; }
        public StorageType StorageType { private set; get; }
    }
}
