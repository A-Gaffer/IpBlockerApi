using IpBlockerApi.interfaces;
using IpBlockerApi.Models.DTOs;
using IpBlockerApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IpBlockerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryBlockService _blockService;
        private readonly IBlockedCountryRepository _repo;

        public CountriesController(ICountryBlockService blockService, IBlockedCountryRepository repo)
        {
            _blockService = blockService;
            _repo = repo;
        }

        // ── 1. Add a blocked country ──────────────────────────────────────────────
        /// <summary>Block a country permanently by its 2-letter ISO code.</summary>
        [HttpPost("block")]
        public async Task<IActionResult> BlockCountry([FromBody] BlockCountryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _blockService.BlockCountryAsync(request.CountryCode);

            if (!success)
                return Conflict(new ErrorResponse { Message = message });   // 409 for duplicates

            return Ok(new { message });
        }

        // ── 2. Delete a blocked country ───────────────────────────────────────────
        /// <summary>Remove a country from the permanent block list.</summary>
        [HttpDelete("block/{countryCode}")]
        public IActionResult UnblockCountry(string countryCode)
        {
            var removed = _blockService.UnblockCountry(countryCode);

            if (!removed)
                return NotFound(new ErrorResponse { Message = $"Country '{countryCode.ToUpper()}' is not blocked." });

            return Ok(new { message = $"Country '{countryCode.ToUpper()}' has been unblocked." });
        }

        // ── 3. Get all blocked countries (paginated + searchable) ─────────────────
        /// <summary>List all permanently blocked countries with pagination and optional search.</summary>
        [HttpGet("blocked")]
        public IActionResult GetBlockedCountries(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var total = _repo.GetAll()
                .Where(c => string.IsNullOrWhiteSpace(search) ||
                            c.CountryCode.ToLower().Contains(search.ToLower()) ||
                            c.CountryName.ToLower().Contains(search.ToLower()))
                .Count();

            var data = _blockService.GetBlockedCountries(page, pageSize, search);

            return Ok(new PagedResponse<object>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Data = data.Select(c => new
                {
                    c.CountryCode,
                    c.CountryName,
                    c.BlockedAt
                })
            });
        }

        // ── 7. Temporal block ─────────────────────────────────────────────────────
        /// <summary>Block a country for a limited time (1–1440 minutes).</summary>
        [HttpPost("temporal-block")]
        public async Task<IActionResult> TemporalBlock([FromBody] TemporalBlockRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _blockService.TemporalBlockCountryAsync(
                request.CountryCode, request.DurationMinutes);

            if (!success)
            {
                // 409 Conflict for duplicate, 400 for invalid code
                return message.Contains("not a valid")
                    ? BadRequest(new ErrorResponse { Message = message })
                    : Conflict(new ErrorResponse { Message = message });
            }

            return Ok(new { message });
        }
    }
}
