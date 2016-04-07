using Newtonsoft.Json;
using System.Collections.Generic;

namespace Locana.DataModel
{
    public class LicenseJson
    {
        [JsonProperty("oss_list")]
        public List<OssInfo> OssList { set; get; }
    }

    public class OssInfo
    {
        [JsonProperty("name")]
        public string Name { set; get; }

        [JsonProperty("license")]
        public string License { set; get; }

        [JsonProperty("url")]
        public string Url { set; get; }
    }

    public class ContributorJson
    {
        [JsonProperty("contributors")]
        public List<Contributor> Contributors { set; get; }
    }

    public class Contributor
    {
        [JsonProperty("name")]
        public string Name { set; get; }

        [JsonProperty("url")]
        public string Url { set; get; }

        [JsonProperty("contribution")]
        public string Contribution { set; get; }
    }
}
