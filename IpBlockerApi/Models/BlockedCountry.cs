namespace IpBlockerApi.Models
{
    public class BlockedCountry
    {
        public string CountryCode { get; set; } = string.Empty;  // e.g. "EG"
        public string CountryName { get; set; } = string.Empty;  // e.g. "Egypt"
        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
    }
}
