using System.Text;
using System.Xml.Linq;

namespace Kazyx.Uwpmm.UPnP.ContentDirectory
{
    public class GetSearchCapabilitiesRequest : ContentDirectoryRequest
    {
        public override string ActionName { get { return "GetSearchCapabilities"; } }

        protected override void AppendSpecificMessage(StringBuilder builder)
        {
        }

        public override Response ParseResponse(XDocument xml)
        {
            return SearchCapabilities.Parse(xml);
        }
    }
}
