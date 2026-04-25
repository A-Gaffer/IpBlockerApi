using IpBlockerApi.Models;
using Newtonsoft.Json;

namespace IpBlockerApi.Services
{
    public class GeolocationService : IGeolocationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<GeolocationService> _logger;

        public GeolocationService(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<GeolocationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        public async Task<GeoLocationResult?> LookupAsync(string ipAddress)
        {
            try
            {
                var apiKey = _config["GeoLocation:ApiKey"];
                var baseUrl = _config["GeoLocation:BaseUrl"] ?? "https://ipapi.co";

                // ipapi.co URL format: https://ipapi.co/{ip}/json/?key={apiKey}
                var url = string.IsNullOrWhiteSpace(apiKey)
                    ? $"{baseUrl}/{ipAddress}/json/"
                    : $"{baseUrl}/{ipAddress}/json/?key={apiKey}";

                var client = _httpClientFactory.CreateClient("GeoLocation");
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Geolocation API returned {Status} for IP {IP}",
                        response.StatusCode, ipAddress);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GeoLocationResult>(json);

                // ipapi.co embeds error details in the JSON body (not HTTP status)
                if (result?.Error == true)
                {
                    _logger.LogWarning("Geolocation API error for IP {IP}: {Reason}",
                        ipAddress, result.ErrorReason);
                    return null;
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error calling geolocation API for IP {IP}", ipAddress);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GeolocationService for IP {IP}", ipAddress);
                return null;
            }
        }
    }
}
