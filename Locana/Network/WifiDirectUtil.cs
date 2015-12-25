using Locana.Utility;
using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFiDirect;
using Windows.Networking.Proximity;

namespace Locana.Network
{
    class WifiDirectUtil
    {
        public static Task<DeviceInformationCollection> FindWfdPeersAsync()
        {
            if (((PeerFinder.SupportedDiscoveryTypes & PeerDiscoveryTypes.Browse) == PeerDiscoveryTypes.Browse))
            {
                DebugUtil.Log("Wi-Fi direct supported");
            }
            else
            {
                DebugUtil.Log("Wi-Fi direct not supported");
            }

            return DeviceInformation.FindAllAsync(WiFiDirectDevice.GetDeviceSelector(WiFiDirectDeviceSelectorType.AssociationEndpoint)).AsTask();
        }

        public static async Task<WiFiDirectDevice> ConnectAsync(DeviceInformation info)
        {
            var param = new WiFiDirectConnectionParameters();
            param.PreferenceOrderedConfigurationMethods.Add(WiFiDirectConfigurationMethod.PushButton);

            var device = await WiFiDirectDevice.FromIdAsync(info.Id, param);
            device.ConnectionStatusChanged += (sender, args) =>
            {
                DebugUtil.Log("WiFi Direct disconnected: " + info.Name);
            };
            return device;
        }
    }
}
