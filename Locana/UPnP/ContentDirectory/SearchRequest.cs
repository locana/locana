using System.Text;
using System.Xml.Linq;

namespace Kazyx.Uwpmm.UPnP.ContentDirectory
{
    public class SearchRequest : ContentDirectoryRequest
    {
        public override string ActionName { get { return "Search"; } }

        public SearchRequest()
        {
            Filter = "*";
        }

        public string ContainerID { get; set; }
        public string SearchCriteria { get; set; }
        public string Filter { get; set; }
        public int StartingIndex { get; set; }
        public int RequestedCount { get; set; }
        public string SortCriteria { get { return ""; } }

        protected override void AppendSpecificMessage(StringBuilder builder)
        {
            builder.Append("<ContainerID>").Append(ContainerID).Append("</ContainerID>").Append("\r\n")
                .Append("<SearchCriteria>").Append(SearchCriteria).Append("</SearchCriteria>").Append("\r\n")
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
