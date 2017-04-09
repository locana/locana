using Newtonsoft.Json;
using System.Collections.Generic;

namespace Locana.DataModel
{
    public class ContributorsInfo
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

        [JsonProperty("what_for")]
        public string WhatFor { set; get; }
    }
}
