using IpBlockerApi.Models;

namespace IpBlockerApi.Services
{
    public interface ILogService
    {
        void LogAttempt(string ip, string countryCode, bool isBlocked, string userAgent);
        IEnumerable<BlockAttemptLog> GetLogs(int page, int pageSize);
        int GetTotalCount();
    }
}
