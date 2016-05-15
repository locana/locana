using System.Text;
using System.Xml.Linq;

namespace Locana.UPnP.XPushList
{
    public class TransferProgressRequest : XPushListRequest
    {
        public int NumTotal { set; get; } = 0;
        public int NumTransferred { set; get; } = 0;

        public override string ActionName { get { return "X_TransferProgress"; } }

        public override Response ParseResponse(XDocument xml)
        {
            return ValueLessResponse.Parse(xml);
        }

        protected override void AppendSpecificMessage(StringBuilder builder)
        {
            builder.Append("<NumTotal>").Append(NumTotal).Append("</NumTotal>").Append("\r\n")
                .Append("<NumTransferd>").Append(NumTransferred).Append("</NumTransferd>").Append("\r\n");
        }
    }
}
