using IpBlockerApi.interfaces;
using IpBlockerApi.Models;

namespace IpBlockerApi.Services
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _repo;

        public LogService(ILogRepository repo) => _repo = repo;

        public void LogAttempt(string ip, string countryCode, bool isBlocked, string userAgent)
        {
            _repo.Add(new BlockAttemptLog
            {
                IpAddress = ip,
                CountryCode = countryCode,
                IsBlocked = isBlocked,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            });
        }

        public IEnumerable<BlockAttemptLog> GetLogs(int page, int pageSize)
            => _repo.GetAll()
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

        public int GetTotalCount() => _repo.GetAll().Count();
    }
}
