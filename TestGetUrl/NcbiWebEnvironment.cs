namespace TestGetUrl
{
    using Newtonsoft.Json;

    public class NcbiWebEnvironment
    {
        [JsonProperty(PropertyName = "webenv")]
        public string WebEnvironment { get; set; }

        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }
    }
}
