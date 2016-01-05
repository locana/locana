using Kazyx.DeviceDiscovery;
using Locana.CameraControl;
using Locana.UPnP;
using Locana.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Networking.Connectivity;

namespace Locana.Network
{
    public class NetworkObserver
    {
        private static NetworkObserver sInstance = new NetworkObserver();

        public static NetworkObserver INSTANCE
        {
            get { return sInstance; }
        }

        private readonly SsdpDiscovery discovery = new SsdpDiscovery();

        private NetworkObserver()
        {
            discovery.SonyCameraDeviceDiscovered += discovery_SonyCameraDeviceDiscovered;
            discovery.DescriptionObtained += cdsDiscovery_DescriptionObtained;
        }

        /*
        public async Task Initialize()
        {
            var filter = new ConnectionProfileFilter
            {
                IsConnected = true,
                IsWwanConnectionProfile = false,
                IsWlanConnectionProfile = true,
            };
            var profiles = await NetworkInformation.FindConnectionProfilesAsync(filter);
            foreach (var profile in profiles)
            {
                var ssid = profile.WlanConnectionProfileDetails.GetConnectedSsid();
                if (IsCameraAccessPoint(ssid))
                {
                    PreviousSsid = ssid;
                    return;
                }
            }
        }
        */

        public event EventHandler DevicesCleared;

        protected void OnDevicesCleared()
        {
            DevicesCleared?.Invoke(this, null);
        }

        private Dictionary<string, TargetDevice> remoteApiDevices = new Dictionary<string, TargetDevice>();

        public List<TargetDevice> CameraDevices
        {
            get { return new List<TargetDevice>(remoteApiDevices.Values); }
        }

        public bool TryGetCameraDevice(string id, out TargetDevice device)
        {
            return remoteApiDevices.TryGetValue(id, out device);
        }

        private Dictionary<string, UpnpDevice> cdsDevices = new Dictionary<string, UpnpDevice>();

        public List<UpnpDevice> CdsDevices
        {
            get { return new List<UpnpDevice>(cdsDevices.Values); }
        }

        public bool TryGetCdsDevice(string id, out UpnpDevice device)
        {
            return cdsDevices.TryGetValue(id, out device);
        }

        public event EventHandler<CameraDeviceEventArgs> CameraDiscovered;

        protected void OnDiscovered(TargetDevice device)
        {
            CameraDiscovered?.Invoke(this, new CameraDeviceEventArgs { CameraDevice = device });
        }

        public event EventHandler<CdServiceEventArgs> CdsDiscovered;

        protected void OnDiscovered(UpnpDevice device)
        {
            CdsDiscovered?.Invoke(this, new CdServiceEventArgs { CdService = device });
        }

        void discovery_SonyCameraDeviceDiscovered(object sender, SonyCameraDeviceEventArgs e)
        {
            var device = new TargetDevice(e.SonyCameraDevice, e.LocalAddress);
            lock (remoteApiDevices)
            {
                if (remoteApiDevices.ContainsKey(e.SonyCameraDevice.UDN))
                {
                    return;
                }
                remoteApiDevices.Add(device.Udn, device);
            }
            OnDiscovered(device);
        }

        void cdsDiscovery_DescriptionObtained(object sender, DeviceDescriptionEventArgs e)
        {
            try
            {
                var device = UpnpDescriptionParser.ParseDescription(XDocument.Parse(e.Description), e.Location);
                device.LocalAddress = e.LocalAddress;

                lock (cdsDevices)
                {
                    if (cdsDevices.ContainsKey(device.UDN))
                    {
                        return;
                    }

                    if (device.Services.Any(service => service.Key == URN.ContentDirectory))
                    {
                        DebugUtil.Log("CDS found. Notify discovered.");
                        cdsDevices.Add(device.UDN, device);
                        OnDiscovered(device);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugUtil.Log("failed to parse upnp device description.");
                DebugUtil.Log(ex.StackTrace);
            }

        }

        private bool Started = false;

        public void ForceRestart()
        {
            Finish();
            Start();
        }

        public void Start()
        {
            if (Started)
            {
                return;
            }

            Started = true;
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

            startTask();
        }

        public void Stop()
        {
            NetworkInformation.NetworkStatusChanged -= NetworkInformation_NetworkStatusChanged;
            Canceller?.Cancel();
            Canceller = null;
            Started = false;
        }

        public void Finish()
        {
            Stop();
            Clear();
        }

        private void SearchCds()
        {
            discovery.SearchUpnpDevices("urn:schemas-upnp-org:service:ContentDirectory:1");
        }

        private void SearchCamera()
        {
            discovery.SearchSonyCameraDevices();
        }

        private void Clear()
        {
            // PreviousSsid = null;
            RefreshDevices();
        }

        public void RefreshDevices()
        {
            remoteApiDevices.Clear();
            cdsDevices.Clear();
            discovery.ClearCache();
            OnDevicesCleared();
        }

        /*
        public bool IsConnectedToCamera
        {
            get { return IsCameraAccessPoint(PreviousSsid); }
        }

        private bool IsCameraAccessPoint(string ssid)
        {
            return ssid?.StartsWith("direct-", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public string PreviousSsid { private set; get; }
        */

        private async Task checkConnection(CancellationTokenSource cancel)
        {
            var adapters = await SsdpDiscovery.GetActiveAdaptersAsync();
            discovery.TargetNetworkAdapters = adapters;

            while (!cancel.IsCancellationRequested)
            {
                /*
                var filter = new ConnectionProfileFilter
                {
                    IsConnected = true,
                    IsWwanConnectionProfile = false,
                    IsWlanConnectionProfile = true,
                };
                var profiles = await NetworkInformation.FindConnectionProfilesAsync(filter);

                var wifiSsid = profiles.Select(profile => profile.WlanConnectionProfileDetails.GetConnectedSsid())
                    .FirstOrDefault(ssid => IsCameraAccessPoint(ssid));

                if (wifiSsid != null)
                {
                    var previous = PreviousSsid;
                    PreviousSsid = wifiSsid;
                    // Connected to Access Point and it is a camera device.
                    if (wifiSsid == previous && devices.Count != 0)
                    {
                        // Keep searching even if CDS provider is discovered.
                        DebugUtil.Log("Some devices discovered on the previous SSID. Finish auto discovery.");
                        return;
                    }

                    if (wifiSsid != previous)
                    {
                        DebugUtil.Log("New access point detected. Refresh.");
                        RefreshDevices();
                    }
                    else
                    {
                        DebugUtil.Log("No devices discovered yet. keep searching.");
                    }
                }
                */

                SearchCamera();
                SearchCds();
                await Task.Delay(5000).ConfigureAwait(false);
            }

            // DebugUtil.Log("Not connected to camera device.");
            // Clear();
        }

        CancellationTokenSource Canceller;

        void NetworkInformation_NetworkStatusChanged(object sender)
        {
            DebugUtil.Log("NetworkInformation NetworkStatusChanged");
            RefreshDevices();
            startTask();
        }

        private void startTask()
        {
            Canceller?.Cancel();
            Canceller = new CancellationTokenSource();
            var task = checkConnection(Canceller);
        }
    }

    public class CdServiceEventArgs : EventArgs
    {
        public UpnpDevice CdService { set; get; }
    }

    public class CameraDeviceEventArgs : EventArgs
    {
        public TargetDevice CameraDevice { set; get; }
    }
}
