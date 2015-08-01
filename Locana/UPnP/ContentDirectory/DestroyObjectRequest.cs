using System.Text;
using System.Xml.Linq;

namespace Kazyx.Uwpmm.UPnP.ContentDirectory
{
    public class DestroyObjectRequest : ContentDirectoryRequest
    {
        public override string ActionName { get { return "DestroyObject"; } }

        public string ObjectID { get; set; }

        protected override void AppendSpecificMessage(StringBuilder builder)
        {
            builder.Append("<ObjectID>").Append(ObjectID).Append("</ObjectID>").Append("\r\n");
        }

        public override Response ParseResponse(XDocument xml)
        {
            return ValueLessResponse.Parse(xml);
        }
    }
}
