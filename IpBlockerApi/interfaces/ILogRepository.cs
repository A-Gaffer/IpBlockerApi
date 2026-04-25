using IpBlockerApi.Models;

namespace IpBlockerApi.interfaces
{
    public interface ILogRepository
    {
        void Add(BlockAttemptLog log);
        IEnumerable<BlockAttemptLog> GetAll();
    }
}
