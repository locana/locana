using System.Collections.Generic;
using System.Xml.Linq;

namespace Locana.UPnP.ContentDirectory
{
    public class SortCapabilities : ContentDirectoryResponse
    {
        public bool AnyPropertiesSupported { get; private set; }

        public IList<string> SupportedProperties { get; private set; }

        public static SortCapabilities Parse(XDocument xml)
        {
            var body = GetBodyOrThrowError(xml);
            var res = body.Element(NS_U + "GetSortCapabilitiesResponse");
            var caps = (string)res.Element("SortCaps");

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

            return new SortCapabilities
            {
                AnyPropertiesSupported = any,
                SupportedProperties = supported,
            };
        }
    }
}
