using System.Text;
using System.Xml.Linq;

namespace Kazyx.Uwpmm.UPnP.ContentDirectory
{
    public class BrowseRequest : ContentDirectoryRequest
    {
        public override string ActionName { get { return "Browse"; } }

        public BrowseRequest()
        {
            Filter = "*";
        }

        public string ObjectID { get; set; }
        public BrowseFlag BrowseFlag { get; set; }
        public string Filter { get; set; }
        public int StartingIndex { get; set; }
        public int RequestedCount { get; set; }
        public string SortCriteria { get { return ""; } }

        protected override void AppendSpecificMessage(StringBuilder builder)
        {
            builder.Append("<ObjectID>").Append(ObjectID).Append("</ObjectID>").Append("\r\n")
                .Append("<BrowseFlag>").Append(BrowseFlag.ToString()).Append("</BrowseFlag>").Append("\r\n")
                .Append("<Filter>").Append(Filter).Append("</Filter>").Append("\r\n")
                .Append("<StartingIndex>").Append(StartingIndex).Append("</StartingIndex>").Append("\r\n")
                .Append("<RequestedCount>").Append(RequestedCount).Append("</RequestedCount>").Append("\r\n")
                .Append("<SortCriteria>").Append(SortCriteria).Append("</SortCriteria>").Append("\r\n");
        }

        public override Response ParseResponse(XDocument xml)
        {
            return RetrievedContents.Parse(xml, ActionName);
        }
    }
}
