using Locana.Common;
using Locana.Controls;
using Locana.DataModel;
using Locana.Playback;
using Locana.Utility;
using Naotaco.ImageProcessor.MetaData;
using Naotaco.ImageProcessor.MetaData.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
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
                    case ViewerState.LocalSelecting:
                        switch (ContentsCollection.SelectivityFactor)
                        {
                            case SelectivityFactor.Delete:
                                DeleteSelectedLocalImages();
                                break;
                            default:
                                DebugUtil.Log("Nothing to do for current SelectivityFactor: " + ContentsCollection.SelectivityFactor);
                                break;
                        }
                        UpdateLocalSelectionMode(SelectivityFactor.None);
                        UpdateInnerState(ViewerState.LocalSingle);
                        break;
                    default:
                        DebugUtil.Log("Nothing to do for current InnerState: " + InnerState);
                        break;
                }
            });
            CommandBarManager.SetEvent(AppBarItem.DeleteMultiple, (s, args) =>
            {
                DebugUtil.Log("Delete clicked");
                UpdateLocalSelectionMode(SelectivityFactor.Delete);
                UpdateInnerState(ViewerState.LocalMulti);
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.New)
            {
                this.navigationHelper.GoBack();
                return;
            }

            displayRequest.RequestActive();

            ChangeProgressText("processing.");

            UpdateInnerState(ViewerState.LocalSingle);

            Canceller = new CancellationTokenSource();

            ContentsCollection = new AlbumGroupCollection(false)
            {
                ContentSortOrder = Album.SortOrder.NewOneFirst,
            };

            FinishLocalMoviePlayback();

            PhotoScreen.DataContext = PhotoData;
            SetStillDetailVisibility(false);

            LoadLocalContents();

            MoviePlayer.LocalMediaFailed += LocalMoviePlayer_MediaFailed;
            MoviePlayer.LocalMediaOpened += LocalMoviePlayer_MediaOpened;

            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;
        }

        private void UpdateLocalSelectionMode(SelectivityFactor factor)
        {
            if (ContentsCollection == null) { return; }

            ContentsCollection.SelectivityFactor = factor;
            switch (factor)
            {
                case SelectivityFactor.None:
                    ContentsGrid.SelectionMode = ListViewSelectionMode.None;
                    break;
                case SelectivityFactor.Delete:
                    ContentsGrid.SelectionMode = ListViewSelectionMode.Multiple;
                    break;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ThumbnailCacheLoader.INSTANCE.CleanupRemainingTasks();

            SystemNavigationManager.GetForCurrentView().BackRequested -= BackRequested;

            MoviePlayer.LocalMediaFailed -= LocalMoviePlayer_MediaFailed;
            MoviePlayer.LocalMediaOpened -= LocalMoviePlayer_MediaOpened;
            FinishLocalMoviePlayback();

            Canceller?.Cancel();

            if (ContentsCollection != null)
            {
                ContentsCollection.Clear();
                ContentsCollection = null;
            }

            HideProgress();

            UpdateInnerState(ViewerState.OutOfPage);

            displayRequest.RequestRelease();

            this.navigationHelper.OnNavigatedFrom(e);
        }

        // private StatusBar statusBar = StatusBar.GetForCurrentView();

        CommandBarManager CommandBarManager = new CommandBarManager();

        private ViewerState InnerState = ViewerState.LocalSingle;

        private void UpdateInnerState(ViewerState state)
        {
            InnerState = state;
            UpdateAppBar();
        }

        private void UpdateAppBar()
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AppBarUnit.Children.Clear();
                CommandBar bar = null;
                switch (InnerState)
                {
                    case ViewerState.LocalSelecting:
                        bar = CommandBarManager.Clear()
                            .Command(AppBarItem.Ok)
                            .CreateNew(1.0);
                        break;
                    case ViewerState.LocalSingle:
                        UpdateLocalSelectionMode(SelectivityFactor.None);
                        {
                            var tmp = CommandBarManager.Clear();
                            //.NoIcon(AppBarItem.AppSetting);
                            if (ContentsCollection != null && ContentsCollection.Count != 0)
                            {
                                tmp.Command(AppBarItem.DeleteMultiple);
                            }
                            bar = tmp.CreateNew(1.0);
                        }
                        break;
                    case ViewerState.LocalStillPlayback:
                        if (PhotoScreen.DetailInfoDisplayed)
                        {
                            bar = CommandBarManager.Clear()
                                .Command(AppBarItem.RotateRight)
                                .Command(AppBarItem.HideDetailInfo)
                                .Command(AppBarItem.RotateLeft)
                                .CreateNew(1.0);
                        }
                        else
                        {
                            bar = CommandBarManager.Clear()
                                .Command(AppBarItem.RotateRight)
                                .Command(AppBarItem.ShowDetailInfo)
                                .Command(AppBarItem.RotateLeft)
                                .CreateNew(1.0);
                        }
                        break;
                    default:
                        return;
                }
                AppBarUnit.Children.Add(bar);
            });
        }

        internal PhotoPlaybackData PhotoData = new PhotoPlaybackData();

        private async void LoadLocalContents()
        {
            ChangeProgressText(SystemUtil.GetStringResource("Progress_LoadingLocalContents"));

            var loader = new LocalContentsLoader();
            loader.SingleContentLoaded += LocalContentsLoader_SingleContentLoaded;
            try
            {
                await loader.Load(ContentsSet.Images, Canceller).ConfigureAwait(false);
            }
            catch
            {
                ShowToast(SystemUtil.GetStringResource("Viewer_NoCameraRoll"));
            }
            finally
            {
                loader.SingleContentLoaded -= LocalContentsLoader_SingleContentLoaded;
                HideProgress();
            }
        }

        async void LocalContentsLoader_SingleContentLoaded(object sender, SingleContentEventArgs e)
        {
            if (InnerState == ViewerState.OutOfPage) return;

            bool updateAppBarAfterAdded = false;
            if (ContentsCollection != null && ContentsCollection.Count == 0)
            {
                updateAppBarAfterAdded = true;
            }
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (ContentsCollection != null)
                {
                    switch (ApplicationSettings.GetInstance().RemoteContentsSet)
                    {
                        case ContentsSet.Images:
                            if (e.File.IsMovie) return;
                            break;
                        case ContentsSet.Movies:
                            if (!e.File.IsMovie) return;
                            break;
                    }
                    ContentsCollection.Add(e.File);
                    if (updateAppBarAfterAdded)
                    {
                        UpdateAppBar();
                    }
                }
            });
        }

        private CancellationTokenSource Canceller;

        private AlbumGroupCollection ContentsCollection;

        private async void InitializeContentsGridContents()
        {
            DebugUtil.Log("InitializeContentsGridContents");
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                ContentsCollection.Clear();
            });
        }

        private async void HideProgress()
        {
            DebugUtil.Log("Hide Progress");
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // await statusBar.ProgressIndicator.HideAsync();
            });
        }

        private async void ChangeProgressText(string text)
        {
            DebugUtil.Log("Show Progress: " + text);
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                /*
                statusBar.ProgressIndicator.ProgressValue = null;
                statusBar.ProgressIndicator.Text = text;
                await statusBar.ProgressIndicator.ShowAsync();
                */
            });
        }

        private void SetStillDetailVisibility(bool visible)
        {
            if (visible)
            {
                IsViewingDetail = true;
                PhotoScreen.Visibility = Visibility.Visible;
                ContentsGrid.IsEnabled = false;
                UpdateInnerState(ViewerState.LocalStillPlayback);
            }
            else
            {
                IsViewingDetail = false;
                PhotoScreen.Visibility = Visibility.Collapsed;
                ContentsGrid.IsEnabled = true;
                UpdateInnerState(ViewerState.LocalSingle);
            }
        }

        void InitBitmapBeforeOpen()
        {
            DebugUtil.Log("Before open");
            PhotoScreen.Init();
        }

        private void ReleaseDetail()
        {
            PhotoScreen.ReleaseImage();
            SetStillDetailVisibility(false);
        }

        private bool IsViewingDetail = false;

        private async void DeleteSelectedLocalImages()
        {
            DebugUtil.Log("DeleteSelectedLocalImages: " + ContentsGrid.SelectedItems.Count);
            var items = ContentsGrid.SelectedItems;
            if (items.Count == 0)
            {
                HideProgress();
                return;
            }

            foreach (var data in new List<object>(items).Select(item => item as Thumbnail).Where(thumb => thumb.CacheFile != null))
            {
                await TryDeleteLocalFile(data);
            }
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
                FinishLocalMoviePlayback();
                e.Handled = true;
            }

            if (ContentsGrid.SelectionMode == ListViewSelectionMode.Multiple)
            {
                DebugUtil.Log("Set selection mode none.");
                UpdateInnerState(ViewerState.LocalSingle);
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
                if (contents.Count > 0)
                {
                    UpdateInnerState(ViewerState.LocalSelecting);
                }
                else
                {
                    UpdateInnerState(ViewerState.LocalMulti);
                }
            }
        }

        private void ContentsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            GridSources.Source = ContentsCollection;
            (SemanticZoom.ZoomedOutView as ListViewBase).ItemsSource = GridSources.View.CollectionGroups;
        }

        private void ContentsGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            GridSources.Source = null;
        }

        private void ContentsGrid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void Playback_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var data = item.DataContext as Thumbnail;
            PlaybackLocalContent(data);
        }

        private void PlaybackLocalContent(Thumbnail content)
        {
            if (IsViewingDetail)
            {
                return;
            }

            if (content.IsMovie)
            {
                PlaybackLocalMovie(content);
            }
            else
            {
                PlaybackLocalImage(content);
            }
        }

        private async void PlaybackLocalImage(Thumbnail content)
        {
            ChangeProgressText(SystemUtil.GetStringResource("Progress_OpeningDetailImage"));

            try
            {
                using (var stream = await content.CacheFile.OpenStreamForReadAsync())
                {
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                    PhotoScreen.SourceBitmap = bitmap;
                    InitBitmapBeforeOpen();
                    PhotoScreen.SetBitmap();
                    try
                    {
                        PhotoData.MetaData = await JpegMetaDataParser.ParseImageAsync(stream);
                    }
                    catch (UnsupportedFileFormatException)
                    {
                        PhotoData.MetaData = null;
                        PhotoScreen.DetailInfoDisplayed = false;
                    }
                    SetStillDetailVisibility(true);
                }
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

        MoviePlaybackData MovieData = new MoviePlaybackData();

        private void PlaybackLocalMovie(Thumbnail content)
        {
            ChangeProgressText(SystemUtil.GetStringResource("Progress_OpeningMovieStream"));
            UpdateInnerState(ViewerState.LocalMoviePlayback);

            MoviePlayerWrapper.DataContext = MovieData;
            MoviePlayer.SetLocalContent(content);
        }

        private void FinishLocalMoviePlayback()
        {
            UpdateInnerState(ViewerState.LocalSingle);

            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MoviePlayer.Finish();
                MoviePlayerWrapper.Visibility = Visibility.Collapsed;
            });
        }

        void LocalMoviePlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                MoviePlayerWrapper.Visibility = Visibility.Visible;
                HideProgress();
            });
        }

        void LocalMoviePlayer_MediaFailed(object sender, string e)
        {
            UpdateInnerState(ViewerState.LocalSingle);

            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MoviePlayerWrapper.Visibility = Visibility.Collapsed;
                DebugUtil.Log("LocalMoviePlayer MediaFailed: " + e);
                ShowToast(SystemUtil.GetStringResource("Viewer_FailedPlaybackMovie"));
                HideProgress();
            });
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var data = item.DataContext as Thumbnail;
            if (data.CacheFile != null)
            {
                await TryDeleteLocalFile(data);
            }
        }

        private async Task TryDeleteLocalFile(Thumbnail data)
        {
            try
            {
                ContentsCollection.Remove(data);
                DebugUtil.Log("Delete " + data.CacheFile.DisplayName);
                await data.CacheFile.DeleteAsync();
            }
            catch (Exception ex)
            {
                DebugUtil.Log("Failed to delete file: " + ex.StackTrace);
            }
        }

        private void ContentsGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ContentsGrid.SelectionMode == ListViewSelectionMode.Multiple)
            {
                return;
            }

            var image = sender as Grid;
            var content = image.DataContext as Thumbnail;
            PlaybackLocalContent(content);
        }
    }

    public enum ViewerState
    {
        LocalSingle,
        LocalStillPlayback,
        LocalMoviePlayback,
        LocalMulti,
        LocalSelecting,
        OutOfPage,
    }
}
