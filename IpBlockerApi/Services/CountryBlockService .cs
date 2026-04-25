using IpBlockerApi.interfaces;
using IpBlockerApi.Models;

namespace IpBlockerApi.Services
{
    public class CountryBlockService : ICountryBlockService
    {
        private readonly IBlockedCountryRepository _permanentRepo;
        private readonly ITemporalBlockRepository _temporalRepo;
        private readonly IGeolocationService _geoService;

        // Simple valid 2-letter ISO codes list (basic validation)
        // A full list would be in a config file, but this covers the concept
        private static readonly HashSet<string> KnownInvalidCodes = new() { "XX", "ZZ" };

        public CountryBlockService(
            IBlockedCountryRepository permanentRepo,
            ITemporalBlockRepository temporalRepo,
            IGeolocationService geoService)
        {
            _permanentRepo = permanentRepo;
            _temporalRepo = temporalRepo;
            _geoService = geoService;
        }

        // ── Permanent blocks ─────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> BlockCountryAsync(string countryCode)
        {
            var code = countryCode.ToUpper();

            if (_permanentRepo.Exists(code))
                return (false, $"Country '{code}' is already blocked.");

            // Resolve country name via geolocation API
            // We look up a known IP for that country — or just trust the code and skip lookup
            // For "add blocked country" we only need the name, so we call geo with the code directly
            // ipapi.co doesn't resolve by code; we store what we can and accept unknown names
            var country = new BlockedCountry
            {
                CountryCode = code,
                CountryName = GetCountryName(code), // local static helper
                BlockedAt = DateTime.UtcNow
            };

            var added = _permanentRepo.TryAdd(country);
            return added
                ? (true, $"Country '{code}' has been blocked successfully.")
                : (false, $"Country '{code}' is already blocked.");
        }

        public bool UnblockCountry(string countryCode)
            => _permanentRepo.TryRemove(countryCode.ToUpper());

        public IEnumerable<BlockedCountry> GetBlockedCountries(int page, int pageSize, string? search)
        {
            var all = _permanentRepo.GetAll();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                all = all.Where(c =>
                    c.CountryCode.ToLower().Contains(s) ||
                    c.CountryName.ToLower().Contains(s));
            }

            return all
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }

        public bool IsCountryPermanentlyBlocked(string countryCode)
            => _permanentRepo.Exists(countryCode.ToUpper());

        // ── Temporal blocks ──────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> TemporalBlockCountryAsync(
            string countryCode, int durationMinutes)
        {
            var code = countryCode.ToUpper();

            // Reject obviously invalid codes
            if (KnownInvalidCodes.Contains(code) || code.Length != 2)
                return (false, $"'{code}' is not a valid country code.");

            if (_temporalRepo.Exists(code))
                return (false, $"Country '{code}' is already temporarily blocked.");

            var block = new TemporalBlock
            {
                CountryCode = code,
                CountryName = GetCountryName(code),
                BlockedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(durationMinutes)
            };

            var added = _temporalRepo.TryAdd(block);
            return added
                ? (true, $"Country '{code}' blocked temporarily until {block.ExpiresAt:u}.")
                : (false, $"Country '{code}' is already temporarily blocked.");
        }

        public bool IsCountryTemporallyBlocked(string countryCode)
            => _temporalRepo.IsCountryBlocked(countryCode.ToUpper());

        public bool IsCountryBlocked(string countryCode)
            => IsCountryPermanentlyBlocked(countryCode) || IsCountryTemporallyBlocked(countryCode);

        // ── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Minimal static lookup for common codes.
        /// In production you'd load this from a JSON file or the geo API.
        /// </summary>
        private static string GetCountryName(string code) => code.ToUpper() switch
        {
            "EG" => "Egypt",
            "US" => "United States",
            "GB" => "United Kingdom",
            "DE" => "Germany",
            "FR" => "France",
            "SA" => "Saudi Arabia",
            "AE" => "United Arab Emirates",
            "TR" => "Turkey",
            "IN" => "India",
            "CN" => "China",
            "RU" => "Russia",
            "BR" => "Brazil",
            "JP" => "Japan",
            "KR" => "South Korea",
            "CA" => "Canada",
            "AU" => "Australia",
            _ => code  // fallback: just store the code as the name
        };
    }
}
