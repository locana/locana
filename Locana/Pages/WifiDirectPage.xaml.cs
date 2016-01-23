using Windows.UI.Xaml.Controls;

namespace Locana.Pages
{

    public sealed partial class WifiDirectPage : Page
    {
        /*
        public WifiDirectPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        private NavigationHelper navigationHelper;
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private ObservableCollection<PeerData> peers = new ObservableCollection<PeerData>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as PeerData;
            DebugUtil.Log(data.Title + " clicked");
            try
            {
                var wfdDevice = await WifiDirectUtil.ConnectAsync(data.Info);
                DebugUtil.Log("Connected to: " + data.Title + " - " + wfdDevice.ConnectionStatus);
                NetworkObserver.INSTANCE.RegisterWifiDirectDevice(wfdDevice);
            }
            catch (Exception ex)
            {
                DebugUtil.Log("Wi-Fi direct connection failed: " + ex.StackTrace);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PeersList.ItemsSource = peers;

            searchTask = Task.Factory.StartNew(async () =>
            {
                while (searchTask != null && !searchTask.IsCanceled)
                {
                    DebugUtil.Log("Find peers async");
                    var devices = await WifiDirectUtil.FindWfdPeersAsync();
                    try
                    {
                        DebugUtil.Log("Peers discovered: " + devices.Count);
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            foreach (var device in devices)
                            {
                                PeerData duplicated = null;
                                foreach (var p in peers)
                                {
                                    if (p.Info.Id == device.Id)
                                    {
                                        duplicated = p;
                                        continue;
                                    }
                                }
                                if (duplicated != null)
                                {
                                    peers.Remove(duplicated);
                                }
                                peers.Add(new PeerData { Info = device });
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        DebugUtil.Log("failed find peers: " + ex.StackTrace);
                    }

                    await Task.Delay(10000);
                }
            });
        }

        Task searchTask;

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            PeersList.ItemsSource = peers;
            if (searchTask != null)
            {
                DebugUtil.Log("Cancel P2P discovery");
                searchTask.AsAsyncAction().Cancel();
            }
        }
        */
    }

    /*
    public class PeerData
    {
        public DeviceInformation Info { set; get; }

        public string Title
        {
            get
            {
                return Info.Name;
            }
        }
    }
    */
}
