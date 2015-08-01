using Kazyx.DeviceDiscovery;
using Kazyx.Uwpmm.CameraControl;
using Kazyx.Uwpmm.Playback;
using Kazyx.Uwpmm.UPnP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Networking.Connectivity;

namespace Kazyx.Uwpmm.Utility
{
    public class NetworkObserver
    {
        private static NetworkObserver sInstance = new NetworkObserver();

        public static NetworkObserver INSTANCE
        {
            get { return sInstance; }
        }

        private SsdpDiscovery discovery = new SsdpDiscovery();
        private SsdpDiscovery cdsDiscovery = new SsdpDiscovery();

        private NetworkObserver()
        {
            discovery.SonyCameraDeviceDiscovered += discovery_SonyCameraDeviceDiscovered;
            cdsDiscovery.DescriptionObtained += cdsDiscovery_DescriptionObtained;
        }

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

        public event EventHandler DevicesCleared;

        protected void OnDevicesCleared()
        {
            DevicesCleared.Raise(this, null);
        }

        private Dictionary<string, TargetDevice> devices = new Dictionary<string, TargetDevice>();

        public List<TargetDevice> CameraDevices
        {
            get { return new List<TargetDevice>(devices.Values); }
        }

        private Dictionary<string, UpnpDevice> cdServices = new Dictionary<string, UpnpDevice>();

        public List<UpnpDevice> CdsProviders
        {
            get { return new List<UpnpDevice>(cdServices.Values); }
        }

        public event EventHandler<CameraDeviceEventArgs> CameraDiscovered;

        protected void OnDiscovered(TargetDevice device)
        {
            CameraDiscovered.Raise(this, new CameraDeviceEventArgs { CameraDevice = device });
        }

        public event EventHandler<CdServiceEventArgs> CdsDiscovered;

        protected void OnDiscovered(UpnpDevice device)
        {
            CdsDiscovered.Raise(this, new CdServiceEventArgs { CdService = device });
        }

        void discovery_SonyCameraDeviceDiscovered(object sender, SonyCameraDeviceEventArgs e)
        {
            var device = new TargetDevice(e.SonyCameraDevice, e.LocalAddress);
            lock (devices)
            {
                if (devices.ContainsKey(e.SonyCameraDevice.UDN))
                {
                    return;
                }
                devices.Add(device.Udn, device);
            }
            OnDiscovered(device);
        }

        void cdsDiscovery_DescriptionObtained(object sender, DeviceDescriptionEventArgs e)
        {
            try
            {
                var device = UpnpDescriptionParser.ParseDescription(XDocument.Parse(e.Description), e.Location);
                device.LocalAddress = e.LocalAddress;

                lock (cdServices)
                {
                    if (cdServices.ContainsKey(device.UDN))
                    {
                        return;
                    }

                    if (device.Services.Any(service => service.Key == URN.ContentDirectory))
                    {
                        DebugUtil.Log("CDS found. Notify discovered.");
                        cdServices.Add(device.UDN, device);
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

        public void Finish()
        {
            NetworkInformation.NetworkStatusChanged -= NetworkInformation_NetworkStatusChanged;
            if (Canceller != null)
            {
                Canceller.Cancel();
                Canceller = null;
            }
            Clear();
            Started = false;
        }

        private void SearchCds()
        {
            cdsDiscovery.SearchUpnpDevices("urn:schemas-upnp-org:service:ContentDirectory:1");
        }

        private void SearchCamera()
        {
            discovery.SearchSonyCameraDevices();
        }

        private void Clear()
        {
            PreviousSsid = null;
            RefreshDevices();
        }

        public void RefreshDevices()
        {
            devices.Clear();
            cdServices.Clear();
            OnDevicesCleared();
        }

        public bool IsConnectedToCamera
        {
            get { return IsCameraAccessPoint(PreviousSsid); }
        }

        private bool IsCameraAccessPoint(string ssid)
        {
            return ssid != null && ssid.StartsWith("direct-", StringComparison.OrdinalIgnoreCase);
        }

        public string PreviousSsid { private set; get; }

        private async Task checkConnection(CancellationTokenSource cancel)
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
                    var previous = PreviousSsid;
                    PreviousSsid = ssid;
                    // Connected to Access Point and it is a camera device.
                    if (ssid == previous && devices.Count != 0)
                    {
                        // Keep searching even if CDS provider is discovered.
                        DebugUtil.Log("Some devices discovered on the previous SSID. Finish auto discovery.");
                        return;
                    }

                    if (ssid != previous)
                    {
                        DebugUtil.Log("New access point detected. Refresh.");
                        RefreshDevices();
                    }
                    else
                    {
                        DebugUtil.Log("No devices discovered yet. keep searching.");
                    }

                    SearchCamera();
                    SearchCds();
                    await Task.Delay(5000);

                    if (!cancel.IsCancellationRequested)
                    {
                        await checkConnection(cancel);
                    }
                    return;
                }
            }

            DebugUtil.Log("Not connected to camera device.");
            Clear();
        }

        CancellationTokenSource Canceller;

        void NetworkInformation_NetworkStatusChanged(object sender)
        {
            DebugUtil.Log("NetworkInformation NetworkStatusChanged");
            startTask();
        }

        private void startTask()
        {
            if (Canceller != null)
            {
                Canceller.Cancel();
                Canceller = null;
            }

            var cancel = new CancellationTokenSource();
            Canceller = cancel;
            var task = checkConnection(cancel);
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
