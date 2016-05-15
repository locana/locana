using System.Xml.Linq;

namespace Locana.UPnP.XPushList
{
    public class PushRoot : XPushListResponse
    {
        public string ObjectID { private set; get; }

        public static PushRoot Parse(XDocument xml)
        {
            var body = GetBodyOrThrowError(xml);
            var res = body.Element(NS_U + "X_GetPushRootResponse");
            var id = res.Element("ObjectID").Value;

            return new PushRoot
            {
                ObjectID = id
            };
        }
    }
}
