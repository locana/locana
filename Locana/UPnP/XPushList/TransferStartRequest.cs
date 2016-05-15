using System.Text;
using System.Xml.Linq;

namespace Locana.UPnP.XPushList
{
    public class TransferStartRequest : XPushListRequest
    {
        public override string ActionName { get { return "X_TransferStart"; } }

        public override Response ParseResponse(XDocument xml)
        {
            return ValueLessResponse.Parse(xml);
        }

        protected override void AppendSpecificMessage(StringBuilder builder)
        {
        }
    }
}
