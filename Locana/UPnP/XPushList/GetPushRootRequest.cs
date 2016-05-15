using System.Text;
using System.Xml.Linq;

namespace Locana.UPnP.XPushList
{
    public class GetPushRootRequest : XPushListRequest
    {
        public override string ActionName { get { return "X_GetPushRoot"; } }

        public override Response ParseResponse(XDocument xml)
        {
            return PushRoot.Parse(xml);
        }

        protected override void AppendSpecificMessage(StringBuilder builder)
        {
        }
    }
}
