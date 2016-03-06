using Kazyx.DeviceDiscovery;
using Locana.CameraControl;
using Locana.UPnP;
using Locana.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        // This collection is kept even if the device went offline.
        private Dictionary<string, string> DeviceNames = new Dictionary<string, string>();

        public bool TryGetDeviceName(string id, out string name)
        {
            return DeviceNames.TryGetValue(id, out name);
        }

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

            UpdateDeviceNameDictionary(device.Udn, device.FriendlyName, device.DeviceName);

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

                UpdateDeviceNameDictionary(device.UDN, device.FriendlyName, device.ModelName);

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
                DebugUtil.Log(() => ex.StackTrace);
            }
        }

        private void UpdateDeviceNameDictionary(string udn, string primaryName, string secondaryName)
        {
            if (primaryName != null)
            {
                DeviceNames[udn] = primaryName;
            }
            else if (secondaryName != null)
            {
                DeviceNames[udn] = secondaryName;
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
        public void RegisterWifiDirectDevice(WiFiDirectDevice device)
        {
            discovery.TargetWifiDirectDevices = device.GetConnectionEndpointPairs();
        }
        */

        private readonly Regex CameraApRegex = new Regex("^DIRECT-[a-z][a-z][A-Z]\\d:");

        public async Task<bool> IsConnectedToCameraApDirectly()
        {
            var filter = new ConnectionProfileFilter
            {
                IsConnected = true,
                IsWwanConnectionProfile = false,
                IsWlanConnectionProfile = true,
            };
            var profiles = await NetworkInformation.FindConnectionProfilesAsync(filter);

            var matched = profiles.Select(profile => profile.WlanConnectionProfileDetails.GetConnectedSsid())
                .FirstOrDefault(ssid => { return ssid != null && CameraApRegex.IsMatch(ssid); });

            return matched != null;
        }

        private async Task checkConnection(CancellationTokenSource cancel)
        {
            var adapters = await SsdpDiscovery.GetActiveAdaptersAsync();
            discovery.TargetNetworkAdapters = adapters;

            while (!cancel.IsCancellationRequested)
            {
                SearchCamera();
                SearchCds();
                await Task.Delay(5000).ConfigureAwait(false);
            }
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
