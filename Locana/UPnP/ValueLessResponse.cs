using System.Xml.Linq;

namespace Kazyx.Uwpmm.UPnP
{
    public class ValueLessResponse : Response
    {
        public static ValueLessResponse Parse(XDocument xml)
        {
            var body = GetBodyOrThrowError(xml);
            return new ValueLessResponse();
        }
    }
}
