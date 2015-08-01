using Kazyx.DeviceDiscovery;
using Kazyx.RemoteApi.AvContent;
using Kazyx.RemoteApi.Camera;
using Kazyx.RemoteApi.System;
using Kazyx.Uwpmm.DataModel;
using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kazyx.Uwpmm.CameraControl
{
    public class DeviceApiHolder
    {
        public CameraApiClient Camera { private set; get; }

        public SystemApiClient System { private set; get; }

        public AvContentApiClient AvContent { private set; get; }

        public DeviceApiHolder(SonyCameraDeviceInfo info)
        {
            if (info.Endpoints.ContainsKey("camera"))
            {
                try
                {
                    Camera = new CameraApiClient(new Uri(info.Endpoints["camera"]));
                }
                catch { };
            }
            if (info.Endpoints.ContainsKey("system"))
            {
                try
                {
                    System = new SystemApiClient(new Uri(info.Endpoints["system"]));
                }
                catch { };
            }
            if (info.Endpoints.ContainsKey("avContent"))
            {
                try
                {
                    AvContent = new AvContentApiClient(new Uri(info.Endpoints["avContent"]));
                }
                catch { };
            }

            if (info.FriendlyName == "DSC-QX10")
            {
                ProductType = ProductType.DSC_QX10;
            }

            capability.PropertyChanged += api_PropertyChanged;
        }

        void api_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Version":
                    OnServerVersionDetected();
                    OnAvailableApisUpdated();
                    break;
                case "SupportedApis":
                    OnSupportedApisUpdated();
                    break;
                case "AvailableApis":
                    OnAvailableApisUpdated();
                    break;
            }
        }

        private readonly ApiCapabilityContainer capability = new ApiCapabilityContainer();

        public ApiCapabilityContainer Capability
        {
            get { return capability; }
        }

        private ProductType _Type = ProductType.UNDEFINED;
        public ProductType ProductType
        {
            get { return _Type; }
            internal set { _Type = value; }
        }

        internal async Task RetrieveApiList()
        {
            if (Camera != null)
            {
                // DSC-QX10 firmware v3.00 has a bug in the response of getMethodTypes
                var methods = (await Camera.GetMethodTypesAsync().ConfigureAwait(false))
                    .Where(method => !(method.Name == "setFocusMode" && ProductType == ProductType.DSC_QX10)).ToList();
                Capability.AddSupported(methods);
            }
            if (System != null)
            {
                Capability.AddSupported(await System.GetMethodTypesAsync().ConfigureAwait(false));
            }
            if (AvContent != null)
            {
                Capability.AddSupported(await AvContent.GetMethodTypesAsync().ConfigureAwait(false));
            }

            if (SupportedApisUpdated != null)
            {
                SupportedApisUpdated.Invoke(this, new SupportedApiEventArgs(Capability.SupportedApis));
            }
        }

        public event EventHandler<SupportedApiEventArgs> SupportedApisUpdated;

        protected void OnSupportedApisUpdated()
        {
            SupportedApisUpdated.Raise(this, new SupportedApiEventArgs(Capability.SupportedApis));
        }

        public event EventHandler<VersionEventArgs> ServerVersionDetected;

        protected void OnServerVersionDetected()
        {
            ServerVersionDetected.Raise(this, new VersionEventArgs(Capability.Version));
        }

        public event EventHandler<AvailableApiEventArgs> AvailiableApisUpdated;

        protected void OnAvailableApisUpdated()
        {
            AvailiableApisUpdated.Raise(this, new AvailableApiEventArgs(Capability.AvailableApis));
        }
    }

    public class SupportedApiEventArgs : EventArgs
    {
        private readonly Dictionary<string, IList<string>> apis;

        internal SupportedApiEventArgs(Dictionary<string, IList<string>> apis)
        {
            this.apis = apis;
        }

        public Dictionary<string, IList<string>> SupportedApis
        {
            get { return apis; }
        }
    }

    public class VersionEventArgs : EventArgs
    {
        private readonly ServerVersion version;

        internal VersionEventArgs(ServerVersion version)
        {
            this.version = version;
        }

        public ServerVersion Version
        {
            get { return version; }
        }
    }

    public class AvailableApiEventArgs : EventArgs
    {
        private readonly IList<string> apis;

        internal AvailableApiEventArgs(IList<string> apis)
        {
            this.apis = apis;
        }

        public IList<string> AvailableApis
        {
            get { return apis; }
        }
    }
}
