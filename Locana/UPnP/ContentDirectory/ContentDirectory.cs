
namespace Kazyx.Uwpmm.UPnP.ContentDirectory
{
    public abstract class ContentDirectoryRequest : Request
    {
        public override string Urn { get { return URN.ContentDirectory; } }
    }

    public abstract class ContentDirectoryResponse : Response
    {
        protected const string NS_U = "{" + URN.ContentDirectory + "}";
        protected const string NS_DC = "{http://purl.org/dc/elements/1.1/}";
        protected const string NS_UPNP = "{urn:schemas-upnp-org:metadata-1-0/upnp/}";
        protected const string NS_DIDL = "{urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/}";
    }
}
