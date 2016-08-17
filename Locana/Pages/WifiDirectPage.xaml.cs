using Locana.DataModel;
using Locana.Network;
using Locana.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFiDirect;
using Windows.Networking.Proximity;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Locana.Pages
{

    public sealed partial class WifiDirectPage : Page
    {
        public WifiDirectPage()
        {
            this.InitializeComponent();
        }

        private ObservableCollection<PeerData> peers = new ObservableCollection<PeerData>();

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as PeerData;
            DebugUtil.Log(() => data.Title + " clicked");
            Connecting = true;
            AppShell.Current.ShowProgressDialog("[TBD] Establishing Wi-Fi Direct connection");

            try
            {
                var param = new WiFiDirectConnectionParameters();
                param.PreferenceOrderedConfigurationMethods.Add(WiFiDirectConfigurationMethod.PushButton);

                var wfdDevice = await WiFiDirectDevice.FromIdAsync(data.Info.Id, param);

                DebugUtil.Log(() => "Connected to: " + data.Title + " - " + wfdDevice.ConnectionStatus);
                wfdDevice.ConnectionStatusChanged += WfdDevice_ConnectionStatusChanged;

                NetworkObserver.INSTANCE.RegisterWifiDirectDevice(wfdDevice);

                data.Connected = true;
            }
            catch (Exception ex)
            {
                DebugUtil.Log(() => "Wi-Fi direct connection failed: " + ex.StackTrace);
                AppShell.Current.Toast.PushToast(new Controls.ToastContent
                {
                    Text = "[TBD] Failed to establish connection.",
                });
            }

            AppShell.Current.HideProgressDialog();
            Connecting = false;
        }

        private bool Connecting;

        private void WfdDevice_ConnectionStatusChanged(WiFiDirectDevice sender, object args)
        {
            if (sender.ConnectionStatus != WiFiDirectConnectionStatus.Connected)
            {
                NetworkObserver.INSTANCE.UnregisterWfdDevice(sender);
            }
            sender.ConnectionStatusChanged -= WfdDevice_ConnectionStatusChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Connecting = false;

            AppShell.Current.BackRequested += BackRequested;

            if (!((PeerFinder.SupportedDiscoveryTypes & PeerDiscoveryTypes.Browse) == PeerDiscoveryTypes.Browse))
            {
                AppShell.Current.Toast.PushToast(new Controls.ToastContent
                {
                    Text = "[TBD] Wi-Fi Direct is not supported on this machine"
                });
                return;
            }

            cancellationToken = new CancellationTokenSource();
            Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!Connecting)
                    {
                        DebugUtil.Log(() => "Find peers async");
                        var devices = await DeviceInformation.FindAllAsync(WiFiDirectDevice.GetDeviceSelector(WiFiDirectDeviceSelectorType.AssociationEndpoint));
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        try
                        {
                            DebugUtil.Log(() => "Peers discovered: " + devices.Count);
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                var toRemove = new List<PeerData>();
                                foreach (var p in peers)
                                {
                                    var contains = false;
                                    foreach (var d in devices)
                                    {
                                        if (p.Info.Id == d.Id)
                                        {
                                            contains = true;
                                            break;
                                        }
                                    }
                                    if (!contains)
                                    {
                                        toRemove.Add(p);
                                    }
                                }

                                foreach (var p in toRemove)
                                {
                                    peers.Remove(p);
                                }

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

                                    if (duplicated == null)
                                    {
                                        peers.Add(new PeerData { Info = device });
                                    }
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            DebugUtil.Log(() => "failed find peers: " + ex.StackTrace);
                        }
                    }

                    await Task.Delay(10000);
                }
            });
        }

        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Connecting)
            {
                AppShell.Current.HideProgressDialog();
                Connecting = false;
                e.Handled = true;
                return;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DebugUtil.Log(() => "Cancel P2P discovery");
            cancellationToken.Cancel();
            peers.Clear();
            AppShell.Current.BackRequested -= BackRequested;
            base.OnNavigatedFrom(e);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PeersList.ItemsSource = peers;
        }

        CancellationTokenSource cancellationToken;
    }

    public class PeerData : ObservableBase
    {
        public DeviceInformation Info { set; get; }

        public string Title
        {
            get
            {
                return Info.Name;
            }
        }

        public Brush ForegroundColor
        {
            get
            {
                if (Connected)
                {
                    return new SolidColorBrush(Colors.Magenta);
                }
                else
                {
                    return new SolidColorBrush(Colors.White);
                }
            }
        }

        private bool _Connected;
        public bool Connected
        {
            set
            {
                if (_Connected != value)
                {
                    _Connected = value;
                    NotifyChanged(nameof(Connected));
                    NotifyChanged(nameof(ForegroundColor));
                }
            }
            get { return _Connected; }
        }
    }
}
