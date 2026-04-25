using IpBlockerApi.Models;

namespace IpBlockerApi.Services
{
    public interface ICountryBlockService
    {
        // Permanent blocks
        Task<(bool Success, string Message)> BlockCountryAsync(string countryCode);
        bool UnblockCountry(string countryCode);
        IEnumerable<BlockedCountry> GetBlockedCountries(int page, int pageSize, string? search);
        bool IsCountryPermanentlyBlocked(string countryCode);

        // Temporal blocks
        Task<(bool Success, string Message)> TemporalBlockCountryAsync(string countryCode, int durationMinutes);
        bool IsCountryTemporallyBlocked(string countryCode);
        bool IsCountryBlocked(string countryCode); // checks both lists
    }
}
