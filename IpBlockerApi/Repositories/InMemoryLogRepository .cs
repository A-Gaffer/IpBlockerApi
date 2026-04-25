using IpBlockerApi.interfaces;
using IpBlockerApi.Models;
using System.Collections.Concurrent;

namespace IpBlockerApi.Repositories
{
    public class InMemoryLogRepository : ILogRepository
    {
        private readonly ConcurrentQueue<BlockAttemptLog> _queue = new();

        public void Add(BlockAttemptLog log) => _queue.Enqueue(log);

        public IEnumerable<BlockAttemptLog> GetAll()
            => _queue.Reverse().ToList();   // newest first
    }
}
