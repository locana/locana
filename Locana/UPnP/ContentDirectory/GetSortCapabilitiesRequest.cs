using System;
using System.Text;
using System.Xml.Linq;

namespace Locana.UPnP.ContentDirectory
{
    public class GetSortCapabilitiesRequest : ContentDirectoryRequest
    {
        public override string ActionName { get { return "GetSortCapabilities"; } }

        public override Response ParseResponse(XDocument xml)
        {
            throw new NotImplementedException();
        }

        protected override void AppendSpecificMessage(StringBuilder builder)
        {
        }
    }
}
