using System.ComponentModel.DataAnnotations;

namespace IpBlockerApi.Models.DTOs
{
    public class TemporalBlockRequest
    {
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string CountryCode { get; set; } = string.Empty;

        [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes (24 hours).")]
        public int DurationMinutes { get; set; }
    }
}
