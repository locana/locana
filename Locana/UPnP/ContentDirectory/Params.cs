using System.Collections.Generic;
using System.Xml.Linq;

namespace Kazyx.Uwpmm.UPnP.ContentDirectory
{
    public class Result
    {
        public IList<Container> Containers { get; set; }
        public IList<Item> Items { get; set; }
    }

    public abstract class Content
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public bool Restricted { get; set; }

        public string Title { get; set; }
        public string WriteStatus { get; set; }

        public string Class { get; set; }
    }

    public class Item : Content
    {
        public IList<Resource> Resources { get; set; }

        public string Date { get; set; }
        public string Genre { get; set; }
    }

    public class Resource
    {
        public ProtocolInfo ProtocolInfo { get; set; }
        public string ResourceUrl { get; set; }
        public long? SizeInByte { get; set; }
        public string Resolution { get; set; }
    }

    public class ProtocolInfo
    {
        public string MimeType { get; set; }
        public string DlnaProfileName { get; set; }
        public bool IsOriginalContent { get; set; }

        public static ProtocolInfo Parse(XAttribute protocolInfo)
        {
            var protocol = new ProtocolInfo();

            if (protocolInfo == null)
            {
                return protocol;
            }

            var commSepa = protocolInfo.Value.Split(':');
            if (commSepa.Length < 3)
            {
                return protocol;
            }

            protocol.MimeType = commSepa[2];

            for (int i = 3; i < commSepa.Length; i++)
            {
                if (commSepa[i].Contains("DLNA.ORG"))
                {
                    var dlnaSepa = commSepa[3].Split(';');
                    foreach (var dlna in dlnaSepa)
                    {
                        if (dlna.StartsWith("DLNA.ORG_PN="))
                        {
                            protocol.DlnaProfileName = dlna.Substring(12);
                        }
                        else if (dlna.StartsWith("DLNA.ORG_CI="))
                        {
                            var ci = 1;
                            if (int.TryParse(dlna.Substring(12), out ci))
                            {
                                protocol.IsOriginalContent = ci == 0;
                            }
                        }
                    }
                    break;
                }
            }

            return protocol;
        }
    }

    public class Container : Content
    {
        public int? ChildCount { get; set; }
        public bool Searchable { get; set; }
    }

    public enum BrowseFlag
    {
        BrowseMetadata,
        BrowseDirectChildren,
    }
}
