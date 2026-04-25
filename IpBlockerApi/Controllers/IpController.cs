using IpBlockerApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IpBlockerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IpController : ControllerBase
    {
        private readonly IGeolocationService _geoService;
        private readonly ICountryBlockService _blockService;
        private readonly ILogService _logService;

        public IpController(
            IGeolocationService geoService,
            ICountryBlockService blockService,
            ILogService logService)
        {
            _geoService = geoService;
            _blockService = blockService;
            _logService = logService;
        }

        // ── 4. IP lookup ──────────────────────────────────────────────────────────
        /// <summary>
        /// Look up country details for an IP address.
        /// If ipAddress is omitted, the caller's own IP is used.
        /// </summary>
        [HttpGet("lookup")]
        public async Task<IActionResult> Lookup([FromQuery] string? ipAddress)
        {
            // If no IP provided, use the caller's IP from HttpContext
            var ip = string.IsNullOrWhiteSpace(ipAddress)
                ? GetCallerIp()
                : ipAddress.Trim();

            // Basic IP format validation
            if (!IPAddress.TryParse(ip, out _))
                return BadRequest(new { message = $"'{ip}' is not a valid IP address." });

            var result = await _geoService.LookupAsync(ip);

            if (result is null)
                return StatusCode(503, new { message = "Geolocation service unavailable or IP not found." });

            return Ok(new
            {
                result.Ip,
                result.CountryCode,
                result.CountryName,
                result.City,
                result.Isp
            });
        }

        // ── 5. Check if caller IP is blocked ──────────────────────────────────────
        /// <summary>
        /// Automatically fetches the caller's IP, resolves its country,
        /// checks if that country is blocked, and logs the attempt.
        /// </summary>
        [HttpGet("check-block")]
        public async Task<IActionResult> CheckBlock()
        {
            var ip = GetCallerIp();
            var userAgent = Request.Headers.UserAgent.ToString();

            // Step 1: resolve country from IP
            var geoResult = await _geoService.LookupAsync(ip);

            if (geoResult is null)
                return StatusCode(503, new { message = "Could not resolve IP geolocation." });

            var countryCode = geoResult.CountryCode;

            // Step 2: check against blocked list (permanent + temporal)
            var isBlocked = _blockService.IsCountryBlocked(countryCode);

            // Step 3: log the attempt
            _logService.LogAttempt(ip, countryCode, isBlocked, userAgent);

            return Ok(new
            {
                ip,
                countryCode,
                countryName = geoResult.CountryName,
                isBlocked,
                message = isBlocked
                    ? $"Access denied — country '{countryCode}' is blocked."
                    : $"Access allowed — country '{countryCode}' is not blocked."
            });
        }

        // ── Helper ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the real caller IP.
        /// X-Forwarded-For is checked first because behind a reverse proxy / load balancer
        /// the RemoteIpAddress is the proxy's IP, not the real client.
        /// </summary>
        private string GetCallerIp()
        {
            // X-Forwarded-For can be a comma-separated list; the first one is the original client
            var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(forwarded))
                return forwarded.Split(',')[0].Trim();

            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
        }
    }
}
