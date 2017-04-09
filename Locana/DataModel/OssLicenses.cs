using Newtonsoft.Json;
using System.Collections.Generic;

namespace Locana.DataModel
{
    public class OssLicenses
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
}
