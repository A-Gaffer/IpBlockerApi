using Newtonsoft.Json;

namespace IpBlockerApi.Models
{
    public class GeoLocationResult
    {
        [JsonProperty("ip")]
        public string Ip { get; set; } = string.Empty;

        [JsonProperty("country_code")]
        public string CountryCode { get; set; } = string.Empty;  // "US"

        [JsonProperty("country_name")]
        public string CountryName { get; set; } = string.Empty;  // "United States"

        [JsonProperty("org")]
        public string Isp { get; set; } = string.Empty;          // ISP / organisation

        [JsonProperty("city")]
        public string City { get; set; } = string.Empty;

        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("reason")]
        public string? ErrorReason { get; set; }
    }
}
