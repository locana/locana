using System;
using System.Text;
using System.Xml.Linq;

namespace Locana.UPnP.XPushList
{
    public class GetListRequest : XPushListRequest
    {
        public override string ActionName
        {
            get
            {
                // TODO Name of action is required
                throw new NotImplementedException();
            }
        }

        public override Response ParseResponse(XDocument xml)
        {
            // TODO Spec of response message is required
            throw new NotImplementedException();
        }

        protected override void AppendSpecificMessage(StringBuilder builder)
        {
            // TODO additional request parameters
        }
    }
}
