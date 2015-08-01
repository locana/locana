using System.Linq;
using System.Xml.Linq;

namespace Kazyx.Uwpmm.UPnP.ContentDirectory
{
    public class RetrievedContents : ContentDirectoryResponse
    {
        public int NumberReturned { get; private set; }
        public int TotalMatches { get; private set; }
        public string UpdateID { get; private set; }

        public Result Result { get; private set; }

        public static RetrievedContents Parse(XDocument xml, string action)
        {
            var body = GetBodyOrThrowError(xml);
            var response = body.Element(NS_U + action + "Response");
            var result = response.Element("Result");
            var root = result.Element(NS_DIDL + "DIDL-Lite");

            var numReturned = int.Parse(response.Element("NumberReturned").Value);
            var total = int.Parse(response.Element("TotalMatches").Value);
            var updateId = response.Element("UpdateID").Value;

            var containers = root.Elements(NS_DIDL + "container")
                .Select(element => new Container
                {
                    Id = element.Attribute("id").Value,
                    ParentId = element.Attribute("parentID").Value,
                    Title = element.Element(NS_DC + "title").Value,
                    Class = element.Element(NS_UPNP + "class").Value,
                    Restricted = BoolConversionHelper.Parse(element.Attribute("restricted")),
                    ChildCount = (int?)element.Attribute("childCount"),
                    WriteStatus = (string)element.Element(NS_DIDL + "writeStatus"),
                }).ToList();

            var items = root.Elements(NS_DIDL + "item")
                .Select(element => new Item
                {
                    Id = element.Attribute("id").Value,
                    ParentId = element.Attribute("parentID").Value,
                    Title = element.Element(NS_DC + "title").Value,
                    Class = element.Element(NS_UPNP + "class").Value,
                    Restricted = BoolConversionHelper.Parse(element.Attribute("restricted")),
                    WriteStatus = (string)element.Element(NS_DIDL + "writeStatus"),
                    Resources = element.Elements(NS_DIDL + "res").Select(res => new Resource
                    {
                        ProtocolInfo = ProtocolInfo.Parse(res.Attribute("protocolInfo")),
                        Resolution = (string)res.Attribute("resolution"),
                        SizeInByte = (long?)res.Attribute("size"),
                        ResourceUrl = (string)res.Value,
                    }).ToList(),
                    Date = (string)element.Element(NS_DC + "date"),
                    Genre = (string)element.Element(NS_UPNP + "upnp"),
                }).ToList();

            return new RetrievedContents
            {
                Result = new Result
                {
                    Containers = containers,
                    Items = items,
                },
                NumberReturned = numReturned,
                TotalMatches = total,
                UpdateID = updateId,
            };
        }
    }
}
