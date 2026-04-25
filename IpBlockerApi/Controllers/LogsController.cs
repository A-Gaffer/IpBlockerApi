using IpBlockerApi.Models.DTOs;
using IpBlockerApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IpBlockerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogsController(ILogService logService) => _logService = logService;

        // ── 6. Get blocked attempt logs ───────────────────────────────────────────
        /// <summary>Return a paginated list of all IP check-block attempts.</summary>
        [HttpGet("blocked-attempts")]
        public IActionResult GetBlockedAttempts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var total = _logService.GetTotalCount();
            var logs = _logService.GetLogs(page, pageSize);

            return Ok(new PagedResponse<object>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Data = logs.Select(l => new
                {
                    l.IpAddress,
                    l.Timestamp,
                    l.CountryCode,
                    l.IsBlocked,
                    l.UserAgent
                })
            });
        }
    }
}
