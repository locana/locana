using System.Text;
using System.Xml.Linq;

namespace Locana.UPnP.XPushList
{
    public class TransferEndRequest : XPushListRequest
    {
        public int ErrorCode { set; get; } = 0;

        public override string ActionName { get { return "X_TransferEnd"; } }

        public override Response ParseResponse(XDocument xml)
        {
            return ValueLessResponse.Parse(xml);
        }

        protected override void AppendSpecificMessage(StringBuilder builder)
        {
            builder.Append("<ErrCode>").Append(ErrorCode).Append("</ErrCode>").Append("\r\n");
        }
    }
}
