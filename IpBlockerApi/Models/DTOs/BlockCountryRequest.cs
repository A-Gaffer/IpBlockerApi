using System.ComponentModel.DataAnnotations;

namespace IpBlockerApi.Models.DTOs
{
    public class BlockCountryRequest
    {
        [Required]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 letters (e.g. EG, US).")]
        public string CountryCode { get; set; } = string.Empty;
    }
}
