namespace Locana.UPnP.XPushList
{
    public abstract class XPushListRequest : Request
    {
        public override string Urn { get { return URN.XPushList; } }
    }

    public abstract class XPushListResponse : Response
    {
        protected const string NS_U = "{" + URN.XPushList + "}";
    }
}
