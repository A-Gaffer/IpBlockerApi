namespace IpBlockerApi.Models
{
    public class TemporalBlock
    {
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }                   // when to auto-unblock

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }
}
