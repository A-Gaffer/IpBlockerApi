namespace IpBlockerApi.Models
{
    public class BlockAttemptLog
    {
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string CountryCode { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
        public string UserAgent { get; set; } = string.Empty;
    }
}
