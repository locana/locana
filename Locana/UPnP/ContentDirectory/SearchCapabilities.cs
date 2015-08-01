using System.Collections.Generic;
using System.Xml.Linq;

namespace Kazyx.Uwpmm.UPnP.ContentDirectory
{
    public class SearchCapabilities : ContentDirectoryResponse
    {
        public bool AnyPropertiesSupported { get; private set; }

        public IList<string> SupportedProperties { get; private set; }

        public static SearchCapabilities Parse(XDocument xml)
        {
            var body = GetBodyOrThrowError(xml);
            var res = body.Element(NS_U + "GetSearchCapabilitiesResponse");
            var caps = (string)res.Element("SearchCaps");

            var supported = new List<string>();
            var any = false;
            if (caps != null)
            {
                if (caps == "*")
                {
                    any = true;
                }
                else
                {
                    var sepa = caps.Split(',');
                    foreach (var capa in sepa)
                    {
                        var val = capa.Trim();
                        if (val.Length != 0)
                        {
                            supported.Add(val);
                        }
                    }
                }
            }

            return new SearchCapabilities
            {
                AnyPropertiesSupported = any,
                SupportedProperties = supported,
            };
        }

    }
}
